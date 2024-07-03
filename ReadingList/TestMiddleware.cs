using ReadingList.Models;


namespace ReadingList
{
    public class TestMiddleware
    {
        private RequestDelegate next;

        public TestMiddleware(RequestDelegate requestDelegate)
        {
            next = requestDelegate;
        }

        public async Task Invoke(HttpContext context, DataContext dataContext)
        {
            if (context.Request.Path == "/test")
            {
                await context.Response.WriteAsync($"There are {dataContext.Books.Count()} books.\n");
                await context.Response.WriteAsync($"There are {dataContext.Authors.Count()} authors.\n");
                await context.Response.WriteAsync($"There are {dataContext.Tags.Count()} tags.\n");
                await context.Response.WriteAsync($"There are {dataContext.BookTags.Count()} book tags.\n");
                await context.Response.WriteAsync($"There are {dataContext.Sources.Count()} sources.\n");
                await context.Response.WriteAsync($"There are {dataContext.BookReadDates.Count()} book read dates.\n");
            }
            else
            {
                await next(context);
            }
        }
    }
}
