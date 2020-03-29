using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ServiceManager.Data;
using ServiceManager.Models;
using Firebase.Auth;
using Firebase.Storage;
using System.Threading;
using System.Text;
using System.IO;
using System.Security;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Twilio;
using Twilio.Types;
using Twilio.Rest.Api.V2010.Account;

namespace ServiceManager.Controllers
{

   
    public class WorkOrdersController : Controller
    {

       

        private readonly ApplicationDbContext _context;
        public IList<SelectListItem> UserList { get; set; }
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<AppRole> _roleManager;
        private readonly StorageAccountOptions _storageAccountOptions;
        private readonly TwilioSMS _twilioSMS;
        
        private Task<ApplicationUser> GetCurrentUser() => _userManager.GetUserAsync(HttpContext.User);
        public WorkOrdersController(ApplicationDbContext context,
                UserManager<ApplicationUser> userManager,
                SignInManager<ApplicationUser> signInManager,
                RoleManager<AppRole> roleManager,
                IOptionsSnapshot<StorageAccountOptions> storageOptions,
                IOptionsSnapshot<TwilioSMS> twilioSMS
            )
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _storageAccountOptions = storageOptions.Value;
            _twilioSMS = twilioSMS.Value;
        }


      


        // GET: WorkOrders
        public async Task<IActionResult> Index(string sortOrder, string currentFilter, string searchString, int? pageNumber)
        {

            ViewData["CurrentSort"] = sortOrder;
            ViewData["NameSortParm"] = sortOrder == "name_asc" ? "name_desc" : "name_asc";            
            ViewData["DateSortParm"] = sortOrder == "Date" ? "date_desc" : "Date";
            ViewData["ContSortParm"] = sortOrder == "cont_assigned" ? "cont_assigned_desc" : "cont_assigned";
            ViewData["StatusSortParm"] = sortOrder == "status_asc" ? "status_desc" : "status_asc";


            if (searchString != null)
            {
                pageNumber = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewData["CurrentFilter"] = searchString;

            var WorkOrder = from s in _context.WorkOrder
                           select s;


            if (!String.IsNullOrEmpty(searchString))
            {
                WorkOrder = WorkOrder.Where(s => s.WorkServiceName.Contains(searchString)
                                       || s.Property_Address.Contains(searchString)
                                       || s.Contractor_Assigned.Contains(searchString));
            }
            switch (sortOrder)
            {
                case "status_desc":
                    WorkOrder = WorkOrder.OrderByDescending(s => s.Service_Status);
                    break;
                case "status_asc":
                    WorkOrder = WorkOrder.OrderBy(s => s.Service_Status);
                    break;
                case "name_desc":
                    WorkOrder = WorkOrder.OrderByDescending(s => s.WorkServiceName);
                    break;
                case "name_asc":
                    WorkOrder = WorkOrder.OrderBy(s => s.WorkServiceName);
                    break;
                case "Date":
                    WorkOrder = WorkOrder.OrderBy(s => s.Requested_Date);
                    break;
                case "date_desc":
                    WorkOrder = WorkOrder.OrderByDescending(s => s.Requested_Date);
                    break;
                case "cont_assigned":
                    WorkOrder = WorkOrder.OrderBy(s => s.Contractor_Assigned);
                    break;
                case "cont_assigned_desc":
                    WorkOrder = WorkOrder.OrderByDescending(s => s.Contractor_Assigned);
                    break;

                default:
                    WorkOrder = WorkOrder.OrderByDescending(s => s.Requested_Date);
                    break;
            }

            int pageSize = 5;
            return View(await PaginatedList<WorkOrder>.CreateAsync(WorkOrder.AsNoTracking(), pageNumber ?? 1, pageSize));
        }


        [HttpGet]
        public ActionResult  GetConfigurationValue()
        {
            string[] parameterValue = { _storageAccountOptions.apiKey,
                                        _storageAccountOptions.authDomain,
                                        _storageAccountOptions.bucket
                                       };

            return Json(parameterValue.ToList());
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> AddUserRoles(string uRole,string uEmail)
        {
            string[] roleNames = { "Admin", "Manager", "Contractor" };
            IdentityResult roleResult;

            foreach (var roleName in roleNames)
            {
                var roleExist = await _roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    roleResult = await _roleManager.CreateAsync(new AppRole(roleName));
                }
            }
            string message;
            var pn = "";
            try
            {
                ApplicationUser user = await _userManager.FindByEmailAsync(uEmail);
                bool isAdmin = await _userManager.IsInRoleAsync(user, uRole);
                await _userManager.AddToRoleAsync(user, uRole);
                message = "Succcess";
                try
                {
                    var userPhoneNumber = _context.Users.Where(s => s.Email == uEmail)
                        .Select(s => new
                        {
                            s.PhoneNumber
                        })
                        .FirstOrDefault().PhoneNumber.ToString();
                    userPhoneNumber = userPhoneNumber.Replace(" ", "").Replace("(", "").Replace(")", "").Replace("-", "");
                    Console.WriteLine("User Number " + userPhoneNumber);
                    TwilioClient.Init(_twilioSMS.accountSid, _twilioSMS.authToken);
                    var to = new PhoneNumber("+387" + userPhoneNumber);
                    var SMSmessage = MessageResource.Create(
                        to,
                        from: new PhoneNumber("+12057517343"),   
                        body: $"Hello {user} !! you have been granted permission to Service Manager {uRole} role!!");
                        pn = to.ToString();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($" Registration Failure : {ex.Message} ");
                    message = ("Error occured, SMS Message has not been sent to user number" + pn);
                }
            }
            catch (Exception)
            {
                message = "Error Occured!";
                throw;
            }
            return Json(message);
            
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> RemoveUserRoles(string uRole, string uEmail)
        {
            string message;
            try
            {
                ApplicationUser user = await _userManager.FindByEmailAsync(uEmail);
                bool isAdmin = await _userManager.IsInRoleAsync(user, uRole);
                await _userManager.RemoveFromRoleAsync(user, uRole);
                message = "Succcess";
            }
            catch (Exception)
            {
                message = "Error Occured!";
                throw;
            }
            return Json(message);

        }
        [HttpGet]
        [Authorize(Roles = "Admin,Manager")]
        public JsonResult  ListUserRoles(int RoleIdFil)
        {
            var genreQuery2 = (from m in _context.UserRoles
                               join e in _context.Users
                               on m.UserId equals e.Id
                               where m.RoleId == RoleIdFil
                               select new ApplicationUser()
                               {
                                   Email = e.Email,
                                   FirstName = e.FirstName,
                                   LastName = e.LastName,
                                   Professional_Skill = e.Professional_Skill,
                               }).Distinct().ToList();
            return Json(genreQuery2);
        }

        [HttpPost]
        public ActionResult Progress()
        {
            return this.Content(Startup.Progress.ToString());
        }

        [HttpGet]
        public async Task<IActionResult> InspectionValidation(string formInitiator)
        {
            ApplicationUser appuser = await GetCurrentUser();
            string  userInit;
            var useremail = appuser.Email.ToString();
            if (formInitiator == useremail)
            {
                userInit = "initrue";
            }
            else
            {
                userInit = "inifalse";
            }
            return Json(userInit);
        }

        [HttpGet]
        public async Task<IActionResult> ContractorValidation(string formContractor)
        {
            ApplicationUser appuser = await GetCurrentUser();
            string userInit;
            var useremail = appuser.Email.ToString();
            if (formContractor == useremail)
            {
                userInit = "contrue";
            }
            else
            {
                userInit = "confalse";
            }
            return Json(userInit);
        }


        public IActionResult ContractorsAndStuff()
        {
            var user = GetCurrentUser();
            var userId = user?.Id;
            string mail = user.Result.Email;
            ViewBag.CurUser = mail;
            UserList = _context.Users.OrderBy(a => a.Email).Select(a =>
                                new SelectListItem
                                {
                                    Value = a.Email,
                                    Text = a.FullName
                                }).ToList();
            ViewBag.UserList = UserList;
            return View();
        }






        // GET: WorkOrders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var workOrder = await _context.WorkOrder
                .FirstOrDefaultAsync(m => m.WorkServiceID == id);
            if (workOrder == null)
            {
                return NotFound();
            }
            
              return View(workOrder);
            
        }



        [Authorize(Roles = "Admin,Manager")]
        // GET: WorkOrders/Create
        public IActionResult Create()
        {
            var user = GetCurrentUser();
            var userId = user?.Id;
            string mail = user.Result.Email;
            ViewBag.CurUser = mail;
            UserList = _context.Users.OrderBy(a => a.Email).Select(a =>
                                new SelectListItem
                                {
                                    Value = a.Email,
                                    Text = a.Email
                                }).ToList();
            ViewBag.UserList = UserList;
            return View();
        }


        // POST: WorkOrders/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("WorkServiceID,Property_Address,Floor,Unit,WorkServiceName,WorkService_Description,RequestedBy,Requested_Date,Contractor_Assigned,Contractor_Comments,Contractor_Start_Date,Contractor_Completion_Date,Service_Status,FolderUrl,Inspected_By,Date_Inspected,Inspection_Comments")] WorkOrder workOrder, IFormCollection form)
        {
            
          
            string eventType = Request.Form["FolderUrl"];
            string Radiobox1 = Request.Form["ImageChoice1"];
            string Radiobox2 = Request.Form["ImageChoice2"];
            

            var uploaderror = "";
           
                if (Request.Form["ImageChoice2"] == "2")
                {
                     var file = form.Files.FirstOrDefault();
                    

                int filenamestartlocation = file.FileName.LastIndexOf("\\") + 1;
                    string filename = file.FileName.Substring(filenamestartlocation);
                    var extension = filename.Substring(filename.LastIndexOf('.'));
                    filename = DateTime.Now.ToString("MM-dd-yyyy HH-mm-ss") + extension;
                    filename = filename.Replace(" ", "");
                    if (file != null && file.Length > 0)
                    {

                    var stream = file.OpenReadStream();
                        var auth = new FirebaseAuthProvider(new FirebaseConfig(_storageAccountOptions.apiKey));
                        var a = await auth.SignInWithEmailAndPasswordAsync(_storageAccountOptions.AuthEmail, _storageAccountOptions.AuthPassword);
                        var cancellation = new CancellationTokenSource();
                        var task = new FirebaseStorage(
                            _storageAccountOptions.bucket,
                            new FirebaseStorageOptions
                            {
                                AuthTokenAsyncFactory = () => Task.FromResult(a.FirebaseToken),
                                ThrowOnCancel = true // when you cancel the upload, exception is thrown. By default no exception is thrown
                        })
                            .Child(eventType)
                            .Child(filename)
                            .PutAsync(stream, cancellation.Token);

                        try
                        {
                            ViewBag.p = await task;
                           
                        }
                        catch (Exception ex)
                        {
                            uploaderror = ex.Message;
                            ViewBag.uppError = uploaderror;
                        }
                    }
                }
                else if (Request.Form["ImageChoice1"] == "1")
                {
                    ApplicationUser appuser = await GetCurrentUser();
                    var useremail = appuser.Email.ToString();
                    string fileLoc = @"c:\tmp\Test.txt";
                    FileStream file1 = new FileStream(fileLoc, FileMode.Open, FileAccess.ReadWrite);
                    var data = "Document Initiazlized by:" + useremail + ", For the folder: " + eventType;
                    byte[] bytes = Encoding.UTF8.GetBytes(data);
                    file1.Write(bytes, 0, bytes.Length);
                    file1.Dispose();
                    FileStream file2 = new FileStream(fileLoc, FileMode.Open, FileAccess.Read);

                    var auth = new FirebaseAuthProvider(new FirebaseConfig(_storageAccountOptions.apiKey));
                    var a = await auth.SignInWithEmailAndPasswordAsync(_storageAccountOptions.AuthEmail, _storageAccountOptions.AuthPassword);
                    var cancellation = new CancellationTokenSource();
                    var task = new FirebaseStorage(
                        _storageAccountOptions.bucket,
                        new FirebaseStorageOptions
                        {
                            AuthTokenAsyncFactory = () => Task.FromResult(a.FirebaseToken),
                            ThrowOnCancel = true
                        })
                        .Child(eventType)
                        .Child(useremail + ".txt")
                        .PutAsync(file2, cancellation.Token);
                    try
                    {
                        ViewBag.p = await task;
                    }
                    catch (Exception ex)
                    {
                        uploaderror = ex.Message;
                        ViewBag.uppError = uploaderror;
                    }
                }
            if (ModelState.IsValid)
            {
                _context.Add(workOrder);
                 await _context.SaveChangesAsync();
                 //return RedirectToAction(nameof(Index));
                 return RedirectToAction("Details", "WorkOrders", new { id = workOrder.WorkServiceID });
            }

            return View(workOrder);
        }
        [Authorize(Roles = "Admin,Manager,Contractor")]
        // GET: WorkOrders/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {


            if (id == null)
            {
                return NotFound();
            }

            var workOrder = await _context.WorkOrder.FindAsync(id);
            if (workOrder == null)
            {
                return NotFound();
            }
            ApplicationUser appuser = await GetCurrentUser();
            var useremail = appuser.Email.ToString();

            bool isAdmin = await _userManager.IsInRoleAsync(appuser, "Admin");

            if ((workOrder.Contractor_Assigned != useremail) && (isAdmin == false))
            {
                return new ForbidResult();
            }
            else
            {
                return View(workOrder);
            }
        }

