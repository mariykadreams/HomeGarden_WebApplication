using System.ComponentModel.DataAnnotations;

namespace KursovaHomeGarden.Models
{
    public class ActionType
    {
        [Key]
        public int action_type_id { get; set; }

        [Required]
        public string type_name { get; set; }
    }
}
