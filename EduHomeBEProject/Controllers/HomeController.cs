﻿using EduHomeBEProject.DAL;
using EduHomeBEProject.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace EduHomeBEProject.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;
        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            HomeVM homeVM = new HomeVM()
            {
                Sliders = _context.Sliders.ToList(),
                Settings = _context.Settings.FirstOrDefault(),
                Categories=_context.Categories.ToList(),
                //Courses = _context.Courses.Include(c => c.Category).Include(c => c.CourseTags).ThenInclude(ct => ct.Tag).ToList()
                Courses =_context.Courses.Include(c=>c.CourseTags).ThenInclude(ct=>ct.Tag).ToList(),
                Events=_context.Events.ToList()
            };
            return View(homeVM);
        }

    }
}