        // POST: WorkOrders/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize(Roles = "Admin,Manager,Contractor")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("WorkServiceID,Property_Address,Floor,Unit,WorkServiceName,WorkService_Description,RequestedBy,Requested_Date,Contractor_Assigned,Contractor_Comments,Contractor_Start_Date,Contractor_Completion_Date,Service_Status,FolderUrl,Inspected_By,Date_Inspected,Inspection_Comments")] WorkOrder workOrder, IFormCollection form)
        {
            string eventType = Request.Form["FolderUrl"];
            int idNo = Convert.ToInt32(Request.Form["WorkServiceID"]);
            var file = form.Files.FirstOrDefault();
            if (id != workOrder.WorkServiceID)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
               

                if (file != null && file.Length > 0)
                {
                    int filenamestartlocation = file.FileName.LastIndexOf("\\") + 1;
                    string filename = file.FileName.Substring(filenamestartlocation);
                    var extension = filename.Substring(filename.LastIndexOf('.'));
                    filename = DateTime.Now.ToString("MM-dd-yyyy HH-mm-ss") + extension;
                    filename = filename.Replace(" ", "");
                    var stream = file.OpenReadStream();
                    var auth = new FirebaseAuthProvider(new FirebaseConfig(_storageAccountOptions.apiKey));
                    var a = await auth.SignInWithEmailAndPasswordAsync(_storageAccountOptions.AuthEmail, _storageAccountOptions.AuthPassword);
                    var cancellation = new CancellationTokenSource();
                    var task = new FirebaseStorage(
                        _storageAccountOptions.bucket,
                        new FirebaseStorageOptions
                        {
                            AuthTokenAsyncFactory = () => Task.FromResult(a.FirebaseToken),
                            ThrowOnCancel = true // when you cancel the upload, exception is thrown. By default no exception is thrown
                        })
                        .Child(eventType)
                        .Child(filename)
                        .PutAsync(stream, cancellation.Token);
                    try
                    {
                        ViewBag.p = await task;
                        
                    }
                    catch (Exception ex)
                    {
                        ViewBag.error = "Error During Image Upload!" + ex;
                        throw;
                    }
                       
                }

                try
                {
                   
                    _context.Update(workOrder);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!WorkOrderExists(workOrder.WorkServiceID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                if (file != null && file.Length > 0)
                {
                        return RedirectToAction("Details", "WorkOrders", new { id = idNo });
                }
                else
                {
                    return RedirectToAction(nameof(Index));
                }
            }
            return View(workOrder);
        }
        [Authorize(Roles = "Admin,Manager")]
        // GET: WorkOrders/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var workOrder = await _context.WorkOrder
                .FirstOrDefaultAsync(m => m.WorkServiceID == id);
            if (workOrder == null)
            {
                return NotFound();
            }

            ApplicationUser appuser = await GetCurrentUser();
            var useremail = appuser.Email.ToString();
            
            bool isAdmin = await _userManager.IsInRoleAsync(appuser, "Admin");

            if ((workOrder.Contractor_Assigned != useremail) && (isAdmin == false))
            {
                return new ForbidResult();
            }
            else
            {
                return View(workOrder);
            }
        }

        // POST: WorkOrders/Delete/5
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin,Manager")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            ApplicationUser appuser = await GetCurrentUser();
            bool isAdmin = await _userManager.IsInRoleAsync(appuser, "Admin");
            var workOrder = await _context.WorkOrder.FindAsync(id);
            if (isAdmin == false)
            {
                return new ForbidResult();
            }
            _context.WorkOrder.Remove(workOrder);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool WorkOrderExists(int id)
        {
            return _context.WorkOrder.Any(e => e.WorkServiceID == id);
        }
    }
}
