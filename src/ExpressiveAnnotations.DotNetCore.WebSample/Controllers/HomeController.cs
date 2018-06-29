using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ExpressiveAnnotations.DotNetCore.WebSample.Models;

namespace ExpressiveAnnotations.DotNetCore.WebSample.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            var model = new PersonViewModel()
            {
                Id = 101,
                FirstName = "Brian",
                LastName = "Randall",
                Age = 5
            };

            ViewData["Message"] = "Test validation using form below.";

            return View(model);
        }

        [HttpPost]
        public IActionResult Update(PersonViewModel model)
        {
            ViewData["Message"] = ModelState.IsValid ? "Update successful!" : "Update failed!";

            return View("Index", model);
        }
        
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
