using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ParkyWeb.Models;
using ParkyWeb.Models.ViewModels;
using ParkyWeb.Repository;
using ParkyWeb.Repository.IRepository;

namespace ParkyWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly INationalParkRepository _npRepo;
        private readonly ITrailRepository _trailRepo;
        private readonly IAccountRepository _accRepo;

        public HomeController(ILogger<HomeController> logger, INationalParkRepository npRepo, ITrailRepository trailRepo, IAccountRepository accRepo)
        {
            _logger = logger;
            _npRepo = npRepo;
            _trailRepo = trailRepo;
            _accRepo = accRepo;
        }

        public async Task<IActionResult> Index()
        {
            IndexVM listOfParksAndTrails = new IndexVM()
            {
                NationalParkList = await _npRepo.GetAllAsync(SD.NationalParkAPIPath, HttpContext.Session.GetString("JWTToken")),
                TrailList = await _trailRepo.GetAllAsync(SD.TrailAPIPath, HttpContext.Session.GetString("JWTToken"))
            };

            return View(listOfParksAndTrails);
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

        [HttpGet]
        public IActionResult Login()
        {
            User obj = new User();
            return View(obj);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(User obj)
        {
            User objUser = await _accRepo.LoginAsync(SD.AccountAPIPath + "authenticate", obj);
            if (objUser.Token==null)
            {
                return View();
            }

            var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
            identity.AddClaim(new Claim(ClaimTypes.Name, objUser.Username));
            identity.AddClaim(new Claim(ClaimTypes.Role, objUser.Role));
            var principal = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,principal);

            HttpContext.Session.SetString("JWTToken", objUser.Token);
            TempData["alert"] = "Welcome " + objUser.Username;
            return RedirectToAction("Index");
        }


        [HttpGet]
        public IActionResult Register()
        {
            User obj = new User();
            return View(obj);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(User obj)
        {
            bool result = await _accRepo.RegisterAsync(SD.AccountAPIPath + "register", obj);
            if (!result)
            {
                return View();
            }
            TempData["alert"] = "Registered " + obj.Username;

            return RedirectToAction("Login");
        }


        public async Task<IActionResult> Logout(User obj)
        {
            await HttpContext.SignOutAsync();
            HttpContext.Session.SetString("JWTToken", "");
            return RedirectToAction("Index");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
