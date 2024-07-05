using System.ComponentModel.DataAnnotations;

namespace ReadingList.Models
{
    public class SourceBindingTarget
    {
        [Required(ErrorMessage = "Please enter the source's name."), StringLength(256)]
        public required string Name { get; set; }

        public Source ToSource() => new() { Name = Name };
    }
}
