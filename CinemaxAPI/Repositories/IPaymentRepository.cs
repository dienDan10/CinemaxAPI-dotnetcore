using CinemaxAPI.Models.Domain;

namespace CinemaxAPI.Repositories
{
    public interface IPaymentRepository : IRepository<Payment>
    {
        //void UpdateStripePaymentID(int id, string sessionId, string paymentIntentId);
        void UpdateStatus(int id, string status);
        // Update the return type if needed based on your models
        //object GetAllInTheater(DateTime startDate, DateTime endDate, int theaterId, int start, int length);
    }
}
