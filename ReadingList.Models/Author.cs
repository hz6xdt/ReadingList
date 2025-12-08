using System.ComponentModel.DataAnnotations;

namespace ReadingList.Models;

public class Author
{
    public long AuthorId { get; set; }
    public required string Name { get; set; }

    public IEnumerable<Book>? Books { get; set; }

    public AuthorDTO ToAuthorDTO()
    {
        return new AuthorDTO
        {
            Id = AuthorId,
            Name = Name
        };
    }
}
