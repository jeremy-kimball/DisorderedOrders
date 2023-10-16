using DisorderedOrdersMVC.DataAccess;
using DisorderedOrdersMVC.Services;

namespace DisorderedOrdersMVC.Models
{
    public class Order
    {
        public int Id { get; set; }
        public Customer Customer { get; set; }
        public List<OrderItem> Items { get; set; } = new List<OrderItem>();

        public Order()
        {
            
        }

        //create order
        public Order(IFormCollection collection, DisorderedOrdersContext _context)
        {
			int customerId = Convert.ToInt16(collection["CustomerId"]);
			Customer customer = _context.Customers.Find(customerId);
			Items = new List<OrderItem>();
			for (var i = 1; i < collection.Count - 1; i++)
			{
				var kvp = collection.ToList()[i];
				if (kvp.Value != "0")
				{
					var product = _context.Products.Where(p => p.Name == kvp.Key).First();
					var orderItem = new OrderItem() { Item = product, Quantity = Convert.ToInt32(kvp.Value) };
					Items.Add(orderItem);
				}
			}
			Customer = customer;
		}

        public void verifyStock()
		{
			foreach (var orderItem in Items)
			{
				if (!orderItem.Item.InStock(orderItem.Quantity))
				{
					orderItem.Quantity = orderItem.Item.StockQuantity;
				}

				orderItem.Item.DecreaseStock(orderItem.Quantity);
			}
		}

		public int calculateTotalPrice()
		{
			var total = 0;
			foreach (var orderItem in Items)
			{
				var itemPrice = orderItem.Item.Price * orderItem.Quantity;
				total += itemPrice;
			}
			return total;
		}

		//process payment
		public void processPayment(DisorderedOrdersContext _context, int total, string paymentType)
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

			_context.Orders.Add(this);
			_context.SaveChanges();
		}
	}
}
