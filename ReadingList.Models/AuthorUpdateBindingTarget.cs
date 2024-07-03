using System.ComponentModel.DataAnnotations;
using ReadingList.Validation;

namespace ReadingList.Models
{
    public class AuthorUpdateBindingTarget
    {
        [Required]
        [PrimaryKey(DbContextType = typeof(DataContext), DataType = typeof(Author))]
        public long Id { get; set; }

        [Required(ErrorMessage = "Please enter the author's name in Last, First format."), StringLength(256)]
        public required string Name { get; set; }

        public Author ToAuthor() => new Author { AuthorId = Id, Name = Name };
    }
}
