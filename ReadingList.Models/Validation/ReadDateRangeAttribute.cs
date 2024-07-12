using System.ComponentModel.DataAnnotations;

namespace ReadingList.Models.Validation
{
    public class ReadDateRangeAttribute : RangeAttribute
    {
        public ReadDateRangeAttribute() : base(typeof(DateOnly),
            DateOnly.FromDateTime(DateTime.Now).AddYears(-100).ToShortDateString(),
            DateOnly.FromDateTime(DateTime.Now).AddDays(1).ToShortDateString())
        { }
    }
}
