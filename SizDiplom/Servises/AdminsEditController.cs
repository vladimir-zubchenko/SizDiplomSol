using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SizDiplom.Models;
using SizDiplom.ViewModels;

namespace SizDiplom.Servises
{
    public class AdminsEditController : Controller
    {
        private ProgDBaseContext db;
        private AdmEditViewModel admEdvm;
        public AdminsEditController(ProgDBaseContext context)
        {
            db = context;
            admEdvm = new AdmEditViewModel();
        }


        // GET: AdminsEditController/Index
        [HttpGet]
        [Authorize(Roles = "admin")]
       
        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Выберите администратора";
            admEdvm.Admins = await db.Users.Where(u => u.Role != "user").ToListAsync();
            return View(admEdvm);
        }

       

        // POST: AdminsEditController/AdminDetails(.....)
        [HttpPost]
        [Authorize(Roles = "admin")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AdminDetails(int Id)
        {
            User? user = await db.Users.FirstOrDefaultAsync(u => u.Login == User.Identity.Name);
            if (user == null)
            {
                ModelState.AddModelError("", "Обрыв связи с базой данных (не удалось получить данные)");
                return RedirectToAction("Login", "Account");
            }
            user = await db.Users.FirstOrDefaultAsync(u => u.Id == Id);
            if (user == null)
            {
                ModelState.AddModelError("", "Обрыв связи с базой данных (не удалось получить данные)");
                return RedirectToAction("Login", "Account");
            }

            admEdvm.Id = user.Id;
            admEdvm.Login = user.Login;
            admEdvm.TabNom = user.TabNom;
            admEdvm.DepartmentId = user.DepartmentId;
            admEdvm.Password = user.Password;
            admEdvm.Role = user.Role;

            admEdvm.Departments.Clear();
            admEdvm.Departments = await db.Departments.ToListAsync();
                      
            admEdvm.Roles.Clear();
            admEdvm.Roles.Add("deptadmin");
            admEdvm.Roles.Add("sizadmin");
            admEdvm.Roles.Add("admin");
            ViewData["Title"] = "Редактируйте данные администратора";

            return View(admEdvm);
        }

        // POST: AdminsEditController/AdminEdit(.....)
        [HttpPost]
        [Authorize(Roles = "admin")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AdminEdit(AdmEditViewModel admEdvm)
        {
            User? user = await db.Users.FirstOrDefaultAsync(u => u.Login == User.Identity.Name);
            if (user == null)
            {
                ModelState.AddModelError("", "Обрыв связи с базой данных (не удалось получить данные)");
                return RedirectToAction("Login", "Account");
            }
            if (!ModelState.IsValid)
            {
                ViewData["Title"] = "Редактируйте данные администратора (ошибки в данных)";
                return View("AdminDetails", admEdvm);
            }
            user = await db.Users.FirstOrDefaultAsync(u => u.Id == admEdvm.Id);
            if (user == null)
            {
                ModelState.AddModelError("", "Обрыв связи с базой данных (не удалось получить данные)");
                return RedirectToAction("Login", "Account");
            }
            user.Login = admEdvm.Login;
            user.Password = admEdvm.Password;
            user.TabNom = admEdvm.TabNom;
            user.Role = admEdvm.Role;
            user.DepartmentId = admEdvm.DepartmentId;
            db.Users.Update(user);
            await db.SaveChangesAsync();
            ViewData["Title"] = "Новые данные администратора сохранены";
            return View("AdminDetails", admEdvm);
        }

        // GET: AdminsEditController/Create
        [HttpGet]
        [Authorize(Roles = "admin")]

        public async Task<IActionResult> Create()
        {
            User? user = await db.Users.FirstOrDefaultAsync(u => u.Login == User.Identity.Name);
            if (user == null)
            {
                ModelState.AddModelError("", "Обрыв связи с базой данных (не удалось получить данные)");
                return RedirectToAction("Login", "Account");
            }
            admEdvm.Departments = await db.Departments.ToListAsync();
            admEdvm.Roles.Clear();            
            admEdvm.Roles.Add("deptadmin");
            admEdvm.Roles.Add("sizadmin");
            admEdvm.Roles.Add("admin");



            ViewData["Title"] = "Создание администратора (введите данные)";           
            return View("AdminDetails", admEdvm);
        }


    }
}
