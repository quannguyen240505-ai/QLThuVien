using APIQLTV.DTOs.Books;
using APIQLTV.Models;
using APIQLTV.Models.Books;
using Microsoft.EntityFrameworkCore;


namespace APIQLTV.Services
{
    public class BookService : IBookService
    {
        private readonly AppDbContext _context;
        public BookService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<BookResponseDTO>> GetAllBooksAsync()
        {
            var books = await _context.Books.ToListAsync();
            return books.Select(b => MapToResponse(b));
        }

        public async Task<BookResponseDTO?> GetBookByIdAsync(int id)
        {
            var book = await _context.Books.FindAsync(id);
            return book == null ? null : MapToResponse(book);
        }

        public async Task<IEnumerable<BookResponseDTO>> SearchBooksAsync(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return await GetAllBooksAsync();

            var books = await _context.Books
                .Where(b => b.Title.Contains(keyword) ||
                            b.Author.Contains(keyword) ||
                            b.Category.Contains(keyword))
                .ToListAsync();
            return books.Select(MapToResponse);
        }

        public async Task<BookResponseDTO> CreateBookAsync(BookCreateUpdateDTO dto)
        {
            var book = new Book
            {
                Title = dto.Title,
                Author = dto.Author,
                Category = dto.Category,
                Publisher = dto.Publisher,
                PublishYear = dto.PublishYear,
                Quantity = dto.Quantity,
                AvailableQuantity = dto.Quantity, // ban đầu bằng tổng số
                Description = dto.Description,
                ImageUrl = dto.ImageUrl
            };
            _context.Books.Add(book);
            await _context.SaveChangesAsync();
            return MapToResponse(book);
        }

        public async Task<bool> UpdateBookAsync(int id, BookCreateUpdateDTO dto)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null) return false;

            var oldQuantity = book.Quantity;
            book.Title = dto.Title;
            book.Author = dto.Author;
            book.Category = dto.Category;
            book.Publisher = dto.Publisher;
            book.PublishYear = dto.PublishYear;
            book.Quantity = dto.Quantity;
            book.Description = dto.Description;
            book.ImageUrl = dto.ImageUrl;

            // điều chỉnh AvailableQuantity dựa trên chênh lệch
            var delta = dto.Quantity - oldQuantity;
            book.AvailableQuantity += delta;
            if (book.AvailableQuantity < 0) book.AvailableQuantity = 0;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteBookAsync(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null) return false;
            _context.Books.Remove(book);
            await _context.SaveChangesAsync();
            return true;
        }

        private BookResponseDTO MapToResponse(Book book)
        {
            return new BookResponseDTO
            {
                BookId = book.BookId,
                Title = book.Title,
                Author = book.Author,
                Category = book.Category,
                Publisher = book.Publisher,
                PublishYear = book.PublishYear,
                Quantity = book.Quantity,
                AvailableQuantity = book.AvailableQuantity,
                Description = book.Description,
                ImageUrl = book.ImageUrl
            };
        }
    }
}