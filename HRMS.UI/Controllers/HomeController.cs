using Microsoft.AspNetCore.Mvc;

namespace HRMS.UI.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
