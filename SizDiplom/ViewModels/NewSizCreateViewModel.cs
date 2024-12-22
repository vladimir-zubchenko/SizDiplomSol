using System.ComponentModel.DataAnnotations;

namespace SizDiplom.ViewModels
{
    public class NewSizCreateViewModel
    {
        [Display(Name = "Введите Название СИЗ")]
        [StringLength(80, ErrorMessage = "длина логина от 4 до 80 символов", MinimumLength = 4)]
        [Required(ErrorMessage = "Не указано Название СИЗ")]
        public string Name { get; set; } = "Название СИЗ";


        [Display(Name = "Введите Инв Номер СИЗ")]
        [StringLength(10, ErrorMessage = "длина логина от 4 до 10 символов", MinimumLength = 4)]
        [Required(ErrorMessage = "Не указан Инв Номер СИЗ")]
        public string TabNom { get; set; } = "Инв Номер СИЗ";



        [Required(ErrorMessage = "Не выбрана дата")]
        [Display(Name = "Выберите Дату следующей поверки")]
        [DataType(DataType.Date)]
        public DateTime CheckDate { get; set; } = DateTime.Today;

    }
}
