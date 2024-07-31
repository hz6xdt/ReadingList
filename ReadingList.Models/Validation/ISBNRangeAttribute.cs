using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReadingList.Models.Validation
{
    public class ISBNRangeAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            return (value as string)?.Length == 10 || (value as string)?.Length == 13;
        }
    }
}
