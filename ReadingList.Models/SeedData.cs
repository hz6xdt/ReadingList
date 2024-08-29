using Microsoft.EntityFrameworkCore;


namespace ReadingList.Models
{
    public static class SeedData
    {
        public static void SeedDatabase(DataContext dataContext)
        {
            dataContext.Database.Migrate();

            if (!dataContext.Books.Any()
             && !dataContext.Sources.Any()
             && !dataContext.BookReadDates.Any()
             && !dataContext.Tags.Any()
             && !dataContext.BookTags.Any()
             && !dataContext.Authors.Any())
            {
                Author a1 = new() { Name = "Zelazny, Roger" };
                Author a2 = new() { Name = "Asimov, Isaac" };
                Author a3 = new() { Name = "Heinlein, Robert A." };

                Source s1 = new() { Name = "Friesen" };
                Source s2 = new() { Name = "Dallas" };


                Book b1 = new()
                {
                    Name = "Nine Princes In Amber",
                    Sequence = 1,
                    Rating = 5,
                    ISBN = "0380014300",
                    ImageUrl = "https://d202m5krfqbpi5.cloudfront.net/books/1290060140l/9724168.jpg",
                    Author = a1
                };
                Book b2 = new()
                {
                    Name = "Roger Zelazny's Visual Guide To Amber",
                    ISBN = "0380755661",
                    Rating = 2,
                    ImageUrl = "https://d202m5krfqbpi5.cloudfront.net/books/1215096661l/62001.jpg",
                    Author = a1,
                    Source = s1
                };
                Book b3 = new()
                {
                    Name = "A Dark Traveling",
                    ISBN = "0380705672",
                    ImageUrl = "http://ecx.images-amazon.com/images/I/31Qot%2Bl688L._SX260_.jpg",
                    Author = a1,
                    Source = s2
                };
                Book b4 = new()
                {
                    Name = "Foundation",
                    Sequence = 1,
                    ISBN = "0380440652",
                    Rating = 4,
                    ImageUrl = "https://i.gr-assets.com/images/S/compressed.photo.goodreads.com/books/1258332664l/1706321.jpg",
                    Author = a2
                };
                Book b5 = new()
                {
                    Name = "I, Robot",
                    Sequence = 1,
                    ImageUrl = "https://i.gr-assets.com/images/S/compressed.photo.goodreads.com/books/1603733745l/55808570._SY475_.jpg",
                    Author = a2
                };
                Book b6 = new()
                {
                    Name = "Starship Troopers",
                    ISBN = "0425057739",
                    Rating = 5,
                    ImageUrl = "https://d202m5krfqbpi5.cloudfront.net/books/1304018517l/573833.jpg",
                    Author = a3,
                    Recommend = true
                };
                Book b7 = new()
                {
                    Name = "Stranger In A Strange Land",
                    ISBN = "0399135863",
                    Rating = 5,
                    ImageUrl = "http://pics.cdn.librarything.com/picsizes/cd/5f/cd5f005467a7dd5593753485251444341587343.jpg",
                    Author = a3
                };

                BookReadDate brd1 = new() { Book = b6, ReadDate = new DateOnly(1989, 3, 28) };
                BookReadDate brd2 = new() { Book = b6, ReadDate = new DateOnly(1996, 1, 05) };
                BookReadDate brd3 = new() { Book = b6, ReadDate = new DateOnly(1999, 9, 12) };
                BookReadDate brd4 = new() { Book = b6, ReadDate = new DateOnly(2015, 9, 7) };
                BookReadDate brd5 = new() { Book = b7, ReadDate = new DateOnly(1991, 1, 8) };
                BookReadDate brd6 = new() { Book = b7, ReadDate = new DateOnly(1996, 4, 6) };
                BookReadDate brd7 = new() { Book = b7, ReadDate = new DateOnly(2015, 12, 29) };
                BookReadDate brd8 = new() { Book = b4, ReadDate = new DateOnly(1988, 3, 14) };
                BookReadDate brd9 = new() { Book = b4, ReadDate = new DateOnly(1997, 5, 3) };
                BookReadDate brd10 = new() { Book = b4, ReadDate = new DateOnly(2021, 6, 23) };
                BookReadDate brd11 = new() { Book = b5, ReadDate = new DateOnly(1997, 7, 6) };
                BookReadDate brd12 = new() { Book = b5, ReadDate = new DateOnly(2021, 8, 7) };
                BookReadDate brd13 = new() { Book = b1, ReadDate = new DateOnly(1989, 11, 4) };
                BookReadDate brd14 = new() { Book = b1, ReadDate = new DateOnly(2009, 12, 22) };
                BookReadDate brd15 = new() { Book = b2, ReadDate = new DateOnly(1989, 2, 5) };
                BookReadDate brd16 = new() { Book = b3, ReadDate = new DateOnly(1988, 3, 6) };

                Tag t1 = new() { Data = "Amber" };
                Tag t2 = new() { Data = "Parallel Worlds" };
                Tag t3 = new() { Data = "Foundation" };
                Tag t4 = new() { Data = "Robots" };
                Tag t5 = new() { Data = "Military" };
                Tag t6 = new() { Data = "Citizenship" };
                Tag t7 = new() { Data = "Extraterrestrials" };
                Tag t8 = new() { Data = "Culture" };
                Tag t9 = new() { Data = "Religion" };

                BookTag bt1 = new() { Book = b6, Tag = t5 };
                BookTag bt2 = new() { Book = b6, Tag = t6 };
                BookTag bt3 = new() { Book = b6, Tag = t5 };
                BookTag bt4 = new() { Book = b7, Tag = t7 };
                BookTag bt5 = new() { Book = b7, Tag = t8 };
                BookTag bt6 = new() { Book = b7, Tag = t9 };
                BookTag bt7 = new() { Book = b4, Tag = t3 };
                BookTag bt8 = new() { Book = b5, Tag = t4 };
                BookTag bt9 = new() { Book = b1, Tag = t1 };
                BookTag bt10 = new() { Book = b2, Tag = t1 };
                BookTag bt11 = new() { Book = b3, Tag = t2 };

                dataContext.Books.AddRange(b1, b2, b2, b4, b5, b6, b7);
                dataContext.BookReadDates.AddRange(brd1, brd2, brd3, brd4, brd5, brd6, brd7, brd8, brd9, brd10, brd11, brd12, brd13, brd14, brd15, brd16);
                dataContext.BookTags.AddRange(bt1, bt2, bt3, bt4, bt5, bt6, bt7, bt8, bt9, bt10, bt11);

                dataContext.SaveChanges();
            }
        }
    }
}
