using Microsoft.AspNetCore.Mvc;
using ReadingList.Models;

namespace ReadingList.Controllers;

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

    [HttpGet("booksPerRating")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<(int Rating, int Books)>))]
    public IEnumerable<BooksPerRating> GetBooksPerRating()
    {
        IEnumerable<BooksPerRating> result = repository.GetBooksPerRating();
        return result;
    }

    [HttpGet("longestUnread")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<TimelineDTO>))]
    public List<TimelineDTO> GetLongestUnread()
    {
        List<TimelineDTO> result = repository.GetLongestUnread();
        return result;
    }

    [HttpPost("hideFromLongestUnread/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(bool))]
    public async Task<bool> HideFromLongestUnread(long id)
    {
        bool result = await repository.HideFromLongestUnread(id);
        return result;
    }
}
