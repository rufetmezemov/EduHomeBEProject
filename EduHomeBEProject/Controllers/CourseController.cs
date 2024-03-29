﻿using EduHomeBEProject.DAL;
using EduHomeBEProject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EduHomeBEProject.Controllers
{
    public class CourseController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public CourseController(AppDbContext context,UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        public IActionResult Index(int page = 1)
        {
            ViewBag.Categories = _context.Categories.ToList();
            ViewBag.CurrentPage = page;
            ViewBag.TotalPage = Math.Ceiling((decimal)_context.Courses.Count() / 3);
            List<Course> model = _context.Courses.Include(c => c.CourseTags).ThenInclude(ct => ct.Tag).Skip((page - 1) * 3).Take(3).ToList();
            //List<Course> model = _context.Courses.Include(c => c.CourseTags).ThenInclude(ct => ct.Tag).ToList();
            return View(model);
        }

        public IActionResult Search(string search)
        {
            List<Course> course = _context.Courses.Where(c => c.Name.ToLower().Trim().Contains(search.ToLower().Trim())).ToList();
            return PartialView("_CoursePartialView",course);
        }

        #region Search Old variant
        //[HttpPost]
        //public async Task<IActionResult> Index(string CrsSearch)
        //{
        //    ViewData["GetCoursedetails"] = CrsSearch;
        //    var crsqury = from x in _context.Courses select x;
        //    if (!String.IsNullOrEmpty(CrsSearch))
        //    {
        //        crsqury = crsqury.Where(x => x.Name.Contains(CrsSearch));
        //    }
        //    return View(await crsqury.AsNoTracking().ToListAsync());
        //}
        #endregion
        public IActionResult RelatedCourses(int id)
        {
            List<Course> courses = _context.Courses.Include(c => c.Category).Where(c => c.Category.Id == id).ToList();
            if (courses == null)
            {
                return NotFound();
            }
            return View(courses);
        }

        public IActionResult Details(int id)
        {
            ViewBag.Categories = _context.Categories.Include(c=>c.Courses).ToList();
            ViewBag.LatestBlogs = _context.Blogs.OrderBy(b=>b.PublishDate).Take(3).ToList();
            Course course = _context.Courses.Include(c => c.CourseTags).ThenInclude(ct => ct.Tag).Include(c=>c.Comments).ThenInclude(c=>c.AppUser).Include(c=>c.Category).ThenInclude(c=>c.Courses).FirstOrDefault(c => c.Id == id);
            if (course == null)
            {
                return NotFound();
            }
            //ViewBag.RelatedCourse = _context.Categories.FirstOrDefault(c => c.Id == categoryId);
            return View(course);
        }


        #region Comment
        [Authorize]
        [AutoValidateAntiforgeryToken]
        [HttpPost]
        public async Task<IActionResult> AddComment(Comment comment)
        {
            AppUser user = await _userManager.FindByNameAsync(User.Identity.Name);
            if (!ModelState.IsValid) return RedirectToAction("Details", "Course", new { id = comment.CourseId });
            if (!_context.Courses.Any(c => c.Id == comment.CourseId))
                return NotFound();
            Comment cmnt = new Comment
            {
                Text = comment.Text,
                Subject=comment.Subject,
                CourseId = comment.CourseId,
                WriteTime = DateTime.Now,
                AppUserId = user.Id
            };
            _context.Comments.Add(cmnt);
            _context.SaveChanges();
            return RedirectToAction("Details", "Course", new { id = comment.CourseId });
        }

        [Authorize]
        public async Task<IActionResult> DeleteComment(int id)
        {
            AppUser user = await _userManager.FindByNameAsync(User.Identity.Name);

            if (!ModelState.IsValid) return RedirectToAction("Details", "Course");
            if (!_context.Comments.Any(c => c.Id == id  && c.AppUserId == user.Id))
                return NotFound();
            Comment comment = _context.Comments.FirstOrDefault(c => c.Id == id && c.AppUserId == user.Id);
            _context.Comments.Remove(comment);
            _context.SaveChanges();
            return RedirectToAction("Details", "Course", new { id = comment.CourseId });
        }
        #endregion
    }
}
