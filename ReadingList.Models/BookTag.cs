namespace ReadingList.Models;

public class BookTag
{
    public long BookTagId { get; set; }

    public long BookId { get; set; }
    public required Book Book { get; set; }
    
    public long TagId { get; set; }
    public required Tag Tag { get; set; }
}
