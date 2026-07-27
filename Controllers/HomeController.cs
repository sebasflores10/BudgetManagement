using BudgetManagement.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace BudgetManagement.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }


        /// <summary>
        /// Método 'NotFound'
        /// Permite mostrar al usuario la vista de error 404
        /// (Udemy): Actualizando Tipos Cuentas
        /// </summary>
        public IActionResult NotFound()
        {
            return View();
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
