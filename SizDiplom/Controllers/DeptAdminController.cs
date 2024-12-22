using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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


        public DeptAdminController(ProgDBaseContext context)
        {
            db = context;
            viewModel = new DepartAdminViewModel(); 
            userChViewModel = new UserChangeViewModel();
            placeChViewModel = new PlaceChangeViewModel();
            carChViewModel = new CarChangeViewModel();
        }
               

          
        [Authorize(Roles = "deptadmin")]
        public async Task<IActionResult> Index()// GET: DeptAdminController  вход для deptptadmin вывод просроченных СИЗ выбор дальнейшего пути
        {
            User? user = await db.Users.FirstOrDefaultAsync(u => u.Login == User.Identity.Name);
            if (user != null)
            {
                // заполняем списки департамента для viewModel

                // await VMinit(user);

                viewModel.usersList = await db.Users.Where(u => u.DepartmentId == user.DepartmentId).ToListAsync();
                viewModel.sizsList = await db.Sizs.Where(s => s.DepartmentId == user.DepartmentId).ToListAsync();
                viewModel.carsList = await db.Cars.Where(c => c.DepartmentId == user.DepartmentId).ToListAsync();
                viewModel.placesList = await db.Places.Where(p => p.DepartmentId == user.DepartmentId).ToListAsync();
                viewModel.CheckDate = DateTime.Today;

                int Dep = user.DepartmentId;
                viewModel.alarmSizsList = await db.Sizs.Where(s => s.DepartmentId == Dep
                                        && s.NextCheckDate <= viewModel.CheckDate.AddDays(7)).ToListAsync();

                ViewData["Title"] = $"Список СИЗ с истекающим сроком поверки для {user.Login} DeptAdminController.Index";
             
                return View(viewModel);
            }
            else
            {
                ModelState.AddModelError("", "Обрыв связи с базой данных (не удалось получить данные)");
                return RedirectToAction("Login", "Account");
            }

        }

        [HttpPost]
        [Authorize(Roles = "deptadmin")]
        public IActionResult NewSizCreateF() // форма для добавления СИЗ
        {
            return View();
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
                    Siz? nSizCr = await db.Sizs.FirstOrDefaultAsync(s => s.TabNom == nSiz.TabNom);
                    if (nSizCr != null)
                    {

                        ViewData["SizSituated"] = "СИЗ С ТАКИМ НОМЕРОМ СУЩЕСТВУЕТ!!!";
                        await SizBelongTo(nSizCr);                        

                        return View("NewSizCreateF", nSiz);
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
                        return View("NewSizCreateF", nSiz);
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Обрыв связи с базой данных (не удалось получить данные)");
                    return RedirectToAction("Login", "Account");
                }

            }
            else return View("NewSizCreateF", nSiz);
        }



        [HttpPost]
        [Authorize(Roles = "deptadmin")]
        public async Task<IActionResult> ChangeUserF(DepartAdminViewModel viewModel)  // форма изменить данные работника
        {
            if (!ModelState.IsValid)
            {
                User? user = await db.Users.FirstOrDefaultAsync(u => u.Id == viewModel.UserId);
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
                
            }
            return View(userChViewModel);
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
                //return RedirectToAction("Index"); 
                return View("ChangeUserF", userChangeViewModel);
            }
            user = await db.Users.FirstOrDefaultAsync(u => u.Id == userChangeViewModel.Id);
            if (user == null)
            {
                ModelState.AddModelError("", "Обрыв связи с базой данных (не удалось получить данные)");
                return RedirectToAction("Index", "DeptAdmin");
            }
            user.Login = userChangeViewModel.Login;
            user.Password = userChangeViewModel.Password;
            user.TabNom = userChangeViewModel.TabNom;
            user.DepartmentId = userChangeViewModel.DepartmentId;
            user.Role = userChangeViewModel.Role;
            db.Users.Update(user);
            await db.SaveChangesAsync();

            return RedirectToAction("Index");

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "deptadmin")]
        public async Task<IActionResult> DeleteUser(UserChangeViewModel userChangeViewModel) // удалить данные работника
        {
            User? user = await db.Users.FirstOrDefaultAsync(u => u.Login == User.Identity.Name);
            if (user == null)
            {
                ModelState.AddModelError("", "Обрыв связи с базой данных (не удалось получить данные)");
                return RedirectToAction("Login", "Account");
            }
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Полученные данные некорректны!!!");                
                return RedirectToAction("ChangeUser", userChangeViewModel); 
            }
            Siz? siz = await db.Sizs.FirstOrDefaultAsync(s => s.UserId == userChangeViewModel.Id);
            if (siz != null)
            {
                ModelState.AddModelError("", "За работником закреплены СИЗ!!!");
                return RedirectToAction("ChangeUser", userChangeViewModel); 
            }
            user = await db.Users.FirstOrDefaultAsync(u => u.Id == userChangeViewModel.Id);
            if (user != null) 
            { 
                db.Users.Remove(user);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            else
            {
                ModelState.AddModelError("", "Такого работника нет в базе (не удалось получить данные)");
                return RedirectToAction("ChangeUser", userChangeViewModel);
            }
            
        }




        [HttpPost]
        [Authorize(Roles = "deptadmin")]
        public async Task<IActionResult> ChangePlaceF(DepartAdminViewModel viewModel) // форма изменить данные склада
        {
            if (!ModelState.IsValid)
            {
                Place? pl = await db.Places.FirstOrDefaultAsync(p => p.Id == viewModel.PlaceId);
                if (pl == null)
                {
                    ModelState.AddModelError("", "Обрыв связи с базой данных (не удалось получить данные)");
                    return RedirectToAction("Login", "Account");
                }
                placeChViewModel.Id = pl.Id;
                placeChViewModel.DepartmentId = pl.DepartmentId;
                placeChViewModel.Name = pl.Name;
                placeChViewModel.Description = pl.Description;
                
                
            }
            return View(placeChViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "deptadmin" )]
        public async Task<IActionResult> ChangePlace(PlaceChangeViewModel plChViewModel) // изменить данные склада
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
                return View(plChViewModel);
            }
            Place? pl = await db.Places.FirstOrDefaultAsync(p => p.Id == plChViewModel.Id);
            if (pl == null)
            {
                ModelState.AddModelError("", "Обрыв связи с базой данных (не удалось получить данные)");
                return RedirectToAction("Index", "DeptAdmin");
            }
            pl.Name = plChViewModel.Name;
            pl.Description = plChViewModel.Description;
            
            db.Places.Update(pl);
            await db.SaveChangesAsync();

            return RedirectToAction("Index");

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "deptadmin")]
        public async Task<IActionResult> DeletePlace(PlaceChangeViewModel plChViewModel)
        {
            User? user = await db.Users.FirstOrDefaultAsync(u => u.Login == User.Identity.Name);
            if (user == null)
            {
                ModelState.AddModelError("", "Обрыв связи с базой данных (не удалось получить данные)");
                return RedirectToAction("Login", "Account");
            }
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Полученные данные некорректны!!!");
                return View("ChangePlaceF", plChViewModel);
            }
            Siz? siz = await db.Sizs.FirstOrDefaultAsync(s => s.PlaceId == plChViewModel.Id);
            if (siz != null)
            {
                ModelState.AddModelError("", "За складом закреплены СИЗ!!!");                
                return View("ChangePlaceF", plChViewModel);
            }
            Place? pl = await db.Places.FirstOrDefaultAsync(p => p.Id == plChViewModel.Id);
            if (pl != null)
            {
                db.Places.Remove(pl);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            else
            {
                ModelState.AddModelError("", "Такого склада нет в базе (не удалось получить данные)");
                return View("ChangePlaceF", plChViewModel);
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
            return RedirectToAction("Index");
        }


        [HttpPost]
        [Authorize(Roles = "deptadmin")]
        public async Task<IActionResult> ChangeCarF(DepartAdminViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                Car? car = await db.Cars.FirstOrDefaultAsync(c => c.Id == viewModel.CarId);
                if (car == null)
                {
                    ModelState.AddModelError("", "Обрыв связи с базой данных (не удалось получить данные)");
                    return RedirectToAction("Login", "Account");
                }
                carChViewModel.Id = car.Id;
                carChViewModel.DepartmentId = car.DepartmentId;
                carChViewModel.CarNomber = car.CarNomber;
                carChViewModel.Description = car.Description;
            }
            return View(carChViewModel);
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
                carChViewModel.DepartmentId = user.DepartmentId;
                ModelState.AddModelError("", "Полученные данные некорректны)");
                return View("ChangeCarF", carChViewModel);
            }
            Car? car = await db.Cars.FirstOrDefaultAsync(c => c.Id == carChViewModel.Id);
            if (car == null)
            {
                ModelState.AddModelError("", "Обрыв связи с базой данных (не удалось получить данные)");
                return RedirectToAction("Index", "DeptAdmin");
            }
            car.CarNomber = carChViewModel.CarNomber;
            car.Description = carChViewModel.Description;

            db.Cars.Update(car);
            await db.SaveChangesAsync();

            return RedirectToAction("Index");

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "deptadmin")]
        public async Task<IActionResult> DeleteCar(CarChangeViewModel carChViewModel)
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
                ModelState.AddModelError("", "Полученные данные некорректны!!!");
                return View("ChangeCarF", carChViewModel);
            }
            Siz? siz = await db.Sizs.FirstOrDefaultAsync(s => s.CarId == carChViewModel.Id);
            if (siz != null)
            {
                ModelState.AddModelError("", "За автомобилем закреплены СИЗ!!!");
                return View("ChangeCarF", carChViewModel);
            }
            Car? car = await db.Cars.FirstOrDefaultAsync(c => c.Id == carChViewModel.Id);
            if (car != null)
            {
                db.Cars.Remove(car);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            else
            {
                ModelState.AddModelError("", "Такого автомобиля нет в базе (не удалось получить данные)");
                return View("ChangeCarF", carChViewModel);
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
            return RedirectToAction("Index");
        }





        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "deptadmin")]
        public async Task<IActionResult> UsersSizs(DepartAdminViewModel viewModel) //список СИЗ выбранного работника
        {
            
            User? user = await db.Users.FirstOrDefaultAsync(u => u.Login == User.Identity.Name);
            if (user != null)
            {
                // формируем список СИЗ работника 
                viewModel.alarmSizsList.Clear();
                viewModel.alarmSizsList = await db.Sizs.Where(s => s.UserId == viewModel.UserId).ToListAsync();
                var UserFoName = await db.Users.FirstOrDefaultAsync(u => u.Id == viewModel.UserId);
                if (UserFoName == null)
                {
                    ModelState.AddModelError("", "Обрыв связи с базой данных (не удалось получить данные)");
                    return RedirectToAction("Login", "Account");
                }
                string SelUserName = UserFoName.Login;

                //await VMinit(user);

                viewModel.usersList = await db.Users.Where(u => u.DepartmentId == user.DepartmentId).ToListAsync();
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
        public async Task<IActionResult> PlaceSizs(DepartAdminViewModel viewModel) //список СИЗ выбранного склада
        {
            User? user = await db.Users.FirstOrDefaultAsync(u => u.Login == User.Identity.Name);
            if (user != null)
            {
                
                // формируем список СИЗ склада 
                viewModel.alarmSizsList.Clear();
                viewModel.alarmSizsList = await db.Sizs.Where(s => s.PlaceId == viewModel.PlaceId).ToListAsync();
                var PlaceFoName = await db.Places.FirstOrDefaultAsync(p => p.Id == viewModel.PlaceId);
                if (PlaceFoName == null)
                {
                    ModelState.AddModelError("", "Обрыв связи с базой данных (не удалось получить данные)");
                    return RedirectToAction("Login", "Account");
                }
                string SelPlaceName = PlaceFoName.Name;

                //await VMinit(user);

                viewModel.usersList = await db.Users.Where(u => u.DepartmentId == user.DepartmentId).ToListAsync();
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
        public async Task<IActionResult> CarSizs(DepartAdminViewModel viewModel) //список СИЗ выбранного автомобиля
        {
            User? user = await db.Users.FirstOrDefaultAsync(u => u.Login == User.Identity.Name);
            if (user != null)
            {
                
                // формируем список СИЗ автомобиля 
                viewModel.alarmSizsList.Clear();
                viewModel.alarmSizsList = await db.Sizs.Where(s => s.CarId == viewModel.CarId).ToListAsync();
                var CarFoName = await db.Cars.FirstOrDefaultAsync(u => u.Id == viewModel.CarId);
                if (CarFoName == null)
                {
                    ModelState.AddModelError("", "Обрыв связи с базой данных (не удалось получить данные)");
                    return RedirectToAction("Login", "Account");
                }
                string SelCarName = CarFoName.CarNomber;

                //await VMinit(user);

                viewModel.usersList = await db.Users.Where(u => u.DepartmentId == user.DepartmentId).ToListAsync();
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
        public async Task<IActionResult> CheckDateSizs(DepartAdminViewModel viewModel) //список СИЗ с просроченной датой поверки
        {
            User? user = await db.Users.FirstOrDefaultAsync(u => u.Login == User.Identity.Name);
            if (user != null)
            {

                // формируем список СИЗ  с просроченной датой поверки
                viewModel.alarmSizsList.Clear();
                viewModel.alarmSizsList = await db.Sizs.Where(s => s.DepartmentId == user.DepartmentId
                                                            && s.NextCheckDate <= viewModel.CheckDate).ToListAsync();
                var DateProm = viewModel.CheckDate;

                //await VMinit(user);

                viewModel.usersList = await db.Users.Where(u => u.DepartmentId == user.DepartmentId).ToListAsync();
                viewModel.sizsList = await db.Sizs.Where(s => s.DepartmentId == user.DepartmentId).ToListAsync();
                viewModel.carsList = await db.Cars.Where(c => c.DepartmentId == user.DepartmentId).ToListAsync();
                viewModel.placesList = await db.Places.Where(p => p.DepartmentId == user.DepartmentId).ToListAsync();

                viewModel.CheckDate = DateProm;
                ViewData["Title"] = $"Список СИЗ с истекающим сроком поверки: на дату {viewModel.CheckDate.Date.ToShortDateString()} ";

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
        public async Task<IActionResult> SelectedSiz(DepartAdminViewModel viewModel) //данные выбранного СИЗ
        {
            User? user = await db.Users.FirstOrDefaultAsync(u => u.Login == User.Identity.Name);
            if (user != null)
            {               
                //await VMinit(user);

                viewModel.alarmSizsList.Clear();
                // получаем выбранный СИЗ из БД                
                Siz? prSiz = await db.Sizs.FirstOrDefaultAsync(s => s.Id == viewModel.SizId);
                if (prSiz != null)
                {                    
                    viewModel.alarmSizsList.Add(prSiz);// заполняем "список" к показу - один элемент 
                    // определяем , куда распределён СИЗ

                    await SizBelongTo(prSiz);
                    
                }

                viewModel.usersList = await db.Users.Where(u => u.DepartmentId == user.DepartmentId).ToListAsync();
                viewModel.sizsList = await db.Sizs.Where(s => s.DepartmentId == user.DepartmentId).ToListAsync();
                viewModel.carsList = await db.Cars.Where(c => c.DepartmentId == user.DepartmentId).ToListAsync();
                viewModel.placesList = await db.Places.Where(p => p.DepartmentId == user.DepartmentId).ToListAsync();
                viewModel.CheckDate = DateTime.Today;

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
        public async Task<IActionResult> SizsUserIdChange(DepartAdminViewModel viewModel) // передать СИЗ работнику
        {
                    viewModel.PlaceId = 0;
                    viewModel.CarId = 0;

            if (viewModel.UserId > 0) 
            {                
                Siz? prSiz = await db.Sizs.FirstOrDefaultAsync(s => s.Id == viewModel.SizId);
                if (prSiz != null )
                {
                    prSiz.UserId = viewModel.UserId;// выбранный в селекте
                    prSiz.PlaceId = 0;
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "deptadmin")]

        public async Task<IActionResult> SizsPlaceIdChange(DepartAdminViewModel viewModel)  // передать СИЗ на склад
        {
            if (viewModel.PlaceId > 0)
            {
                    viewModel.UserId = 0;
                    viewModel.CarId = 0;
                Siz? prSiz = await db.Sizs.FirstOrDefaultAsync(s => s.Id == viewModel.SizId);
                if (prSiz != null)
                {
                    prSiz.PlaceId = viewModel.PlaceId; // выбранный в селекте
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


        public async Task<IActionResult> SizsBelongDelete(DepartAdminViewModel viewModel) // изъять СИЗ  
        {
            if (viewModel.SizId > 0)
            {
                viewModel.CarId = 0;
                viewModel.UserId = 0;
                viewModel.PlaceId = 0;
                Siz? prSiz = await db.Sizs.FirstOrDefaultAsync(s => s.Id == viewModel.SizId);
                if (prSiz != null)
                {
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
