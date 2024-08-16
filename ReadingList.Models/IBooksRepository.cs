namespace ReadingList.Models
{
    public interface IBooksRepository
    {
        IEnumerable<BookDTO> GetReadingList();
        Task<BookDTO> AddReadingListEntry(ReadBindingTarget readingListEntry);

        Task<int> GetBookCount();
        IEnumerable<BookDTO> GetBooks(int pageNumber, int pageSize);
        Task<BookDTO?> GetBook(long id);
        Task<BookDTO> AddBook(BookBindingTarget newBook);
        Task<BookDTO?> UpdateBook(BookUpdateBindingTarget changedBook);
        Task<Book?> DeleteBook(long id);


        Task<int> GetAuthorCount();
        IEnumerable<AuthorDTO> GetAuthors(int pageNumber, int pageSize);
        Task<AuthorDTO?> GetAuthor(long id);
        Task<AuthorDTO> AddAuthor(AuthorBindingTarget newAuthor);
        Task<AuthorDTO?> UpdateAuthor(AuthorUpdateBindingTarget changedAuthor);
        Task<Author?> DeleteAuthor(long id);


        Task<int> GetTagCount();
        IEnumerable<TagDTO> GetTags(int pageNumber, int pageSize);
        Task<TagDTO?> GetTag(long id);
        Task<TagDTO> AddTag(TagBindingTarget newTag);
        Task<TagDTO?> UpdateTag(TagUpdateBindingTarget changedTag);
        Task<Tag?> DeleteTag(long id);


        IEnumerable<SourceDTO> GetSources();
        Task<SourceDTO?> GetSource(long id);
        Task<SourceDTO> AddSource(SourceBindingTarget newSource);
        Task<SourceDTO?> UpdateSource(SourceUpdateBindingTarget changedSource);
        Task<DeleteResult> DeleteSource(long id);
    }
}
