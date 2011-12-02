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
using Postal;
using Worki.Infrastructure.Helpers;

namespace Worki.Web
{
    public class MemberQuotationPaymentHandler : IPaymentHandler
    {
        ILogger _Logger;

		public MemberQuotationPaymentHandler(ILogger logger)
        {
            _Logger = logger;
        }

        public bool  CreateTransactions(int quotationMemberId, string payKey, IEnumerable<PaymentItem> payments)
        {
 	        bool isCreated = false;

            var context = ModelFactory.GetUnitOfWork();
   
            try
            {
                var qRepo = ModelFactory.GetRepository<IQuotationRepository>(context);
				var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
				MemberQuotation quotation = qRepo.Get(quotationMemberId);

				if (quotation != null && payments.Count() == 1)
                {
					var eworky = payments.Where(p => p.Index == 0).FirstOrDefault();

					quotation.MemberQuotationTransactions.Add(new MemberQuotationTransaction
					{
						Amount = (decimal)eworky.Amount,
						CreatedDate = DateTime.UtcNow,
                        PaymentType = (int)TransactionConstants.Payment.PayPal,
						StatusId = (int)TransactionConstants.Status.Created,
						RequestId = payKey,
					});

					quotation.MemberQuotationLogs.Add(new MemberQuotationLog
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

        public void CompleteTransactions(string payKey, IEnumerable<PaymentItem> payments)
        {
            var context = ModelFactory.GetUnitOfWork();
			var tRepo = ModelFactory.GetRepository<IQuotationTransactionRepository>(context);
            var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
            var transactions = tRepo.GetMany(trx => trx.RequestId == payKey);

            try
            {
                if (transactions.Count != 1)
                {
                    throw new Exception("Transaction count (" + transactions.Count + ") differ from 1");
                }

                bool alreadyProcessed = transactions.Where(t => t.UpdatedDate == null).Count() == 0;
                if (alreadyProcessed)
                {
                    throw new Exception("Transaction (Paypal ID " + payKey + ") already processed");
                }

				var eworkyTransaction = transactions.FirstOrDefault();
				var quotation = eworkyTransaction.MemberQuotation;
                var eworkyId = mRepo.GetAdminId();

                var eworky = payments.Where(p => p.Index == 0).FirstOrDefault();
                
                //check payment amounts
                if (eworkyTransaction.Amount != eworky.Amount)
                {
					throw new Exception(string.Format(PaymentHandlerFactory.Constants.AmountIncorrectError, payKey, eworky.TransactionId, eworky.Amount, eworkyTransaction.Amount));
                }

                eworkyTransaction.UpdatedDate = DateTime.UtcNow;
                eworkyTransaction.StatusId = (int)TransactionConstants.Status.Completed;
                eworkyTransaction.TransactionId = eworky.TransactionId;

				quotation.MemberQuotationLogs.Add(new MemberQuotationLog
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

        public void PaymentCompleted(string payKey)
        {
            var context = ModelFactory.GetUnitOfWork();
			var tRepo = ModelFactory.GetRepository<IQuotationTransactionRepository>(context);

            var transaction = tRepo.Get(trx => trx.RequestId == payKey);
            var quotation = transaction.MemberQuotation;
			var offer = quotation.Offer;
            var localisation = offer.Localisation;

            //send mail to owner 
            dynamic ownerMail = new Email(MVC.Emails.Views.Email);
            ownerMail.From = MiscHelpers.EmailConstants.ContactDisplayName + "<" + MiscHelpers.EmailConstants.ContactMail + ">";
			ownerMail.To = quotation.Owner.Email;
            ownerMail.Subject = Worki.Resources.Email.BookingString.PayementSubject;
			ownerMail.ToName = quotation.Owner.MemberMainData.FirstName;
			ownerMail.Content = "TODO";

            //send mail to client 
            dynamic clientMail = new Email(MVC.Emails.Views.Email);
            clientMail.From = MiscHelpers.EmailConstants.ContactDisplayName + "<" + MiscHelpers.EmailConstants.ContactMail + ">";
			clientMail.To = quotation.Client.Email;
            clientMail.Subject = Worki.Resources.Email.BookingString.PayementSubject;
			clientMail.ToName = quotation.Client.MemberMainData.FirstName;
			clientMail.Content = "TODO";

            ownerMail.Send();
            clientMail.Send();

            try
            {
				quotation.MemberQuotationLogs.Add(new MemberQuotationLog
                {
                    CreatedDate = DateTime.UtcNow,
                    Event = "Payment completed",
					EventType = (int)MemberQuotationLog.QuotationEvent.Payment
                });

                context.Commit();
            }
            catch (Exception ex)
            {
                context.Complete();
                _Logger.Error("PaymentCompleted", ex);
            }
        }
    }
}
