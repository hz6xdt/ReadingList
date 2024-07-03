using Azure;

namespace ReadingList.Models
{
    public class Tag
    {
        public long TagId { get; set; }
        public required string Data { get; set; }

        public IEnumerable<BookTag>? BookTags { get; set; }

        public TagDTO ToTagDTO()
        {
            return new TagDTO
            {
                Id = TagId,
                Data = Data
            };
        }
    }
}
