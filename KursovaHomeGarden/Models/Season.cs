using System.ComponentModel.DataAnnotations;

namespace KursovaHomeGarden.Models
{
    public class Season
    {
        [Key]
        public int season_id { get; set; }

        [Required]
        public string season_name { get; set; }

        [Required]
        public DateTime season_start { get; set; }

        [Required]
        public DateTime season_end { get; set; }

        [Required]
        public decimal temperature_range_min { get; set; }

        [Required]
        public decimal temperature_range_max { get; set; }
    }

}
