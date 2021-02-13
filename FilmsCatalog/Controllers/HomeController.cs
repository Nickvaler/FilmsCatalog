using FilmsCatalog.Data;
using FilmsCatalog.Models;
using FilmsCatalog.ViewModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FilmsCatalog.Controllers
{
    public class HomeController : Controller
    {
        private ApplicationDbContext _db;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger, 
            ApplicationDbContext context,
            UserManager<User> userManager)
        {
            _logger = logger;
            _db = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            int pageSize = 8;
            var movies = await _db.Movies.Skip((page-1)*pageSize).Take(pageSize).ToListAsync();
            var moviesCount = await _db.Movies.CountAsync();
            IndexViewModel model = new IndexViewModel();
            model.Movies = movies;
            model.PageViewModel = new PageViewModel(moviesCount, page, pageSize);
            return View(model);
        }

        [HttpGet]
        public IActionResult Add()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Add(AddViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                byte[] imageData = null;
                if (model.Poster != null)
                {
                    using (var binaryReader = new BinaryReader(model.Poster.OpenReadStream()))
                    {
                        imageData = binaryReader.ReadBytes((int)model.Poster.Length);
                    }
                }
                _db.Movies.Add(new Movie()
                {
                    Name = model.Name,
                    Description = model.Description,
                    YearOfIssue = model.YearOfIssue,
                    Director = model.Director,
                    User = user,
                    Poster = imageData
                });
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            else
            {
                return View(model);
            }
            
        }

        public async Task<IActionResult> Info(int id)
        {
            var movie = _db.Movies.Where(x => x.Id == id).FirstOrDefault();
            InfoVIewModel info = new InfoVIewModel();
            info.Movie = movie;
            var user = await _userManager.GetUserAsync(User);

            if (movie != null && user.Id == movie.UserId)
            {
                info.CanEdit = true;
            }
            return View(info);
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var movie = _db.Movies.Where(x => x.Id == id).FirstOrDefault();
            return View(movie);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(AddViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                byte[] imageData = null;
                if (model.Poster != null)
                {
                    using (var binaryReader = new BinaryReader(model.Poster.OpenReadStream()))
                    {
                        imageData = binaryReader.ReadBytes((int)model.Poster.Length);
                    }
                }
                var movie = _db.Movies.Find(model.Id);
                movie.Name = model.Name;
                movie.Description = model.Description;
                movie.YearOfIssue = model.YearOfIssue;
                movie.Director = model.Director;
                movie.User = user;
                movie.Poster = imageData ?? movie.Poster;

                _db.Movies.Update(movie);
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            else
            {
                return View(model);
            }

        }
        public IActionResult Privacy()
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
