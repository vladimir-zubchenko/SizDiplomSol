using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using SizDiplom.Models;
using SizDiplom.ViewModels;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace SizDiplom.Controllers
{
    public class AccountController : Controller
    {
        private ProgDBaseContext db;
        private RegisterModel regModel;
        public AccountController(ProgDBaseContext context)
        {
            db = context;
            regModel = new RegisterModel();
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

                

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginModel model)
        {
            if (ModelState.IsValid)
            {
                User? user = await db.Users.FirstOrDefaultAsync(u => u.Login == model.Login && u.Password == model.Password);
                if (user != null)
                {
                    await Authenticate(user); // аутентификация
                    if (user.Role == "user")  return RedirectToAction("SimpleUser", "Home");
                    if (user.Role == "deptadmin") return RedirectToAction("Index", "DeptAdmin");
                    if (user.Role == "admin") return RedirectToAction("Index", "Admin");
                    if (user.Role == "sizadmin") return RedirectToAction("Index", "SizAdmin");
                }
                ModelState.AddModelError("", "Некорректные логин и(или) пароль.");
            }
            return View(model);
        }

        [HttpGet]
        [Authorize(Roles = "admin, deptadmin")]
        public async Task<IActionResult> RegisterAsync()
        {
            User? user = await db.Users.FirstOrDefaultAsync(u => u.Login == User.Identity.Name);
            if (user == null)
            {
                ModelState.AddModelError("", "Обрыв связи с базой данных (не удалось получить данные)");
                return RedirectToAction("Login", "Account");
            }
            regModel.Roles.Clear();
            regModel.Departments.Clear();
            if (user.Role == "deptadmin")
            {
                Department? dep = await db.Departments.FirstOrDefaultAsync(d => d.Id == user.DepartmentId);
                if (dep == null)
                {
                    ModelState.AddModelError("", "Обрыв связи с базой данных (не удалось получить данные)");
                    return RedirectToAction("Login", "Account");
                }
                regModel.Departments.Add(dep);
                regModel.Roles.Add("user");
                regModel.ControllerName = "DeptAdmin";               
            }
            else if(user.Role == "admin")
            {
                regModel.Departments = await db.Departments.ToListAsync();
                regModel.Roles.Add("deptadmin");
                regModel.Roles.Add("sizadmin");
                regModel.Roles.Add("admin");
                regModel.ControllerName = "Admin";
            }
            
            return View(regModel);
        }

        [HttpPost]
        [Authorize(Roles = "admin, deptadmin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                User? user = await db.Users.FirstOrDefaultAsync(u => u.Login == model.Login && u.Password == model.Password);
                if (user == null)
                {
                    // добавляем пользователя в бд
                    User newReg = new User { Login = model.Login, Password = model.Password, TabNom = model.TabNom,
                                                DepartmentId = model.DepartmentId, Role = model.RoleName };
                    db.Users.Add(newReg);
                    await db.SaveChangesAsync();                   
                    return RedirectToAction("Login", "Account");
                }
                else
                    ModelState.AddModelError("", "Такой пользователь уже существует");
                return View("Register", model);
            }
            regModel.Roles.Clear();
            regModel.Roles.Add(model.RoleName);
            regModel.Departments.Clear();
            Department? dep = await db.Departments.FirstOrDefaultAsync(d => d.Id == model.DepartmentId);
            if (dep == null)
            {
                ModelState.AddModelError("", "Обрыв связи с базой данных (не удалось получить данные)");
                return RedirectToAction("Login", "Account");
            }
            regModel.Departments.Add(dep);
            regModel.ControllerName = model.ControllerName;
            return View(regModel);
        }

        private async Task Authenticate(User user)
        {            
            var claims = new List<Claim>
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, user.Login),
                new Claim(ClaimsIdentity.DefaultRoleClaimType, user.Role),
                //new Claim("Password", user.Password),
                //new Claim("DepartmentId", user.DepartmentId.ToString())
            };
            // создаем объект ClaimsIdentity
            ClaimsIdentity ident = new ClaimsIdentity(claims, "ApplicationCookie", ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);
            // установка аутентификационных куки
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(ident));
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }
    }
}
