using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReadingList.Models.Migrations
{
    /// <inheritdoc />
    public partial class LongestUnreadStoredProcedure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE OR ALTER PROCEDURE dbo.GetLongestUnreadBooks
                AS
                BEGIN
                    SET NOCOUNT ON;
                    SELECT top 40 books.BookId as Id, Books.Name, ISBN, ImageUrl, Authors.Name as Author, [Sequence],
                        Rating, Recommend, max(BookReadDates.ReadDate) as ReadDate, '' as ReadDates, '' as Tags, '' as Source
                    from Books
                    JOIN BookReadDates ON Books.BookID = BookReadDates.BookID
                    JOIN Authors ON Books.AuthorID = Authors.AuthorID
                    where books.HideFromLongestUnread = 0
                    GROUP BY books.BookId, books.Name, Books.ISBN, books.ImageUrl, Authors.Name, [Sequence],
                        Rating, Recommend
                    ORDER BY max(BookReadDates.ReadDate);
                END");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DROP PROCEDURE IF EXISTS dbo.GetLongestUnreadBooks;
            ");
        }
    }
}
