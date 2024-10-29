using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Task_Management_System.Controllers
{
    [Authorize(Roles = "Client")]

    public class ClientController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
