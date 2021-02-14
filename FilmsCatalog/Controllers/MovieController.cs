using FilmsCatalog.Data;
using FilmsCatalog.Database;
using FilmsCatalog.Enums;
using FilmsCatalog.Helpers;
using FilmsCatalog.Models;
using FilmsCatalog.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FilmsCatalog.Controllers
{
    [Authorize]
    public class MovieController : Controller
    {
        private ApplicationDbContext _db;
        private IWebHostEnvironment _appEnvironment;
        private IConfiguration _configuration;
        private readonly UserManager<User> _userManager;
        private readonly int _fileSize;
        public MovieController(ApplicationDbContext context,
                               UserManager<User> userManager,
                               IWebHostEnvironment webHostEnvironment,
                               IConfiguration configuration)
        {
            _db = context;
            _userManager = userManager;
            _appEnvironment = webHostEnvironment;
            _configuration = configuration;
            _fileSize = _configuration.GetValue<int>("PosterSize");
        }

        [HttpGet]
        public async Task<IActionResult> AddEdit(PageInteractionType val, int id)
        {
            if (val == PageInteractionType.Add)
            {
                return View(new AddEditViewModel());
            }
            else if (val == PageInteractionType.Edit)
            {
                var movie = _db.Movies.Include(x => x.Poster).Where(x => x.Id == id).FirstOrDefault();

                if (movie is null)
                {
                    return View("NotFound");
                }
                else
                {
                    var user = await _userManager.GetUserAsync(User);
                    if (movie.UserId == user.Id)
                    {
                        byte[] imageData = null;
                        if (System.IO.File.Exists(_appEnvironment.WebRootPath + movie.Poster.Path))
                        {
                            imageData = await System.IO.File.ReadAllBytesAsync(_appEnvironment.WebRootPath + movie.Poster.Path);
                        }
                        else
                        {
                            imageData = await System.IO.File.ReadAllBytesAsync(_appEnvironment.WebRootPath + "/Files/NotFound.jpeg");
                        }
                        var addEditViewModel = new AddEditViewModel()
                        {
                            Name = movie.Name,
                            Description = movie.Description,
                            YearOfIssue = movie.YearOfIssue,
                            Director = movie.Director,
                            PosterArr = imageData,
                            IsEdit = true
                        };
                        return View(addEditViewModel);
                    }
                    else
                    {
                        return Forbid("Отказано в доступе.");
                    }
                }
            }
            else
            {
                return BadRequest();
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddEdit(AddEditViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);

            if (ModelState.IsValid && user != null)
            {
                Poster file = new Poster();
                byte[] imageData = null;
                if (model?.Poster != null)
                {
                    imageData = FormFileExtensions.GetImageByteArr(model.Poster);
                    if (FormFileExtensions.ValidateImageSize(model.Poster, _fileSize))
                    {
                        ModelState.AddModelError("Poster", $"Размер файла не должен превышать {_fileSize} МБ.");
                        return View(model);
                    }
                    if (FormFileExtensions.ValidateImageExtension(model.Poster))
                    {
                        ModelState.AddModelError("Poster", $"Неправильный тип файла.");
                        return View(model);
                    }
                    if (FormFileExtensions.ValidatePictureData(model.Poster))
                    {
                        ModelState.AddModelError("Poster", $"Не получается прочитать файл.");
                        return View(model);
                    }
                    string path = "/Files/" + Guid.NewGuid() + model.Poster.FileName;
                    using (var fileStream = new FileStream(_appEnvironment.WebRootPath + path, FileMode.Create))
                    {
                        await model.Poster.CopyToAsync(fileStream);
                    }
                    file.Name = model.Poster.FileName;
                    file.Path = path;

                }
                else
                {
                    string name = "NotFound.jpeg";
                    string path = $"{_appEnvironment.WebRootPath}/Files/{name}";
                    if (System.IO.File.Exists(path))
                    {
                        file.Name = name;
                        file.Path = $"/Files/{name}";
                    }
                    else
                    {
                        file.Name = string.Empty;
                        file.Path = string.Empty;
                    }
                }

                if (model.IsEdit)
                {
                    var movie = _db.Movies.Include(x => x.Poster).FirstOrDefault(x=>x.Id == model.Id);
                    if (movie is null)
                    {
                        return BadRequest();
                    }
                    else if (movie.User.Id == user.Id)
                    {
                        movie.Name = model.Name;
                        movie.Description = model.Description;
                        movie.YearOfIssue = model.YearOfIssue;
                        movie.Director = model.Director;
                        movie.User = user;
                        if (!string.IsNullOrEmpty(file.Name) && file.Name != "NotFound.jpeg")
                        {
                            if (System.IO.File.Exists(_appEnvironment.WebRootPath + movie.Poster.Path))
                            {
                                System.IO.File.Delete(_appEnvironment.WebRootPath + movie.Poster.Path);
                            }
                            movie.Poster = file;
                        }
                        _db.Movies.Update(movie);
                    }
                    else
                    {
                        return Forbid("Отказано в доступе.");
                    }
                }
                else
                {
                    _db.Movies.Add(new Movie()
                    {
                        Name = model.Name,
                        Description = model.Description,
                        YearOfIssue = model.YearOfIssue,
                        Director = model.Director,
                        User = user,
                        Poster = file
                    });
                }
                await _db.SaveChangesAsync();
                return RedirectToAction("Index", "Home");
            }
            else
            {
                return View(model);
            }

        }

    }
}
