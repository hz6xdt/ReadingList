using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReadingList.Models;


namespace ReadingList.Controllers
{
    [ApiController]
    [Route("api/r1/[controller]")]
    //[Authorize(AuthenticationSchemes = "Identity.Application, Bearer", Roles = "Admin")]
    [Authorize(AuthenticationSchemes = "Bearer", Roles = "Admin")]
    public class ReadListController(IBooksRepository repository, ILogger<ReadListController> logger) : ControllerBase
    {
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<BookDTO>))]
        [AllowAnonymous]
        public IEnumerable<BookDTO> GetReadingList()
        {
            logger.LogDebug("Response for GET / started");

            return repository.GetReadingList();
        }


        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BookDTO))]
        public async Task<IActionResult> AddReadingListEntry(ReadBindingTarget target)
        {
            logger.LogDebug("Response for POST started");

            BookDTO newBook = await repository.AddReadingListEntry(target);

            return Ok(newBook);
        }
    }
}
