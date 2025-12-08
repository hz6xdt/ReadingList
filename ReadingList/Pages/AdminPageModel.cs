using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ReadingList.Pages;

[Authorize(AuthenticationSchemes = "Identity.Application, Bearer", Roles = "Admin")]
public class AdminPageModel : PageModel
{
}
