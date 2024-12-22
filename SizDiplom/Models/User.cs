namespace SizDiplom.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Login { get; set; } = string.Empty;
        public string TabNom { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public int DepartmentId { get; set; } = 0;
        public string Role { get; set; } = string.Empty;
        
    }
}
