using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReadingList.Models;
using System.Text.Json;


namespace ReadingList.Controllers
{
    [ApiController]
    [Route("api/r1/[controller]")]
    //[Authorize(AuthenticationSchemes = "Identity.Application, Bearer", Roles = "Admin")]
    [Authorize(AuthenticationSchemes = "Bearer", Roles = "Admin")]
    public class SourcesController(IBooksRepository repository, ILogger<SourcesController> logger) : ControllerBase
    {
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<SourceDTO>))]
        [AllowAnonymous]
        public IEnumerable<SourceDTO> GetSources()
        {
            logger.LogDebug("Response for GET / started");

            return repository.GetSources();
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SourceDTO))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [AllowAnonymous]
        public async Task<IActionResult> GetSource(long id)
        {
            logger.LogDebug("Response for GET /id started");

            SourceDTO? s = await repository.GetSource(id);

            return s == null ? NotFound() : Ok(s);
        }

        [HttpGet("filter/{startsWith}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<SourceListItem>))]
        public IEnumerable<SourceListItem> GetFilteredSources(string startsWith = "%")
        {
            logger.LogDebug("Response for GET /filter/{startsWith} started", startsWith);

            return repository.GetFilteredSources(startsWith);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(SourceDTO))]
        public async Task<IActionResult> AddSource(SourceBindingTarget target)
        {
            logger.LogDebug("Response for POST started");

            SourceDTO newSource = await repository.AddSource(target);

            return CreatedAtAction(nameof(GetSource), new { id = newSource.Id }, newSource);
        }

        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SourceUpdateBindingTarget))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateSource(SourceUpdateBindingTarget changedSource)
        {
            logger.LogDebug("Response for PUT started");

            SourceDTO? source = await repository.UpdateSource(changedSource);

            return source == null ? NotFound() : Ok(changedSource);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> DeleteSource(long id)
        {
            logger.LogDebug("Response for DELETE started");

            DeleteResult result = await repository.DeleteSource(id);

            if (result.Success)
            {
                return Ok(result.Source);
            }

            var exceptionType = result.ExceptionType ?? string.Empty;
            var json = JsonSerializer.Serialize(result.ExceptionMessage);

            return exceptionType.Equals("DbUpdateConcurrencyException") ? Conflict(json) : NotFound(json);
        }
    }
}
