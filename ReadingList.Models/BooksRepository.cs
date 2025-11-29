using System.Collections.ObjectModel;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ReadingList.Models
{
    public class BooksRepository(DataContext dataContext, ILogger<BooksRepository> logger) : IBooksRepository
    {
        public List<TimelineDTO> GetLongestUnread()
        {
            logger.LogDebug("\r\n\r\n\r\nGetLongestUnread");

            string query = @"EXEC dbo.GetLongestUnreadBooks";

            List<TimelineDTO> result = dataContext.Database
                .SqlQueryRaw<TimelineDTO>(query)
                .ToList();

            return result;
        }

        public async Task<bool> HideFromLongestUnread(long id)
        {
            logger.LogDebug("\r\n\r\n\r\nHideFromLongestUnread");

            Book? book = await dataContext.Books.FindAsync(id);

            if (book == null)
            {
                logger.LogDebug("\r\n\r\n\r\nNo book with given ID: {id} was found to update.", id);
                return false;
            }

            book.HideFromLongestUnread = true;
            dataContext.Books.Update(book);
            await dataContext.SaveChangesAsync();

            logger.LogDebug("\r\n\r\n\r\nRemoved book: {book.Name} from longest unread list.", book.Name);

            return true;
        }


        public List<TimelineDTO> GetTimeline(DateOnly startDate)
        {
            logger.LogDebug("\r\n\r\n\r\nGetTimeLine -- startDate: {startDate}", startDate);

            DateOnly endDate = startDate.AddMonths(6);
            bool found = false;

            while (!found)
            {
                int readCount = (from br in dataContext.BookReadDates
                                 where br.ReadDate >= startDate && br.ReadDate < endDate
                                 orderby br.ReadDate
                                 select br)
                                 .Count();

                if (readCount > 0)
                {
                    found = true;
                }
                else
                {
                    endDate = endDate.AddMonths(6);
                    if (endDate > DateOnly.FromDateTime(DateTime.UtcNow))
                    {
                        return [];
                    }
                }
            }


            IEnumerable<TimelineDTO> result = (from b in dataContext.Books
                            .Include(b => b.Author)
                            .Include(b => b.Source)
                            .Include(b => b.BookTags)
                                               join br in dataContext.BookReadDates on b.BookId equals br.BookId
                                               where br.ReadDate >= startDate && br.ReadDate < endDate
                                               orderby br.ReadDate
                                               select new
                                               {
                                                   b.BookId,
                                                   b.Name,
                                                   b.ISBN,
                                                   Author = b.Author == null ? null : b.Author.Name,
                                                   b.Sequence,
                                                   b.Rating,
                                                   b.Recommend,
                                                   br.ReadDate,
                                                   ReadDates = from brd in b.BookReadDates
                                                               select brd.ReadDate.ToString("yyyy-MM-dd"),
                                                   Tags = from bt in b.BookTags
                                                          select bt.Tag.Data,
                                                   Source = b.Source == null ? null : b.Source.Name,
                                                   ImageUrl = b.ImageUrl ?? "http://2.bp.blogspot.com/_aDCnyPs488U/SyAtBDSHFHI/AAAAAAAAGDI/tFkGgFeISHI/s400/BookCoverGreenBrown.jpg"
                                               }).ToList()
                          .Select(b => new TimelineDTO
                          {
                              Id = b.BookId,
                              Name = b.Name,
                              ISBN = b.ISBN,
                              Author = b.Author,
                              Sequence = b.Sequence,
                              Rating = b.Rating,
                              Recommend = b.Recommend,
                              ReadDate = b.ReadDate,
                              ReadDates = b.ReadDates.Aggregate((x, y) => x + "; " + y),
                              Tags = b.Tags.DefaultIfEmpty(null).Aggregate((x, y) => x + "; " + y),
                              Source = b.Source,
                              ImageUrl = b.ImageUrl
                          });

            return [.. result];
        }


        public IEnumerable<BookDTO> GetReadingList()
        {
            logger.LogDebug("\r\n\r\n\r\nGetReadingList");

            DateOnly sixMonthsAgo = DateOnly.FromDateTime(DateTime.Now).AddMonths(-6);

            IEnumerable<BookDTO> result = (from b in dataContext.Books
                            .Include(b => b.Author)
                            .Include(b => b.Source)
                            .Include(b => b.BookTags)
                                           join br in dataContext.BookReadDates on b.BookId equals br.BookId
                                           where br.ReadDate >= sixMonthsAgo
                                           orderby br.ReadDate descending
                                           select new
                                           {
                                               b.BookId,
                                               b.Name,
                                               b.ISBN,
                                               Author = b.Author == null ? null : b.Author.Name,
                                               b.Sequence,
                                               b.Rating,
                                               b.Recommend,
                                               ReadDates = from brd in b.BookReadDates
                                                           select brd.ReadDate.ToString("yyyy-MM-dd"),
                                               Tags = from bt in b.BookTags
                                                      select bt.Tag.Data,
                                               Source = b.Source == null ? null : b.Source.Name,
                                               ImageUrl = b.ImageUrl ?? "http://2.bp.blogspot.com/_aDCnyPs488U/SyAtBDSHFHI/AAAAAAAAGDI/tFkGgFeISHI/s400/BookCoverGreenBrown.jpg"
                                           }).ToList()
                          .Select(b => new BookDTO
                          {
                              Id = b.BookId,
                              Name = b.Name,
                              ISBN = b.ISBN,
                              Author = b.Author,
                              Sequence = b.Sequence,
                              Rating = b.Rating,
                              Recommend = b.Recommend,
                              ReadDates = b.ReadDates.DefaultIfEmpty(null).Aggregate((x, y) => x + "; " + y),
                              Tags = b.Tags.DefaultIfEmpty(null).Aggregate((x, y) => x + "; " + y),
                              Source = b.Source,
                              ImageUrl = b.ImageUrl
                          });

            return result;
        }

        public async Task<BookDTO> AddReadingListEntry(ReadBindingTarget readingListEntry)
        {
            logger.LogDebug("\r\n\r\n\r\nAddReadingListEntry: {JsonSerializer.Serialize(readingListEntry)}", JsonSerializer.Serialize(readingListEntry));

            //is this a new book, or one already in the database?
            long bookId = await (from b in dataContext.Books
                                 where b.Name == readingListEntry.Name
                                 select b.BookId).FirstOrDefaultAsync();

            //is this a new author, or one already in the database?
            long authorId = await (from a in dataContext.Authors
                                   where a.Name == readingListEntry.Author
                                   select a.AuthorId).FirstOrDefaultAsync();

            Dictionary<string, long> tagList = [];

            if (!string.IsNullOrWhiteSpace(readingListEntry.Tags))
            {
                foreach (string item in readingListEntry.Tags.Split(';'))
                {
                    string data = item.Trim();
                    long tagId = await (from t in dataContext.Tags
                                        where t.Data == data
                                        select t.TagId).FirstOrDefaultAsync();

                    tagList.Add(data, tagId);
                }
            }

            long sourceId = await (from s in dataContext.Sources
                                   where s.Name == readingListEntry.Source
                                   select s.SourceId).FirstOrDefaultAsync();

            long bookReadId = await (from br in dataContext.BookReadDates
                                     where br.BookId == bookId
                                        && br.ReadDate == readingListEntry.ReadDate
                                     select br.BookId).FirstOrDefaultAsync();

            List<long> bookTagIds = [];

            foreach (long tagId in tagList.Values)
            {
                var bookTag = await (from b in dataContext.Books
                                     .Include("BookTags")
                                     where b.BookId == bookId
                                     select new
                                     {
                                         tag = (from t in b.BookTags
                                                where t.TagId == tagId
                                                select new
                                                {
                                                    Id = t.TagId
                                                }).FirstOrDefault()
                                     }).FirstOrDefaultAsync();

                if (bookTag != null && bookTag.tag != null)
                {
                    bookTagIds.Add(bookTag.tag.Id);
                }
            }


            if (!string.IsNullOrWhiteSpace(readingListEntry.Source) && sourceId == 0)
            {
                Source newSource = new() { Name = readingListEntry.Source };

                dataContext.Sources.Add(newSource);
                dataContext.SaveChanges();

                sourceId = newSource.SourceId;
            }


            if (!string.IsNullOrWhiteSpace(readingListEntry.Author) && authorId == 0)
            {
                Author newAuthor = new() { Name = readingListEntry.Author };

                dataContext.Authors.Add(newAuthor);
                dataContext.SaveChanges();

                authorId = newAuthor.AuthorId;
            }


            Dictionary<string, long> newTagListIds = [];

            foreach (KeyValuePair<string, long> tagEntry in tagList)
            {
                if (tagEntry.Value == 0)
                {
                    Tag newTag = new() { Data = tagEntry.Key };

                    dataContext.Tags.Add(newTag);
                    dataContext.SaveChanges();

                    // save for update, to avoid problem with continuing enumeration
                    // of this list after it is updated.
                    newTagListIds.Add(tagEntry.Key, newTag.TagId);
                }
            }

            //now it's safe to update the list with new values, if any.
            foreach (KeyValuePair<string, long> item in newTagListIds)
            {
                tagList[item.Key] = item.Value;
            }


            Author? author = dataContext.Authors.Find(authorId);
            Source? source = dataContext.Sources.Find(sourceId);


            Book book = new() { Name = string.Empty };


            //adding new book, or updating exiting?
            if (bookId == 0)
            {
                book = new()
                {
                    Name = readingListEntry.Name,
                    ISBN = readingListEntry.ISBN,
                    Author = author,
                    Sequence = readingListEntry.Sequence,
                    Rating = readingListEntry.Rating,
                    Recommend = readingListEntry.Recommend,
                    Source = source,
                    ImageUrl = readingListEntry.ImageUrl ?? Book.DefaultCoverImageUrl,
                    BookReadDates = []
                };
                book.BookReadDates.Add(new() { Book = book, ReadDate = readingListEntry.ReadDate });

                foreach (KeyValuePair<string, long> tagEntry in tagList)
                {
                    Tag? tag = dataContext.Tags.Find(tagEntry.Value);
                    if (tag != null)
                    {
                        book.BookTags ??= [];
                        book.BookTags.Add(new() { Book = book, Tag = tag });
                    }
                }

                dataContext.Books.Add(book);
                dataContext.SaveChanges();

                bookId = book.BookId;
            }
            else
            {
                book = await (from b in dataContext.Books
                              .Include("BookTags")
                              where b.BookId == bookId
                              select b).FirstAsync();
                book.ISBN = readingListEntry.ISBN;
                book.Author = author;
                book.AuthorId = authorId == 0 ? null : authorId;
                book.Sequence = readingListEntry.Sequence;
                book.Rating = readingListEntry.Rating;
                book.Recommend = readingListEntry.Recommend;
                book.Source = source;
                book.SourceId = sourceId == 0 ? null : sourceId;
                book.ImageUrl = readingListEntry.ImageUrl ?? Book.DefaultCoverImageUrl;


                if (bookReadId == 0)
                {
                    book.BookReadDates ??= [];
                    book.BookReadDates.Add(new() { Book = book, ReadDate = readingListEntry.ReadDate });
                }

                book.BookTags ??= [];

                foreach (KeyValuePair<string, long> tagEntry in tagList)
                {
                    if (!book.BookTags.Any(bt => bt.TagId == tagEntry.Value))
                    {
                        Tag tag = await (from t in dataContext.Tags
                                         where t.Data == tagEntry.Key
                                         select t).FirstAsync();
                        book.BookTags.Add(new() { Book = book, Tag = tag });
                    }
                }

                foreach (BookTag bt in book.BookTags)
                {
                    bt.Tag ??= await (from t in dataContext.Tags
                                      where t.TagId == bt.TagId
                                      select t).FirstAsync();
                }

                dataContext.SaveChanges();
            }


            logger.LogDebug("\r\n\r\n\r\nadded/updated book: {book.Name}", book.Name);

            return book.ToBookDTO();
        }





        public async Task<DateOnly> GetStartDate()
        {
            DateOnly result = await (from br in dataContext.BookReadDates
                                     orderby br.ReadDate
                                     select br.ReadDate).FirstOrDefaultAsync();

            return result == default ? DateOnly.FromDateTime(DateTime.UtcNow) : result;
        }


        public async Task<int> GetBookCount()
        {
            int result = await dataContext.Books.CountAsync();
            return result;
        }



        public IEnumerable<BookDTO> GetBooks(int pageNumber = 1, int pageSize = 10)
        {
            logger.LogDebug("\r\n\r\n\r\nGetBooks -- page: {pageNumber} -- pageSize: {pageSize}", pageNumber, pageSize);

            IEnumerable<BookDTO> result = (from b in dataContext.Books
                            .Include(b => b.Author)
                            .Include(b => b.Source)
                            .Include(b => b.BookTags)
                            .OrderBy(b => dataContext.TitleWithArticleRemoved(b.Name))
                            .Skip((pageNumber - 1) * pageSize)
                            .Take(pageSize)
                                           select new
                                           {
                                               b.BookId,
                                               b.Name,
                                               b.ISBN,
                                               Author = b.Author == null ? string.Empty : b.Author.Name,
                                               b.Sequence,
                                               b.Rating,
                                               b.Recommend,
                                               ReadDates = from brd in b.BookReadDates
                                                           select brd.ReadDate.ToString("yyyy-MM-dd"),
                                               Tags = from bt in b.BookTags
                                                      select bt.Tag.Data,
                                               Source = b.Source == null ? string.Empty : b.Source.Name,
                                               ImageUrl = b.ImageUrl ?? "http://2.bp.blogspot.com/_aDCnyPs488U/SyAtBDSHFHI/AAAAAAAAGDI/tFkGgFeISHI/s400/BookCoverGreenBrown.jpg"
                                           }).ToList()
                            .Select(b => new BookDTO
                            {
                                Id = b.BookId,
                                Name = b.Name,
                                ISBN = b.ISBN,
                                Author = b.Author,
                                Sequence = b.Sequence,
                                Rating = b.Rating,
                                Recommend = b.Recommend,
                                ReadDates = b.ReadDates.DefaultIfEmpty(null).Aggregate((x, y) => x + "; " + y),
                                Tags = b.Tags.DefaultIfEmpty(null).Aggregate((x, y) => x + "; " + y),
                                Source = b.Source,
                                ImageUrl = b.ImageUrl
                            });

            return result;
        }


        public IEnumerable<BookDTO> GetFilteredBookList(string startsWith)
        {
            logger.LogDebug("\r\n\r\n\r\nGetFilteredBookList -- startsWith: {startsWith}", startsWith);

            IEnumerable<BookDTO> result = (from b in dataContext.Books
                            .Where(b => EF.Functions.Like(dataContext.TitleWithArticleRemoved(b.Name), $"{startsWith}%"))
                            .Include(b => b.Author)
                            .Include(b => b.Source)
                            .Include(b => b.BookTags)
                            .OrderBy(b => dataContext.TitleWithArticleRemoved(b.Name))
                                           select new
                                           {
                                               b.BookId,
                                               b.Name,
                                               b.ISBN,
                                               Author = b.Author == null ? string.Empty : b.Author.Name,
                                               b.Sequence,
                                               b.Rating,
                                               b.Recommend,
                                               ReadDates = from brd in b.BookReadDates
                                                           select brd.ReadDate.ToString("yyyy-MM-dd"),
                                               Tags = from bt in b.BookTags
                                                      select bt.Tag.Data,
                                               Source = b.Source == null ? string.Empty : b.Source.Name,
                                               ImageUrl = b.ImageUrl ?? "http://2.bp.blogspot.com/_aDCnyPs488U/SyAtBDSHFHI/AAAAAAAAGDI/tFkGgFeISHI/s400/BookCoverGreenBrown.jpg"
                                           }).ToList()
                            .Select(b => new BookDTO
                            {
                                Id = b.BookId,
                                Name = b.Name,
                                ISBN = b.ISBN,
                                Author = b.Author,
                                Sequence = b.Sequence,
                                Rating = b.Rating,
                                Recommend = b.Recommend,
                                ReadDates = b.ReadDates.DefaultIfEmpty(null).Aggregate((x, y) => x + "; " + y),
                                Tags = b.Tags.DefaultIfEmpty(null).Aggregate((x, y) => x + "; " + y),
                                Source = b.Source,
                                ImageUrl = b.ImageUrl
                            });

            return result;
        }


        public List<BookListItem> GetFilteredBooks(string startsWith)
        {
            logger.LogDebug("\r\n\r\n\r\nGetFilteredBooks -- startsWith: {startsWith}", startsWith);

            IEnumerable<BookListItem> result = (from b in dataContext.Books
                            .Where(b => EF.Functions.Like(b.Name, $"{startsWith}%"))
                            .Include(b => b.Author)
                            .Include(b => b.Source)
                            .Include(b => b.BookTags)
                            .OrderBy(b => b.Name)
                                                select new
                                                {
                                                    b.Name,
                                                    b.ISBN,
                                                    Author = b.Author == null ? string.Empty : b.Author.Name,
                                                    b.Sequence,
                                                    b.Rating,
                                                    b.Recommend,
                                                    Tags = from bt in b.BookTags
                                                           select bt.Tag.Data,
                                                    Source = b.Source == null ? string.Empty : b.Source.Name,
                                                    ImageUrl = b.ImageUrl ?? "http://2.bp.blogspot.com/_aDCnyPs488U/SyAtBDSHFHI/AAAAAAAAGDI/tFkGgFeISHI/s400/BookCoverGreenBrown.jpg"
                                                }).ToList()
                            .Select(b => new BookListItem
                            {
                                Text = b.Name,
                                ISBN = b.ISBN,
                                Author = b.Author,
                                Sequence = b.Sequence,
                                Rating = b.Rating,
                                Recommend = b.Recommend,
                                Tags = b.Tags.DefaultIfEmpty(null).Aggregate((x, y) => x + "; " + y),
                                Source = b.Source,
                                ImageUrl = b.ImageUrl
                            });

            return [.. result];
        }


        public async Task<BookDTO?> GetBook(long id)
        {
            logger.LogDebug("\r\n\r\n\r\nGetBook: {id}", id);

            var book = await (from b in dataContext.Books
                                .Include(b => b.Author)
                                .Include(b => b.Source)
                                .Where(b => b.BookId == id)
                              select new
                              {
                                  b.BookId,
                                  b.Name,
                                  b.ISBN,
                                  Author = b.Author == null ? string.Empty : b.Author.Name,
                                  b.Sequence,
                                  b.Rating,
                                  b.Recommend,
                                  ReadDates = from br in b.BookReadDates
                                              select br.ReadDate.ToString("yyyy-MM-dd"),
                                  Tags = from bd in b.BookTags
                                         select bd.Tag.Data,
                                  Source = b.Source == null ? string.Empty : b.Source.Name,
                                  ImageUrl = b.ImageUrl ?? "http://2.bp.blogspot.com/_aDCnyPs488U/SyAtBDSHFHI/AAAAAAAAGDI/tFkGgFeISHI/s400/BookCoverGreenBrown.jpg"
                              }).FirstOrDefaultAsync();

            if (book == null)
            {
                logger.LogDebug("\r\n\r\n\r\nbook with id: {id} not found", id);
                return null;
            }

            logger.LogDebug("\r\n\r\n\r\nreturning book: {book.Name}", book.Name);

            BookDTO result = new()
            {
                Id = book.BookId,
                Name = book.Name,
                ISBN = book.ISBN,
                Author = book.Author,
                Sequence = book.Sequence,
                Rating = book.Rating,
                Recommend = book.Recommend,
                ReadDates = book.ReadDates.DefaultIfEmpty(null).Aggregate((x, y) => x + "; " + y),
                Tags = book.Tags.DefaultIfEmpty(null).Aggregate((x, y) => x + "; " + y),
                Source = book.Source,
                ImageUrl = book.ImageUrl
            };

            return result;
        }


        public async Task<BookDTO> AddBook(BookBindingTarget newBook)
        {
            logger.LogDebug("\r\n\r\n\r\nAddBook: {JsonSerializer.Serialize(newBook)}", JsonSerializer.Serialize(newBook));

            //is this a new author, or one already in the database?
            long authorId = await (from a in dataContext.Authors
                                   where a.Name == newBook.Author
                                   select a.AuthorId).FirstOrDefaultAsync();

            if (!string.IsNullOrWhiteSpace(newBook.Author) && authorId == 0)
            {
                Author newAuthor = new() { Name = newBook.Author };

                await dataContext.Authors.AddAsync(newAuthor);
                await dataContext.SaveChangesAsync();

                authorId = newAuthor.AuthorId;
            }


            long sourceId = await (from s in dataContext.Sources
                                   where s.Name == newBook.Source
                                   select s.SourceId).FirstOrDefaultAsync();


            if (!string.IsNullOrWhiteSpace(newBook.Source) && sourceId == 0)
            {
                Source newSource = new() { Name = newBook.Source };

                await dataContext.Sources.AddAsync(newSource);
                await dataContext.SaveChangesAsync();

                sourceId = newSource.SourceId;
            }


            Dictionary<string, long?> tagList = [];

            //new or existing tags?
            if (!string.IsNullOrWhiteSpace(newBook.Tags))
            {
                foreach (string item in newBook.Tags.Split(';'))
                {
                    long tagId = await (from t in dataContext.Tags
                                        where t.Data == item.Trim()
                                        select t.TagId).FirstOrDefaultAsync();

                    tagList.Add(item.Trim(), tagId);
                }

                foreach (KeyValuePair<string, long?> tagEntry in tagList)
                {
                    if (tagEntry.Value == 0)
                    {
                        Tag newTag = new() { Data = tagEntry.Key };

                        await dataContext.Tags.AddAsync(newTag);
                        await dataContext.SaveChangesAsync();

                        tagList[tagEntry.Key] = newTag.TagId;
                    }
                }
            }


            Author? author = await dataContext.Authors.FindAsync(authorId);
            Source? source = await dataContext.Sources.FindAsync(sourceId);


            Book book = new()
            {
                ISBN = newBook.ISBN,
                Name = newBook.Name,
                Author = author,
                Sequence = newBook.Sequence,
                Rating = newBook.Rating,
                Recommend = newBook.Recommend,
                Source = source,
                ImageUrl = newBook.ImageUrl ?? "http://2.bp.blogspot.com/_aDCnyPs488U/SyAtBDSHFHI/AAAAAAAAGDI/tFkGgFeISHI/s400/BookCoverGreenBrown.jpg"
            };


            Collection<BookTag> bookTags = [];

            foreach (KeyValuePair<string, long?> tagEntry in tagList)
            {
                Tag? tag = await dataContext.Tags.FindAsync(tagEntry.Value);
                if (tag != null)
                {
                    bookTags.Add(new() { Tag = tag, Book = book });
                }
            }
            book.BookTags = bookTags;


            if (!string.IsNullOrWhiteSpace(newBook.ReadDates))
            {
                Collection<BookReadDate> bookReadDates = [];

                foreach (string item in newBook.ReadDates.Split(';'))
                {
                    if (DateOnly.TryParse(item.Trim(), out DateOnly readDate))
                    {
                        bookReadDates.Add(new() { ReadDate = readDate, Book = book });
                    }
                }
                book.BookReadDates = bookReadDates;
            }


            await dataContext.Books.AddAsync(book);
            await dataContext.SaveChangesAsync();


            logger.LogDebug("\r\n\r\n\r\nadded book: {book.Name}", book.Name);

            return book.ToBookDTO();
        }


        public async Task<BookDTO?> UpdateBook(BookUpdateBindingTarget changedBook)
        {
            logger.LogDebug("\r\n\r\n\r\nUpdateBook: {JsonSerializer.Serialize(changedBook)}", JsonSerializer.Serialize(changedBook));

            Book? book = await dataContext.Books
                        .Include(b => b.Author)
                        .Include(b => b.Source)
                        .Include(b => b.BookTags)
                        .Include(b => b.BookReadDates)
                        .FirstOrDefaultAsync(b => b.BookId == changedBook.Id);

            if (book == null)
            {
                logger.LogDebug("\r\n\r\n\r\nNo book with given ID: {changedBook.Id} is available to update.", changedBook.Id);
                return null;
            }


            //is this a new author, or one already in the database?
            long authorId = await (from a in dataContext.Authors
                                   where a.Name == changedBook.Author
                                   select a.AuthorId).FirstOrDefaultAsync();

            if (!string.IsNullOrWhiteSpace(changedBook.Author) && authorId == 0)
            {
                Author newAuthor = new() { Name = changedBook.Author };

                await dataContext.Authors.AddAsync(newAuthor);
                await dataContext.SaveChangesAsync();

                authorId = newAuthor.AuthorId;
            }



            long sourceId = await (from s in dataContext.Sources
                                   where s.Name == changedBook.Source
                                   select s.SourceId).FirstOrDefaultAsync();

            if (!string.IsNullOrWhiteSpace(changedBook.Source) && sourceId == 0)
            {
                Source newSource = new() { Name = changedBook.Source };

                await dataContext.Sources.AddAsync(newSource);
                await dataContext.SaveChangesAsync();

                sourceId = newSource.SourceId;
            }


            Dictionary<string, long?> tagList = [];

            //new or existing tags?
            if (!string.IsNullOrWhiteSpace(changedBook.Tags))
            {
                foreach (string item in changedBook.Tags.Split(';'))
                {
                    long tagId = await (from t in dataContext.Tags
                                        where t.Data == item.Trim()
                                        select t.TagId).FirstOrDefaultAsync();

                    tagList.Add(item.Trim(), tagId);
                }

                foreach (KeyValuePair<string, long?> tagEntry in tagList)
                {
                    if (tagEntry.Value == 0)
                    {
                        Tag newTag = new() { Data = tagEntry.Key };

                        await dataContext.Tags.AddAsync(newTag);
                        await dataContext.SaveChangesAsync();

                        tagList[tagEntry.Key] = newTag.TagId;
                    }
                }
            }


            Author? author = await dataContext.Authors.FindAsync(authorId);
            Source? source = await dataContext.Sources.FindAsync(sourceId);


            book.Name = changedBook.Name;
            book.ISBN = changedBook.ISBN;
            book.Author = author;
            book.Sequence = changedBook.Sequence;
            book.Rating = changedBook.Rating;
            book.Recommend = changedBook.Recommend;
            book.Source = source;
            book.ImageUrl = changedBook.ImageUrl ?? "http://2.bp.blogspot.com/_aDCnyPs488U/SyAtBDSHFHI/AAAAAAAAGDI/tFkGgFeISHI/s400/BookCoverGreenBrown.jpg";


            Collection<BookTag> bookTags = [];

            foreach (KeyValuePair<string, long?> tagEntry in tagList)
            {
                Tag? tag = await dataContext.Tags.FindAsync(tagEntry.Value);
                if (tag != null)
                {
                    bookTags.Add(new() { Tag = tag, Book = book });
                }
            }
            book.BookTags = bookTags;


            if (!string.IsNullOrWhiteSpace(changedBook.ReadDates))
            {
                Collection<BookReadDate> bookReadDates = [];

                foreach (string item in changedBook.ReadDates.Split(';'))
                {
                    if (DateOnly.TryParse(item.Trim(), out DateOnly readDate))
                    {
                        bookReadDates.Add(new() { ReadDate = readDate, Book = book });
                    }
                }
                book.BookReadDates = bookReadDates;
            }


            dataContext.Books.Update(book);
            await dataContext.SaveChangesAsync();


            logger.LogDebug("\r\n\r\n\r\nupdated book: {book.Name}", book.Name);

            return book.ToBookDTO();
        }

        public async Task<Book?> DeleteBook(long id)
        {
            logger.LogDebug("\r\n\r\n\r\nDeleteBook: {id}", id);

            Book? book = await dataContext.Books.FindAsync(id);
            if (book != null)
            {
                dataContext.Books.Remove(book);
                try
                {
                    await dataContext.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    logger.LogDebug("\r\n\r\n\r\nDbUpdateConcurrencyException.");
                    return null;
                }

                logger.LogDebug("\r\n\r\n\r\ndeleted book: {book.Name}", book.Name);
            }

            return book;
        }



        public async Task<int> GetAuthorCount()
        {
            int result = await dataContext.Authors.CountAsync();
            return result;
        }


        public IEnumerable<AuthorDTO> GetAuthors(int pageNumber = 1, int pageSize = 10)
        {
            logger.LogDebug("\r\n\r\n\r\nGetAuthors -- page: {pageNumber} -- pageSize: {pageSize}", pageNumber, pageSize);

            IEnumerable<AuthorDTO> result = (from a in dataContext.Authors
                .Include(a => a.Books!)
                .ThenInclude(b => b.BookTags)
                .OrderBy(a => a.Name)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                                             select new
                                             {
                                                 a.AuthorId,
                                                 a.Name,
                                                 Books = from b in a.Books
                                                         orderby b.BookTags!.First().TagId, b.Sequence, b.Name
                                                         select b.BookId + "~" + b.Name
                                             }).ToList()
                .Select(a => new AuthorDTO
                {
                    Id = a.AuthorId,
                    Name = a.Name,
                    Books = a.Books.DefaultIfEmpty(null).Aggregate((x, y) => x + "; " + y)
                });

            return result;
        }

        public IEnumerable<AuthorDTO> GetFilteredAuthorList(string startsWith)
        {
            logger.LogDebug("\r\n\r\n\r\nGetFilteredAuthorList -- startsWith: {startsWith}", startsWith);

            IEnumerable<AuthorDTO> result = (from a in dataContext.Authors
                .Where(a => EF.Functions.Like(a.Name, $"{startsWith}%"))
                .Include(a => a.Books)
                .OrderBy(a => a.Name)
                                             select new
                                             {
                                                 a.AuthorId,
                                                 a.Name,
                                                 Books = from b in a.Books
                                                         orderby b.BookTags!.First().TagId, b.Sequence, b.Name
                                                         select b.BookId + "~" + b.Name
                                             }).ToList()
                .Select(a => new AuthorDTO
                {
                    Id = a.AuthorId,
                    Name = a.Name,
                    Books = a.Books.DefaultIfEmpty(null).Aggregate((x, y) => x + "; " + y)
                });

            return result;
        }


        public List<AuthorListItem> GetFilteredAuthors(string startsWith)
        {
            logger.LogDebug("\r\n\r\n\r\nGetFilteredAuthors -- startsWith: {startsWith}", startsWith);

            IEnumerable<AuthorListItem> result = (from a in dataContext.Authors
                            .Where(a => EF.Functions.Like(a.Name, $"{startsWith}%"))
                            .OrderBy(a => a.Name)
                                                  select new
                                                  {
                                                      a.Name
                                                  }).ToList()
                            .Select(a => new AuthorListItem
                            {
                                Text = a.Name
                            });

            return [.. result];
        }


        public async Task<AuthorDTO?> GetAuthor(long id)
        {
            logger.LogDebug("\r\n\r\n\r\nGetAuthor: {id}", id);

            var author = await (from a in dataContext.Authors
                                .Include(a => a.Books)
                                .Where(a => a.AuthorId == id)
                                select new
                                {
                                    a.AuthorId,
                                    a.Name,
                                    Books = from b in a.Books
                                            orderby b.BookTags!.First().TagId, b.Sequence, b.Name
                                            select b.BookId + "~" + b.Name
                                }).FirstOrDefaultAsync();

            if (author == null)
            {
                logger.LogDebug("\r\n\r\n\r\nNo author with given ID: {id} was found.", id);
                return null;
            }

            AuthorDTO result = new()
            {
                Id = author.AuthorId,
                Name = author.Name,
                Books = author.Books.DefaultIfEmpty(null).Aggregate((x, y) => x + "; " + y)
            };

            logger.LogDebug("\r\n\r\n\r\nreturning author: {author.Name}", author.Name);

            return result;
        }


        public async Task<AuthorDTO> AddAuthor(AuthorBindingTarget newAuthor)
        {
            logger.LogDebug("\r\n\r\n\r\nAddAuthor: {JsonSerializer.Serialize(newAuthor)}", JsonSerializer.Serialize(newAuthor));

            Author author = newAuthor.ToAuthor();
            await dataContext.Authors.AddAsync(author);
            await dataContext.SaveChangesAsync();

            logger.LogDebug("\r\n\r\n\r\nadded author: {author.Name}", author.Name);

            return author.ToAuthorDTO();
        }


        public async Task<AuthorDTO?> UpdateAuthor(AuthorUpdateBindingTarget changedAuthor)
        {
            logger.LogDebug("\r\n\r\n\r\nUpdateAuthor: {JsonSerializer.Serialize(changedAuthor)}", JsonSerializer.Serialize(changedAuthor));

            Author? author = await dataContext.Authors.FindAsync(changedAuthor.Id);

            if (author == null)
            {
                logger.LogDebug("\r\n\r\n\r\nNo author with given ID: {changedAuthor.Id} was found to update.", changedAuthor.Id);
                return null;
            }

            author.Name = changedAuthor.Name;
            dataContext.Authors.Update(author);
            await dataContext.SaveChangesAsync();

            logger.LogDebug("\r\n\r\n\r\nUpdated author: {author.Name}", author.Name);

            return author.ToAuthorDTO();
        }


        public async Task<Author?> DeleteAuthor(long id)
        {
            logger.LogDebug("\r\n\r\n\r\nDeleteAuthor: {id}", id);

            Author? author = await dataContext.Authors.FindAsync(id);
            if (author != null)
            {
                dataContext.Authors.Remove(author);
                try
                {
                    await dataContext.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    logger.LogDebug("\r\n\r\n\r\nDbUpdateConcurrencyException.");
                    return null;
                }

                logger.LogDebug("\r\n\r\n\r\ndeleted author: {author.Name}", author.Name);
            }

            return author;
        }


        public async Task<int> GetTagCount()
        {
            int result = await dataContext.Tags.CountAsync();
            return result;
        }


        public IEnumerable<TagDTO> GetTags(int pageNumber = 1, int pageSize = 10)
        {
            logger.LogDebug("\r\n\r\n\r\nGetTags -- page: {pageNumber} -- pageSize: {pageSize}", pageNumber, pageSize);

            IEnumerable<TagDTO> result = (from t in dataContext.Tags
                            .Include(bt => bt.BookTags!)
                            .ThenInclude(b => b.Book)
                            .OrderBy(t => t.Data)
                            .Skip((pageNumber - 1) * pageSize)
                            .Take(pageSize)
                                          select new
                                          {
                                              t.TagId,
                                              t.Data,
                                              Books = string.Join("; ", t.BookTags!
                                                .OrderBy(t => t.TagId)
                                                .ThenBy(b => b.Book.Sequence)
                                                .ThenBy(b => b.Book.Name)
                                                .Select(b => b.Book.Sequence == null ? b.BookId + "~" + b.Book.Name : b.BookId + "~" + b.Book.Sequence + " - " + b.Book.Name))
                                          }).ToList()
                            .Select(t => new TagDTO
                            {
                                Id = t.TagId,
                                Data = t.Data,
                                Books = t.Books
                            });
            return result;
        }

        public IEnumerable<TagDTO> GetFilteredTagList(string contains)
        {
            logger.LogDebug("\r\n\r\n\r\nGetFilteredTagList -- contains: {contains}", contains);

            IEnumerable<TagDTO> result = (from t in dataContext.Tags
                            .Where(t => EF.Functions.Like(t.Data, $"%{contains}%"))
                            .Include(bt => bt.BookTags!)
                            .ThenInclude(b => b.Book)
                            .OrderBy(t => t.Data)
                                          select new
                                          {
                                              t.TagId,
                                              t.Data,
                                              Books = string.Join("; ", t.BookTags!
                                                .OrderBy(t => t.TagId)
                                                .ThenBy(b => b.Book.Sequence)
                                                .ThenBy(b => b.Book.Name)
                                                .Select(b => b.Book.Sequence == null ? b.BookId + "~" + b.Book.Name : b.BookId + "~" + b.Book.Sequence + " - " + b.Book.Name))
                                          }).ToList()
                            .Select(t => new TagDTO
                            {
                                Id = t.TagId,
                                Data = t.Data,
                                Books = t.Books
                            });

            return result;
        }

        public List<TagListItem> GetFilteredTags(string contains)
        {
            logger.LogDebug("\r\n\r\n\r\nGetFilteredTags -- contains: {contains}", contains);

            IEnumerable<TagListItem> result = (from t in dataContext.Tags
                            .Where(t => EF.Functions.Like(t.Data, $"%{contains}%"))
                            .OrderBy(t => t.Data)
                                               select new
                                               {
                                                   t.Data,
                                               }).ToList()
                            .Select(t => new TagListItem
                            {
                                Text = t.Data,
                            });

            return [.. result];
        }


        public async Task<TagDTO?> GetTag(long id)
        {
            logger.LogDebug("\r\n\r\n\r\nGetTag: {id}", id);

            var tag = await (from t in dataContext.Tags
                             .Include(bt => bt.BookTags!)
                             .ThenInclude(b => b.Book)
                             .Where(t => t.TagId == id)
                             select new
                             {
                                 t.TagId,
                                 t.Data,
                                 Books = string.Join("; ", t.BookTags!
                                                .OrderBy(t => t.TagId)
                                                .ThenBy(b => b.Book.Sequence)
                                                .ThenBy(b => b.Book.Name)
                                                .Select(b => b.Book.Sequence == null ? b.BookId + "~" + b.Book.Name : b.BookId + "~" + b.Book.Sequence + " - " + b.Book.Name))
                             }).FirstOrDefaultAsync();

            if (tag == null)
            {
                logger.LogDebug("\r\n\r\n\r\nNo tag with given ID: {id} was found.", id);
                return null;
            }

            TagDTO result = new()
            {
                Id = tag.TagId,
                Data = tag.Data,
                Books = tag.Books
            };

            logger.LogDebug("\r\n\r\n\r\nreturning tag: {tag.Data}", tag.Data);

            return result;
        }


        public async Task<TagDTO> AddTag(TagBindingTarget newTag)
        {
            logger.LogDebug("\r\n\r\n\r\nAddTag: {JsonSerializer.Serialize(newTag)}", JsonSerializer.Serialize(newTag));

            Tag tag = newTag.ToTag();
            await dataContext.Tags.AddAsync(tag);
            await dataContext.SaveChangesAsync();

            logger.LogDebug("\r\n\r\n\r\nadded tag: {tag.Data}", tag.Data);

            return tag.ToTagDTO();
        }


        public async Task<TagDTO?> UpdateTag(TagUpdateBindingTarget changedTag)
        {
            logger.LogDebug("\r\n\r\n\r\nUpdateTag: {JsonSerializer.Serialize(changedTag)}", JsonSerializer.Serialize(changedTag));

            Tag? tag = await dataContext.Tags.FindAsync(changedTag.Id);

            if (tag == null)
            {
                logger.LogDebug("\r\n\r\n\r\nNo tag with given ID: {changedTag.Id} was found to update.", changedTag.Id);
                return null;
            }

            tag.Data = changedTag.Data;
            dataContext.Tags.Update(tag);
            await dataContext.SaveChangesAsync();

            logger.LogDebug("\r\n\r\n\r\nupdated tag: {tag.Data}", tag.Data);

            return tag.ToTagDTO();
        }


        public async Task<Tag?> DeleteTag(long id)
        {
            logger.LogDebug("\r\n\r\n\r\nDeleteTag: {id}", id);

            Tag? tag = await dataContext.Tags.FindAsync(id);
            if (tag != null)
            {
                dataContext.Tags.Remove(tag);
                try
                {
                    await dataContext.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    logger.LogDebug("\r\n\r\n\r\nDbUpdateConcurrencyException.");
                    return null;
                }

                logger.LogDebug("\r\n\r\n\r\ndeleted tag: {tag.Data}", tag.Data);
            }

            return tag;
        }



        public IEnumerable<SourceDTO> GetSources()
        {
            logger.LogDebug("\r\n\r\n\r\nGetSources");

            IEnumerable<SourceDTO> result = (from s in dataContext.Sources
                            .Include(s => s.Books)
                            .OrderBy(s => s.Name)
                                             select new
                                             {
                                                 s.SourceId,
                                                 s.Name,
                                                 Books = from b in s.Books
                                                         orderby b.BookTags!.First().TagId, b.Sequence, b.Name
                                                         select b.BookId + "~" + b.Name
                                             }).ToList()
                            .Select(s => new SourceDTO
                            {
                                Id = s.SourceId,
                                Name = s.Name,
                                Books = s.Books.DefaultIfEmpty(null).Aggregate((x, y) => x + "; " + y)
                            });

            return result;
        }

        public List<SourceListItem> GetFilteredSources(string startsWith)
        {
            logger.LogDebug("\r\n\r\n\r\nGetFilteredSources -- startsWith: {startsWith}", startsWith);

            IEnumerable<SourceListItem> result = (from s in dataContext.Sources
                            .Where(s => EF.Functions.Like(s.Name, $"{startsWith}%"))
                            .OrderBy(s => s.Name)
                                                  select new
                                                  {
                                                      s.Name
                                                  }).ToList()
                            .Select(s => new SourceListItem
                            {
                                Text = s.Name
                            });

            return [.. result];
        }

        public async Task<SourceDTO?> GetSource(long id)
        {
            logger.LogDebug("\r\n\r\n\r\nGetSource: {id}", id);

            var source = await (from s in dataContext.Sources
                                .Include(b => b.Books)
                                .Where(s => s.SourceId == id)
                                select new
                                {
                                    s.SourceId,
                                    s.Name,
                                    Books = from b in s.Books
                                            orderby b.BookTags!.First().TagId, b.Sequence, b.Name
                                            select b.BookId + "~" + b.Name
                                }).FirstOrDefaultAsync();

            if (source == null)
            {
                logger.LogDebug("\r\n\r\n\r\nno source found with id: {id}", id);
                return null;
            }

            SourceDTO result = new()
            {
                Id = source.SourceId,
                Name = source.Name,
                Books = source.Books.DefaultIfEmpty(null).Aggregate((x, y) => x + "; " + y)
            };

            logger.LogDebug("\r\n\r\n\r\nreturning source: {source.Name}", source.Name);

            return result;
        }


        public async Task<SourceDTO> AddSource(SourceBindingTarget newSource)
        {
            logger.LogDebug("\r\n\r\n\r\nAddSource: {JsonSerializer.Serialize(newSource)}", JsonSerializer.Serialize(newSource));

            Source source = newSource.ToSource();
            await dataContext.Sources.AddAsync(source);
            await dataContext.SaveChangesAsync();

            logger.LogDebug("\r\n\r\n\r\nadded source: {source.Name}", source.Name);

            return source.ToSourceDTO();
        }


        public async Task<SourceDTO?> UpdateSource(SourceUpdateBindingTarget changedSource)
        {
            logger.LogDebug("\r\n\r\n\r\nUpdateSource: {JsonSerializer.Serialize(changedSource)}", JsonSerializer.Serialize(changedSource));

            Source? source = await dataContext.Sources.FindAsync(changedSource.Id);

            if (source == null)
            {
                logger.LogDebug("\r\n\r\n\r\nNo source with given ID: {changedSource.Id} was found to update.", changedSource.Id);
                return null;
            }

            source.Name = changedSource.Name;
            dataContext.Sources.Update(source);
            await dataContext.SaveChangesAsync();

            logger.LogDebug("\r\n\r\n\r\nupdated source: {source.Name}", source.Name);

            return source.ToSourceDTO();
        }


        public async Task<DeleteResult> DeleteSource(long id)
        {
            logger.LogDebug("\r\n\r\n\r\nDeleteSource: {id}", id);

            DeleteResult result = new();

            Source? source = await dataContext.Sources.FindAsync(id);

            if (source != null)
            {
                dataContext.Sources.Remove(source);
                try
                {
                    await dataContext.SaveChangesAsync();
                    result.Success = true;
                    result.Source = source;
                    logger.LogDebug("\r\n\r\n\r\ndeleted source: {source.Name}", source.Name);
                }
                catch (DbUpdateConcurrencyException x)
                {
                    logger.LogDebug("\r\n\r\n\r\nDbUpdateConcurrencyException: {x}", x.ToString());
                    result.ExceptionType = "DbUpdateConcurrencyException";
                    result.ExceptionMessage = "There was a conflict with what other users are currently doing.  Please wait and then try again after refreshing your browser.";
                }
                catch (DbUpdateException x)
                {
                    logger.LogDebug("\r\n\r\n\r\nDbUpdateException: {x}", x.ToString());
                    result.ExceptionType = "DbUpdateException";
                    result.ExceptionMessage = "Verify that any book with this source has been deleted first, and then try again.";
                }
            }

            return result;
        }


        public async Task<int> GetUniqueBooksReadCount(DateOnly asOfDate)
        {
            int result = await dataContext.BookReadDates
                .Where(brd => brd.ReadDate <= asOfDate)
                .Select(brd => brd.BookId)
                .Distinct()
                .CountAsync();
            return result;
        }

        public IEnumerable<BooksReadPerYear> GetBooksReadPerYear()
        {
            IEnumerable<BooksReadPerYear> result = (from brd in dataContext.BookReadDates
                                                    group brd by brd.ReadDate.Year into g
                                                    orderby g.Key
                                                    select new BooksReadPerYear
                                                    {
                                                        Year = g.Key,
                                                        BooksRead = g.Select(brd => brd.BookId).Count()
                                                    }).ToList();
            return result;
        }

        public IEnumerable<BooksPerRating> GetBooksPerRating()
        {
            IEnumerable<BooksPerRating> result = (from b in dataContext.Books
                                                  group b by b.Rating into g
                                                  orderby g.Key descending
                                                  select new BooksPerRating
                                                  {
                                                      Rating = g.Key,
                                                      Books = g.Count()
                                                  }).ToList();
            return result;
        }
    }

    public class DeleteResult
    {
        public bool Success { get; set; } = false;

        public Source? Source { get; set; }

        public string? ExceptionType { get; set; }
        public string? ExceptionMessage { get; set; }
    }
}
