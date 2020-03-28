using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ServiceManager.Models;
using Microsoft.Extensions.Configuration;
using ServiceManager.Data;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ServiceManager.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, IConfiguration configuration)
        {
            _logger = logger;
            _context = context;
            _configuration = configuration;
        }

        public async Task<IActionResult> Index()
        {
            IQueryable<string> genreQuery = from m in _context.WorkOrder
                                            orderby m.FolderUrl
                                            select m.FolderUrl;


            var FirebaseStorageFolders = new WOStorageFolder
            {
                StrageFolder = new SelectList(await genreQuery.Distinct().ToListAsync()),
            };
            return View(FirebaseStorageFolders);
        }

        [HttpGet]
        public JsonResult ContractorInfo(string ContrFolder)
        {
            var genreQuery2 =  (from m in _context.WorkOrder
                               join e in _context.Users
                               on m.Contractor_Assigned equals e.Email
                               where m.FolderUrl == ContrFolder
                               select new ApplicationUser()
                               {
                                  Email = e.Email,
                                  FirstName = e.FirstName,
                                  LastName = e.LastName,
                                  PhoneNumber = e.PhoneNumber,
                                  Professional_Skill = e.Professional_Skill,
                               }).Distinct().ToList();
       
            return Json(genreQuery2);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
