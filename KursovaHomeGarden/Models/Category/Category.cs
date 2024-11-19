using System.ComponentModel.DataAnnotations;

namespace KursovaHomeGarden.Models.Category
{
    public class Category
    {
        [Key]
        public int category_id { get; set; }
        public string category_name { get; set; }
    }
}
