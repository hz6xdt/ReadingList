using System.ComponentModel.DataAnnotations;

namespace ReadingList.Models
{
    public class BookReadDate
    {
        public long BookReadDateId { get; set; }

        public long BookId { get; set; }
        public required Book Book { get; set; }

        [DataType(DataType.Date)]
        public required DateTime ReadDate { get; set; } = DateTime.Now;
    }
}
