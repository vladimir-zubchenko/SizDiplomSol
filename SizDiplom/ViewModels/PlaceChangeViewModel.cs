using System.ComponentModel.DataAnnotations;

namespace SizDiplom.ViewModels
{
    public class PlaceChangeViewModel
    {
        public int Id { get; set; }

        public int DepartmentId { get; set; }      

        [Display(Name = "Введите название склада")]
        [StringLength(30, ErrorMessage = "длина названия от 4 до 30 символов", MinimumLength = 4)]
        [Required(ErrorMessage = "Не указано название склада")]
        public string Name { get; set; } = string.Empty;


        [Display(Name = "Добавьте описание склада")]
        [StringLength(30, ErrorMessage = "длина описания до 30 символов")]        
        public string Description { get; set; } = string.Empty;

        
    }
}
