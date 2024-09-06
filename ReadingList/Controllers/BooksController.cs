using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReadingList.Models;


namespace ReadingList.Controllers
{
    [ApiController]
    [Route("api/r1/[controller]")]
    //[Authorize(AuthenticationSchemes = "Identity.Application, Bearer", Roles = "Admin")]
    [Authorize(AuthenticationSchemes = "Bearer", Roles = "Admin")]
    public class BooksController(IBooksRepository repository, ILogger<BooksController> logger, IConfiguration configuration) : ControllerBase
    {
        [HttpGet]
        [AllowAnonymous]
        public async Task<int> GetBookCount()
        {
            int result = await repository.GetBookCount();
            return result;
        }

        [HttpGet("page/{pageNumber:int}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<BookDTO>))]
        [AllowAnonymous]
        public IEnumerable<BookDTO> GetPageOfBooks(int pageNumber = 1)
        {
            int pageSize = configuration.GetValue<int>("Data:PageSize", 10);

            logger.LogDebug("Response for GET /page/{pageNumber} started, with pageSize: {pageSize}", pageNumber, pageSize);

            if (pageNumber < 1)
            {
                pageNumber = 1;
            }

            return repository.GetBooks(pageNumber, pageSize);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BookDTO))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [AllowAnonymous]
        public async Task<IActionResult> GetBook(long id)
        {
            logger.LogDebug("Response for GET /id started");

            BookDTO? b = await repository.GetBook(id);

            return b == null ? NotFound() : Ok(b);
        }

        [HttpGet("filter/{startsWith}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<BookListItem>))]
        public List<BookListItem> GetFilteredBooks(string startsWith = "%")
        {
            logger.LogDebug("Response for GET /filter/{startsWith} started", startsWith);

            return repository.GetFilteredBooks(startsWith);
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

            return book == null ? NotFound() : Ok(book);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteBook(long id)
        {
            logger.LogDebug("Response for DELETE started");

            Book? book = await repository.DeleteBook(id);

            return book == null ? NotFound() : Ok(book);
        }
    }
}
