using SizDiplom.Models;
using System.ComponentModel.DataAnnotations;

namespace SizDiplom.ViewModels
{
    public class DepartmentViewModel
    {

        public int Id { get; set; }
        
        [Display(Name = "Введите название департамента здесь")]
        [Required(ErrorMessage = "Не указано название департамента")]
        [StringLength(30, ErrorMessage = "длина названия от 4 до 30 символов", MinimumLength = 4)]
        public string Name { get; set; } = string.Empty;
        public List<Department> Departments { get; set; }=new List<Department>();
    }
}
