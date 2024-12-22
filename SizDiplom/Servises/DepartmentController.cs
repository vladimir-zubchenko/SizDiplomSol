using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SizDiplom.Models;
using SizDiplom.ViewModels;

namespace SizDiplom.Servises
{
    public class DepartmentController : Controller
    {
        private ProgDBaseContext db;
        private DepartmentViewModel depViewModel;
        

        public DepartmentController(ProgDBaseContext dbContext)
        {
            db = dbContext;
            depViewModel = new DepartmentViewModel();
        }


        // GET: DepartmentController
        [HttpGet]
        [Authorize(Roles = "admin")]
       
        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Выберите департамент";
            depViewModel.Departments = await db.Departments.ToListAsync();  
            return View(depViewModel);
        }

        // POST: DepartmentController/DeptDetails(.....)
        [HttpPost]
        [Authorize (Roles = "admin")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeptDetails(int DepartmentId)
        {
            User? user = await db.Users.FirstOrDefaultAsync(u => u.Login == User.Identity.Name);
            if (user == null)
            {
                ModelState.AddModelError("", "Обрыв связи с базой данных (не удалось получить данные)");
                return RedirectToAction("Login", "Account");
            }
            Department? dep = await db.Departments.FirstOrDefaultAsync(d => d.Id == DepartmentId);
            if (dep == null)
            {
                ModelState.AddModelError("", "Обрыв связи с базой данных (не удалось получить данные)");
                return RedirectToAction("Login", "Account");
            }

            depViewModel.Id = dep.Id;
            depViewModel.Name = dep.Name;
            ViewData["Title"] = "Работать с департаментом";

                return View(depViewModel);
        }

        // GET: DepartmentController/Create
        [HttpGet]
        [Authorize(Roles = "admin")]
        
        public ActionResult Create()
        {
            depViewModel = new DepartmentViewModel();
            ViewData["Title"] = "Работать с департаментом";
            ViewData["Warning"] = "ДОБАВЛЕНИЕ НОВОГО ДЕПАРТАМЕНТА";
            
            return View("Create", depViewModel);
        }

        // POST: DepartmentController/Create
        [HttpPost]
        [Authorize(Roles = "admin")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(DepartmentViewModel depViewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(depViewModel);
            }
                User? user = await db.Users.FirstOrDefaultAsync(u => u.Login == User.Identity.Name);
            if (user == null)
            {
                ModelState.AddModelError("", "Обрыв связи с базой данных (не удалось получить данные)");
                return RedirectToAction("Login", "Account");
            }
            Department? dep = await db.Departments.FirstOrDefaultAsync(d => d.Name == depViewModel.Name);
            if (dep != null)
            {
                ViewData["Title"] = "РАБОТА С ДЕПАРТАМЕНТАМИ";                              
                ViewData["Warning"] = "ДЕПАРТАМЕНТ С ТАКИМ НАЗВАНИЕМ СУЩЕСТВУЕТ";
                return View("Create", depViewModel);
            }
            dep = new Department
            {
                Name = depViewModel.Name
            };
            db.Departments.Add(dep);
            await db.SaveChangesAsync();
            ViewData["Title"] = "ДОБАВЛЕНИЕ НОВОГО ДЕПАРТАМЕНТА";
            ViewData["Warning"] = "НОВЫЙ ДЕПАРТАМЕНТ ДОБАВЛЕН В БД";
            return View("Create", depViewModel);

        }

        

        // POST: DepartmentController/Edit/5
        [HttpPost]
        [Authorize(Roles = "admin")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditAsync(DepartmentViewModel depVM)
        {
            if (!ModelState.IsValid)
            {
                return View("DeptDetails",depVM);
            }
            User? user = await db.Users.FirstOrDefaultAsync(u => u.Login == User.Identity.Name);
            if (user == null)
            {
                ModelState.AddModelError("", "Обрыв связи с базой данных (не удалось получить данные)");
                return RedirectToAction("Login", "Account");
            }
            Department? dep = await db.Departments.FirstOrDefaultAsync(d => d.Id == depVM.Id);
            if (dep == null)
            {
                ModelState.AddModelError("", "Обрыв связи с базой данных (не удалось получить данные)");
                return RedirectToAction("Login", "Account");
            }            
            dep.Name = depVM.Name;
            db.Departments.Update(dep);
            await db.SaveChangesAsync();
            ViewData["Title"] = "РАБОТА С ДЕПАРТАМЕНТАМИ";
            ViewData["Warning"] = "НАЗВАНИЕ ДЕПАРТАМЕНТА ИЗМЕНЕНО";           

            return View("DeptDetails", depVM);
        }

        // POST: DepartmentController/Delete/5
        [HttpPost]
        [Authorize(Roles = "admin")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteAsync(DepartmentViewModel depVM)
        {
            User? user = await db.Users.FirstOrDefaultAsync(u => u.Login == User.Identity.Name);
            if (user == null)
            {
                ModelState.AddModelError("", "Обрыв связи с базой данных (не удалось получить данные)");
                return RedirectToAction("Login", "Account");
            }
            Department? dep = await db.Departments.FirstOrDefaultAsync(d => d.Id == depVM.Id);
            if (dep == null)
            {
                ModelState.AddModelError("", "Обрыв связи с базой данных (не удалось получить данные)");
                return RedirectToAction("Login", "Account");
            }
            Siz? siz = await db.Sizs.FirstOrDefaultAsync(s => s.DepartmentId == depVM.Id);
            if (siz != null) 
            {
                ViewData["Title"] = "РАБОТА С ДЕПАРТАМЕНТАМИ";
                ViewData["Warning"] = "ЗА ДЕПАРТАМЕНТОМ ЗАКРЕПЛЕНЫ СИЗ ";
                return View("DeptDetails", depVM);
            }
            Car? car = await db.Cars.FirstOrDefaultAsync(c => c.DepartmentId == depVM.Id);
            if (car != null) 
            {
                ViewData["Title"] = "РАБОТА С ДЕПАРТАМЕНТАМИ";
                ViewData["Warning"] = "ЗА ДЕПАРТАМЕНТОМ ЗАКРЕПЛЕНЫ АВТОМОБИЛИ ";
                return View("DeptDetails", depVM);
            }
            Place? place = await db.Places.FirstOrDefaultAsync(p => p.DepartmentId == depVM.Id);
            if (place != null)
            {
                ViewData["Title"] = "РАБОТА С ДЕПАРТАМЕНТАМИ";
                ViewData["Warning"] = "ЗА ДЕПАРТАМЕНТОМ ЗАКРЕПЛЕНЫ СКЛАДЫ ";
                return View("DeptDetails", depVM);                
            }
            User? us = await db.Users.FirstOrDefaultAsync(u => u.DepartmentId == depVM.Id);
            if (us != null)
            {
                ViewData["Title"] = "РАБОТА С ДЕПАРТАМЕНТАМИ";
                ViewData["Warning"] = "В ДЕПАРТАМЕНТЕ ЕСТЬ РАБОТНИКИ ";
                return View("DeptDetails", depVM);                
            }
            
            db.Departments.Remove(dep);
            await db.SaveChangesAsync();
            

            return RedirectToAction("Index","Admin");
            
        }

       
    }
}
