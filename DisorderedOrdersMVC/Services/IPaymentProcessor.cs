namespace DisorderedOrdersMVC.Services
{
    public interface IPaymentProcessor
    {
        public bool ProcessPayment(int amount);
    }
}
