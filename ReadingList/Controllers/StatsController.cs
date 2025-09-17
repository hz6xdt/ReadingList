using Microsoft.AspNetCore.Mvc;
using ReadingList.Models;

namespace ReadingList.Controllers
{
    [ApiController]
    [Route("api/r1/[controller]")]
    public class StatsController(IBooksRepository repository) : ControllerBase
    {
        [HttpGet("uniqueBooksReadCount/{asOfDate}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(int))]
        public async Task<int> GetUniqueBooksReadCount(DateOnly asOfDate)
        {
            int result = await repository.GetUniqueBooksReadCount(asOfDate);
            return result;
        }

        [HttpGet("booksReadPerYear")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<(int Year, int BooksRead)>))]
        public IEnumerable<BooksReadPerYear> GetBooksReadPerYear()
        {
            IEnumerable<BooksReadPerYear> result = repository.GetBooksReadPerYear();
            return result;
        }
    }
}
