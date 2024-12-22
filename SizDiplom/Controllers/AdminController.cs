using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SizDiplom.Models;
using SizDiplom.ViewModels;

namespace SizDiplom.Controllers
{
    public class AdminController : Controller
    {
        private ProgDBaseContext db;
       
        public AdminController(ProgDBaseContext context) 
        {
            db = context;
            
        }

        [HttpGet]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> IndexAsync()  // стартовая страница
        {
            User? user = await db.Users.FirstOrDefaultAsync(u => u.Login == User.Identity.Name);
            if (user == null)
            {
                ModelState.AddModelError("", "Обрыв связи с базой данных (не удалось получить данные)");
                return RedirectToAction("Login", "Account");
            }
            
            ViewData["Title"] = "ПАНЕЛЬ АДМИНИСТРАТОРА";

                return View();
        }
    }
}
