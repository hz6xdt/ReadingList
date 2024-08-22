using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ReadingList.Models
{
    public class BookBindingTarget
    {
        [DisplayName("Title")]
        [Required(ErrorMessage = "Please enter the book's title."), StringLength(256)]
        public required string Name { get; set; }

        [Range(1, 200, ErrorMessage = "Sequence must be between 1 and 200 (if any).")]
        public int? Sequence { get; set; }

        [Range(0, 5, ErrorMessage = "Rating must be between 0 and 5.")]
        public int Rating { get; set; } = 0;

        public bool Recommend { get; set; } = false;

        [StringLength(16)]
        public string? ISBN { get; set; }

        [DataType(DataType.ImageUrl), StringLength(512)]
        public string ImageUrl { get; set; } = "http://2.bp.blogspot.com/_aDCnyPs488U/SyAtBDSHFHI/AAAAAAAAGDI/tFkGgFeISHI/s400/BookCoverGreenBrown.jpg";

        [StringLength(256)]
        public string? Author { get; set; }

        [StringLength(256)]
        public string? ReadDates { get; set; }

        [StringLength(256)]
        public string? Tags { get; set; }

        [StringLength(256)]
        public string? Source { get; set; }

        public Book ToBook() => new()
        {
            Name = Name,
            Sequence = Sequence,
            Recommend = Recommend,
            ISBN = ISBN,
            ImageUrl = ImageUrl
        };
    }
}
