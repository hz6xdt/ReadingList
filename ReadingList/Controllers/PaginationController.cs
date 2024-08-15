using Microsoft.AspNetCore.Mvc;
using ReadingList.Models;

namespace ReadingList.Controllers
{
    [ApiController]
    [Route("api/r1/[controller]/{itemType}")]
    public class PaginationController(IBooksRepository repository, IConfiguration configuration) : ControllerBase
    {
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Pagination))]
        public async Task<IActionResult> GetPaginationInfo(string itemType)
        {
            int pageSize = configuration.GetValue<int>("Data:PageSize", 10);

            int totalItems = 0;

            switch (itemType)
            {
                case "books":
                    totalItems = await repository.GetBookCount();
                    break;
                case "authors":
                    totalItems = await repository.GetAuthorCount();
                    break;
                case "tags":
                    totalItems = await repository.GetTagCount();
                    break;
            }

            return Ok( new Pagination()
            {
                PageSize = pageSize,
                TotalItems = totalItems
            });
        }
    }
}
