using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SizDiplom.Models;
using SizDiplom.ViewModels;

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
        public IActionResult Index()
        {
            return View();
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
                ViewData["Warning"] = "СИЗ не найден проверьте инвентарный номер";
                return View("Index");
            }
            sizVM.Siz = siz;
            return View(sizVM);



        }
    }
}
