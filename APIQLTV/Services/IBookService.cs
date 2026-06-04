using APIQLTV.DTOs.Books;

namespace APIQLTV.Services
{
    public interface IBookService
    {
        Task<IEnumerable<BookResponseDTO>> GetAllBooksAsync();
        Task<BookResponseDTO?> GetBookByIdAsync(int id);
        Task<IEnumerable<BookResponseDTO>> SearchBooksAsync(string keyword);
        Task<BookResponseDTO> CreateBookAsync(BookCreateUpdateDTO dto);
        Task<bool> UpdateBookAsync(int id, BookCreateUpdateDTO dto);
        Task<bool> DeleteBookAsync(int id);
    }
}