using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol;
using SizDiplom.Models;
using SizDiplom.ViewModels;

namespace SizDiplom.Controllers
{
    public class DeptAdminController : Controller
    {
        private ProgDBaseContext db;
        private DepartAdminViewModel viewModel;
        private UserChangeViewModel userChViewModel;
        private PlaceChangeViewModel placeChViewModel;
        private CarChangeViewModel carChViewModel;
        private NewSizCreateViewModel newSizCreateViewModel;


        public DeptAdminController(ProgDBaseContext context)
        {
            db = context;
            viewModel = new DepartAdminViewModel(); 
            userChViewModel = new UserChangeViewModel();
            placeChViewModel = new PlaceChangeViewModel();
            carChViewModel = new CarChangeViewModel();
            newSizCreateViewModel = new NewSizCreateViewModel();
        }


        [HttpGet]  
        [Authorize(Roles = "deptadmin")]
        public async Task<IActionResult> Index()// GET: DeptAdminController  вход для deptptadmin вывод просроченных СИЗ выбор дальнейшего пути
        {
            User? user = await db.Users.FirstOrDefaultAsync(u => u.Login == User.Identity.Name);
            if (user != null)
            {
                // заполняем списки департамента для viewModel

                // await VMinit(user);

                viewModel.usersList = await db.Users.Where(u => u.DepartmentId == user.DepartmentId && u.Role == "user").ToListAsync();
                viewModel.sizsList = await db.Sizs.Where(s => s.DepartmentId == user.DepartmentId).ToListAsync();
                viewModel.carsList = await db.Cars.Where(c => c.DepartmentId == user.DepartmentId).ToListAsync();
                viewModel.placesList = await db.Places.Where(p => p.DepartmentId == user.DepartmentId).ToListAsync();
                viewModel.CheckDate = DateTime.Today;

                int Dep = user.DepartmentId;
                viewModel.alarmSizsList = await db.Sizs.Where(s => s.DepartmentId == Dep
                                        && s.NextCheckDate <= viewModel.CheckDate.AddDays(7)).ToListAsync();

                ViewData["Title"] = $"Список СИЗ с истёкшим сроком поверки для {user.Login}";
             
                return View(viewModel);
            }
            else
            {
                ModelState.AddModelError("", "Обрыв связи с базой данных (не удалось получить данные)");
                return RedirectToAction("Login", "Account");
            }

        }

        //[HttpGet]
        //[Authorize(Roles = "deptadmin")]
        //public async Task<IActionResult> NewSizCreateFAsync() // форма для добавления СИЗ
        //{
        //    User? user = await db.Users.FirstOrDefaultAsync(u => u.Login == User.Identity.Name);
        //    if (user == null)
        //    {
        //        ModelState.AddModelError("", "Обрыв связи с базой данных (не удалось получить данные)");
        //        return RedirectToAction("Login", "Account");
        //    }
        //    ViewData["Title"] = "Создание записи для новогоСИЗ";
        //    newSizCreateViewModel.DepartmentId = user.DepartmentId;
        //    return View("NewSizCreateFAsync", newSizCreateViewModel);
        //}

