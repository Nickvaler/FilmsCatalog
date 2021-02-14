using System.ComponentModel.DataAnnotations;

namespace FilmsCatalog.Database
{
    public class Poster
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Path { get; set; }
    }
}
