namespace DisorderedOrdersMVC.Services
{
    public class PayPalProcessor : IPaymentProcessor, IRefundProcessor
	{
        public bool ProcessPayment(int amount)
        {
            // payment processing.  This would be an API call to paypal with payment details.

            return true;
        }

        public bool ProcessRefund(int amount)
        {
            // refund processing.  This would be an API call to paypal with refund details.

            return true;
        }
    }
}
