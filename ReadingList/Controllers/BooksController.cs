using Microsoft.AspNetCore.Mvc;
using ReadingList.Models;


namespace ReadingList.Controllers
{
    [ApiController]
    [Route("api/r1/[controller]")]
    public class BooksController : ControllerBase
    {
        private IBooksRepository repository;
        private ILogger<BooksController> logger;

        public BooksController(IBooksRepository repository, ILogger<BooksController> logger)
        {
            this.repository = repository;
            this.logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<BookDTO>))]
        public IEnumerable<BookDTO> GetBooks()
        {
            logger.LogDebug("Response for GET / started");

            return repository.GetBooks();
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BookDTO))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetBook(long id)
        {
            logger.LogDebug("Response for GET /id started");

            BookDTO? b = await repository.GetBook(id);

            if (b == null)
            {
                return NotFound();
            }

            return Ok(b);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(BookDTO))]
        public async Task<IActionResult> AddBook(BookBindingTarget target)
        {
            logger.LogDebug("Response for POST started");

            BookDTO newBook = await repository.AddBook(target);

            return CreatedAtAction(nameof(GetBook), new { id = newBook.Id }, newBook);
        }

        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BookUpdateBindingTarget))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateBook(BookUpdateBindingTarget changedBook)
        {
            logger.LogDebug("Response for PUT started");

            BookDTO? book = await repository.UpdateBook(changedBook);

            if (book == null)
            {
                return NotFound();
            }

            return Ok(book);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteBook(long id)
        {
            logger.LogDebug("Response for DELETE started");

            Book? book = await repository.DeleteBook(id);

            if (book == null)
            {
                return NotFound();
            }

            return Ok(book);
        }
    }
}
