using ReadingList.Models.Validation;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ReadingList.Models
{
    public class ReadBindingTarget
    {
        [Required(ErrorMessage = "Please enter the date that the book was read.")]
        [ReadDateRange(ErrorMessage = "Please enter a date between 100 years ago and tomorrow.")]
        public DateOnly ReadDate { get; set; } = DateOnly.FromDateTime(DateTime.Now);

        [DisplayName("Title")]
        [Required(ErrorMessage = "Please enter the book's title."), StringLength(256)]
        public required string Name { get; set; }

        [ISBNRange(ErrorMessage = "ISBN must be either 10 or 13 characters (if any).")]
        public string? ISBN { get; set; }

        [StringLength(256)]
        public string? Author { get; set; }

        [StringLength(256)]
        public string? Tags { get; set; }

        [Range(1, 200, ErrorMessage = "Sequence must be between 1 and 200 (if any).")]
        public int? Sequence { get; set; }

        [Range(0, 5, ErrorMessage = "Rating must be between 0 and 5.")]
        public int Rating { get; set; } = 0;

        [StringLength(256)]
        public string? Source { get; set; }

        [DataType(DataType.ImageUrl), StringLength(512), Url(ErrorMessage = "Please enter the fully-qualified URL of the book's cover image.")]
        public string? ImageUrl { get; set; }

        public bool Recommend { get; set; } = false;


        public Book ToBook()
        {
            Book b = new()
            {
                Name = Name,
                Sequence = Sequence,
                Rating = Rating,
                Recommend = Recommend,
                ISBN = ISBN,
                ImageUrl = ImageUrl ?? Book.DefaultCoverImageUrl,
            };
            b.BookReadDates = [new() { Book = b, ReadDate = ReadDate }];
            return b;
        }
    }
}
