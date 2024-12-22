using Microsoft.EntityFrameworkCore;

using System.Drawing.Text;

namespace SizDiplom.Models
{
    public class ProgDBaseContext:DbContext
    {
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Siz> Sizs { get; set; } = null!;
        public DbSet<Department> Departments { get; set; } = null!;
        public DbSet<Car> Cars { get; set; } = null!;
        public DbSet<Place> Places { get; set; } = null!;

        public ProgDBaseContext(DbContextOptions<ProgDBaseContext> options) : base(options)
        {
            //Database.EnsureDeleted();   // удаляем бд со старой схемой
            Database.EnsureCreated();   // создаем бд с новой схемой
        }
        public ProgDBaseContext()
        {
            //Database.EnsureDeleted();   // удаляем бд со старой схемой
            Database.EnsureCreated();   // создаем бд с новой схемой

        }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            List<Siz> listS = new();
            List<User> listU = new();
            List<Place> listP = new();
            List<Car> listC = new();
            List<Department> listD = new();

            int d = 3;
            int s = 150;
            int u = 24;
            int p = 9;
            int c = 8;
            Random rnd = new Random();


            // DepartmentsList
            for (int i = 1; i <= d; i++)
            {
                listD.Add(new Department { Id = i, Name = $"Department {i}" });
            }
            modelBuilder.Entity<Department>().HasData(listD);

            //CarsList
            for (int i = 1; i <= c; i++)
            {
                listC.Add(new Car { Id = i, CarNomber = $"Nom {rnd.Next(1000, 9999)}", Description = $"AVTO {i}", DepartmentId = rnd.Next(1, d+1) });
            }
            modelBuilder.Entity<Car>().HasData(listC);

            // PlaceList
            for (int i = 1; i <= p; i++)
            {
                listP.Add(new Place { Id = i, Name = $"Склад {i}", Description = $"Описание склада {1}", DepartmentId = rnd.Next(1, d+1) });
            }
            modelBuilder.Entity<Place>().HasData(listP);

            // UsersList
            listU.Add(new User { Id = 1, Login = "Admin", TabNom = "1001", Password = "passA", Role = "admin", DepartmentId = 1 });
            listU.Add(new User { Id = 2, Login = "DeptAdmin1", TabNom = "1002", Password = "passD", Role = "deptadmin", DepartmentId = 1 });
            listU.Add(new User { Id = 3, Login = "DeptAdmin2", TabNom = "1003", Password = "passD", Role = "deptadmin", DepartmentId = 2 });
            listU.Add(new User { Id = 4, Login = "DeptAdmin3", TabNom = "1004", Password = "passD", Role = "deptadmin", DepartmentId = 3 });
            for (int i = 5; i <= u; i++)
            {
                listU.Add(new User
                {
                    Id = i,
                    Login = $"User{i-4}",
                    Password = "passU",
                    TabNom = $"TabNom {1000 + i}",
                    Role = "user",
                    DepartmentId = rnd.Next(1, d + 1)
                });
            }
            modelBuilder.Entity<User>().HasData(listU);

            // SizsList
            for (int i = 1; i <= s; i++)
            {
                listS.Add(new Siz
                {
                    Id = i,
                    Name = $"SizName N {rnd.Next(1, 15)}",
                    TabNom = $"siz TabNom {10000 + i}",
                    NextCheckDate = DateTime.Today.AddDays(rnd.Next(5, 360)),
                    DepartmentId = rnd.Next(1, d + 1),
                    CarId = 0,
                    PlaceId = 0,
                    UserId = 0
                });
            }
            modelBuilder.Entity<Siz>().HasData(listS);

        }       

    }
}
