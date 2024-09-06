namespace ReadingList.Models
{
    public class SourceListItem : IListItem
    {
        public required string Text { get; set; }
        public bool IsSelected { get; set; }
    }
}
