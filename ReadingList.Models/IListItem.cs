namespace ReadingList.Models;

public interface IListItem
{
    public string Text { get; set; }
    public bool IsSelected { get; set; }
}
