using System.ComponentModel.DataAnnotations;

namespace ReadingList.Models
{
    public class SourceDTO
    {
        public long Id { get; set; }

        [Required(ErrorMessage = "Please enter the source's name."), StringLength(256)]
        public string? Name { get; set; }

        public string? Books { get; set; }
    }
}
