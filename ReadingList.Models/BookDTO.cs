using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ReadingList.Models;

public class BookDTO
{
    public long Id { get; set; }

    [DisplayName("Title")]
    [Required(ErrorMessage = "Please enter the book's title."), StringLength(256)]
    public required string Name { get; set; }

    [StringLength(16)]
    public string? ISBN { get; set; }

    [DataType(DataType.ImageUrl), StringLength(512)]
    public string? ImageUrl { get; set; }

    [StringLength(256)]
    public string? Author { get; set; }

    [Range(1, 200, ErrorMessage = "Sequence must be between 1 and 200 (if any).")]
    public int? Sequence { get; set; }

    [Range(0, 5, ErrorMessage = "Rating must be between 0 and 5.")]
    public int Rating { get; set; } = 0;

    public bool Recommend { get; set; }

    [StringLength(256)]
    public string? ReadDates { get; set; }

    [StringLength(256)]
    public string? Tags { get; set; }

    [StringLength(256)]
    public string? Source { get; set; }
}
