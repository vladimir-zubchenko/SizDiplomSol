using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SizDiplom.Models;
using SizDiplom.ViewModels;
using System.Runtime.Intrinsics.Arm;

namespace SizDiplom.Controllers
{
    public class SizAdminController : Controller
    {
        private ProgDBaseContext db;
        private SizAdminViewModel sizVM;

        public SizAdminController(ProgDBaseContext dbcontext )
        {
            db = dbcontext;
            sizVM = new SizAdminViewModel();
        }
        [HttpGet]
        [Authorize(Roles = "sizadmin")]
        public async Task<IActionResult> Index()
        {
            //sizVM.AlarmSizsList = await db.Sizs.Where(s => s.NextCheckDate <= DateTime.Today.AddDays(7)).ToListAsync();
            sizVM.AlarmSizsList = await db.Sizs.Where(s => s.NextCheckDate <= DateTime.Today).ToListAsync();
            return View(sizVM);
        }
        // POST: SizAdminController/SizDetale(.....)
        [HttpPost]
        [Authorize(Roles = "sizadmin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SizDetale(string SizNomber)
        {
            User? user = await db.Users.FirstOrDefaultAsync(u => u.Login == User.Identity.Name);
            if (user == null)
            {
                ModelState.AddModelError("", "Обрыв связи с базой данных (не удалось получить данные)");
                return RedirectToAction("Login", "Account");
            }
            Siz? siz = await db.Sizs.FirstOrDefaultAsync(s => s.TabNom == SizNomber);
            if (siz == null)
            {
                
                //sizVM.AlarmSizsList = await db.Sizs.Where(s => s.NextCheckDate <= DateTime.Today.AddDays(7)).ToListAsync();
                sizVM.AlarmSizsList = await db.Sizs.Where(s => s.NextCheckDate <= DateTime.Today).ToListAsync();
                sizVM.AlarmMessage = "СИЗ не найден проверьте инвентарный номер";
                sizVM.CheckDate = DateTime.Today;
                return View("Index", sizVM);
                
            }
            
            sizVM.Siz = siz;
            return View(sizVM);
        }

        


        // POST: SizAdminController/SizEdit(.....)
        [HttpPost]
        [Authorize(Roles = "sizadmin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SizEdit(SizAdminViewModel sizVM)
        {
            User? user = await db.Users.FirstOrDefaultAsync(u => u.Login == User.Identity.Name);
            if (user == null)
            {
                ModelState.AddModelError("", "Обрыв связи с базой данных (не удалось получить данные)");
                return RedirectToAction("Login", "Account");
            }
            Siz? siz = await db.Sizs.FirstOrDefaultAsync(s => s.Id == sizVM.Siz.Id);
            if (siz == null)
            {
                ViewData["Warning"] = "СИЗ не найден. Обрыв связи с базой данных (не удалось получить данные)";
                sizVM.AlarmSizsList = await db.Sizs.Where(s => s.NextCheckDate <= DateTime.Today).ToListAsync();
                return View("Index");
            }
            siz.NextCheckDate = sizVM.CheckDate;
            db.Sizs.Update(siz);
            await db.SaveChangesAsync();    

            ViewData["Title"] = "Работа с СИЗ";
            sizVM.Siz = siz;
            sizVM.SizNomber = siz.TabNom;


            return View("SizDetale", sizVM);

        }

        // POST: SizAdminController/CheckNewDate(.....)
        [HttpPost]
        [Authorize(Roles = "sizadmin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckNewDate(DateTime CheckDate)
        {
            User? user = await db.Users.FirstOrDefaultAsync(u => u.Login == User.Identity.Name);
            if (user == null)
            {
                ModelState.AddModelError("", "Обрыв связи с базой данных (не удалось получить данные)");
                return RedirectToAction("Login", "Account");
            }
            sizVM.AlarmSizsList = await db.Sizs.Where(s => s.NextCheckDate <= CheckDate).ToListAsync();
            sizVM.CheckDate = CheckDate;
            return View("Index", sizVM);

        }
    }
}
