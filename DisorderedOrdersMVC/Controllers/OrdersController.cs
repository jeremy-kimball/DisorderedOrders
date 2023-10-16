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
            var order = createOrder(collection, _context);
            // verify stock available
            verifyStock(order);
            // calculate total price
            var total = calculateTotalPrice(order);
            // process payment
            processPayment(_context, order, total, paymentType);

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

        //create order
        static Order createOrder(IFormCollection collection, DisorderedOrdersContext _context)
        {
			int customerId = Convert.ToInt16(collection["CustomerId"]);
			Customer customer = _context.Customers.Find(customerId);
			var order = new Order() { Customer = customer };
			for (var i = 1; i < collection.Count - 1; i++)
			{
				var kvp = collection.ToList()[i];
				if (kvp.Value != "0")
				{
					var product = _context.Products.Where(p => p.Name == kvp.Key).First();
					var orderItem = new OrderItem() { Item = product, Quantity = Convert.ToInt32(kvp.Value) };
					order.Items.Add(orderItem);
				}
			}
            return order;
		}
        //verify stock available
        static void verifyStock(Order order)
        {
			foreach (var orderItem in order.Items)
			{
				if (!orderItem.Item.InStock(orderItem.Quantity))
				{
					orderItem.Quantity = orderItem.Item.StockQuantity;
				}

				orderItem.Item.DecreaseStock(orderItem.Quantity);
			}
		}
        //calculate total price
        static int calculateTotalPrice(Order order)
        {
			var total = 0;
			foreach (var orderItem in order.Items)
			{
				var itemPrice = orderItem.Item.Price * orderItem.Quantity;
				total += itemPrice;
			}
            return total;
		}
		//process payment
		static void processPayment(DisorderedOrdersContext _context, Order order, int total, string paymentType)
		{
            IPaymentProcessor processor;
            if (paymentType == "bitcoin")
            {
                processor = new BitcoinProcessor();
            }
            else if (paymentType == "paypal")
            {
                processor = new PayPalProcessor();
            }
            else
            {
                processor = new CreditCardProcessor();
            }

            processor.ProcessPayment(total);

            _context.Orders.Add(order);
            _context.SaveChanges();
		}
	}
}
