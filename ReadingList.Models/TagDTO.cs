using System.ComponentModel.DataAnnotations;

namespace ReadingList.Models
{
    public class TagDTO
    {
        public long Id { get; set; }

        [Required(ErrorMessage = "Please enter the tag value."), StringLength(256)]
        public string? Data { get; set; }

        public string? Books { get; set; }
    }
}
