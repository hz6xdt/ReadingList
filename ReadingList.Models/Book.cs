using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ReadingList.Models;

public class Book
{
    public const string DefaultCoverImageUrl = "https://2.bp.blogspot.com/_aDCnyPs488U/SyAtBDSHFHI/AAAAAAAAGDI/tFkGgFeISHI/s400/BookCoverGreenBrown.jpg";

    public long BookId { get; set; }

    [DisplayName("Title")]
    public required string Name { get; set; }
    public int? Sequence { get; set; }
    public int Rating { get; set; } = 0;
    public bool Recommend { get; set; } = false;
    public bool HideFromLongestUnread { get; set; } = false;
    public string? ISBN { get; set; }
    [Url]
    public string ImageUrl { get; set; } = DefaultCoverImageUrl;

    public long? AuthorId { get; set; }
    public Author? Author { get; set; }

    public long? SourceId { get; set; }
    public Source? Source { get; set; }

    public ICollection<BookReadDate>? BookReadDates { get; set; }
    public ICollection<BookTag>? BookTags { get; set; }

    public BookDTO ToBookDTO()
    {
        BookDTO bookDTO = new()
        {
            Id = BookId,
            Name = Name,
            ISBN = ISBN,
            Author = Author?.Name,
            Sequence = Sequence,
            Rating = Rating,
            Recommend = Recommend,
            ReadDates = (from brd in BookReadDates
                         select brd.ReadDate.ToString("yyyy-MM-dd")).DefaultIfEmpty(null).Aggregate((x, y) => x + "; " + y),
            Tags = (from bt in BookTags
                    select bt.Tag.Data).DefaultIfEmpty(null).Aggregate((x, y) => x + "; " + y),
            Source = Source?.Name,
            ImageUrl = ImageUrl
        };
        return bookDTO;
    }
}
