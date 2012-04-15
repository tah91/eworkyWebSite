using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Web;
using System.IO;
using System.Xml;
using System.Web.Mvc;
using Newtonsoft.Json;
using Worki.Infrastructure.Logging;
using Worki.Infrastructure.Repository;
using Worki.Data.Models;
using System.Globalization;
using Postal;
using Worki.Infrastructure.Helpers;
using Worki.Section;

namespace Worki.Web
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
            public const string AmountIncorrectError = "Amount incorrect, RequestId : {0}, TransactionId : {1}, Amount : {2}, ExpectedAmount {3}";
		}

        public bool  CreateTransactions(int memberBookingId, string payKey, IEnumerable<PaymentItem> payments)
        {
 	        bool isCreated = false;

            var context = ModelFactory.GetUnitOfWork();
   
            try
            {
                var bRepo = ModelFactory.GetRepository<IBookingRepository>(context);
				var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
                MemberBooking booking = bRepo.Get(memberBookingId);

                if (booking != null)
                {
					var receiver = payments.Where(p => p.Index == 0).FirstOrDefault();
					var eworky = payments.Where(p => p.Index == 1).FirstOrDefault();

					booking.Transactions.Add(new Transaction
					{
						ReceiverId = booking.Offer.Localisation.OwnerID.Value,
						Amount = (decimal)receiver.Amount,
						CreatedDate = DateTime.UtcNow,
                        PaymentType = (int)TransactionConstants.Payment.PayPal,
						StatusId = (int)TransactionConstants.Status.Created,
						RequestId = payKey,
					});

					if (eworky != null)
					{
						booking.Transactions.Add(new Transaction
						{
							ReceiverId = mRepo.GetAdminId(),
							Amount = (decimal)eworky.Amount,
							CreatedDate = DateTime.UtcNow,
							PaymentType = (int)TransactionConstants.Payment.PayPal,
							StatusId = (int)TransactionConstants.Status.Created,
							RequestId = payKey,
						});
					}

					booking.MemberBookingLogs.Add(new MemberBookingLog
					{
						CreatedDate = DateTime.UtcNow,
						Event = "Paypal Payment Requested"
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

		public bool CompleteTransactions(string payKey, IEnumerable<PaymentItem> payments)
		{
			var context = ModelFactory.GetUnitOfWork();
			var tRepo = ModelFactory.GetRepository<ITransactionRepository>(context);
			var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
			var transactions = tRepo.GetMany(trx => trx.RequestId == payKey);
			bool completed = false;

			try
			{
				bool alreadyProcessed = transactions.Where(t => t.UpdatedDate == null).Count() == 0;
				if (alreadyProcessed)
				{
					throw new Exception("Transaction (Paypal ID " + payKey + ") already processed");
				}

				var booking = transactions[0].MemberBooking;
				var ownerId = booking.Offer.Localisation.OwnerID.Value;
				var eworkyId = mRepo.GetAdminId();

				var offer = booking.Offer;
				var clientId = booking.MemberId;
				var localisation = offer.Localisation;

				var owner = payments.Where(p => p.Index == 0).FirstOrDefault();
				var ownerTransaction = transactions.Where(t => t.ReceiverId == ownerId).FirstOrDefault();
				ownerTransaction.UpdatedDate = DateTime.UtcNow;
				ownerTransaction.StatusId = (int)TransactionConstants.Status.Completed;
				ownerTransaction.TransactionId = owner.TransactionId;

				var eworky = payments.Where(p => p.Index == 1).FirstOrDefault();
				if (eworky != null)
				{
					var eworkyTransaction = transactions.Where(t => t.ReceiverId == eworkyId).FirstOrDefault();
					eworkyTransaction.UpdatedDate = DateTime.UtcNow;
					eworkyTransaction.StatusId = (int)TransactionConstants.Status.Completed;
					eworkyTransaction.TransactionId = eworky.TransactionId;
				}

				//check payment amounts
				//if (ownerTransaction.Amount != owner.Amount)
				//{
				//    throw new Exception(string.Format(Constants.AmountIncorrectError, payKey, owner.TransactionId, owner.Amount, ownerTransaction.Amount));
				//}
				//if (eworkyTransaction.Amount != eworky.Amount)
				//{
				//    throw new Exception(string.Format(Constants.AmountIncorrectError, payKey, eworky.TransactionId, eworky.Amount, eworkyTransaction.Amount));
				//}

                booking.PaymentType = (int)Offer.PaymentTypeEnum.Paypal;
				booking.StatusId = (int)MemberBooking.Status.Paid;

				booking.MemberBookingLogs.Add(new MemberBookingLog
				{
					CreatedDate = DateTime.UtcNow,
					Event = "Paypal transaction completed",
				});

				booking.MemberBookingLogs.Add(new MemberBookingLog
				{
					CreatedDate = DateTime.UtcNow,
					Event = "Payment completed",
					EventType = (int)MemberBookingLog.BookingEvent.Payment,
					LoggerId = clientId
				});

				//send mail to owner
				dynamic ownerMail = new Email(MVC.Emails.Views.Email);
				ownerMail.From = MiscHelpers.EmailConstants.ContactDisplayName + "<" + MiscHelpers.EmailConstants.ContactMail + ">";
				ownerMail.To = booking.Owner.Email;
				ownerMail.Subject = string.Format(Worki.Resources.Email.BookingString.PayementSubject, booking.Id);
				ownerMail.ToName = booking.Owner.MemberMainData.FirstName;
				ownerMail.Content = string.Format(Worki.Resources.Email.BookingString.PayementOwner, booking.Id);

				context.Commit();
				completed = true;

				ownerMail.Send();
			}
			catch (Exception ex)
			{
				context.Complete();
				_Logger.Error("CompleteTransactions", ex);
			}

			return completed;
		}
    }
}
