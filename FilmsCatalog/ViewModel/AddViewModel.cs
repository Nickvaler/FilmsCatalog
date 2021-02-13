using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
namespace FilmsCatalog.ViewModel
{
    public class AddViewModel
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public int YearOfIssue { get; set; }

        [Required]
        public string Director { get; set; }
        public IFormFile Poster { get; set; }
    }
}
