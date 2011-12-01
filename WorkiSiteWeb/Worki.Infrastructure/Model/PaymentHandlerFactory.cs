﻿using Ninject;
using Worki.Infrastructure.UnitOfWork;
using Ninject.Parameters;
using System;
using Ninject.Planning.Bindings;
using System.Collections.Generic;

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

    public class PaymentItem
    {
        public int Index { get; set; }
        public string Email { get; set; }
        public decimal Amount { get; set; }
        public string TransactionId { get; set; }
    }

    public interface IPaymentHandler
    {
        /// <summary>
        /// Create transactions for each payments, called when make the paypal request
        /// </summary>
        /// <param name="id">eworky product id</param>
        /// <param name="payKey">paypal request id</param>
        /// <param name="payments">list of payments</param>
        /// <returns>true if transactions are created</returns>
        bool CreateTransactions(int id, string payKey, IEnumerable<PaymentItem> payments);

        /// <summary>
        /// Complete successfull transactions by marking them as completed
        /// </summary>
        /// <param name="payKey">paypal request id to fetch corresponding transactions</param>
        /// <param name="payments">payments made by the request, needed to verify the amount</param>
        void CompleteTransactions(string payKey, IEnumerable<PaymentItem> payments);
    }
}