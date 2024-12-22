using SizDiplom.Models;
using System.Collections.Generic;

namespace SizDiplom.ViewModels
{
    public class AdminViewModel
    {
        public int UserId { get; set; }
        public int DepartmentId { get; set; }
        
        public List<Department> Departments { get; set; } = new List<Department>();
        public List<User> Users { get; set; } = new List<User>();
    }
}
