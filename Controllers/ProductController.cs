using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Text.Json;
using TestMVC.Data;
using TestMVC.Models;

namespace TestMVC.Controllers
{
    [Authorize]
    public class ProductController : Controller
    {
        private readonly DatabaseContext _databaseContext;
        private readonly IUtilityService _utilityService;

        public ProductController(DatabaseContext databaseContext, IUtilityService utilityService)
        {
            _databaseContext = databaseContext;
            _utilityService = utilityService;
        }

        public async Task<IActionResult> Index()
        {
            var list = await _databaseContext.Product.Include(c => c.Orders.Where(x => x.Transaction.Status == "D")).ToListAsync();
            ViewData["Title"] = "List Of Products";
            ViewBag.Products = JsonSerializer.Serialize(list, new JsonSerializerOptions
            {
                ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles
            });
            return View(list);
        }

        public async Task<IActionResult> Details(int id)
        {
            var data = await _databaseContext.Product.FindAsync(id);
            ViewData["Title"] = data.Name;
            return View(data);
        }

        public async Task<IActionResult> Form(int id = 0)
        {
            var data = await _databaseContext.Product.FindAsync(id);
            ViewData["Title"] = data?.Name ?? "Create Product";
            return View(data);
        }

        [HttpPost]
        public IActionResult Download([FromBody] List<Product> data)
        {
            Dictionary<string, string> headers = new Dictionary<string, string>
            {
                { "ID", nameof(Product.Id) },
                { "Name", nameof(Product.Name) },
                { "Description", nameof(Product.Description) },
                { "Price", nameof(Product.Price) }
            };
            var bytes = _utilityService.ExportExcel(headers, data, "test.xslx");
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "test.xlsx");
        }

        [HttpPost]
        public IActionResult Pdf([FromBody] List<Product> data)
        {
            Dictionary<string, string> headers = new Dictionary<string, string>
            {
                { "Name", nameof(Product.Name) },
                { "Description", nameof(Product.Description) },
                { "Price", nameof(Product.Price) }
            };
            var bytes = _utilityService.ExportPDF(headers, data, "test.pdf");
            return File(bytes, "application/pdf", "test.pdf");
        }

        public IActionResult Template()
        {
            Dictionary<string, string> headers = new Dictionary<string, string>
            {
                { "Name", nameof(Product.Name) },
                { "Description", nameof(Product.Description) },
                { "Price", nameof(Product.Price) }
            };
            var bytes = _utilityService.ExportExcel<Product>(headers, null, "test.xslx");
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "test.xlsx");
        }

        [HttpPost]
        public async Task<IActionResult> Upload()
        {
            if (Request.Form.Files.Count == 0) return BadRequest("No file provided");
            var file = Request.Form.Files[0];
            string[] allowed = ["xlsx", "xls"];

            if (!allowed.Contains(file.FileName.ToLower().Split('.').LastOrDefault())) return BadRequest("Only excel file is allowed");

            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);

            using var wb = new XLWorkbook(stream);
            var ws = wb.Worksheets.FirstOrDefault();
            if (ws != null)
            {
                var list = new List<Product>();
                foreach (var item in ws.RowsUsed().Skip(1))
                {
                    var data = item.Cells();
                    var prod = new Product
                    {
                        Name = item.Cell(1).GetString(),
                        Description = item.Cell(2).GetString(),
                        Price = Convert.ToDecimal(item.Cell(3).GetDouble())
                    };
                    list.Add(prod);
                }

                _databaseContext.AddRange(list);

                await _databaseContext.SaveChangesAsync();
            }

            stream.Close();
            stream.Dispose();
            wb.Dispose();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Form(Product data)
        {
            if (!ModelState.IsValid) return View();

            EntityEntry<Product> state = _databaseContext.Product.Update(data);

            if (state.State == EntityState.Added)
            {
                bool exist = await _databaseContext.Product.AnyAsync(c => c.Name == data.Name);

                ModelState.AddModelError(nameof(Product.Name), "Product name already exists");
                return View();
            }


            await _databaseContext.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var prod = await _databaseContext.Product.Include(c => c.Orders).FirstOrDefaultAsync(x => x.Id == id);

            if (prod.OrderQty > 0)
            {
                return BadRequest("Product has included in transaction, cannot delete");
            }

            _databaseContext.Product.Remove(prod);

            await _databaseContext.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
