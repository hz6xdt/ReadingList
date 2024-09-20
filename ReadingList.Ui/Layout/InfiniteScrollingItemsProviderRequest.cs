namespace ReadingList.Ui.Layout
{
    public sealed class InfiniteScrollingItemsProviderRequest(DateOnly startDate, CancellationToken cancellationToken)
    {
        public DateOnly StartDate { get; set; } = startDate;
        public CancellationToken CancellationToken { get; } = cancellationToken;
    }
}
