using System.ComponentModel.DataAnnotations;

namespace SizDiplom.ViewModels
{
    public class LoginModel
    {
        [Display(Name = "Введите логин")]
        [Required(ErrorMessage = "Не указан Login")]
        [StringLength(30, ErrorMessage = "длина логина от 4 до 30 символов", MinimumLength = 4)]
        public string Login { get; set; }

        [Required(ErrorMessage = "Не указан пароль")]        
        [StringLength(30, ErrorMessage = "длина пароля от 4 до 30 символов", MinimumLength = 4)]
        [DataType(DataType.Password)]
        public string Password { get; set; }

    }
}

