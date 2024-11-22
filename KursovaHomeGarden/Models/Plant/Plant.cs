using Microsoft.Identity.Client.Cache;
using KursovaHomeGarden.Models.Category;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace KursovaHomeGarden.Models.Plant
{
    public class Plant
    {
        [Key]
        public int plant_id { get; set; }

        [Required]
        [Display(Name = "Name")]
        public string name { get; set; }

        [Display(Name = "Description")]
        public string description { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Price cannot be less than 0.")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal price { get; set; }

        public string? img { get; set; }

        [Required]
        public int category_id { get; set; }

        public Category.Category? Category { get; set; }

        [Required]
        public int care_level_id { get; set; }

        public CareLevel.CareLevel? CareLevel { get; set; }
    }


}
