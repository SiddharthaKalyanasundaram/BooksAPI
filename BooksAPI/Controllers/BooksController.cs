using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using BooksAPI.Models;
using System.Linq.Expressions;
using BooksAPI.DTOs;
using System.Threading.Tasks;

namespace BooksAPI.Controllers
{
    [RoutePrefix("api/books")]
    public class BooksController : ApiController
    {
        private BooksAPIContext db = new BooksAPIContext();

        // Typed lambda expression for Select() method. 
        private static readonly Expression<Func<Book, BookDTO>> AsBookDto =
            x => new BookDTO
            {
                Title = x.Title,
                Author = x.Author.Name,
                Genre = x.Genre
            };

        // GET api/Books
        [Route("")]
        public IQueryable<BookDTO> GetBooks()
        {
            return db.Books.Include(b => b.Author).Select(AsBookDto);
        }

        // GET api/Books/5
        [Route("{id:int}")]
        [ResponseType(typeof(BookDTO))]
        public async Task<IHttpActionResult> GetBook(int id)
        {
            BookDTO book = await db.Books.Include(b => b.Author)
                .Where(b => b.BookID == id)
                .Select(AsBookDto)
                .FirstOrDefaultAsync();
            if (book == null)
            {
                return NotFound();
            }

            return Ok(book);
        }

        [Route("{id:int}/details")]
        [ResponseType(typeof(BookDetailsDTO))]
        public async Task<IHttpActionResult> GetBookDetail(int id)
        {
            var book = await (from b in db.Books.Include(b => b.Author)
                              where b.AuthorId == id
                              select new BookDetailsDTO
                              {
                                  Title = b.Title,
                                  Genre = b.Genre,
                                  PublishDate = b.PublishDate,
                                  Price = b.Price,
                                  Description = b.Description,
                                  Author = b.Author.Name
                              }).FirstOrDefaultAsync();

            if (book == null)
            {
                return NotFound();
            }
            return Ok(book);
        }

        [Route("{genre}")]
        public IQueryable<BookDTO> GetBooksByGenre(string genre)
        {
            return db.Books.Include(b => b.Author)
                .Where(b => b.Genre.Equals(genre, StringComparison.OrdinalIgnoreCase))
                .Select(AsBookDto);
        }

        [Route("~api/authors/{authorId}/books")]
        public IQueryable<BookDTO> GetBooksByAuthor(int authorId)
        {
            return db.Books.Include(b => b.Author)
                .Where(b => b.AuthorId == authorId)
                .Select(AsBookDto);
        }

        [Route("date/{pubdate:datetime:regex(\\d{4}-\\d{2}-\\d{2})}")]
        [Route("date/{*pubdate:datetime:regex(\\d{4}/\\d{2}/\\d{2})}")]
        public IQueryable<BookDTO> GetBooks(DateTime pubdate)
        {
            return db.Books.Include(b => b.Author)
                .Where(b => DbFunctions.TruncateTime(b.PublishDate)
                    == DbFunctions.TruncateTime(pubdate))
                .Select(AsBookDto);
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}
