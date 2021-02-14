using FilmsCatalog.Data;
using FilmsCatalog.Models;
using FilmsCatalog.ViewModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Server;
using Microsoft.AspNetCore.Hosting;

namespace FilmsCatalog.Controllers
{
    public class HomeController : Controller
    {
        private ApplicationDbContext _db;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<HomeController> _logger;
        private IWebHostEnvironment _appEnvironment;

        public HomeController(ILogger<HomeController> logger,
                              ApplicationDbContext context,
                              IWebHostEnvironment webHostEnvironment,
                              UserManager<User> userManager)
        {
            _logger = logger;
            _db = context;
            _userManager = userManager;
            _appEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            int pageSize = 8;
            var movies = await _db.Movies.Include(x=>x.Poster).Skip((page-1)*pageSize).Take(pageSize).ToListAsync();
            foreach (var movie in movies)
            {
                movie.Poster.Path = CheckPoster(movie);
            }
            var moviesCount = await _db.Movies.CountAsync();
            IndexViewModel model = new IndexViewModel();
            model.Movies = movies;
            model.PageViewModel = new PageViewModel(moviesCount, page, pageSize);
            return View(model);
        }

        private string CheckPoster(Movie movie)
        {
            if (System.IO.File.Exists(_appEnvironment.WebRootPath + movie.Poster.Path))
            {
                return movie.Poster.Path;
            }
            else
            {
                return "/Files/NotFound.jpeg";
            }
        }

        public async Task<IActionResult> Info(int id)
        {
            var movie = _db.Movies.Include(x => x.Poster).Where(x => x.Id == id).FirstOrDefault();
            InfoVIewModel info = new InfoVIewModel();
            info.Movie = movie;
            var user = await _userManager.GetUserAsync(User);

            if (movie != null && user?.Id == movie.UserId)
            {
                movie.Poster.Path = CheckPoster(movie);
                info.CanEdit = true;
            }
            return View(info);
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
