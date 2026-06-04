using AppQLTV.Models;

namespace AppQLTV.Services
{
    public interface IBookService
    {
        Task<List<Book>> GetBooksAsync();
        Task<Book?> GetBookByIdAsync(int id);
        Task<List<Book>> SearchBooksAsync(string keyword);
        Task<Book> CreateBookAsync(Book book);
        Task UpdateBookAsync(Book book);
        Task DeleteBookAsync(int id);
    }
}