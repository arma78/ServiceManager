using AutoMapper;
using ServiceManager.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using System.Text.Encodings.Web;
using System.Linq;
using ServiceManager.Data;


namespace ServiceManager.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailSender _emailSender;

        public AccountController(ApplicationDbContext context, IMapper mapper, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IEmailSender emailSender)
        {
            _context = context;
            _mapper = mapper;
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
        }


        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordModel forgotPasswordModel)
        {
            if (!ModelState.IsValid)
                return View(forgotPasswordModel);

            var user = await _userManager.FindByEmailAsync(forgotPasswordModel.Email);
            if (user == null)
                return RedirectToAction(nameof(ForgotPasswordConfirmation));
            string usertomail = user.Email.ToString();
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var callback = Url.Action(nameof(ResetPassword), nameof(AccountController), new { token, email = user.Email }, Request.Scheme);

            if (callback.Contains("AccountController"))
            {
                callback = callback.Replace("AccountController", "Account");
            }


                var message = new Message(new string[] { usertomail }, "Reset password token",
                $"Please reset your password by <a href='{HtmlEncoder.Default.Encode(callback)}'>clicking here</a>.",null);
           
           await _emailSender.SendEmailAsync(message);

            return RedirectToAction(nameof(ForgotPasswordConfirmation));
        }
       
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }
        [HttpGet]
        public ActionResult OnGetCode(string Emailv, string pcv)
        {
      
            string UserMessage = "";
   
           
            string Error1 = "";
            string Error2 = "";
            string Error3 = "";
            try
            {
             
                var emailconfirmation = _context.Users.Where(s => s.Email == Emailv)
                           .Select(s => new
                           {
                               EmailConfirmed = s.EmailConfirmed.ToString()
                           })
                           .ToList().FirstOrDefault().EmailConfirmed;
                if (emailconfirmation == "False") { UserMessage = "0"; }

                    if (emailconfirmation == "False")
                {
                    UserMessage = "2";
                    if (Emailv != null && pcv != null)
                    {
                        try
                        {
                            var userPhoneNumber = _context.Users.Where(s => s.PhoneCodeValidator == pcv && s.Email == Emailv)
                               .Select(s => new
                               {
                                   PhoneCodeValidator = s.PhoneCodeValidator.ToString()
                               })
                                .ToList().FirstOrDefault().PhoneCodeValidator;

                            if (userPhoneNumber == pcv && Emailv != null)
                            {
                                UserMessage = "3";
                                try
                                {
                                    var PhoneCode = _context.Users.FirstOrDefault(p => p.PhoneCodeValidator == pcv && p.Email == Emailv);
                                    PhoneCode.EmailConfirmed = true;
                                    PhoneCode.PhoneNumberConfirmed = true;
                                    _context.SaveChanges();
                                    UserMessage = "5";
                                }
                                catch (Exception ex)
                                {
                                    Error3 = "Confirmation code error occured: " + ex.Message;
                                }
                                
                            }
                            else
                            {
                                UserMessage = "4";
                            }
                           
                        }
                        catch (Exception ex)
                        {
                            Error2 = "Did you enter the correct code and email?: " + ex.Message;
                            UserMessage = "6";
                        }
                    }

                }
                else if (emailconfirmation == "True")
                {
                    UserMessage = "1";
                }


            }
            catch (Exception ex)
            {
                Error1 = "Error Retrieving : " + ex.Message;
            }
            string[] message = {Error1, Error2, Error3, UserMessage};
            
         
            return new JsonResult(message.ToList());
        }

        [HttpGet]
        public IActionResult ResetPassword(string token, string email)
        {
            var model = new ResetPasswordModel { Token = token, Email = email };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordModel resetPasswordModel)
        {
            if (!ModelState.IsValid)
                return View(resetPasswordModel);

            var user = await _userManager.FindByEmailAsync(resetPasswordModel.Email);
            if (user == null)
                RedirectToAction(nameof(ResetPasswordConfirmation));

            var resetPassResult = await _userManager.ResetPasswordAsync(user, resetPasswordModel.Token, resetPasswordModel.Password);
            if (!resetPassResult.Succeeded)
            {
                foreach (var error in resetPassResult.Errors)
                {
                    ModelState.TryAddModelError(error.Code, error.Description);
                }

                return View();
            }

            return RedirectToAction(nameof(ResetPasswordConfirmation));
        }

        [HttpGet]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            else
                return RedirectToAction(nameof(HomeController.Index), "Home");
        }
    }
}