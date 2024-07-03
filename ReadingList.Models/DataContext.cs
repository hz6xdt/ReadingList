using Microsoft.EntityFrameworkCore;

namespace ReadingList.Models
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }

        public DbSet<Book> Books => Set<Book>();
        public DbSet<Author> Authors => Set<Author>();
        public DbSet<Tag> Tags => Set<Tag>();
        public DbSet<Source> Sources => Set<Source>();
        public DbSet<BookTag> BookTags => Set<BookTag>();
        public DbSet<BookReadDate> BookReadDates => Set<BookReadDate>();
    }
}
