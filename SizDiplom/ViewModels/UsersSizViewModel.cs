using SizDiplom.Models;
// для вывода списка СИЗ простых пользователей
namespace SizDiplom.ViewModels
{
    public class UsersSizViewModel
    {
        public UsersSizViewModel(User user, List<Siz> sizsList, List<Siz> alsizList)
        {
            User = user;
            SizsList = sizsList;
            AlarmSizList = alsizList;
        }

        public User User {  get; set; } // данные пользователя
        public List<Siz> SizsList { get; set; } // список СИЗ пользователя
        public List<Siz> AlarmSizList { get; set; } // список просроченных СИЗ пользователя

    }
}
