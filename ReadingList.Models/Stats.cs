namespace ReadingList.Models;

public class UniqueBooksRead
{
    public DateOnly ReadDate { get; set; }
    public int BooksRead { get; set; }
}

public class BooksReadPerYear
{
    public int Year { get; set; }
    public int BooksRead { get; set; }
}
