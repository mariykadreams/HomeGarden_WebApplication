using System.ComponentModel.DataAnnotations;
using KursovaHomeGarden.Models;

namespace KursovaHomeGarden.Models
{
    public class Fertilize
    {
        [Key]
        public int Fert_type_id { get; set; }

        [Required]
        public string type_name { get; set; }

        [Required]
        public string units { get; set; }

        public string? note { get; set; }


    }
}
