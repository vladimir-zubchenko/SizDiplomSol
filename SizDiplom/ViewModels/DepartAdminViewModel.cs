using SizDiplom.Models;
using System.ComponentModel.DataAnnotations;

namespace SizDiplom.ViewModels
{
    public class DepartAdminViewModel
    {
        
        public List<Siz> sizsList { get; set; } = new List<Siz>();//список  СИЗ депертамента
        public List<Siz> alarmSizsList { get; set; } = new List<Siz>(); // произвольный список СИЗ департамента (по требованию задачи)
        public List<User> usersList { get; set; } = new List<User>(); // список работников департамента
        public List<Car> carsList { get; set; } = new List<Car>(); // список автомобилей департамента
        public List<Place> placesList { get; set; } = new List<Place>(); //  список складов департамента      
            

        [Required(ErrorMessage = "Не выбрана дата")]
        [Display(Name = "Выберите Дату следующей поверки")]
        [DataType(DataType.Date)]        
        public DateTime CheckDate { get; set; } = DateTime.Today;  // необходимо для формирования заново Списков модели по дате

        [Required(ErrorMessage = "Не выбран СИЗ")]        
        [Display(Name = "Выберите СИЗ")]
        public int SizId { get; set; }                  // для просмотра и редактирования данных СИЗ

        [Required(ErrorMessage = "Не выбран работник")]
        [Display(Name = "Выберите работника")]
        public int UserId { get; set; }     // необходимо для формирования заново Списков модели по выбранному работнику

        [Display(Name = "Выберите автомобиль")]
        [Required(ErrorMessage = "Не выбран автомобиль")]
        public int CarId { get; set; }      // необходимо для формирования заново Списков модели по выбранному автомобилю

        [Display(Name = "Выберите склад")]
        [Required(ErrorMessage = "Не выбран склад")]
        public int PlaceId { get; set; }        // необходимо для формирования заново Списков модели по выбранному складу

    }
}
