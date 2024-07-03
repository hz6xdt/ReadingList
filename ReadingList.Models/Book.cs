using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ReadingList.Models
{
    public class Book
    {
        public const string DefaultCoverImageUrl = "http://2.bp.blogspot.com/_aDCnyPs488U/SyAtBDSHFHI/AAAAAAAAGDI/tFkGgFeISHI/s400/BookCoverGreenBrown.jpg";

        public long BookId { get; set; }

        [DisplayName("Title")]
        public required string Name { get; set; }
        public int? Sequence { get; set; }
        public bool Recommend { get; set; } = false;
        public string? ISBN { get; set; }
        [Url]
        public string ImageUrl { get; set; } = DefaultCoverImageUrl;

        public long? AuthorId { get; set; }
        public Author? Author { get; set; }

        public long? SourceId { get; set; }
        public Source? Source { get; set; }

        public IEnumerable<BookReadDate>? BookReadDates { get; set; }
        public IEnumerable<BookTag>? BookTags { get; set; }

        public BookDTO ToBookDTO()
        {
            return new BookDTO
            {
                Id = BookId,
                Name = Name,
                ISBN = ISBN,
                Author = Author == null ? string.Empty : Author.Name,
                Sequence = Sequence,
                Recommend = Recommend,
                ReadDates = (from brd in BookReadDates
                             select brd.ReadDate.ToString("yyyy-MM-dd")).Aggregate((x, y) => (x + "; " + y)),
                Tags = (from bt in BookTags
                        select bt.Tag.Data).Aggregate((x, y) => (x + "; " + y)),
                Source = Source == null ? string.Empty : Source.Name,
                ImageUrl = ImageUrl
            };
        }
    }
}
