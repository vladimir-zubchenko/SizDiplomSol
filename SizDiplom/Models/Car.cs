namespace SizDiplom.Models
{
    public class Car
    {
        public int Id { get; set; }
        public string CarNomber { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int DepartmentId { get; set; } = 0;

    }
}
