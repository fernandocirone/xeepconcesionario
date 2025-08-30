using Microsoft.AspNetCore.Mvc;

namespace xeepconcesionario.Controllers
{
    public class EstadisticasController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
