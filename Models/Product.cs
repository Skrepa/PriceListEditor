using System.ComponentModel.DataAnnotations; 
using System.ComponentModel.DataAnnotations.Schema; 
using Newtonsoft.Json;

namespace PriceListEditor.Models
{
    // Модель для товара
    public class Product
    {
        public int Id { get; set; } 

        [Required(ErrorMessage = "Название продукта обязательно")] 
        [Display(Name = "Название товара")]
        public string Name { get; set; } 

        [Required(ErrorMessage = "Код продукта обязателен")]
        [Display(Name = "Код товара")]
        public int Code { get; set; } 

        [Required(ErrorMessage = "PriceListId обязателен")]
        public int PriceListId { get; set; } 

        [NotMapped]
        public Dictionary<string, string> AdditionalColumns { get; set; } = new Dictionary<string, string>();

        public string AdditionalColumnsJson
        {
            get => JsonConvert.SerializeObject(AdditionalColumns);
            set => AdditionalColumns = string.IsNullOrEmpty(value) ? new Dictionary<string, string>() : JsonConvert.DeserializeObject<Dictionary<string, string>>(value);
        }
    }
}
