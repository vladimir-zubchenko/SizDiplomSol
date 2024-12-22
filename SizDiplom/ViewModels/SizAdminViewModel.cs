using SizDiplom.Models;
using System.ComponentModel.DataAnnotations;

namespace SizDiplom.ViewModels
{
    public class SizAdminViewModel
    {

        public List<Siz> SizsList { get; set; } = new List<Siz>();//список  СИЗ РЕЗЕРВ
        public List<Siz> AlarmSizsList { get; set; } = new List<Siz>(); // список СИЗ (по требованию задачи)

        public Siz? Siz { get; set; } // выбранный из БД СИЗ 


        [Display(Name = "Выберите Дату следующей поверки")]
        [DataType(DataType.Date)]
        public DateTime CheckDate { get; set; } = DateTime.Today;  // необходимо для формирования заново Списков модели по дате

        
        [Display(Name = "Выберите СИЗ")]
        public int SizId { get; set; }  // для просмотра и редактирования данных СИЗ


        [Display(Name = "Введите инвентарный номер СИЗ")]
        public string SizName { get; set; } = string.Empty;  // инвентарный номер СИЗ для поиска в БД      

    }
}
