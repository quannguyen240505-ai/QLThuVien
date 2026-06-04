using AppQLTV.Models;
using Microsoft.JSInterop;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace AppQLTV.Services
{
    public class BorrowService : IBorrowService
    {
        private readonly HttpClient _httpClient;
        private readonly IJSRuntime _jsRuntime;

        public BorrowService(HttpClient httpClient, IJSRuntime jsRuntime)
        {
            _httpClient = httpClient;
            _jsRuntime = jsRuntime;
        }

        private async Task AddAuthorizationHeader()
        {
            var token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "authToken");
            if (!string.IsNullOrEmpty(token))
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        public async Task<BorrowRequest> CreateRequestAsync(int bookId, string? notes)
        {
            await AddAuthorizationHeader();
            var response = await _httpClient.PostAsJsonAsync("api/borrow", new { bookId, notes });
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<BorrowRequest>();
        }

        public async Task<List<BorrowRequest>> GetMyRequestsAsync()
        {
            await AddAuthorizationHeader();
            return await _httpClient.GetFromJsonAsync<List<BorrowRequest>>("api/borrow/my-requests");
        }

        public async Task<List<BorrowRequest>> GetPendingRequestsAsync()
        {
            await AddAuthorizationHeader();
            return await _httpClient.GetFromJsonAsync<List<BorrowRequest>>("api/borrow/pending");
        }

        public async Task<List<BorrowRequest>> GetBorrowedRequestsAsync()
        {
            await AddAuthorizationHeader();
            return await _httpClient.GetFromJsonAsync<List<BorrowRequest>>("api/borrow/borrowed");
        }

        public async Task<bool> ApproveRequestAsync(int requestId, bool isApproved, string? rejectReason)
        {
            await AddAuthorizationHeader();
            var response = await _httpClient.PutAsJsonAsync("api/borrow/approve", new { id = requestId, isApproved, rejectReason });
            response.EnsureSuccessStatusCode();
            return true;
        }

        public async Task<bool> ConfirmReturnAsync(int requestId)
        {
            await AddAuthorizationHeader();
            var response = await _httpClient.PutAsJsonAsync($"api/borrow/return/{requestId}", new { });
            response.EnsureSuccessStatusCode();
            return true;
        }

        public async Task<List<BorrowRequest>> GetOverdueRequestsAsync()
        {
            await AddAuthorizationHeader();
            return await _httpClient.GetFromJsonAsync<List<BorrowRequest>>("api/borrow/overdue");
        }
    }
}