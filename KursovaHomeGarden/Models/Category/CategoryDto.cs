using System.ComponentModel.DataAnnotations;

namespace KursovaHomeGarden.Models.Category
{
    public class CategoryDto
    {
        [Required(ErrorMessage = "Name is required")]
        [Display(Name = "Name")]
        public string category_name { get; set; }
    }
}
