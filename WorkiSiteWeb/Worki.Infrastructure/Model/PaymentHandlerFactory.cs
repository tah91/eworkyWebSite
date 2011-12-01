using Ninject;
using Worki.Infrastructure.UnitOfWork;
using Ninject.Parameters;
using System;
using Ninject.Planning.Bindings;

namespace Worki.Infrastructure.Repository
{
    public static class PaymentHandlerFactory
	{
        public const string HandlerTypeString = "HandlerType";

        public enum HandlerType
        {
            Booking
        }

        static IKernel _Kernel;

		public static void RegisterKernel(IKernel kernel)
		{
			_Kernel = kernel;
		}

		public static IPaymentHandler GetHandler(HandlerType type)
		{
            Func<IBindingMetadata, bool> func = (metadata) =>
            {
                return metadata.Has(HandlerTypeString) && metadata.Get<int>(HandlerTypeString) == (int)type;
            };
            return (IPaymentHandler)_Kernel.Get(typeof(IPaymentHandler));
		}
	}

    public interface IPaymentHandler
    {
        bool CreateTransactions(int memberBookingId, string payKey, double receiverAmount, double workiFee);
        void CompleteTransactions(string payKey, string ownerTransactionId, string eworkyTransactionId, double ownerAmount, double eworkyAmount);
    }
}