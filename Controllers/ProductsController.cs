using Microsoft.AspNetCore.Mvc; 
using Microsoft.EntityFrameworkCore; 
using Newtonsoft.Json;
using PriceListEditor.Models; 
using X.PagedList;

namespace PriceListEditor.Controllers
{
    // Контроллер для управления товарами
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context; 

        // Конструктор контроллера
        public ProductsController(ApplicationDbContext context)
        {
            _context = context; 
        }

        // Метод для отображения списка товаров
        [HttpGet]
        public async Task<IActionResult> Index(int? priceListId, string sortOrder, int pageNumber = 1, int pageSize = 5)
        {
            return await SortAsync(priceListId, sortOrder, pageNumber, pageSize);
        }

        // Метод для получения данных с сортировкой (AJAX)
        public async Task<IActionResult> SortAsync(int? priceListId, string sortOrder, int pageNumber, int pageSize)
        {
            if (priceListId == null)
            {
                return NotFound();
            }

            var priceList = await _context.PriceLists
                .Include(pl => pl.Products)
                .Include(pl => pl.PriceListColumns)
                .FirstOrDefaultAsync(pl => pl.Id == priceListId);

            if (priceList == null)
            {
                return NotFound();
            }

            //ViewBag.PriceList = priceList;
            ViewBag.PriceListColumns = priceList.PriceListColumns;
            ViewBag.PriceListId = priceListId;
            ViewBag.PriceListName = priceList.Name;


            TempData["PriceListId"] = priceListId;

            var products = priceList.Products.AsQueryable();


            ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewBag.CodeSortParm = sortOrder == "Code" ? "code_desc" : "Code";


            switch (sortOrder)
            {
                case "name_desc":
                    products = products.OrderByDescending(p => p.Name);
                    break;
                case "Code":
                    products = products.OrderBy(p => p.Code);
                    break;
                case "code_desc":
                    products = products.OrderByDescending(p => p.Code);
                    break;
                default:
                    products = products.OrderBy(p => p.Name);
                    break;
            }

            ViewBag.TotalProducts = priceList.Products.Count();

            var pagedProducts = await products.ToPagedListAsync(pageNumber, pageSize);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_ProductList", pagedProducts);
            }

            return View("Index", pagedProducts);
        }

        
        [HttpGet]
        public async Task<IActionResult> Create(int priceListId)
        {
            var priceList = await _context.PriceLists
                .Include(pl => pl.PriceListColumns)
                .FirstOrDefaultAsync(pl => pl.Id == priceListId);

            if (priceList == null)
            {
                return NotFound();
            }

            ViewBag.PriceListColumns = priceList.PriceListColumns; 
            var product = new Product { PriceListId = priceListId };
            return View(product);
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product, Dictionary<string, string> AdditionalColumns)
        {
            if (AdditionalColumns == null && !AdditionalColumns.Any())
            {
                product.AdditionalColumnsJson = "{}"; 
            }
            else
            {
                // Обработка значений дополнительных колонок
                var processedAdditionalColumns = new Dictionary<string, object>();
                foreach (var column in AdditionalColumns)
                {
                    var columnInfo = await _context.PriceListColumns.FirstOrDefaultAsync(c => c.Name == column.Key);
                    if (columnInfo != null)
                    {
                        switch (columnInfo.DataType)
                        {
                            case "int":
                                if (int.TryParse(column.Value, out int intValue))
                                {
                                    processedAdditionalColumns[column.Key] = intValue;
                                }
                                else
                                {
                                    ModelState.AddModelError($"AdditionalColumns[{column.Key}]", $"Поле {column.Key} должно быть числом.");
                                }
                                break;
                            case "string":
                                processedAdditionalColumns[column.Key] = column.Value;
                                break;
                            case "textarea":
                                processedAdditionalColumns[column.Key] = column.Value;
                                break;
                        }
                    }
                }
                product.AdditionalColumns = processedAdditionalColumns.ToDictionary(kv => kv.Key, kv => kv.Value.ToString());
            }

            if (ModelState.IsValid)
            {
                _context.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index), new { priceListId = product.PriceListId });
            }

            // Если модель не валидна, вернуть представление с ошибками
            var priceList = await _context.PriceLists
                .Include(pl => pl.PriceListColumns)
                .FirstOrDefaultAsync(pl => pl.Id == product.PriceListId);

            ViewBag.PriceListColumns = priceList?.PriceListColumns ?? new List<PriceListColumn>();
            return View(product);
        }


        
        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) 
            {
                return NotFound(); 
            }

            var product = await _context.Products
                .FirstOrDefaultAsync(m => m.Id == id); 
            if (product == null) 
            {
                return NotFound(); 
            }

            product.AdditionalColumns = JsonConvert.DeserializeObject<Dictionary<string, string>>(product.AdditionalColumnsJson);

            return View(product); 
        }

        // Метод для удаления товара
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken] 
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id); 
            _context.Products.Remove(product); 
            await _context.SaveChangesAsync(); 
            return RedirectToAction(nameof(Index), new { priceListId = product.PriceListId }); 
        }
    }
}
