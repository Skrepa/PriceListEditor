using System.ComponentModel.DataAnnotations;

namespace PriceListEditor.Models
{
    // Модель для прайс-листа
    public class PriceList
    {
        public int Id { get; set; } 

        [Required(ErrorMessage = "Название прайс-листа обязательно")]    
        public string Name { get; set; } 

        public ICollection<Product> Products { get; set; } = new List<Product>();

        public ICollection<PriceListColumn> PriceListColumns { get; set; } = new List<PriceListColumn>();
    }
}
