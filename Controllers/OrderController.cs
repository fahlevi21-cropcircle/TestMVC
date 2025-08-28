using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TestMVC.Data;
using TestMVC.Models;

namespace TestMVC.Controllers
{
    public enum QtyMode
    {
        Increase,
        Decrease
    };

    [Authorize]
    public class OrderController : Controller
    {
        private readonly DatabaseContext _context;

        public OrderController(DatabaseContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var data = await _context.Transaction.Include(x => x.Orders).ThenInclude(x => x.Product).FirstOrDefaultAsync(x => x.Status == "D");

            var list = await _context.Transaction.Include(x => x.Orders).ThenInclude(x => x.Product).Where(x => x.Status == "A").ToListAsync();
            ViewData["list"] = list;

            return View(data);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Order data)
        {

            var transaction = await _context.Transaction.Include(c => c.Orders).FirstOrDefaultAsync(x => x.Status == "D");
            if (transaction == null)
            {
                transaction = new Transaction();
                transaction.Status = "D";
                transaction.CreatedAt = DateTime.Now;
                transaction.Orders = new List<Order>();
                _context.Add(transaction);
            }

            var order = transaction.Orders.FirstOrDefault(x => x.ProductId == data.ProductId);
            if (order == null)
            {
                order = data;
                order.Qty = 1;
                order.TotalAmount = data.Product.Price;
                transaction.Orders.Add(order);

                //mark Product entity as unchanged
                var entity = _context.Entry(order.Product);
                entity.State = EntityState.Unchanged;
            }
            else
            {
                order.Qty++;
                order.TotalAmount += data.Product.Price;
            }


            await _context.SaveChangesAsync();


            return RedirectToRoute("Index","Product");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateQty(int orderId, QtyMode mode)
        {
            var order = await _context.Order.Include(x => x.Product).FirstOrDefaultAsync(x => x.Id == orderId);

            if (order == null) return BadRequest("Order is not found/deleted");

            if (mode == QtyMode.Increase)
            {
                order.Qty++;
                order.TotalAmount += order.Product.Price;
            }
            else
            {
                order.Qty--;
                order.TotalAmount -= order.Product.Price;

                if (order.Qty == 0) _context.Remove(order);
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int orderId)
        {
            var order = await _context.Order.FindAsync(orderId);
            if (order == null) return BadRequest("Order is not found/deleted");

            _context.Order.Remove(order);

            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> SaveTrans(int id)
        {
            var tr = await _context.Transaction.FindAsync(id);
            if (tr == null) return BadRequest("No open transaction found");

            tr.CreatedAt = DateTime.Now;
            tr.Status = "A";

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> CancelTrans(int id)
        {
            var tr = await _context.Transaction.FindAsync(id);
            if (tr == null) return BadRequest("No open transaction found");

            tr.CreatedAt = DateTime.Now;
            tr.Status = "Z";

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
