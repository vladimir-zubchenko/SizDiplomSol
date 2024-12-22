using SizDiplom.Models;
using System.ComponentModel.DataAnnotations;

namespace SizDiplom.ViewModels
{
    public class RegisterModel
    {
        [Display(Name = "Введите логин (ФИО)")]
        [StringLength(30, ErrorMessage = "длина логина от 4 до 30 символов", MinimumLength = 4)]
        [Required(ErrorMessage = "Не указан логин")]
        public string Login { get; set; }

        [Display(Name ="Укажите Ваш табельный номер")]
        [StringLength(6, ErrorMessage = "длина Табельного номера от 4 до 6 символов", MinimumLength = 4)]
        [Required(ErrorMessage = "Не указан Табельный номер")]
        public string TabNom { get; set; }

        public List<Department> Departments { get; set; }=new List<Department>();

        public List<string> Roles { get; set; } =new List<string>();
        public string RoleName { get; set; } = string.Empty;
        public int DepartmentId { get; set; }
        public string ControllerName { get; set; } = string.Empty;

        [Display(Name = "Укажите пароль")]
        [Required(ErrorMessage = "Не указан пароль")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Повторите пароль")]
        [Required(ErrorMessage = "Не указан пароль")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Пароль введен неверно")]
        public string ConfirmPassword { get; set; }
    }
}
