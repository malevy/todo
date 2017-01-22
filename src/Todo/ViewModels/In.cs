using System.ComponentModel.DataAnnotations;

namespace Todo.ViewModels.In
{
    public class TodoVM
    {
        [Required]
        public string Description { get; set; }
        public string Important { get; set; }
    }
}