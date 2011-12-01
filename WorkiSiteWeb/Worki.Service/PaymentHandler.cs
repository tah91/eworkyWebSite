using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Web;
using System.IO;
using System.Xml;
using Newtonsoft.Json;
using Worki.Infrastructure.Logging;
using Worki.Infrastructure.Repository;
using Worki.Data.Models;
using System.Globalization;


namespace Worki.Service
{
    public class MemberBookingPaymentHandler : IPaymentHandler
    {
        ILogger _Logger;

        public MemberBookingPaymentHandler(ILogger logger)
        {
            _Logger = logger;
        }

        public class Constants
		{
			public static double BookingCom = 0.10;
            public const string AmountIncorrectError = "Amount incorrect, RequestId : {0}, TransactionId : {1}, Amount : {2}, ExpectedAmount {3}";
		}

        /// <summary>
		/// Create 2 transactions for the memberbooking, called when make the paypal request
		/// </summary>
		/// <param name="memberBookingId">memberbooking id</param>
		/// <param name="payKey">paypal request id</param>
		/// <param name="receiverAmount">ammount for the owner</param>
		/// <param name="workiFee">ammount for eworky</param>
		/// <returns>true if transactions are created</returns>
        public bool  CreateTransactions(int memberBookingId, string payKey, double receiverAmount, double workiFee)
        {
 	        bool isCreated = false;

            var context = ModelFactory.GetUnitOfWork();
   
            try
            {
                var bRepo = ModelFactory.GetRepository<IBookingRepository>(context);
				var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
                MemberBooking booking = bRepo.GetBooking(memberBookingId);

                if (booking != null)
                {
					booking.Transactions.Add(new Transaction
					{
						ReceiverId = booking.Offer.Localisation.OwnerID.Value,
						Amount = (decimal)receiverAmount,
						CreatedDate = DateTime.UtcNow,
						PaymentType = (int)Transaction.Payment.PayPal,
						StatusId = (int)Transaction.Status.Created,
						RequestId = payKey,
					});

					booking.Transactions.Add(new Transaction
					{
						ReceiverId = mRepo.GetAdminId(),
						Amount = (decimal)workiFee,
						CreatedDate = DateTime.UtcNow,
						PaymentType = (int)Transaction.Payment.PayPal,
						StatusId = (int)Transaction.Status.Created,
						RequestId = payKey,
					});

					booking.MemberBookingLogs.Add(new MemberBookingLog
					{
						CreatedDate = DateTime.UtcNow,
						Event = "Paypal Payment Requested",
					});

                    context.Commit();

                    isCreated = true;
                }
            }
            catch (Exception ex)
            {
                context.Complete();
				_Logger.Error("CreateTransactions", ex);
            }

            return isCreated;
        }

        /// <summary>
		/// Complete succelfull transactions by marking them as completed
		/// </summary>
		/// <param name="payKey">paypal request id to fetch corresponding transactions</param>
		/// <param name="ownerTransactionId">paypal transaction id to set</param>
		/// <param name="eworkyTransactionId">paypal transaction id to set</param>
		/// <param name="ownerAmount">owner amount from request</param>
		/// <param name="eworkyAmount">eworky amount from request</param>
        public void  CompleteTransactions(string payKey, string ownerTransactionId, string eworkyTransactionId, double ownerAmount, double eworkyAmount)
        {
 	                    var context = ModelFactory.GetUnitOfWork();
            var tRepo = ModelFactory.GetRepository<ITransactionRepository>(context);
			var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
            var transactions = tRepo.GetMany(trx => trx.RequestId == payKey);

            if (transactions.Count == 0)
				return;

			bool alreadyProcessed = transactions.Where(t => t.UpdatedDate == null).Count() == 0;
            if (alreadyProcessed)
                return;

			var booking = transactions[0].MemberBooking;

            try
            {
				double storedOwnerAmount,storedEworkyAmount;
				GetAmounts(booking.Price, out storedOwnerAmount, out storedEworkyAmount);
				if (storedOwnerAmount != ownerAmount)
				{
					throw new Exception(string.Format(Constants.AmountIncorrectError, payKey, ownerTransactionId, ownerAmount, storedOwnerAmount));
				}
				if (storedEworkyAmount != eworkyAmount)
				{
					throw new Exception(string.Format(Constants.AmountIncorrectError, payKey, eworkyTransactionId, eworkyAmount, storedEworkyAmount));
				}

				var adminId = mRepo.GetAdminId();
                foreach (var transaction in transactions)
                {
                    transaction.UpdatedDate = DateTime.UtcNow;
                    transaction.StatusId = (int)Transaction.Status.Completed;
					transaction.TransactionId = transaction.ReceiverId == adminId ? eworkyTransactionId : ownerTransactionId;
                }

				booking.MemberBookingLogs.Add(new MemberBookingLog
				{
					CreatedDate = DateTime.UtcNow,
					Event = "Paypal transaction completed",
				});
                context.Commit();
            }
            catch (Exception ex)
            {
                context.Complete();
				_Logger.Error("CompleteTransactions", ex);
            }
        }

        /// <summary>
		/// Get amounts after commission processing
		/// </summary>
		/// <param name="totalAmount">total amount</param>
		/// <param name="ownerAmount">amount for owner</param>
		/// <param name="eworkyAmount">amount for eworky</param>
        public void GetAmounts(double totalAmount, out double ownerAmount, out double eworkyAmount)
		{
			ownerAmount = (1 - Constants.BookingCom) * totalAmount;
			eworkyAmount = Constants.BookingCom * totalAmount;
		}
    }
}
