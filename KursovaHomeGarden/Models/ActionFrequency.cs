using System.ComponentModel.DataAnnotations;

namespace KursovaHomeGarden.Models
{
    public class ActionFrequency
    {
        [Key]
        public int Action_frequency_id { get; set; }

        [Required]
        public string Interval { get; set; }

        [Required]
        public decimal volume { get; set; }

        public string? notes { get; set; }
    }

}
