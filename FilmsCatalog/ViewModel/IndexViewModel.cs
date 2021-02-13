using FilmsCatalog.Models;
using System.Collections.Generic;

namespace FilmsCatalog.ViewModel
{
    public class IndexViewModel
    {
        public IEnumerable<Movie> Movies{ get; set; }
        public PageViewModel PageViewModel { get; set; }
    }
}
