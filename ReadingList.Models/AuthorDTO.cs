using System.ComponentModel.DataAnnotations;

namespace ReadingList.Models
{
    public class AuthorDTO
    {
        public long Id { get; set; }

        [Required(ErrorMessage = "Please enter the author's name in Last, First format."), StringLength(256)]
        public required string Name { get; set; }

        public string? Books { get; set; }
    }
}
