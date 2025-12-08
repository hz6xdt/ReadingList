using System.ComponentModel.DataAnnotations;

namespace ReadingList.Models;

public class TagBindingTarget
{
    [Required(ErrorMessage = "Please enter the tag value."), StringLength(256)]
    public required string Data { get; set; }

    public Tag ToTag() => new Tag { Data = Data };
}