        [HttpGet]
        [Authorize(Roles = "deptadmin")]
        public async Task<IActionResult> NewSizCreate()
        {
            User? user = await db.Users.FirstOrDefaultAsync(u => u.Login == User.Identity.Name);
            if (user == null)
            {
                ModelState.AddModelError("", "Обрыв связи с базой данных (не удалось получить данные)");
                return RedirectToAction("Login", "Account");
            }
            ViewData["Title"] = "Создание записи для новогоСИЗ";
            newSizCreateViewModel.DepartmentId = user.DepartmentId;
            return View("NewSizCreate", newSizCreateViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "deptadmin")]
        public async Task<IActionResult> NewSizCreate(NewSizCreateViewModel nSiz) // добавление нового СИЗ
        {
            if (ModelState.IsValid)
            {
                User? user = await db.Users.FirstOrDefaultAsync(u => u.Login == User.Identity.Name);
                if (user != null)
                {
                    ViewData["Title"] = $"Создание записи для новогоСИЗ";
                    Siz? nSizCr = await db.Sizs.FirstOrDefaultAsync(s => s.TabNom == nSiz.TabNom);
                    if (nSizCr != null)
                    {
                        
                        ViewData["SizSituated"] = "СИЗ С ТАКИМ НОМЕРОМ СУЩЕСТВУЕТ!!!";
                        await SizBelongTo(nSizCr);                        

                        return View("NewSizCreate", nSiz);
                    }
                    else 
                    {
                        nSizCr = new Siz
                        {
                            Name = nSiz.Name,
                            TabNom = nSiz.TabNom,
                            NextCheckDate = nSiz.CheckDate,
                            DepartmentId = user.DepartmentId
                        };
                        db.Sizs.Add(nSizCr);
                        await db.SaveChangesAsync();

                        nSizCr = await db.Sizs.FirstOrDefaultAsync(s => s.TabNom == nSiz.TabNom);
                        if (nSizCr == null)
                        {
                            ModelState.AddModelError("", "Обрыв связи с базой данных (не удалось получить данные)");
                            return RedirectToAction("Login", "Account");
                        }
                        ViewData["SizSituated"] = "СИЗ ВНЕСЁН В БАЗУ ДАННЫХ";
                        await SizBelongTo(nSizCr);
                        return View("NewSizCreate", nSiz);
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Обрыв связи с базой данных (не удалось получить данные)");
                    return RedirectToAction("Login", "Account");
                }

            }

            else return View("NewSizCreate", nSiz);
        }



        [HttpPost]
        [Authorize(Roles = "deptadmin")]
        public async Task<IActionResult> ChangeUserF(int UserId)  // форма изменить данные работника
        {
            User? userDa = await db.Users.FirstOrDefaultAsync(u => u.Login == User.Identity.Name);
            if (userDa == null)
            {
                ModelState.AddModelError("", "Обрыв связи с базой данных (не удалось получить данные)");
                return RedirectToAction("Login", "Account");
            }
            if (UserId > 0)
            {
                User? user = await db.Users.FirstOrDefaultAsync(u => u.Id == UserId);
                if (user == null)
                {
                    ModelState.AddModelError("", "Обрыв связи с базой данных (не удалось получить данные)");
                    return RedirectToAction("Login", "Account");
                }
                userChViewModel.Id = user.Id;
                userChViewModel.Login = user.Login;
                userChViewModel.TabNom = user.TabNom;
                userChViewModel.DepartmentId = user.DepartmentId;
                userChViewModel.Password = user.Password;
                userChViewModel.Role = user.Role;
                Department? dep = await db.Departments.FirstOrDefaultAsync(d => d.Id == user.DepartmentId);
                if (dep == null)
                {
                    ModelState.AddModelError("", "Обрыв связи с базой данных (не удалось получить данные)");
                    return RedirectToAction("Login", "Account");
                }
                userChViewModel.DepartmentName = dep.Name;
                
                return View(userChViewModel);
            }
            return RedirectToAction("Login", "Account");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "deptadmin")]
        public async Task<IActionResult> ChangeUser(UserChangeViewModel userChangeViewModel) // изменить данные работника
        {
            User? user = await db.Users.FirstOrDefaultAsync(u => u.Login == User.Identity.Name);
            if (user == null)
            {
                ModelState.AddModelError("", "Обрыв связи с базой данных (не удалось получить данные)");
                return RedirectToAction("Login", "Account");
            }

            if (!ModelState.IsValid) 
            {
                ModelState.AddModelError("", "Полученные данные некорректны)");                 
                return View("ChangeUserF", userChangeViewModel);
            }

            user = await db.Users.FirstOrDefaultAsync(u => u.Id == userChangeViewModel.Id);
            if (user == null)
            {
                ViewData["Title"] = "Ошибка!";
                ViewData["TitleMess"] = "Такого работника нет в базе данных (не удалось получить данные)";
                return View("ErrorMess");
            }
            user.Login = userChangeViewModel.Login;
            user.Password = userChangeViewModel.Password;
            user.TabNom = userChangeViewModel.TabNom;
            user.DepartmentId = userChangeViewModel.DepartmentId;
            user.Role = userChangeViewModel.Role;
            db.Users.Update(user);
            await db.SaveChangesAsync();
            ViewData["Title"] = $"Сообщение для {user.Login}";
            ViewData["TitleMess"] = "Изменения внесены в базу данных";
            return View("ErrorMess");

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "deptadmin")]
        public async Task<IActionResult> DeleteUser(int Id) // удалить данные работника
        {
            User? user = await db.Users.FirstOrDefaultAsync(u => u.Login == User.Identity.Name);
            if (user == null)
            {
                ModelState.AddModelError("", "Обрыв связи с базой данных (не удалось получить данные)");
                return RedirectToAction("Login", "Account");
            }
            if (Id <= 0)
            {
                ViewData["Title"] = "Ошибка!";
                ViewData["TitleMess"] = "Полученные данные некорректны!";
                return View("ErrorMess");
            }
            Siz? siz = await db.Sizs.FirstOrDefaultAsync(s => s.UserId == Id);
            if (siz != null)
            {
                ViewData["Title"] = "Ошибка!";
                ViewData["TitleMess"] = "За работником закреплены СИЗ!";
                return View("ErrorMess");
            }

            user = await db.Users.FirstOrDefaultAsync(u => u.Id == Id);
            if (user != null) 
            { 
                db.Users.Remove(user);
                await db.SaveChangesAsync();
                ViewData["Title"] = "Выполнено";
                ViewData["TitleMess"] = "Данные удалены из базы";
                return View("ErrorMess");
            }
            else
            {
                ViewData["Title"] = "Ошибка!";
                ViewData["TitleMess"] = "Не удалось получить данные";
                return View("ErrorMess");                
            }
            
        }




        [HttpPost]
        
