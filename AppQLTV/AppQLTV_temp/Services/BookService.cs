using AppQLTV.Models;
using Microsoft.JSInterop;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace AppQLTV.Services
{
    public class BookService : IBookService
    {
        private readonly HttpClient _httpClient;
        private readonly IJSRuntime _jsRuntime;

        public BookService(HttpClient httpClient, IJSRuntime jsRuntime)
        {
            _httpClient = httpClient;
            _jsRuntime = jsRuntime;
        }

        private async Task AddAuthorizationHeader()
        {
            var token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "authToken");
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }
        }

        public async Task<List<Book>> GetBooksAsync()
        {
            await AddAuthorizationHeader();
            return await _httpClient.GetFromJsonAsync<List<Book>>("api/books");
        }

        public async Task<Book?> GetBookByIdAsync(int id)
        {
            await AddAuthorizationHeader();
            return await _httpClient.GetFromJsonAsync<Book>($"api/books/{id}");
        }

        public async Task<List<Book>> SearchBooksAsync(string keyword)
        {
            await AddAuthorizationHeader();
            return await _httpClient.GetFromJsonAsync<List<Book>>($"api/books/search?keyword={keyword}");
        }

        public async Task<Book> CreateBookAsync(Book book)
        {
            await AddAuthorizationHeader();
            var response = await _httpClient.PostAsJsonAsync("api/books", book);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Book>();
        }

        public async Task UpdateBookAsync(Book book)
        {
            await AddAuthorizationHeader();
            var response = await _httpClient.PutAsJsonAsync($"api/books/{book.BookId}", book);
            response.EnsureSuccessStatusCode();
        }

        public async Task DeleteBookAsync(int id)
        {
            await AddAuthorizationHeader();
            var response = await _httpClient.DeleteAsync($"api/books/{id}");
            response.EnsureSuccessStatusCode();
        }
    }
}