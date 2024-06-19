using Microsoft.EntityFrameworkCore; 

namespace PriceListEditor.Models
{
    // Контекст базы данных
    public class ApplicationDbContext : DbContext
    {
        // Конструктор, принимающий опции для контекста
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<PriceList> PriceLists { get; set; }
        
        public DbSet<Product> Products { get; set; }
        
        public DbSet<PriceListColumn> PriceListColumns { get; set; }

    }
}
