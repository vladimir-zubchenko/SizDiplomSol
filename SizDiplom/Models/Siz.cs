namespace SizDiplom.Models
{
    public class Siz
    {
        public Siz()
        {
            PlaceId = 0;
            CarId = 0;
            UserId = 0;
            NextCheckDate = DateTime.Now;
        }

        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string TabNom { get; set; } = string.Empty;
        public int DepartmentId { get; set; } = 0;
        public DateTime NextCheckDate { get; set; }
        public int PlaceId { get; set; }= 0;
        public int CarId { get; set; } = 0;
        public int UserId { get; set; } = 0;

    }
}
