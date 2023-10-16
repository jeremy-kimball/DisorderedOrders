using DisorderedOrdersMVC.DataAccess;
using DisorderedOrdersMVC.Models;
using DisorderedOrdersMVC.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DisorderedOrdersMVC.Controllers
{
    public class OrdersController : Controller
    {
        private readonly DisorderedOrdersContext _context;

        public OrdersController(DisorderedOrdersContext context)
        {
            _context = context;
        }

        public IActionResult New(int customerId)
        {
            var products = _context.Products.Where(p => p.StockQuantity > 0);
            ViewData["CustomerId"] = customerId;

            return View(products);
        }

        [HttpPost]
        [Route("/orders")]
        public IActionResult Create(IFormCollection collection, string paymentType)
        {
            // create order
            var order = new Order(collection, _context);
            // verify stock available
            order.verifyStock();
            // calculate total price
            var total = order.calculateTotalPrice();
            // process payment
            order.processPayment(_context, total, paymentType);

			return RedirectToAction("Show", new { id = order.Id });
		}

        [Route("/orders/{id:int}")]
        public IActionResult Show(int id)
        {
            var order = _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.Items)
                    .ThenInclude(i => i.Item)
                .Where(o => o.Id == id).First();

            var total = 0;
            foreach (var orderItem in order.Items)
            {
                var itemPrice = orderItem.Item.Price * orderItem.Quantity;
                total += itemPrice;
            }
            ViewData["total"] = total;

            return View(order);
        }
	}
}