        [Authorize(Roles = "deptadmin")]
        public async Task<IActionResult> ChangePlaceF(int PlaceId) // форма изменить данные склада
        {
            User? user = await db.Users.FirstOrDefaultAsync(u => u.Login == User.Identity.Name);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (PlaceId > 0)
            {
                Place? pl = await db.Places.FirstOrDefaultAsync(p => p.Id == PlaceId);
                if (pl == null)
                {
                    ViewData["Title"] = "Ошибка!";
                    ViewData["TitleMess"] = "Нет такого склада в базе данных (не удалось получить данные)";
                    return View("ErrorMess");
                }
                placeChViewModel.Id = pl.Id;
                placeChViewModel.DepartmentId = pl.DepartmentId;
                placeChViewModel.Name = pl.Name;
                placeChViewModel.Description = pl.Description;                
                
                return View(placeChViewModel);
            }
            return RedirectToAction("Login", "Account");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "deptadmin" )]
        public async Task<IActionResult> ChangePlace(PlaceChangeViewModel plChViewModel) // изменить данные склада
        {
            User? user = await db.Users.FirstOrDefaultAsync(u => u.Login == User.Identity.Name);
            if (user == null)
            {                
                return RedirectToAction("Login", "Account");
            }

            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Полученные данные некорректны)");
                return View("ChangePlaceF", plChViewModel);
            }

            Place? pl = await db.Places.FirstOrDefaultAsync(p => p.Id == plChViewModel.Id);
            if (pl == null)
            {
                ViewData["Title"] = "Ошибка!";
                ViewData["TitleMess"] = "Такого склада нет в базе данных (не удалось получить данные)";
                return View("ErrorMess");
            }
            pl.Name = plChViewModel.Name;
            pl.Description = plChViewModel.Description;
            
            db.Places.Update(pl);
            await db.SaveChangesAsync();
            ViewData["Title"] = "Выполнено";
            ViewData["TitleMess"] = "Изменения внесены в базу данных";
            return View("ErrorMess");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "deptadmin")]
        public async Task<IActionResult> DeletePlace(int Id)
        {
            User? user = await db.Users.FirstOrDefaultAsync(u => u.Login == User.Identity.Name);
            if (user == null)
            {
                //ModelState.AddModelError("", "Обрыв связи с базой данных (не удалось получить данные)");
                return RedirectToAction("Login", "Account");
            }
            
            if (Id <= 0)
            {
                ViewData["Title"] = "Ошибка!";
                ViewData["TitleMess"] = "Полученные данные некорректны!";
                return View("ErrorMess");
            }
            Siz? siz = await db.Sizs.FirstOrDefaultAsync(s => s.PlaceId == Id);
            if (siz != null)
            {
                //Place? place = await db.Places.FirstOrDefaultAsync(p => p.Id == Id);
                //if (place == null)
                //{
                //    ViewData["Title"] = "Ошибка!";
                //    ViewData["TitleMess"] = "Не удалось получить данные о складе!";
                //    return View("ErrorMess");
                //}
                //placeChViewModel.Id = place.Id;
                //placeChViewModel.DepartmentId = place.DepartmentId;
                //placeChViewModel.Name = place.Name;
                //placeChViewModel.Description = place.Description;                
                //ViewData["PlaceMessage"] = "За складом закреплены СИЗ!!!";
                //return View("ChangePlaceF", placeChViewModel);
                ViewData["Title"] = "Ошибка!";
                ViewData["TitleMess"] = "За складом закреплены СИЗ!";
                return View("ErrorMess");

            }

            Place? pl = await db.Places.FirstOrDefaultAsync(p => p.Id == Id);
            if (pl != null)
            {
                db.Places.Remove(pl);
                await db.SaveChangesAsync();
                ViewData["Title"] = "Выполнено";
                ViewData["TitleMess"] = "Данные удалены из базы";
                return View("ErrorMess");
            }
            else
            {
                ViewData["Title"] = "Ошибка!";
                ViewData["TitleMess"] = "Такого склада нет в базе данных (не удалось получить данные)";
                return View("ErrorMess");
            }

        }

        [HttpGet]
        [Authorize(Roles = "deptadmin")]
        public async Task<IActionResult> NewPlaceCreate() // форма для добавления склада
        {
            User? user = await db.Users.FirstOrDefaultAsync(u => u.Login == User.Identity.Name);
            if (user == null)
            {
                ModelState.AddModelError("", "Обрыв связи с базой данных (не удалось получить данные)");
                return RedirectToAction("Login", "Account");
            }
            placeChViewModel = new PlaceChangeViewModel();
            placeChViewModel.DepartmentId = user.DepartmentId;

            return View("NewPlaceCreate", placeChViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "deptadmin")]
        public async Task<IActionResult> NewPlaceCreate(PlaceChangeViewModel plChViewModel)
        {
            User? user = await db.Users.FirstOrDefaultAsync(u => u.Login == User.Identity.Name);
            if (user == null)
            {
                ModelState.AddModelError("", "Обрыв связи с базой данных (не удалось получить данные)");
                return RedirectToAction("Login", "Account");
            }
            if (!ModelState.IsValid)
            {
                plChViewModel.DepartmentId = user.DepartmentId;
                ModelState.AddModelError("", "Ошибка ввода данных");
                return View(plChViewModel);
            }
            Place? place = await db.Places.FirstOrDefaultAsync(p => p.Name == plChViewModel.Name);
            if (place != null)
            {
                plChViewModel.DepartmentId = user.DepartmentId;
                ModelState.AddModelError("", "Такой склад уже существует");
                return View(plChViewModel);
            }
            place = new Place { Name = plChViewModel.Name,
                                Description = plChViewModel.Description,
                                DepartmentId = plChViewModel.DepartmentId};
            db.Places.Add(place);
            await db.SaveChangesAsync();
            ViewData["Title"] = "Выполнено";
            ViewData["TitleMess"] = "Данные внесены в базу";
            return View("ErrorMess");
            
        }


        [HttpPost]
        [Authorize(Roles = "deptadmin")]
        public async Task<IActionResult> ChangeCarF(int CarId)
        {
            if (CarId > 0)
            {
                Car? car = await db.Cars.FirstOrDefaultAsync(c => c.Id == CarId);
                if (car == null)
                {
                    ModelState.AddModelError("", "Обрыв связи с базой данных (не удалось получить данные)");
                    return RedirectToAction("Login", "Account");
                }
                carChViewModel.Id = car.Id;
                carChViewModel.DepartmentId = car.DepartmentId;
                carChViewModel.CarNomber = car.CarNomber;
                carChViewModel.Description = car.Description;
                return View(carChViewModel);
            }
            return RedirectToAction("Login", "Account");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "deptadmin")]
        public async Task<IActionResult> ChangeCar(CarChangeViewModel carChViewModel)
        {
            User? user = await db.Users.FirstOrDefaultAsync(u => u.Login == User.Identity.Name);
            if (user == null)
            {
                ModelState.AddModelError("", "Обрыв связи с базой данных (не удалось получить данные)");
                return RedirectToAction("Login", "Account");
            }

            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Полученные данные некорректны)");
                return View("ChangeCarF", carChViewModel);
            }

            Car? car = await db.Cars.FirstOrDefaultAsync(c => c.Id == carChViewModel.Id);
            if (car == null)
            {
                ViewData["Title"] = "Ошибка!";
                ViewData["TitleMess"] = "Такого автомобиля нет в базе данных (не удалось получить данные)";
                return View("ErrorMess");
            }
            car.CarNomber = carChViewModel.CarNomber;
            car.Description = carChViewModel.Description;

            db.Cars.Update(car);
            await db.SaveChangesAsync();
            ViewData["Title"] = "Выполнено";
            ViewData["TitleMess"] = "Изменения внесены в базу данных";
            return View("ErrorMess");

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "deptadmin")]
        public async Task<IActionResult> DeleteCar(int Id)
        {
            User? user = await db.Users.FirstOrDefaultAsync(u => u.Login == User.Identity.Name);
            if (user == null)
            {
                ModelState.AddModelError("", "Обрыв связи с базой данных (не удалось получить данные)");
                return RedirectToAction("Login", "Account");
            }
            if (Id <= 0)
            {
                ViewData["Title"] = "Ошибка!";
                ViewData["TitleMess"] = "Полученные данные некорректны!";
                return View("ErrorMess");
            }
            Siz? siz = await db.Sizs.FirstOrDefaultAsync(s => s.CarId == Id);
            if (siz != null)
            {
                //Car? carF = await db.Cars.FirstOrDefaultAsync(c => c.Id == Id);
                //if(carF == null)
                //{
                //    ViewData["Title"] = "Ошибка!";
                //    ViewData["TitleMess"] = "Не удалось получить данные об автомобиле!";
                //    return View("ErrorMess");

                //}
                //carChViewModel.Id = carF.Id;
                //carChViewModel.DepartmentId = carF.DepartmentId;
                //carChViewModel.CarNomber = carF.CarNomber;
                //carChViewModel.Description = carF.Description;
                //ViewData["CarMessage"] = "За автомобилем закреплены СИЗ!";
                //return View("ChangeCarF", carChViewModel);
                ViewData["Title"] = "Ошибка!";
                ViewData["TitleMess"] = "За автомобилем закреплены СИЗ!";
                return View("ErrorMess");
            }
            Car? car = await db.Cars.FirstOrDefaultAsync(c => c.Id == Id);
            if (car != null)
            {
                db.Cars.Remove(car);
                await db.SaveChangesAsync();
                ViewData["Title"] = "Выполнено";
                ViewData["TitleMess"] = "Данные удалены из базы";
                return View("ErrorMess");                
            }
            else
            {
                ViewData["Title"] = "Ошибка!";
                ViewData["TitleMess"] = "Не удалось получить данные об автомобиле!";
                return View("ErrorMess");
            }

        }

        [HttpGet]
        [Authorize(Roles = "deptadmin")]
        public async Task<IActionResult> NewCarCreate() // форма для добавления машины
        {
            User? user = await db.Users.FirstOrDefaultAsync(u => u.Login == User.Identity.Name);
            if (user == null)
            {
                ModelState.AddModelError("", "Обрыв связи с базой данных (не удалось получить данные)");
                return RedirectToAction("Login", "Account");
            }
            carChViewModel = new CarChangeViewModel();
            carChViewModel.DepartmentId = user.DepartmentId;

            ModelState.AddModelError("", "Введите данные новой машины");
            return View("NewCarCreate", carChViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "deptadmin")]
        public async Task<IActionResult> NewCarCreate(CarChangeViewModel carChViewModel)
        {
            User? user = await db.Users.FirstOrDefaultAsync(u => u.Login == User.Identity.Name);
            if (user == null)
            {
                ModelState.AddModelError("", "Обрыв связи с базой данных (не удалось получить данные)");
                return RedirectToAction("Login", "Account");
            }
            if (!ModelState.IsValid)
            {
                carChViewModel.DepartmentId = user.DepartmentId;
                ModelState.AddModelError("", "Ошибка ввода данных");
                return View("ChangeCarF",carChViewModel);
            }
            Car? car = await db.Cars.FirstOrDefaultAsync(c => c.CarNomber == carChViewModel.CarNomber);
            if (car != null)
            {
                carChViewModel.DepartmentId = user.DepartmentId;
                ModelState.AddModelError("", "Такой автомобиль уже существует");
                return View("ChangeCarF", carChViewModel);
            }
            car = new Car
            {
                CarNomber = carChViewModel.CarNomber,
                Description = carChViewModel.Description,
                DepartmentId = carChViewModel.DepartmentId
            };
            db.Cars.Add(car);
            await db.SaveChangesAsync();
            ViewData["Title"] = "Выполнено";
            ViewData["TitleMess"] = "Данные внесены в базу";
            return View("ErrorMess");
            
        }





        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "deptadmin")]
        public async Task<IActionResult> UsersSizs(int UserId) //список СИЗ выбранного работника
        {
            
            User? user = await db.Users.FirstOrDefaultAsync(u => u.Login == User.Identity.Name);
            if (user != null)
            {
                // формируем список СИЗ работника 
                viewModel.alarmSizsList.Clear();
                viewModel.alarmSizsList = await db.Sizs.Where(s => s.UserId == UserId).ToListAsync();
                var UserFoName = await db.Users.FirstOrDefaultAsync(u => u.Id == UserId);
                if (UserFoName == null)
                {
                    ModelState.AddModelError("", "Обрыв связи с базой данных (не удалось получить данные)");
                    return RedirectToAction("Login", "Account");
                }
                string SelUserName = UserFoName.Login;

                //await VMinit(user);

                viewModel.usersList = await db.Users.Where(u => u.DepartmentId == user.DepartmentId && u.Role == "user").ToListAsync();
                viewModel.sizsList = await db.Sizs.Where(s => s.DepartmentId == user.DepartmentId).ToListAsync();
                viewModel.carsList = await db.Cars.Where(c => c.DepartmentId == user.DepartmentId).ToListAsync();
                viewModel.placesList = await db.Places.Where(p => p.DepartmentId == user.DepartmentId).ToListAsync();
                viewModel.CheckDate = DateTime.Today;

                ViewData["Title"] = $"Список СИЗ работника {SelUserName} : на дату {viewModel.CheckDate.Date.ToShortDateString()} ";

                return View("Index",viewModel);
            }
            else
            {
                ModelState.AddModelError("", "Обрыв связи с базой данных (не удалось получить данные)");
                return RedirectToAction("Login", "Account");
            }   
            
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "deptadmin")]
        public async Task<IActionResult> PlaceSizs(int PlaceId) //список СИЗ выбранного склада
        {
            User? user = await db.Users.FirstOrDefaultAsync(u => u.Login == User.Identity.Name);
            if (user != null)
            {
                
                // формируем список СИЗ склада 
                viewModel.alarmSizsList.Clear();
                viewModel.alarmSizsList = await db.Sizs.Where(s => s.PlaceId == PlaceId).ToListAsync();
                var PlaceFoName = await db.Places.FirstOrDefaultAsync(p => p.Id == PlaceId);
                if (PlaceFoName == null)
                {
                    ModelState.AddModelError("", "Обрыв связи с базой данных (не удалось получить данные)");
                    return RedirectToAction("Login", "Account");
                }
                string SelPlaceName = PlaceFoName.Name;

                //await VMinit(user);

                viewModel.usersList = await db.Users.Where(u => u.DepartmentId == user.DepartmentId && u.Role == "user").ToListAsync();
                viewModel.sizsList = await db.Sizs.Where(s => s.DepartmentId == user.DepartmentId).ToListAsync();
                viewModel.carsList = await db.Cars.Where(c => c.DepartmentId == user.DepartmentId).ToListAsync();
                viewModel.placesList = await db.Places.Where(p => p.DepartmentId == user.DepartmentId).ToListAsync();
                viewModel.CheckDate = DateTime.Today;

                ViewData["Title"] = $"Список СИЗ на складе {SelPlaceName} : на дату {viewModel.CheckDate.Date.ToShortDateString()} ";

                return View("Index", viewModel);
            }
            else
            {
                ModelState.AddModelError("", "Обрыв связи с базой данных (не удалось получить данные)");
                return RedirectToAction("Login", "Account");
            }

        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "deptadmin")]
        public async Task<IActionResult> CarSizs(int CarId) //список СИЗ выбранного автомобиля
        {
            User? user = await db.Users.FirstOrDefaultAsync(u => u.Login == User.Identity.Name);
            if (user != null)
            {
                
                // формируем список СИЗ автомобиля 
                viewModel.alarmSizsList.Clear();
                viewModel.alarmSizsList = await db.Sizs.Where(s => s.CarId == CarId).ToListAsync();
                var CarFoName = await db.Cars.FirstOrDefaultAsync(c => c.Id == CarId);
                if (CarFoName == null)
                {
                    ModelState.AddModelError("", "Обрыв связи с базой данных (не удалось получить данные)");
                    return RedirectToAction("Login", "Account");
                }
                string SelCarName = CarFoName.CarNomber;

                //await VMinit(user);

                viewModel.usersList = await db.Users.Where(u => u.DepartmentId == user.DepartmentId && u.Role == "user").ToListAsync();
                viewModel.sizsList = await db.Sizs.Where(s => s.DepartmentId == user.DepartmentId).ToListAsync();
                viewModel.carsList = await db.Cars.Where(c => c.DepartmentId == user.DepartmentId).ToListAsync();
                viewModel.placesList = await db.Places.Where(p => p.DepartmentId == user.DepartmentId).ToListAsync();
                viewModel.CheckDate = DateTime.Today;

                ViewData["Title"] = $"Список СИЗ автомобиля {SelCarName} : на дату {viewModel.CheckDate.Date.ToShortDateString()} ";

                return View("Index", viewModel);
            }
            else
            {
                ModelState.AddModelError("", "Обрыв связи с базой данных (не удалось получить данные)");
                return RedirectToAction("Login", "Account");
            }

        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "deptadmin")]
        public async Task<IActionResult> CheckDateSizs(DateTime CheckDate) //список СИЗ с просроченной датой поверки
        {
            User? user = await db.Users.FirstOrDefaultAsync(u => u.Login == User.Identity.Name);
            if (user != null)
            {

                // формируем список СИЗ  с просроченной датой поверки
                viewModel.alarmSizsList.Clear();
                viewModel.alarmSizsList = await db.Sizs.Where(s => s.DepartmentId == user.DepartmentId
                                                            && s.NextCheckDate <= CheckDate).ToListAsync();
                var DateProm = CheckDate;

                //await VMinit(user);

                viewModel.usersList = await db.Users.Where(u => u.DepartmentId == user.DepartmentId && u.Role == "user").ToListAsync();
                viewModel.sizsList = await db.Sizs.Where(s => s.DepartmentId == user.DepartmentId).ToListAsync();
                viewModel.carsList = await db.Cars.Where(c => c.DepartmentId == user.DepartmentId).ToListAsync();
                viewModel.placesList = await db.Places.Where(p => p.DepartmentId == user.DepartmentId).ToListAsync();

                viewModel.CheckDate = CheckDate;
                ViewData["Title"] = $"Список СИЗ для {user.Login}";
                ViewData["TitleMess"] = $"СИЗ с просроченной датой поверки на {viewModel.CheckDate.ToShortDateString()}";
                return View("Index", viewModel);
            }
            else
            {
                ModelState.AddModelError("", "Обрыв связи с базой данных (не удалось получить данные)");
                return RedirectToAction("Login", "Account");
            }

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "deptadmin")]
        public async Task<IActionResult> SelSizByNom(string Nomber)
        {
            User? user = await db.Users.FirstOrDefaultAsync(u => u.Login == User.Identity.Name);
            if (user != null)
            {                              
                Siz? prSiz = await db.Sizs.FirstOrDefaultAsync(s => s.TabNom == Nomber);
                if (prSiz != null)
                {
                    viewModel.alarmSizsList.Clear();
                    viewModel.alarmSizsList.Add(prSiz);// заполняем "список" к показу - один элемент 
                    // определяем , куда распределён СИЗ
                    await SizBelongTo(prSiz);

                    viewModel.usersList = await db.Users.Where(u => u.DepartmentId == user.DepartmentId && u.Role == "user").ToListAsync();
                    viewModel.sizsList = await db.Sizs.Where(s => s.DepartmentId == user.DepartmentId).ToListAsync();
                    viewModel.carsList = await db.Cars.Where(c => c.DepartmentId == user.DepartmentId).ToListAsync();
                    viewModel.placesList = await db.Places.Where(p => p.DepartmentId == user.DepartmentId).ToListAsync();
                    viewModel.CheckDate = DateTime.Today;
                    viewModel.SizId = prSiz.Id;
                    ViewData["Title"] = $"Страница просмотра и редактирования данных СИЗ";

                    return View("SelectedSiz", viewModel);

                    //return RedirectToAction("SelectedSiz","DeptAdmin", new {SizId = prSiz.Id});
                }
                ViewData["Title"] = "Ошибка !";
                ViewData["TitleMess"] = "Не удалось найти СИЗ с таким Табельным номером";
                return View("ErrorMess");
            }
            else
            {
                ModelState.AddModelError("", "Обрыв связи с базой данных (не удалось получить данные)");
                return RedirectToAction("Login", "Account");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "deptadmin")]
        public async Task<IActionResult> SelectedSiz(int SizId) //данные выбранного СИЗ
        {
            User? user = await db.Users.FirstOrDefaultAsync(u => u.Login == User.Identity.Name);
            if (user != null)
            {               
                //await VMinit(user);

                viewModel.alarmSizsList.Clear();
                // получаем выбранный СИЗ из БД                
                Siz? prSiz = await db.Sizs.FirstOrDefaultAsync(s => s.Id == SizId);
                if (prSiz != null)
                {                    
                    viewModel.alarmSizsList.Add(prSiz);// заполняем "список" к показу - один элемент 
                    // определяем , куда распределён СИЗ

                    await SizBelongTo(prSiz);
                    
                }

                viewModel.usersList = await db.Users.Where(u => u.DepartmentId == user.DepartmentId && u.Role == "user").ToListAsync();
                viewModel.sizsList = await db.Sizs.Where(s => s.DepartmentId == user.DepartmentId).ToListAsync();
                viewModel.carsList = await db.Cars.Where(c => c.DepartmentId == user.DepartmentId).ToListAsync();
                viewModel.placesList = await db.Places.Where(p => p.DepartmentId == user.DepartmentId).ToListAsync();
                viewModel.CheckDate = DateTime.Today;
                viewModel.SizId = SizId;
                ViewData["Title"] = $"Страница просмотра и редактирования данных СИЗ";

                return View(viewModel);
            }
            else
            {
                ModelState.AddModelError("", "Обрыв связи с базой данных (не удалось получить данные)");
                return RedirectToAction("Login", "Account");
            }

        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "deptadmin")]
        public async Task<IActionResult> SizsUserIdChange(int UserId,int SizId) // передать СИЗ работнику
        {

            if (UserId > 0) 
            {                
                Siz? prSiz = await db.Sizs.FirstOrDefaultAsync(s => s.Id == SizId);
                if (prSiz != null )
                {
                    prSiz.UserId = UserId;// выбранный в селекте
                    prSiz.PlaceId = 0;
                    prSiz.CarId = 0;
                    viewModel.PlaceId = 0;
                    viewModel.CarId = 0;
                    viewModel.SizId= SizId;
                    db.Sizs.Update(prSiz);
                    await db.SaveChangesAsync();
                    ViewData["Title"] = $"Новые данные СИЗ";
                    viewModel.alarmSizsList.Clear();
                    viewModel.alarmSizsList.Add(prSiz);

                    await SizBelongTo(prSiz);
                   
                    return View(viewModel);
                }
                else
                {
                    ModelState.AddModelError("", "Обрыв связи с базой данных (не удалось получить данные)");
                    return RedirectToAction("Login", "Account");
                }
            }
            else
            {
                ModelState.AddModelError("", "Обрыв связи с базой данных (не удалось получить данные)");
                return RedirectToAction("Login", "Account");
            }
           
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "deptadmin")]

        public async Task<IActionResult> SizsPlaceIdChange(int PlaceId, int SizId)  // передать СИЗ на склад
        {
            if (PlaceId > 0)
            {
                Siz? prSiz = await db.Sizs.FirstOrDefaultAsync(s => s.Id == SizId);
                if (prSiz != null)
                {
                    prSiz.PlaceId = PlaceId; // выбранный в селекте
                    prSiz.UserId = 0;
                    prSiz.CarId = 0;
                    viewModel.UserId = 0;
                    viewModel.CarId = 0;
                    viewModel.SizId = SizId;
                    db.Sizs.Update(prSiz);
                    await db.SaveChangesAsync();
                    ViewData["Title"] = $"Новые данные СИЗ";
                    viewModel.alarmSizsList.Clear();
                    viewModel.alarmSizsList.Add(prSiz);

                    await SizBelongTo(prSiz);

                    return View(viewModel);
                }
                else
                {
                    ModelState.AddModelError("", "Обрыв связи с базой данных (не удалось получить данные)");
                    return RedirectToAction("Login", "Account");
                }
            }
            else
            {
                ModelState.AddModelError("", "Обрыв связи с базой данных (не удалось получить данные)");
                return RedirectToAction("Login", "Account");
            }

        }

        public async Task<IActionResult> SizsCarIdChange(DepartAdminViewModel viewModel) // передать СИЗ в автомобиль
        {
            if (viewModel.CarId > 0)
            {
                viewModel.UserId = 0;
                viewModel.PlaceId = 0;
                Siz? prSiz = await db.Sizs.FirstOrDefaultAsync(s => s.Id == viewModel.SizId);
                if (prSiz != null)
                {
                    prSiz.PlaceId = 0; 
                    prSiz.UserId = 0;
                    prSiz.CarId = viewModel.CarId;  // выбранный в селекте
                    db.Sizs.Update(prSiz);
                    await db.SaveChangesAsync();
                    ViewData["Title"] = $"Новые данные СИЗ";
                    viewModel.alarmSizsList.Clear();
                    viewModel.alarmSizsList.Add(prSiz);

                    await SizBelongTo(prSiz);

                    return View(viewModel);
                }
                else
                {
                    ModelState.AddModelError("", "Обрыв связи с базой данных (не удалось получить данные)");
                    return RedirectToAction("Login", "Account");
                }
            }
            else
            {
                ModelState.AddModelError("", "Обрыв связи с базой данных (не удалось получить данные)");
                return RedirectToAction("Login", "Account");
            }
        }

        public async Task<IActionResult> SizDelete(int SizId)
        {
            User? user = await db.Users.FirstOrDefaultAsync(u => u.Login == User.Identity.Name);
            if (user == null)
            {                
                return RedirectToAction("Login", "Account");
            }

            if (SizId <= 0)
            {
                ViewData["Title"] = "Ошибка!";
                ViewData["TitleMess"] = "Полученные данные некорректны!";
                return View("ErrorMess");
            }
            Siz? siz = await db.Sizs.FirstOrDefaultAsync(s => s.Id == SizId);
            if (siz != null)
            {
                if(siz.UserId > 0 || siz.CarId > 0 || siz.PlaceId >0) 
                { 
                    ViewData["Title"] = "Ошибка!";
                    ViewData["TitleMess"] = "Невозможно удалить. СИЗ распределён!";
                    return View("ErrorMess");
                }
                else if(siz.UserId < 0 || siz.CarId < 0 || siz.PlaceId < 0)
                {
                    ViewData["Title"] = "Ошибка!";
                    ViewData["TitleMess"] = "Полученные данные некорректны!";
                    return View("ErrorMess");
                }
                db.Sizs.Remove(siz);
                await db.SaveChangesAsync();
                ViewData["Title"] = "Выполнено";
                ViewData["TitleMess"] = "Данные удалены из базы";
                return View("ErrorMess");
            }
            ViewData["Title"] = "Ошибка!";
            ViewData["TitleMess"] = "Получены данные некорректны!(ошибка связи)";
            return View("ErrorMess");
        }

        public async Task<IActionResult> SizsBelongDelete(int SizId) // изъять СИЗ  
        {
            if (SizId > 0)
            {
                Siz? prSiz = await db.Sizs.FirstOrDefaultAsync(s => s.Id == SizId);
                if (prSiz != null)
                {
                    viewModel.CarId = 0;
                    viewModel.UserId = 0;
                    viewModel.PlaceId = 0;
                    viewModel.SizId = SizId;
                    prSiz.PlaceId = 0;
                    prSiz.UserId = 0;
                    prSiz.CarId = 0;  
                    db.Sizs.Update(prSiz);
                    await db.SaveChangesAsync();
                    ViewData["Title"] = $"Новые данные СИЗ";
                    viewModel.alarmSizsList.Clear();
                    viewModel.alarmSizsList.Add(prSiz);

                    await SizBelongTo(prSiz);

                    return View(viewModel);
                }
                else
                {
                    ModelState.AddModelError("", "Обрыв связи с базой данных (не удалось получить данные)");
                    return RedirectToAction("Login", "Account");
                }
            }
            else
            {
                ModelState.AddModelError("", "Обрыв связи с базой данных (не удалось получить данные)");
                return RedirectToAction("Login", "Account");
            }

        }


        private async Task SizBelongTo(Siz prSiz)  // где местонахождение СИЗ?
        {
            string str = "";
            ViewData["SizPlace"] = $"СИЗ {prSiz.Name} не распределён.";
            if (prSiz.CarId > 0)
            {
                var CarPr = await db.Cars.FirstOrDefaultAsync(c => c.Id == prSiz.CarId);
                if (CarPr != null)
                {
                    str = "в автомобиле с госноменомером: " + CarPr.CarNomber;
                }
                else
                {
                    ViewData["ErrConDB"] = "Обрыв связи с базой данных (не удалось получить данные)";                    
                }
                ViewData["SizPlace"] = $"СИЗ {prSiz.Name} должен находиться {str}";
            }
            if (prSiz.PlaceId > 0)
            {
                var PlPr = await db.Places.FirstOrDefaultAsync(c => c.Id == prSiz.PlaceId);
                if (PlPr != null)
                {
                    str = "на складе: " + PlPr.Name;
                }
                else
                {
                    ViewData["ErrConDB"] = "Обрыв связи с базой данных (не удалось получить данные)";
                }
                ViewData["SizPlace"] = $"СИЗ {prSiz.Name} должен находиться {str}";
            }
            if (prSiz.UserId > 0)
            {
                var UsPr = await db.Users.FirstOrDefaultAsync(c => c.Id == prSiz.UserId);
                if (UsPr != null)
                {
                    str = " у работника : " + UsPr.Login;
                }
                else
                {
                    ViewData["ErrConDB"] = "Обрыв связи с базой данных (не удалось получить данные)";
                }
                ViewData["SizPlace"] = $"СИЗ {prSiz.Name} должен находиться {str}";
            }

            if (ViewData["ErrConDB"] != null)
            {
                ViewData["SizPlace"] = ViewData["ErrConDB"];
            }

        }

    }
}
