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
        public string name { get; set; }
        public string description { get; set; }
        public decimal price { get; set; }
        public string img { get; set; }

        public int category_id { get; set; }
        [ForeignKey("category_id")]
        public Category.Category Category { get; set; }

        public int care_level_id { get; set; }
        [ForeignKey("care_level_id")]
        public CareLevel.CareLevel CareLevel { get; set; }
    }
}
