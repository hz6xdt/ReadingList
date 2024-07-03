using ReadingList.Validation;
using System.ComponentModel.DataAnnotations;

namespace ReadingList.Models
{
    public class SourceUpdateBindingTarget
    {
        [Required]
        [PrimaryKey(DbContextType = typeof(DataContext), DataType = typeof(Source))]
        public long Id { get; set; }

        [Required(ErrorMessage = "Please enter the source's name."), StringLength(256)]
        public required string Name { get; set; }

        public Source ToSource() => new Source { SourceId = Id, Name = Name };
    }
}
