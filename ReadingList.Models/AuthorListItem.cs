namespace ReadingList.Models;

public class AuthorListItem : IListItem
{
    public required string Text { get; set; }
    public bool IsSelected { get; set; }
}
