using Microsoft.AspNetCore.Mvc;
using ReadingList.Models;


namespace ReadingList.Controllers
{
    [ApiController]
    [Route("api/r1/[controller]")]
    public class SourcesController : ControllerBase
    {
        private IBooksRepository repository;
        private ILogger<SourcesController> logger;

        public SourcesController(IBooksRepository repository, ILogger<SourcesController> logger)
        {
            this.repository = repository;
            this.logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<SourceDTO>))]
        public IEnumerable<SourceDTO> GetSources()
        {
            logger.LogDebug("Response for GET / started");

            return repository.GetSources();
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SourceDTO))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetSource(long id)
        {
            logger.LogDebug("Response for GET /id started");

            SourceDTO? s = await repository.GetSource(id);

            if (s == null)
            {
                return NotFound();
            }

            return Ok(s);
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

            if (source == null)
            {
                return NotFound();
            }

            return Ok(changedSource);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteSource(long id)
        {
            logger.LogDebug("Response for DELETE started");

            Source? source = await repository.DeleteSource(id);

            if (source == null)
            {
                return NotFound();
            }

            return Ok(source);
        }
    }
}
