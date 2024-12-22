using System.ComponentModel.DataAnnotations;

namespace SizDiplom.ViewModels
{
    public class CarChangeViewModel
    {
        public int Id { get; set; }

        public int DepartmentId { get; set; }

        [Display(Name = "Гос номер машины")]
        [StringLength(30, ErrorMessage = "длина от 4 до 30 символов", MinimumLength = 4)]
        [Required(ErrorMessage = "Не указан Гос номер машины")]
        public string CarNomber { get; set; } = "Гос номер машины";


        [Display(Name = "Добавьте описание машины")]
        [StringLength(30, ErrorMessage = "длина описания до 30 символов")]
        public string Description { get; set; } = string.Empty;
    }
}
