using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReadingList.Models;


namespace ReadingList.Controllers
{
    [ApiController]
    [Route("api/r1/[controller]")]
    //[Authorize(AuthenticationSchemes = "Identity.Application, Bearer", Roles = "Admin")]
    [Authorize(AuthenticationSchemes = "Bearer", Roles = "Admin")]
    public class AuthorsController(IBooksRepository repository, ILogger<AuthorsController> logger, IConfiguration configuration) : ControllerBase
    {
        [HttpGet]
        [AllowAnonymous]
        public async Task<int> GetAuthorCount()
        {
            int result = await repository.GetAuthorCount();
            return result;
        }

        [HttpGet("page/{pageNumber:int}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<AuthorDTO>))]
        [AllowAnonymous]
        public IEnumerable<AuthorDTO> GetPageOfAuthors(int pageNumber = 1)
        {
            int pageSize = configuration.GetValue<int>("Data:PageSize", 10);

            logger.LogDebug("\r\n\r\n\r\nResponse for GET /page/{pageNumber} started, with pageSize: {pageSize}", pageNumber, pageSize);

            if (pageNumber < 1)
            {
                pageNumber = 1;
            }

            return repository.GetAuthors(pageNumber, pageSize);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AuthorDTO))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [AllowAnonymous]
        public async Task<IActionResult> GetAuthor(long id)
        {
            logger.LogDebug("\r\n\r\n\r\nResponse for GET /id started");

            AuthorDTO? a = await repository.GetAuthor(id);

            return a == null ? NotFound() : Ok(a);
        }

        [HttpGet("filter/{startsWith}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<AuthorListItem>))]
        public List<AuthorListItem> GetFilteredAuthors(string startsWith = "%")
        {
            logger.LogDebug("\r\n\r\n\r\nResponse for GET /filter/{startsWith} started", startsWith);

            return repository.GetFilteredAuthors(startsWith);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(AuthorDTO))]
        public async Task<IActionResult> AddAuthor(AuthorBindingTarget target)
        {
            logger.LogDebug("\r\n\r\n\r\nResponse for POST started");

            AuthorDTO newAuthor = await repository.AddAuthor(target);

            return CreatedAtAction(nameof(GetAuthor), new { id = newAuthor.Id }, newAuthor);
        }

        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AuthorUpdateBindingTarget))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateAuthor(AuthorUpdateBindingTarget changedAuthor)
        {
            logger.LogDebug("\r\n\r\n\r\nResponse for PUT started");

            AuthorDTO? author = await repository.UpdateAuthor(changedAuthor);

            return author == null ? NotFound() : Ok(changedAuthor);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteAuthor(long id)
        {
            logger.LogDebug("\r\n\r\n\r\nResponse for DELETE started");

            Author? author = await repository.DeleteAuthor(id);

            return author == null ? NotFound() : Ok(author);
        }
    }
}
