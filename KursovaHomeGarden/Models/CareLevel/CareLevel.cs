using System.ComponentModel.DataAnnotations;

namespace KursovaHomeGarden.Models.CareLevel
{
    public class CareLevel
    {
        [Key]
        public int care_level_id { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [Display(Name = "Name")]
        public string level_name { get; set; }
    }
}

