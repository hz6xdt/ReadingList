using System.ComponentModel.DataAnnotations;

namespace ReadingList.Models
{
    public class AuthorBindingTarget
    {
        [Required(ErrorMessage = "Please enter the author's name in Last, First format."), StringLength(256)]
        public required string Name { get; set; }

        public Author ToAuthor() => new Author { Name = Name };
    }
}
