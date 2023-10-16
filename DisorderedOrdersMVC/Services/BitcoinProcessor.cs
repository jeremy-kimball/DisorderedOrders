namespace DisorderedOrdersMVC.Services
{
    public class BitcoinProcessor : IPaymentProcessor
    {
        public bool ProcessPayment(int amount)
        {
            // payment processing. This would be an API call to a crypto service that would create a payment transaction.
            
            return true;
        }
    }
}
