namespace ReadingList.Models
{
    public interface IBooksRepository
    {
        List<TimelineDTO> GetTimeline(DateOnly startDate);
        IEnumerable<BookDTO> GetReadingList();
        Task<BookDTO> AddReadingListEntry(ReadBindingTarget readingListEntry);

        Task<int> GetBookCount();
        Task<DateOnly> GetStartDate();

        IEnumerable<BookDTO> GetBooks(int pageNumber, int pageSize);
        Task<BookDTO?> GetBook(long id);
        IEnumerable<BookDTO> GetFilteredBookList(string startsWith);
        List<BookListItem> GetFilteredBooks(string startsWith);
        Task<BookDTO> AddBook(BookBindingTarget newBook);
        Task<BookDTO?> UpdateBook(BookUpdateBindingTarget changedBook);
        Task<Book?> DeleteBook(long id);


        Task<int> GetAuthorCount();
        IEnumerable<AuthorDTO> GetAuthors(int pageNumber, int pageSize);
        IEnumerable<AuthorDTO> GetFilteredAuthorList(string startsWith);
        List<AuthorListItem> GetFilteredAuthors(string startsWith);
        Task<AuthorDTO?> GetAuthor(long id);
        Task<AuthorDTO> AddAuthor(AuthorBindingTarget newAuthor);
        Task<AuthorDTO?> UpdateAuthor(AuthorUpdateBindingTarget changedAuthor);
        Task<Author?> DeleteAuthor(long id);


        Task<int> GetTagCount();
        IEnumerable<TagDTO> GetTags(int pageNumber, int pageSize);
        IEnumerable<TagDTO> GetFilteredTagList(string startsWith);
        List<TagListItem> GetFilteredTags(string startsWith);
        Task<TagDTO?> GetTag(long id);
        Task<TagDTO> AddTag(TagBindingTarget newTag);
        Task<TagDTO?> UpdateTag(TagUpdateBindingTarget changedTag);
        Task<Tag?> DeleteTag(long id);


        IEnumerable<SourceDTO> GetSources();
        List<SourceListItem> GetFilteredSources(string startsWith);
        Task<SourceDTO?> GetSource(long id);

        Task<SourceDTO> AddSource(SourceBindingTarget newSource);
        Task<SourceDTO?> UpdateSource(SourceUpdateBindingTarget changedSource);
        Task<DeleteResult> DeleteSource(long id);
    }
}
