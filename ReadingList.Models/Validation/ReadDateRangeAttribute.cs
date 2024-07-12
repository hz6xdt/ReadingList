using System.ComponentModel.DataAnnotations;

namespace ReadingList.Models.Validation
{
    public class ReadDateRangeAttribute : RangeAttribute
    {
        public ReadDateRangeAttribute() : base(typeof(DateTime),
            DateTime.Now.AddYears(-100).ToShortDateString(),
            DateTime.Now.AddDays(1).ToShortDateString())
        { }
    }
}
