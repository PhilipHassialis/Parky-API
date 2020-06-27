using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ParkyWeb.Models;
using ParkyWeb.Models.ViewModels;
using ParkyWeb.Repository.IRepository;

namespace ParkyWeb.Controllers
{
    [Authorize]
    public class TrailsController : Controller
    {
        private readonly INationalParkRepository _npRepo;
        private readonly ITrailRepository _trailRepo;

        public TrailsController(INationalParkRepository npRepo, ITrailRepository trailRepo)
        {
            _npRepo = npRepo;
            _trailRepo = trailRepo;
        }

        public IActionResult Index()
        {
            return View(new Trail() { });
        }
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Upsert(int? id)
        {
            IEnumerable<NationalPark> npList = await _npRepo.GetAllAsync(SD.NationalParkAPIPath, HttpContext.Session.GetString("JWTToken"));

            TrailVM objVM = new TrailVM()
            {
                NationalParkList = npList.Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString()
                })
            };

            if (id == null)
            {
                objVM.Trail = new Trail();
                return View(objVM); // insert or create
            }
            // flow come here for update
            objVM.Trail = await _trailRepo.GetAsync(SD.TrailAPIPath, id.GetValueOrDefault(), HttpContext.Session.GetString("JWTToken"));
            if (objVM == null)
            {
                return NotFound();
            }
            return View(objVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]

        public async Task<IActionResult> Upsert(TrailVM obj)
        {
            if (ModelState.IsValid)
            {

                if (obj.Trail.Id == 0)
                {
                    await _trailRepo.CreateAsync(SD.TrailAPIPath, obj.Trail, HttpContext.Session.GetString("JWTToken"));
                }
                else
                {
                    await _trailRepo.UpdateAsync(SD.TrailAPIPath + obj.Trail.Id, obj.Trail, HttpContext.Session.GetString("JWTToken"));
                }
                return RedirectToAction(nameof(Index));
            }
            else
            {

                IEnumerable<NationalPark> npList = await _npRepo.GetAllAsync(SD.NationalParkAPIPath, HttpContext.Session.GetString("JWTToken"));

                TrailVM objVM = new TrailVM()
                {
                    NationalParkList = npList.Select(i => new SelectListItem
                    {
                        Text = i.Name,
                        Value = i.Id.ToString()
                    }),
                    Trail = obj.Trail
                };
                return View(objVM);
            }
        }

        public async Task<IActionResult> GetAllTrails()
        {
            return Json(new { data = await _trailRepo.GetAllAsync(SD.TrailAPIPath, HttpContext.Session.GetString("JWTToken")) });
        }

        [HttpDelete]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var status = await _npRepo.DeleteAsync(SD.TrailAPIPath, id, HttpContext.Session.GetString("JWTToken"));
            if (status)
            {
                return Json(new { success = true, message = "Delete Successful" });
            }
            else
            {
                return Json(new { success = false, message = "Delete Failed" });
            }
        }
    }
}