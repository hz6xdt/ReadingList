using ReadingList.Models.Validation;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ReadingList.Models
{
    public class ReadBindingTarget
    {
        [Required(ErrorMessage = "Please enter the date that the book was read."), ReadDateRange]
        public DateTime ReadDate { get; set; } = DateTime.Now;

        [DisplayName("Title")]
        [Required(ErrorMessage = "Please enter the book's title."), StringLength(256)]
        public required string Name { get; set; }

        [StringLength(16)]
        public string? ISBN { get; set; }

        [StringLength(256)]
        public string? Author { get; set; }

        [StringLength(256)]
        public string? Tags { get; set; }

        [Range(1, 200, ErrorMessage = "Sequence must be between 1 and 200 (if any).")]
        public int? Sequence { get; set; }

        [StringLength(256)]
        public string? Source { get; set; }

        [DataType(DataType.ImageUrl), StringLength(512)]
        public string? ImageUrl { get; set; }

        public bool Recommend { get; set; } = false;


        public Book ToBook()
        {
            Book b = new()
            {
                Name = Name,
                Sequence = Sequence,
                Recommend = Recommend,
                ISBN = ISBN,
                ImageUrl = ImageUrl ?? Book.DefaultCoverImageUrl,
            };
            b.BookReadDates = [new() { Book = b, ReadDate = ReadDate }];
            return b;
        }
    }
}
