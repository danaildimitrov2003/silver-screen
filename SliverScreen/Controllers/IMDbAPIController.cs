﻿using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SliverScreen.Controllers
{
    public class IMDbAPIController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
