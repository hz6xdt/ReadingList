namespace ReadingList.Models
{
    public class TagListItem : IListItem
    {
        public required string Text { get; set; }
        public bool IsSelected { get; set; }
    }
}
