using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ReadingList.Pages
{
    [Authorize(Roles="Admin")]
    public class AdminPageModel : PageModel
    {
    }
}
