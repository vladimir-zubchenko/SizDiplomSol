using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SizDiplom.Models;
using SizDiplom.ViewModels;
using System.Diagnostics;
using System.Security.Claims;

namespace SizDiplom.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private ProgDBaseContext db;
        
        public HomeController(ILogger<HomeController> logger, ProgDBaseContext dBase )
        {
            _logger = logger;
            db = dBase;
        }

       
        [Authorize(Roles ="user")]
        public async Task<IActionResult> SimpleUser()
        {
            UsersSizViewModel usSizModel;               

                int usId = 0;
                List<Siz> SizList = new List<Siz>();
                User? user = await db.Users.FirstOrDefaultAsync(u => u.Login == User.Identity.Name);
            if (user != null)
            {
                usId = user.Id;
                SizList = await db.Sizs.Where(s => s.UserId == usId).ToListAsync();

                usSizModel = new UsersSizViewModel(user, SizList);

            }
            else
            {
                ModelState.AddModelError("", "Обрыв связи с базой данных (не удалось получить данные)");
                return RedirectToAction("Login", "Account");
            }
            
            return View(usSizModel);//вывод списка Сиз работника на экран.
        }

        

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}