using System.ComponentModel.DataAnnotations;
using KursovaHomeGarden.Models.Plant;

namespace KursovaHomeGarden.Models
{
    // Models/ActionFrequency.cs
    public class ActionFrequency
    {
        [Key]
        public int Action_frequency_id { get; set; }

        [Required]
        public string Interval { get; set; }

        public decimal? volume { get; set; }

        public string? notes { get; set; }

        public int plant_id { get; set; }
        public KursovaHomeGarden.Models.Plant.Plant? Plant { get; set; }

        public int season_id { get; set; }
        public Season? Season { get; set; }

        public int action_type_id { get; set; }
        public ActionType? ActionType { get; set; }

        public int? Fert_type_id { get; set; }
        public Fertilize? Fertilize { get; set; }
    }

}


