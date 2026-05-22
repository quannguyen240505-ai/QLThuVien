using APIQLTV.Models;
using APIQLTV.Models.Books;
using Microsoft.EntityFrameworkCore;

namespace APIQLTV.Data
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(AppDbContext context)
        {
            // Tự động tạo database nếu chưa có
            await context.Database.EnsureCreatedAsync();

            // Kiểm tra nếu bảng Books đã có dữ liệu thì bỏ qua
            if (context.Books.Any()) return;

            var books = new List<Book>
            {
                new() { Title = "Nhà giả kim", Author = "Paulo Coelho", Category = "Tiểu thuyết", Publisher = "NXB Hội Nhà Văn", PublishYear = 2015, Quantity = 10, AvailableQuantity = 10, Description = "Cuốn sách về hành trình tìm kiếm kho báu và lẽ sống.", ImageUrl = "https://example.com/nha-gia-kim.jpg" },
                new() { Title = "Đắc nhân tâm", Author = "Dale Carnegie", Category = "Kỹ năng sống", Publisher = "NXB Tổng hợp TP.HCM", PublishYear = 2016, Quantity = 15, AvailableQuantity = 12, Description = "Nghệ thuật ứng xử và gây ảnh hưởng đến người khác.", ImageUrl = "https://example.com/dac-nhan-tam.jpg" },
                new() { Title = "Lập trình C#", Author = "Nguyễn Văn A", Category = "Công nghệ thông tin", Publisher = "NXB Khoa học Kỹ thuật", PublishYear = 2020, Quantity = 5, AvailableQuantity = 5, Description = "Hướng dẫn lập trình C# từ cơ bản đến nâng cao.", ImageUrl = "https://example.com/csharp.jpg" },
                new() { Title = "Clean Code", Author = "Robert C. Martin", Category = "Công nghệ thông tin", Publisher = "Prentice Hall", PublishYear = 2008, Quantity = 7, AvailableQuantity = 7, Description = "Nguyên tắc viết mã nguồn sạch và dễ bảo trì.", ImageUrl = "https://example.com/clean-code.jpg" },
                new() { Title = "Sapiens: Lược sử loài người", Author = "Yuval Noah Harari", Category = "Lịch sử", Publisher = "NXB Thế giới", PublishYear = 2017, Quantity = 8, AvailableQuantity = 8, Description = "Hành trình của nhân loại từ thời tiền sử đến hiện đại.", ImageUrl = "https://example.com/sapiens.jpg" },
                new() { Title = "Atomic Habits", Author = "James Clear", Category = "Phát triển bản thân", Publisher = "Random House", PublishYear = 2018, Quantity = 12, AvailableQuantity = 10, Description = "Thay đổi thói quen nhỏ để thành công lớn.", ImageUrl = "https://example.com/atomic-habits.jpg" },
                new() { Title = "The Silent Patient", Author = "Alex Michaelides", Category = "Tâm lý", Publisher = "Celadon Books", PublishYear = 2019, Quantity = 6, AvailableQuantity = 6, Description = "Bí ẩn về một nữ họa sĩ đột nhiên im lặng.", ImageUrl = "https://example.com/silent-patient.jpg" },
                new() { Title = "Đời ngắn đừng ngủ dài", Author = "Robin Sharma", Category = "Kỹ năng sống", Publisher = "NXB Trẻ", PublishYear = 2018, Quantity = 9, AvailableQuantity = 9, Description = "Thức dậy và sống trọn vẹn từng khoảnh khắc.", ImageUrl = "https://example.com/doi-ngan-dung-ngu-dai.jpg" },
                new() { Title = "Thinking, Fast and Slow", Author = "Daniel Kahneman", Category = "Tâm lý học", Publisher = "Farrar, Straus and Giroux", PublishYear = 2011, Quantity = 4, AvailableQuantity = 4, Description = "Hai hệ thống tư duy của con người.", ImageUrl = "https://example.com/thinking-fast-slow.jpg" },
                new() { Title = "The Hobbit", Author = "J.R.R. Tolkien", Category = "Giả tưởng", Publisher = "George Allen & Unwin", PublishYear = 1937, Quantity = 3, AvailableQuantity = 3, Description = "Chuyến phiêu lưu của Bilbo Baggins.", ImageUrl = "https://example.com/hobbit.jpg" }
            };

            context.Books.AddRange(books);
            await context.SaveChangesAsync();
        }
    }
}