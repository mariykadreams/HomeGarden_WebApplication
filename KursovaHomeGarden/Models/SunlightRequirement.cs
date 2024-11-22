using System.ComponentModel.DataAnnotations;

namespace KursovaHomeGarden.Models
{
    public class SunlightRequirement
    {
        [Key]
        public int sunlight_requirements_id { get; set; }

        [Required]
        public string light_intensity { get; set; }

        [Required]
        public int hours_per_day { get; set; }

        public string? notes { get; set; }
    }
}
