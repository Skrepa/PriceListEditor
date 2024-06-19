using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PriceListEditor.Models;

namespace PriceListEditor.Controllers
{
    // Контроллер для управления прайс-листами
    public class PriceListsController : Controller
    {
        private readonly ApplicationDbContext _context; 

        // Конструктор контроллера
        public PriceListsController(ApplicationDbContext context)
        {
            _context = context; 
        }

        // Метод для отображения списка прайс-листов
        public async Task<IActionResult> Index()
        {
            return View(await _context.PriceLists.Include(pl => pl.Products).ToListAsync()); 
        }

        
        [HttpGet]
        public IActionResult Create()
        {
            var model = new PriceList();

            var existingColumns = _context.PriceListColumns
            .GroupBy(c => c.Name)
            .Select(g => g.First())
            .ToList();

            ViewBag.ExistingColumns = existingColumns; 

            return View(model);
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PriceList priceList, List<int> selectedColumnIds, List<PriceListColumn> newColumns)
        {
            if (ModelState.IsValid)
            {
                //Добавление выбранных существующих колонок
                if (selectedColumnIds != null && selectedColumnIds.Any())
                {
                    foreach (var columnId in selectedColumnIds)
                    {
                        var existingColumn = await _context.PriceListColumns.FindAsync(columnId);
                        if (existingColumn != null)
                        {
                            priceList.PriceListColumns.Add(existingColumn);
                        }
                    }
                }

                // Добавление новых колонок
                if (newColumns != null && newColumns.Any())
                {
                    foreach (var column in newColumns)
                    {
                        priceList.PriceListColumns.Add(column);
                    }
                }

                _context.Add(priceList);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // Загрузите существующие колонки для отображения
            ViewBag.ExistingColumns = await _context.PriceListColumns.ToListAsync();

            return View(priceList);
        }
    }
}
