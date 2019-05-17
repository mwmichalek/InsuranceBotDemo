using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CDPHP.Bot.Survey {
    public class HomeController : Controller {

        public ActionResult Index() {
            return View();
        }
    }
}
