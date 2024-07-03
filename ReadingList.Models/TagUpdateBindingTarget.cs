using ReadingList.Validation;
using System.ComponentModel.DataAnnotations;

namespace ReadingList.Models
{
    public class TagUpdateBindingTarget
    {
        [Required]
        [PrimaryKey(DbContextType = typeof(DataContext), DataType = typeof(Tag))]
        public long Id { get; set; }

        [Required(ErrorMessage = "Please enter the tag value."), StringLength(256)]
        public required string Data { get; set; }

        public Tag ToTag() => new Tag { TagId = Id, Data = Data };
    }
}
