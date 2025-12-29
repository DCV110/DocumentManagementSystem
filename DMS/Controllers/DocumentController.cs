using Microsoft.AspNetCore.Mvc;

namespace DMS.Controllers
{
    public class DocumentController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
