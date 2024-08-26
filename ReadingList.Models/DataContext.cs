using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ReadingList.Models
{
    public class DataContext(DbContextOptions<DataContext> options) : IdentityDbContext<IdentityUser>(options)
    {
        public DbSet<Book> Books => Set<Book>();
        public DbSet<Author> Authors => Set<Author>();
        public DbSet<Tag> Tags => Set<Tag>();
        public DbSet<Source> Sources => Set<Source>();
        public DbSet<BookTag> BookTags => Set<BookTag>();
        public DbSet<BookReadDate> BookReadDates => Set<BookReadDate>();

        public string TitleWithArticleRemoved(string title) => throw new NotSupportedException();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.HasDbFunction(typeof(DataContext).GetMethod(nameof(TitleWithArticleRemoved), new[] { typeof(string) })).HasName("TitleWithArticleRemoved");
        }
    }
}
