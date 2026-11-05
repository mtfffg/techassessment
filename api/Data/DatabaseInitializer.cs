using Microsoft.EntityFrameworkCore;

namespace Data;

public static class DatabaseInitializer
{
    public static async Task Initialize(BookStoreDbContext db)
    {
        await db.Database.EnsureCreatedAsync();
    }
}
