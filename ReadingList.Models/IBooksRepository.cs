using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ReadingList.Models
{
    public interface IBooksRepository
    {
        IEnumerable<BookDTO> GetReadingList();

        IEnumerable<BookDTO> GetBooks();
        Task<BookDTO?> GetBook(long id);
        Task<BookDTO> AddBook(BookBindingTarget newBook);
        Task<BookDTO?> UpdateBook(BookUpdateBindingTarget changedBook);
        Task<Book?> DeleteBook(long id);


        IEnumerable<AuthorDTO> GetAuthors();
        Task<AuthorDTO?> GetAuthor(long id);
        Task<AuthorDTO> AddAuthor(AuthorBindingTarget newAuthor);
        Task<AuthorDTO?> UpdateAuthor(AuthorUpdateBindingTarget changedAuthor);
        Task<Author?> DeleteAuthor(long id);


        IEnumerable<TagDTO> GetTags();
        Task<TagDTO?> GetTag(long id);
        Task<TagDTO> AddTag(TagBindingTarget newTag);
        Task<TagDTO?> UpdateTag(TagUpdateBindingTarget changedTag);
        Task<Tag?> DeleteTag(long id);


        IEnumerable<SourceDTO> GetSources();
        Task<SourceDTO?> GetSource(long id);
        Task<SourceDTO> AddSource(SourceBindingTarget newSource);
        Task<SourceDTO?> UpdateSource(SourceUpdateBindingTarget changedSource);
        Task<Source?> DeleteSource(long id);
    }
}
