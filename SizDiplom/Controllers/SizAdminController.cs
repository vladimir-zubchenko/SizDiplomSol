using Microsoft.AspNetCore.Mvc;
using SizDiplom.Models;

namespace SizDiplom.Controllers
{
    public class SizAdminController : Controller
    {
        private ProgDBaseContext db;

        public SizAdminController(ProgDBaseContext dbcontext )
        {
            db = dbcontext;
        }
        
        public IActionResult Index()
        {
            return View();
        }
    }
}
