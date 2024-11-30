using KursovaHomeGarden.Areas.Identity.Data;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace KursovaHomeGarden.Models
{
    public class UserPlant
    {
        [Key]
        public int user_plant_id { get; set; }

        [Required]
        public DateTime purchase_date { get; set; }

        [Required]
        public int plant_id { get; set; }

        [ForeignKey("plant_id")]
        public Plant.Plant? Plant { get; set; }

        [Required]
        public string user_id { get; set; }

        [ForeignKey("user_id")]
        public ApplicationUser? User { get; set; }
    }
}
