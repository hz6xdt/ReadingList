using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReadingList.Models;


namespace ReadingList.Controllers
{
    [ApiController]
    [Route("api/r1/[controller]")]
    [Authorize(AuthenticationSchemes = "Identity.Application, Bearer", Roles = "Admin")]
    public class TagsController : ControllerBase
    {
        private IBooksRepository repository;
        private ILogger<TagsController> logger;

        public TagsController(IBooksRepository repository, ILogger<TagsController> logger)
        {
            this.repository = repository;
            this.logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<TagDTO>))]
        [AllowAnonymous]
        public IEnumerable<TagDTO> GetTags()
        {
            logger.LogDebug("Response for GET / started");

            return repository.GetTags();
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TagDTO))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [AllowAnonymous]
        public async Task<IActionResult> GetTag(long id)
        {
            logger.LogDebug("Response for GET /id started");

            TagDTO? a = await repository.GetTag(id);

            if (a == null)
            {
                return NotFound();
            }

            return Ok(a);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(TagDTO))]
        public async Task<IActionResult> AddTag(TagBindingTarget target)
        {
            logger.LogDebug("Response for POST started");

            TagDTO newTag = await repository.AddTag(target);

            return CreatedAtAction(nameof(GetTag), new { id = newTag.Id }, newTag);
        }

        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TagUpdateBindingTarget))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateTag(TagUpdateBindingTarget changedTag)
        {
            logger.LogDebug("Response for PUT started");

            TagDTO? Tag = await repository.UpdateTag(changedTag);

            if (Tag == null)
            {
                return NotFound();
            }

            return Ok(Tag);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteTag(long id)
        {
            logger.LogDebug("Response for DELETE started");

            Tag? Tag = await repository.DeleteTag(id);

            if (Tag == null)
            {
                return NotFound();
            }

            return Ok(Tag);
        }
    }
}
