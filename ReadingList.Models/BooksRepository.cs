using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.Text.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ReadingList.Models
{
    public class BooksRepository : IBooksRepository
    {
        private DataContext dataContext;
        private ILogger<BooksRepository> logger;

        public BooksRepository(DataContext dataContext, ILogger<BooksRepository> logger)
        {
            this.dataContext = dataContext;
            this.logger = logger;
        }


        public IEnumerable<BookDTO> GetReadingList()
        {
            logger.LogDebug("GetReadingList");

            DateTime sixMonthsAgo = DateTime.Now.Date.AddMonths(-6);

            var result = (from b in dataContext.Books
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
                              Author = b.Author == null ? string.Empty : b.Author.Name,
                              b.Sequence,
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
                              Recommend = b.Recommend,
                              ReadDates = b.ReadDates.DefaultIfEmpty(string.Empty).Aggregate((x, y) => (x + "; " + y)),
                              Tags = b.Tags.DefaultIfEmpty(string.Empty).Aggregate((x, y) => (x + "; " + y)),
                              Source = b.Source,
                              ImageUrl = b.ImageUrl
                          });

            return result;
        }

        public async Task<BookDTO> AddReadingListEntry(ReadBindingTarget readingListEntry)
        {
            logger.LogDebug("AddReadingListEntry: {JsonSerializer.Serialize(readingListEntry)}", JsonSerializer.Serialize(readingListEntry));

            //is this a new book, or one already in the database?
            var bookId = await (from b in dataContext.Books
                                where b.Name == readingListEntry.Name
                                select b.BookId).FirstOrDefaultAsync();

            //is this a new author, or one already in the database?
            var authorId = await (from a in dataContext.Authors
                                  where a.Name == readingListEntry.Author
                                  select a.AuthorId).FirstOrDefaultAsync();

            Dictionary<string, long> tagList = [];

            foreach (var item in (readingListEntry.Tags ?? string.Empty).Split(';'))
            {
                var data = item.Trim();
                var tagId = await (from t in dataContext.Tags
                                   where t.Data == data
                                   select t.TagId).FirstOrDefaultAsync();

                tagList.Add(data, tagId);
            }

            var sourceId = await (from s in dataContext.Sources
                                  where s.Name == readingListEntry.Source
                                  select s.SourceId).FirstOrDefaultAsync();

            var bookReadId = await (from br in dataContext.BookReadDates
                                    where br.BookId == bookId
                                       && br.ReadDate == readingListEntry.ReadDate
                                    select br.BookId).FirstOrDefaultAsync();

            List<long> bookTagIds = [];

            foreach (var tagId in tagList.Values)
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


            if (readingListEntry.Source != null && sourceId == 0)
            {
                Source newSource = new() { Name = readingListEntry.Source };

                dataContext.Sources.Add(newSource);
                dataContext.SaveChanges();

                sourceId = newSource.SourceId;
            }


            if (readingListEntry.Author != null && authorId == 0)
            {
                Author newAuthor = new() { Name = readingListEntry.Author };

                dataContext.Authors.Add(newAuthor);
                dataContext.SaveChanges();

                authorId = newAuthor.AuthorId;
            }


            Dictionary<string, long> newTagListIds = [];

            foreach (var tagEntry in tagList)
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
            foreach (var item in newTagListIds)
            {
                tagList[item.Key] = item.Value;
            }


            Author author = dataContext.Authors.Find(authorId) ?? new Author { Name = string.Empty };
            Source source = dataContext.Sources.Find(sourceId) ?? new Source { Name = string.Empty };


            Book book = new() { Name = string.Empty };


            //adding new book, or updating exiting?
            if (bookId == 0)
            {
                book = new Book()
                {
                    Name = readingListEntry.Name,
                    ISBN = readingListEntry.ISBN,
                    Author = author,
                    Sequence = readingListEntry.Sequence,
                    Recommend = readingListEntry.Recommend,
                    Source = source,
                    ImageUrl = readingListEntry.ImageUrl ?? Book.DefaultCoverImageUrl
                };

                book.BookReadDates = [];
                book.BookReadDates.Add(new BookReadDate() { Book = book, ReadDate = readingListEntry.ReadDate });

                foreach (var tagEntry in tagList)
                {
                    Tag? tag = dataContext.Tags.Find(tagEntry.Value);
                    if (tag != null)
                    {
                        book.BookTags = [new BookTag { Book = book, Tag = tag }];
                    }
                }

                dataContext.Books.Add(book);
                dataContext.SaveChanges();

                bookId = book.BookId;
            }
            else
            {
                book = await (from b in dataContext.Books
                            .Include("Tags")
                              where b.BookId == bookId
                              select b).FirstAsync();
                book.ISBN = readingListEntry.ISBN;
                book.Author = author;
                book.Sequence = readingListEntry.Sequence;
                book.Recommend = readingListEntry.Recommend;
                book.Source = source;
                book.ImageUrl = readingListEntry.ImageUrl ?? Book.DefaultCoverImageUrl;


                if (bookReadId == 0)
                {
                    book.BookReadDates ??= [];
                    book.BookReadDates.Add(new BookReadDate() { Book = book, ReadDate = readingListEntry.ReadDate });
                }

                book.BookTags ??= [];

                foreach (var tagEntry in tagList)
                {
                    if (!book.BookTags.Any(d => d.Tag.Data == tagEntry.Key))
                    {
                        book.BookTags.Add(new BookTag() { Book = book, Tag = new() { Data = tagEntry.Key } });
                    }
                }

                dataContext.SaveChanges();
            }


            logger.LogDebug("added/updated book: {book.Name}", book.Name);

            return book.ToBookDTO();
        }






        public IEnumerable<BookDTO> GetBooks()
        {
            logger.LogDebug("GetBooks");

            var result = (from b in dataContext.Books
                            .Include(b => b.Author)
                            .Include(b => b.Source)
                            .Include(b => b.BookTags)
                            .OrderBy(b => b.Name)
                            select new
                            {
                                b.BookId,
                                b.Name,
                                b.ISBN,
                                Author = b.Author == null ? string.Empty : b.Author.Name,
                                b.Sequence,
                                b.Recommend,
                                ReadDates = from brd in b.BookReadDates
                                            select brd.ReadDate.ToString("yyyy-MM-dd"),
                                Tags = from bt in b.BookTags
                                       select bt.Tag.Data,
                                Source = b.Source == null ? string.Empty : b.Source.Name,
                                ImageUrl = b.ImageUrl == null ? "http://2.bp.blogspot.com/_aDCnyPs488U/SyAtBDSHFHI/AAAAAAAAGDI/tFkGgFeISHI/s400/BookCoverGreenBrown.jpg" : b.ImageUrl
                            }).ToList()
                            .Select(b => new BookDTO
                            {
                                Id = b.BookId,
                                Name = b.Name,
                                ISBN = b.ISBN,
                                Author = b.Author,
                                Sequence = b.Sequence,
                                Recommend = b.Recommend,
                                ReadDates = b.ReadDates.DefaultIfEmpty(string.Empty).Aggregate((x, y) => (x + "; " + y)),
                                Tags = b.Tags.DefaultIfEmpty(string.Empty).Aggregate((x, y) => (x + "; " + y)),
                                Source = b.Source,
                                ImageUrl = b.ImageUrl
                            });

            return result;
        }


        public async Task<BookDTO?> GetBook(long id)
        {
            logger.LogDebug($"GetBook: {id}");

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
                                  b.Recommend,
                                  ReadDates = from br in b.BookReadDates
                                              select br.ReadDate.ToString("yyyy-MM-dd"),
                                  Tags = from bd in b.BookTags
                                         select bd.Tag.Data,
                                  Source = b.Source == null ? string.Empty : b.Source.Name,
                                  ImageUrl = b.ImageUrl == null ? "http://2.bp.blogspot.com/_aDCnyPs488U/SyAtBDSHFHI/AAAAAAAAGDI/tFkGgFeISHI/s400/BookCoverGreenBrown.jpg" : b.ImageUrl
                              }).FirstOrDefaultAsync();

            if (book == null)
            {
                logger.LogDebug($"book with id: {id} not found");
                return null;
            }

            logger.LogDebug($"returning book: {book.Name}");

            var result = new BookDTO
            {
                Id = book.BookId,
                Name = book.Name,
                ISBN = book.ISBN,
                Author = book.Author,
                Sequence = book.Sequence,
                Recommend = book.Recommend,
                ReadDates = book.ReadDates.DefaultIfEmpty(string.Empty).Aggregate((x, y) => (x + "; " + y)),
                Tags = book.Tags.DefaultIfEmpty(string.Empty).Aggregate((x, y) => (x + "; " + y)),
                Source = book.Source,
                ImageUrl = book.ImageUrl
            };

            return result;
        }


        public async Task<BookDTO> AddBook(BookBindingTarget newBook)
        {
            logger.LogDebug($"AddBook: {JsonSerializer.Serialize(newBook)}");

            //is this a new author, or one already in the database?
            var authorId = await (from a in dataContext.Authors
                                  where a.Name == newBook.Author
                                  select a.AuthorId).FirstOrDefaultAsync();

            if (!string.IsNullOrWhiteSpace(newBook.Author) && authorId == 0)
            {
                Author newAuthor = new Author() { Name = newBook.Author };

                await dataContext.Authors.AddAsync(newAuthor);
                await dataContext.SaveChangesAsync();

                authorId = newAuthor.AuthorId;
            }


            var sourceId = await (from s in dataContext.Sources
                                  where s.Name == newBook.Source
                                  select s.SourceId).FirstOrDefaultAsync();


            if (!string.IsNullOrWhiteSpace(newBook.Source) && sourceId == 0)
            {
                Source newSource = new Source() { Name = newBook.Source };

                await dataContext.Sources.AddAsync(newSource);
                await dataContext.SaveChangesAsync();

                sourceId = newSource.SourceId;
            }


            Dictionary<string, long?> tagList = new Dictionary<string, long?>();

            //new or existing tags?
            if (!string.IsNullOrWhiteSpace(newBook.Tags))
            {
                foreach (var item in newBook.Tags.Split(';'))
                {
                    var tagId = await (from t in dataContext.Tags
                                       where t.Data == item.Trim()
                                       select t.TagId).FirstOrDefaultAsync();

                    tagList.Add(item.Trim(), tagId);
                }

                foreach (var tagEntry in tagList)
                {
                    if (tagEntry.Value == 0)
                    {
                        Tag newTag = new Tag() { Data = tagEntry.Key };

                        await dataContext.Tags.AddAsync(newTag);
                        await dataContext.SaveChangesAsync();

                        tagList[tagEntry.Key] = newTag.TagId;
                    }
                }
            }


            Author? author = await dataContext.Authors.FindAsync(authorId);
            Source? source = await dataContext.Sources.FindAsync(sourceId);


            Book book = new Book()
            {
                ISBN = newBook.ISBN,
                Name = newBook.Name,
                Author = author,
                Sequence = newBook.Sequence,
                Recommend = newBook.Recommend,
                Source = source,
                ImageUrl = newBook.ImageUrl ?? "http://2.bp.blogspot.com/_aDCnyPs488U/SyAtBDSHFHI/AAAAAAAAGDI/tFkGgFeISHI/s400/BookCoverGreenBrown.jpg"
            };


            var bookTags = new Collection<BookTag>();

            foreach (var tagEntry in tagList)
            {
                Tag? tag = await dataContext.Tags.FindAsync(tagEntry.Value);
                if (tag != null)
                {
                    bookTags.Add(new BookTag { Tag = tag, Book = book });
                }
            }
            book.BookTags = bookTags;


            if (!string.IsNullOrWhiteSpace(newBook.ReadDates))
            {
                var bookReadDates = new Collection<BookReadDate>();

                foreach (var item in newBook.ReadDates.Split(';'))
                {
                    if (DateTime.TryParse(item.Trim(), out DateTime readDate))
                    {
                        bookReadDates.Add(new BookReadDate { ReadDate = readDate, Book = book });
                    }
                }
                book.BookReadDates = bookReadDates;
            }


            await dataContext.Books.AddAsync(book);
            await dataContext.SaveChangesAsync();


            logger.LogDebug($"added book: {book.Name}");

            return book.ToBookDTO();
        }


        public async Task<BookDTO?> UpdateBook(BookUpdateBindingTarget changedBook)
        {
            logger.LogDebug($"UpdateBook: {JsonSerializer.Serialize(changedBook)}");

            Book? book = await dataContext.Books
                        .Include(b => b.Author)
                        .Include(b => b.Source)
                        .Include(b => b.BookTags)
                        .Include(b => b.BookReadDates)
                        .FirstOrDefaultAsync(b => b.BookId == changedBook.Id);

            if (book == null)
            {
                logger.LogDebug($"No book with given ID: {changedBook.Id} is available to update.");
                return null;
            }


            //is this a new author, or one already in the database?
            var authorId = await (from a in dataContext.Authors
                                  where a.Name == changedBook.Author
                                  select a.AuthorId).FirstOrDefaultAsync();

            if (!string.IsNullOrWhiteSpace(changedBook.Author) && authorId == 0)
            {
                Author newAuthor = new Author() { Name = changedBook.Author };

                await dataContext.Authors.AddAsync(newAuthor);
                await dataContext.SaveChangesAsync();

                authorId = newAuthor.AuthorId;
            }



            var sourceId = await (from s in dataContext.Sources
                                  where s.Name == changedBook.Source
                                  select s.SourceId).FirstOrDefaultAsync();

            if (!string.IsNullOrWhiteSpace(changedBook.Source) && sourceId == 0)
            {
                Source newSource = new Source() { Name = changedBook.Source };

                await dataContext.Sources.AddAsync(newSource);
                await dataContext.SaveChangesAsync();

                sourceId = newSource.SourceId;
            }


            Dictionary<string, long?> tagList = new Dictionary<string, long?>();

            //new or existing tags?
            if (!string.IsNullOrWhiteSpace(changedBook.Tags))
            {
                foreach (var item in changedBook.Tags.Split(';'))
                {
                    var tagId = await (from t in dataContext.Tags
                                       where t.Data == item.Trim()
                                       select t.TagId).FirstOrDefaultAsync();

                    tagList.Add(item.Trim(), tagId);
                }

                foreach (var tagEntry in tagList)
                {
                    if (tagEntry.Value == 0)
                    {
                        Tag newTag = new Tag() { Data = tagEntry.Key };

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
            book.Recommend = changedBook.Recommend;
            book.Source = source;
            book.ImageUrl = changedBook.ImageUrl ?? "http://2.bp.blogspot.com/_aDCnyPs488U/SyAtBDSHFHI/AAAAAAAAGDI/tFkGgFeISHI/s400/BookCoverGreenBrown.jpg";


            var bookTags = new Collection<BookTag>();

            foreach (var tagEntry in tagList)
            {
                Tag? tag = await dataContext.Tags.FindAsync(tagEntry.Value);
                if (tag != null)
                {
                    bookTags.Add(new BookTag { Tag = tag , Book = book});
                }
            }
            book.BookTags = bookTags;


            if (!string.IsNullOrWhiteSpace(changedBook.ReadDates))
            {
                var bookReadDates = new Collection<BookReadDate>();

                foreach (var item in changedBook.ReadDates.Split(';'))
                {
                    if (DateTime.TryParse(item.Trim(), out DateTime readDate))
                    {
                        bookReadDates.Add(new BookReadDate { ReadDate = readDate, Book = book });
                    }
                }
                book.BookReadDates = bookReadDates;
            }


            dataContext.Books.Update(book);
            await dataContext.SaveChangesAsync();


            logger.LogDebug($"updated book: {book.Name}");

            return book.ToBookDTO();
        }

        public async Task<Book?> DeleteBook(long id)
        {
            logger.LogDebug($"DeleteBook: {id}");

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
                    logger.LogDebug($"DbUpdateConcurrencyException.");
                    return null;
                }

                logger.LogDebug($"deleted book: {book.Name}");
            }

            return book;
        }



        public IEnumerable<AuthorDTO> GetAuthors()
        {
            logger.LogDebug("GetAuthors");

            var result = (from a in dataContext.Authors
                .Include(a => a.Books)
                .OrderBy(a => a.Name)
                          select new
                          {
                              a.AuthorId,
                              a.Name,
                              Books = from b in a.Books
                                      select b.Name
                          }).ToList()
                .Select(a => new AuthorDTO
                {
                    Id = a.AuthorId,
                    Name = a.Name,
                    Books = a.Books.DefaultIfEmpty(string.Empty).Aggregate((x, y) => (x + "; " + y))
                });

            return result;
        }


        public async Task<AuthorDTO?> GetAuthor(long id)
        {
            logger.LogDebug($"GetAuthor: {id}");

            var author = await (from a in dataContext.Authors
                                .Include(a => a.Books)
                                .Where(a => a.AuthorId == id)
                                select new
                                {
                                    a.AuthorId,
                                    a.Name,
                                    Books = from b in a.Books
                                            select b.Name
                                }).FirstOrDefaultAsync();

            if (author == null)
            {
                logger.LogDebug($"No author with given ID: {id} was found.");
                return null;
            }

            var result = new AuthorDTO
            {
                Id = author.AuthorId,
                Name = author.Name,
                Books = author.Books.DefaultIfEmpty(string.Empty).Aggregate((x, y) => (x + "; " + y))
            };

            logger.LogDebug($"returning author: {author.Name}");

            return result;
        }


        public async Task<AuthorDTO> AddAuthor(AuthorBindingTarget newAuthor)
        {
            logger.LogDebug($"AddAuthor: {JsonSerializer.Serialize(newAuthor)}");

            Author author = newAuthor.ToAuthor();
            await dataContext.Authors.AddAsync(author);
            await dataContext.SaveChangesAsync();

            logger.LogDebug($"added author: {author.Name}");

            return author.ToAuthorDTO();
        }


        public async Task<AuthorDTO?> UpdateAuthor(AuthorUpdateBindingTarget changedAuthor)
        {
            logger.LogDebug($"UpdateAuthor: {JsonSerializer.Serialize(changedAuthor)}");

            Author? author = await dataContext.Authors.FindAsync(changedAuthor.Id);

            if (author == null)
            {
                logger.LogDebug($"No author with given ID: {changedAuthor.Id} was found to update.");
                return null;
            }

            author.Name = changedAuthor.Name;
            dataContext.Authors.Update(author);
            await dataContext.SaveChangesAsync();

            logger.LogDebug($"updated author: {author.Name}");

            return author.ToAuthorDTO();
        }


        public async Task<Author?> DeleteAuthor(long id)
        {
            logger.LogDebug($"DeleteAuthor: {id}");

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
                    logger.LogDebug($"DbUpdateConcurrencyException.");
                    return null;
                }

                logger.LogDebug($"deleted author: {author.Name}");
            }

            return author;
        }


        public IEnumerable<TagDTO> GetTags()
        {
            logger.LogDebug("GetTags");

            var result = (from t in dataContext.Tags
                            .Include(bt => bt.BookTags!)
                            .ThenInclude(b => b.Book)
                            .OrderBy(t => t.Data)
                            select new
                            {
                                t.TagId,
                                t.Data,
                                Books = string.Join("; ",t.BookTags!.Select(b => b.Book.Name))
                            }).ToList()
                            .Select(t => new TagDTO
                            {
                                Id = t.TagId,
                                Data = t.Data,
                                Books = t.Books
                            });

            return result;
        }


        public async Task<TagDTO?> GetTag(long id)
        {
            logger.LogDebug($"GetTag: {id}");

            var tag = await (from t in dataContext.Tags
                             .Include(bt => bt.BookTags!)
                             .ThenInclude(b => b.Book)
                             .Where(t => t.TagId == id)
                             select new
                             {
                                 t.TagId,
                                 t.Data,
                                 Books = string.Join("; ", t.BookTags!.Select(b => b.Book.Name))
                             }).FirstOrDefaultAsync();

            if (tag == null)
            {
                logger.LogDebug($"No tag with given ID: {id} was found.");
                return null;
            }

            var result = new TagDTO
            {
                Id = tag.TagId,
                Data = tag.Data,
                Books = tag.Books
            };

            logger.LogDebug($"returning tag: {tag.Data}");

            return result;
        }


        public async Task<TagDTO> AddTag(TagBindingTarget newTag)
        {
            logger.LogDebug($"AddTag: {JsonSerializer.Serialize(newTag)}");

            Tag tag = newTag.ToTag();
            await dataContext.Tags.AddAsync(tag);
            await dataContext.SaveChangesAsync();

            logger.LogDebug($"added tag: {tag.Data}");

            return tag.ToTagDTO();
        }


        public async Task<TagDTO?> UpdateTag(TagUpdateBindingTarget changedTag)
        {
            logger.LogDebug($"UpdateDatum: {JsonSerializer.Serialize(changedTag)}");

            Tag? tag = await dataContext.Tags.FindAsync(changedTag.Id);

            if (tag == null)
            {
                logger.LogDebug($"No tag with given ID: {changedTag.Id} was found to update.");
                return null;
            }

            tag.Data = changedTag.Data;
            dataContext.Tags.Update(tag);
            await dataContext.SaveChangesAsync();

            logger.LogDebug($"updated tag: {tag.Data}");

            return tag.ToTagDTO();
        }


        public async Task<Tag?> DeleteTag(long id)
        {
            logger.LogDebug($"DeleteTag: {id}");

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
                    logger.LogDebug($"DbUpdateConcurrencyException.");
                    return null;
                }

                logger.LogDebug($"deleted tag: {tag.Data}");
            }

            return tag;
        }



        public IEnumerable<SourceDTO> GetSources()
        {
            logger.LogDebug("GetSources");

            var result = (from s in dataContext.Sources
                            .Include(s => s.Books)
                            .OrderBy(s => s.Name)
                            select new
                            {
                                s.SourceId,
                                s.Name,
                                Books = from b in s.Books
                                        select b.Name
                            }).ToList()
                            .Select(s => new SourceDTO
                            {
                                Id = s.SourceId,
                                Name = s.Name,
                                Books = s.Books.DefaultIfEmpty(string.Empty).Aggregate((x, y) => (x + "; " + y))
                            });

            return result;
        }

        public async Task<SourceDTO?> GetSource(long id)
        {
            logger.LogDebug($"GetSource: {id}");

            var source = await (from s in dataContext.Sources
                                .Include(b => b.Books)
                                .Where(s => s.SourceId == id)
                                select new
                                {
                                    s.SourceId,
                                    s.Name,
                                    Books = from b in s.Books
                                            select b.Name
                                }).FirstOrDefaultAsync();

            if (source == null)
            {
                logger.LogDebug($"no source found with id: {id}");
                return null;
            }

            var result = new SourceDTO
            {
                Id = source.SourceId,
                Name = source.Name,
                Books = source.Books.DefaultIfEmpty(string.Empty).Aggregate((x, y) => (x + "; " + y))
            };

            logger.LogDebug($"returning source: {source.Name}");

            return result;
        }


        public async Task<SourceDTO> AddSource(SourceBindingTarget newSource)
        {
            logger.LogDebug($"AddSource: {JsonSerializer.Serialize(newSource)}");

            Source source = newSource.ToSource();
            await dataContext.Sources.AddAsync(source);
            await dataContext.SaveChangesAsync();

            logger.LogDebug($"added source: {source.Name}");

            return source.ToSourceDTO();
        }


        public async Task<SourceDTO?> UpdateSource(SourceUpdateBindingTarget changedSource)
        {
            logger.LogDebug($"UpdateSource: {JsonSerializer.Serialize(changedSource)}");

            Source? source = await dataContext.Sources.FindAsync(changedSource.Id);

            if (source == null)
            {
                logger.LogDebug($"No source with given ID: {changedSource.Id} was found to update.");
                return null;
            }

            source.Name = changedSource.Name;
            dataContext.Sources.Update(source);
            await dataContext.SaveChangesAsync();

            logger.LogDebug($"updated source: {source.Name}");

            return source.ToSourceDTO();
        }


        public async Task<Source?> DeleteSource(long id)
        {
            logger.LogDebug($"DeleteSource: {id}");

            Source? source = await dataContext.Sources.FindAsync(id);
            if (source != null)
            {
                dataContext.Sources.Remove(source);
                try
                {
                    await dataContext.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    logger.LogDebug($"DbUpdateConcurrencyException.");
                    return null;
                }

                logger.LogDebug($"deleted source: {source.Name}");
            }

            return source;
        }
    }
}
