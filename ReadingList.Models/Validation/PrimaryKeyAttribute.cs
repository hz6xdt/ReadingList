using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel.DataAnnotations;

namespace ReadingList.Validation
{
    public class PrimaryKeyAttribute : ValidationAttribute
    {
        public Type? DbContextType { get; set; }

        public Type? DataType { get; set; }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (DbContextType != null && DataType != null)
            {
                if (validationContext.GetRequiredService(DbContextType) is DbContext dataContext && dataContext.Find(DataType, value) == null)
                {
                    return new ValidationResult(ErrorMessage ?? $"Enter an existing {DataType.ToString().Split('.').LastOrDefault()} ID value.");
                }
            }
            return ValidationResult.Success;
        }
    }
}
