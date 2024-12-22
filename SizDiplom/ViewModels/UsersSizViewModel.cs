using SizDiplom.Models;
// для вывода списка СИЗ простых пользователей
namespace SizDiplom.ViewModels
{
    public class UsersSizViewModel
    {
        public UsersSizViewModel(User user, List<Siz> sizsList)
        {
            User = user;
            SizsList = sizsList;
        }

        public User User {  get; set; } // данные пользователя
        public List<Siz> SizsList { get; set; } // список СИЗ пользователя

        
    }
}
