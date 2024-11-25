using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KursovaHomeGarden.Models
{
    public class Season
    {
        [Key]
        public int season_id { get; set; }

        [Required(ErrorMessage = "Season name is required")]
        public string season_name { get; set; }

        [Required(ErrorMessage = "Start date is required")]
        [DataType(DataType.Date)]
        public DateTime season_start { get; set; }

        [Required(ErrorMessage = "End date is required")]
        [DataType(DataType.Date)]
        public DateTime season_end { get; set; }

        [Required(ErrorMessage = "Minimum temperature is required")]
        [Range(-100, 100, ErrorMessage = "Temperature must be between -100°C and 100°C")]
        [Column(TypeName = "decimal(5,1)")]
        public decimal temperature_range_min { get; set; }

        [Required(ErrorMessage = "Maximum temperature is required")]
        [Range(-100, 100, ErrorMessage = "Temperature must be between -100°C and 100°C")]
        [Column(TypeName = "decimal(5,1)")]
        public decimal temperature_range_max { get; set; }
    }

}
