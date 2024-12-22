using SizDiplom.Models;
using System.ComponentModel.DataAnnotations;

namespace SizDiplom.ViewModels
{
    public class UserChangeViewModel
    {
        [Display(Name = "Введите логин")]
        [StringLength(30, ErrorMessage = "длина логина от 4 до 30 символов", MinimumLength = 4)]
        [Required(ErrorMessage = "Не указан логин")]
        public string Login { get; set; } = string.Empty;

        public int Id { get; set; }

        [Display(Name = "Укажите Ваш табельный номер")]
        [StringLength(6, ErrorMessage = "длина Табельного номера от 4 до 6 символов", MinimumLength = 4)]
        [Required(ErrorMessage = "Не указан Табельный номер")]
        public string TabNom { get; set; } = string.Empty;

        public string DepartmentName { get; set; } = string.Empty;

        
        public int DepartmentId { get; set; }

        [Display(Name = "Укажите пароль")]
        [Required(ErrorMessage = "Не указан пароль")]       
        [StringLength(30, ErrorMessage = "длина пароля от 4 до 30 символов", MinimumLength = 4)]
        [DataType(DataType.Password)]
        
        public string Password { get; set; } = string.Empty;

        public string Role { get; set; } = "user";

    }
}
