using System.ComponentModel.DataAnnotations;

namespace ReadingList.Models
{
    public class Source
    {
        public long SourceId { get; set; }
        public required string Name { get; set; }

        public IEnumerable<Book>? Books { get; set; }

        public SourceDTO ToSourceDTO()
        {
            return new SourceDTO
            {
                Id = SourceId,
                Name = Name
            };
        }
    }
}
