using System.Net.Http.Json;
using AppQLTV.Models;

namespace AppQLTV.Services
{
    public class BookService : IBookService
    {
        private readonly HttpClient _http;
        public BookService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<Book>> GetBooksAsync()
        {
            var result = await _http.GetFromJsonAsync<List<Book>>("api/books");
            return result ?? new List<Book>();
        }

        public async Task<Book?> GetBookByIdAsync(int id)
        {
            return await _http.GetFromJsonAsync<Book>($"api/books/{id}");
        }

        public async Task<List<Book>> SearchBooksAsync(string keyword)
        {
            var result = await _http.GetFromJsonAsync<List<Book>>($"api/books/search?keyword={Uri.EscapeDataString(keyword)}");
            return result ?? new List<Book>();
        }

        public async Task<Book> CreateBookAsync(Book book)
        {
            var response = await _http.PostAsJsonAsync("api/books", book);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Book>() ?? book;
        }

        public async Task UpdateBookAsync(Book book)
        {
            var response = await _http.PutAsJsonAsync($"api/books/{book.BookId}", book);
            response.EnsureSuccessStatusCode();
        }

        public async Task DeleteBookAsync(int id)
        {
            await _http.DeleteAsync($"api/books/{id}");
        }
    }
}