using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReadingList.Models;


namespace ReadingList.Controllers
{
    [ApiController]
    [Route("api/r1/[controller]")]
    [Authorize(AuthenticationSchemes = "Identity.Application, Bearer", Roles = "Admin")]
    public class AuthorsController(IBooksRepository repository, ILogger<AuthorsController> logger) : ControllerBase
    {
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<AuthorDTO>))]
        [AllowAnonymous]
        public IEnumerable<AuthorDTO> GetAuthors()
        {
            logger.LogDebug("Response for GET / started");

            return repository.GetAuthors();
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AuthorDTO))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [AllowAnonymous]
        public async Task<IActionResult> GetAuthor(long id)
        {
            logger.LogDebug("Response for GET /id started");

            AuthorDTO? a = await repository.GetAuthor(id);

            if (a == null)
            {
                return NotFound();
            }

            return Ok(a);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(AuthorDTO))]
        public async Task<IActionResult> AddAuthor(AuthorBindingTarget target)
        {
            logger.LogDebug("Response for POST started");

            AuthorDTO newAuthor = await repository.AddAuthor(target);

            return CreatedAtAction(nameof(GetAuthor), new { id = newAuthor.Id }, newAuthor);
        }

        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AuthorUpdateBindingTarget))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateAuthor(AuthorUpdateBindingTarget changedAuthor)
        {
            logger.LogDebug("Response for PUT started");

            AuthorDTO? author = await repository.UpdateAuthor(changedAuthor);

            if (author == null)
            {
                return NotFound();
            }

            return Ok(changedAuthor);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteAuthor(long id)
        {
            logger.LogDebug("Response for DELETE started");

            Author? author = await repository.DeleteAuthor(id);

            if (author == null)
            {
                return NotFound();
            }

            return Ok(author);
        }
    }
}
