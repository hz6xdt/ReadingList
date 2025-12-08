namespace ReadingList.Models;

public interface ITimelineItem
{
    public long Id { get; set; }
    public string Name { get; set; }
    public string? ISBN { get; set; }
    public string? ImageUrl { get; set; }
    public string? Author { get; set; }
    public int? Sequence { get; set; }
    public int Rating { get; set; }
    public bool Recommend { get; set; }
    public DateOnly ReadDate { get; set; }
    public string ReadDates { get; set; }
    public string? Tags { get; set; }
    public string? Source { get; set; }
}
