using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReadingList.Models;


namespace ReadingList.Controllers
{
    [ApiController]
    [Route("api/r1/[controller]")]
    //[Authorize(AuthenticationSchemes = "Identity.Application, Bearer", Roles = "Admin")]
    [Authorize(AuthenticationSchemes = "Bearer", Roles = "Admin")]
    public class TagsController(IBooksRepository repository, ILogger<TagsController> logger, IConfiguration configuration) : ControllerBase
    {
        [HttpGet]
        [AllowAnonymous]
        public async Task<int> GetTagCount()
        {
            int result = await repository.GetTagCount();
            return result;
        }

        [HttpGet("page/{pageNumber:int}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<TagDTO>))]
        [AllowAnonymous]
        public IEnumerable<TagDTO> GetPageOfTags(int pageNumber = 1)
        {
            int pageSize = configuration.GetValue<int>("Data:PageSize", 10);

            logger.LogDebug("Response for GET /page/{pageNumber} started, with pageSize: {pageSize}", pageNumber, pageSize);

            if (pageNumber < 1)
            {
                pageNumber = 1;
            }

            return repository.GetTags(pageNumber, pageSize);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TagDTO))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [AllowAnonymous]
        public async Task<IActionResult> GetTag(long id)
        {
            logger.LogDebug("Response for GET /id started");

            TagDTO? a = await repository.GetTag(id);

            return a == null ? NotFound() : Ok(a);
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

            return Tag == null ? NotFound() : Ok(Tag);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteTag(long id)
        {
            logger.LogDebug("Response for DELETE started");

            Tag? Tag = await repository.DeleteTag(id);

            return Tag == null ? NotFound() : Ok(Tag);
        }
    }
}
