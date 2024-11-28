using System.ComponentModel.DataAnnotations;

namespace KursovaHomeGarden.Models
{
    public class SunlightRequirement
    {
        [Key]
        public int sunlight_requirements_id { get; set; }

        [Required]
        [Display(Name = "Light Intensity")]
        public string light_intensity { get; set; }

        [Required]
        [Display(Name = "Hours Per Day")]
        [Range(0, 24, ErrorMessage = "Hours must be between 0 and 24")]
        public int hours_per_day { get; set; }

        [Display(Name = "Notes")]
        public string? notes { get; set; }

        public ICollection<Plant.Plant>? Plants { get; set; }
    }
}
