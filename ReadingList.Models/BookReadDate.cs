using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ReadingList.Models
{
    public class BookReadDate
    {
        public long BookReadDateId { get; set; }

        public long BookId { get; set; }
        public required Book Book { get; set; }

        [Column(TypeName = "Date")]
        [DataType(DataType.Date)]
        public required DateOnly ReadDate { get; set; } = DateOnly.FromDateTime(DateTime.Now);
    }
}
