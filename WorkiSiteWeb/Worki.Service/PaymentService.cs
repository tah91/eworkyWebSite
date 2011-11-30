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
    public interface IPaymentService
    {
		/// <summary>
		/// Send a payment request to Paypals API
		/// </summary>
		/// <param name="receiverAmount">payment for the owner</param>
		/// <param name="workiFee">Worki fee for this transaction</param>
		/// <param name="returnUrl">Return url if the payment is accepted by Paypal</param>
		/// <param name="ipnUrl">Return url if the payment is accepted by Paypal</param>
		/// <param name="cancelUrl">Cancel url if the payment is cancelled by the customer</param>
		/// <param name="senderEmail">The buyer email</param>
		/// <param name="receiverEmail">The receiver email</param>
		/// <param name="workiAccountEmail">Worki's Paypal account email</param>
		/// <returns>The customer Paypal approval url, null if an error occurred</returns>
        string PayWithPayPal(int memberBookingId, double receiverAmount, double workiFee, string returnUrl, string cancelUrl, string ipnUrl, string senderEmail, string receiverEmail, string workiAccountEmail);

		/// <summary>
		/// Checks if the provided IPN request is valid and really comes from PayPal. If all's OK process the message
		/// </summary>
		/// <param name="req">paypal request</param>
		/// <param name="status">payment status</param>
		/// <param name="requestId">paypal request id</param>
		/// <returns>A string list of errors, empty list if none</returns>
		List<string> ProcessPaypalIPNMessage(HttpRequestBase req, out string status, out string requestId);

		/// <summary>
		/// Get amounts after commission processing
		/// </summary>
		/// <param name="totalAmount">total amount</param>
		/// <param name="ownerAmount">amount for owner</param>
		/// <param name="eworkyAmount">amount for eworky</param>
		void GetAmounts(double totalAmount, out double ownerAmount, out double eworkyAmount);
    }

    #region Constants for Paypal

    public class PayPalConstants
    {
        private const string _postbackSandboxUrl = "https://www.sandbox.paypal.com/cgi-bin/webscr";
        private const string _postbackProductionUrl = "https://www.paypal.com/cgi-bin/webscr";
        private const string _paymentSandBoxUrl = "https://svcs.sandbox.paypal.com/AdaptivePayments/Pay";
        private const string _paymentProductionUrl = "https://svcs.paypal.com/AdaptivePayments/API_operation";
        private const string _approvalProductionUrl = "https://www.paypal.com/webscr?cmd=_ap-payment&paykey=";
        private const string _approvalSandBoxUrl = "https://www.sandbox.paypal.com/webscr?cmd=_ap-payment&paykey=";

        private static bool IsProduction
        {
            get
            {
                return false; // TODO Implements
            }
        }

        /// <summary>
        /// Paypal API Url to call
        /// </summary>
        public static string PaymentUrl
        {
            get
            {
                return IsProduction ? _paymentProductionUrl : _paymentSandBoxUrl;
            }
        }

        /// <summary>
        /// Paypal url with the Payment form
        /// </summary>
        public static string ApprovalUrl
        {
            get
            {
                return IsProduction ? _approvalProductionUrl : _approvalSandBoxUrl;
            }
        }

        /// <summary>
        /// Paypal Url for IPN request checking
        /// </summary>
        public static string PostbackUrl
        {
            get
            {
                return IsProduction ? _postbackProductionUrl : _postbackSandboxUrl;
            }
        }

        public class FeePayer
        {
            public static string Sender = "SENDER";
            public static string PrimvaryReceiver = "PRIMARYRECEIVER";
            public static string EachReceiver = "EACHRECEIVER";
            public static string SecondaryOnly = "SECONDARYONLY";
        }

        public class MessageFormat
        {
            public static string NameValue = "NV";
            public static string Xml = "XML";
            public static string Json = "JSON";
        }

		public class Commission
		{
			public static double BookingCom = 0.10;
		}
    }

    #endregion


    public class PaymentService : IPaymentService
    {
        #region private

        ILogger _Logger;

		private const string ApiUsername = "t.ifti_1322172136_biz_api1.hotmail.fr";
		private const string ApiPassword = "1322172161";
		private const string ApiSignature = "AeiLOX9D9hPNdgMhxGPb255O5u61AanPdNVGf5h0Kf6YW4deoOscaJ66";
        private const string ApiTestApplicationId = "APP-80W284485P519543T";

        #endregion

        public PaymentService(ILogger logger)
        {
            _Logger = logger;
        }

        #region Private methods

        /// <summary>
        /// Create a HttpWebRequest for Paypal with required Headers
        /// </summary>
        /// <param name="requestFormat">Format of data sent to Paypal</param>
		/// <param name="responseFormat">Format of data received from Paypal</param>
        /// <returns>created request</returns>
        private HttpWebRequest CreatePaypalRequest(string requestFormat, string responseFormat)
        {
            string url = PayPalConstants.PaymentUrl;
            string applicationId = ApiTestApplicationId;
            string ip = HttpContext.Current.Request.ServerVariables["LOCAL_ADDR"];

            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
            request.Headers.Add("X-PAYPAL-SECURITY-USERID", ApiUsername);
            request.Headers.Add("X-PAYPAL-SECURITY-PASSWORD", ApiPassword);
            request.Headers.Add("X-PAYPAL-SECURITY-SIGNATURE", ApiSignature);
            request.Headers.Add("X-PAYPAL-DEVICE-IPADDRESS", ip);
            request.Headers.Add("X-PAYPAL-REQUEST-DATA-FORMAT", requestFormat);
            request.Headers.Add("X-PAYPAL-RESPONSE-DATA-FORMAT", responseFormat);
            request.Headers.Add("X-PAYPAL-APPLICATION-ID", applicationId);
            
            return request;
        }

		/// <summary>
		/// Create 2 transactions for the memberbooking, called when make the paypal request
		/// </summary>
		/// <param name="memberBookingId">memberbooking id</param>
		/// <param name="payKey">paypal request id</param>
		/// <param name="receiverAmount">ammount for the owner</param>
		/// <param name="workiFee">ammount for eworky</param>
		/// <returns>true if transactions are created</returns>
        private bool CreateTransactions(int memberBookingId, string payKey, double receiverAmount, double workiFee)
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

		const string AmountIncorrectError = "Amount incorrect, RequestId : {0}, TransactionId : {1}, Amount : {2}, ExpectedAmount {3}";

		/// <summary>
		/// Complete succelfull transactions by marking them as completed
		/// </summary>
		/// <param name="payKey">paypal request id to fetch corresponding transactions</param>
		/// <param name="ownerTransactionId">paypal transaction id to set</param>
		/// <param name="eworkyTransactionId">paypal transaction id to set</param>
		/// <param name="ownerAmount">owner amount from request</param>
		/// <param name="eworkyAmount">eworky amount from request</param>
		private void CompleteTransactions(string payKey, string ownerTransactionId, string eworkyTransactionId, double ownerAmount, double eworkyAmount)
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
					throw new Exception(string.Format(AmountIncorrectError, payKey, ownerTransactionId, ownerAmount, storedOwnerAmount));
				}
				if (storedEworkyAmount != eworkyAmount)
				{
					throw new Exception(string.Format(AmountIncorrectError, payKey, eworkyTransactionId, eworkyAmount, storedEworkyAmount));
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
        /// Validate the paypal request via postbackurl
        /// </summary>
        /// <param name="paypalRequest">the request to validate</param>
        /// <returns>string telling if paypalrequest is valid</returns>
        private string ValidateIPNRequest(HttpRequestBase paypalRequest)
        {
            string strResponse = null;

            try
            {
				HttpWebRequest req = (HttpWebRequest)WebRequest.Create(PayPalConstants.PostbackUrl);
                req.Method = "POST";
                req.ContentType = "application/x-www-form-urlencoded";
                byte[] param = paypalRequest.BinaryRead(HttpContext.Current.Request.ContentLength);
                string strRequest = Encoding.ASCII.GetString(param);
                strRequest += "&cmd=_notify-validate";
                req.ContentLength = strRequest.Length;
               
                //Send the request to PayPal and get the response
                StreamWriter streamOut = new StreamWriter(req.GetRequestStream(), System.Text.Encoding.ASCII);
                streamOut.Write(strRequest);
                streamOut.Close();
                StreamReader streamIn = new StreamReader(req.GetResponse().GetResponseStream());
                strResponse = streamIn.ReadToEnd();
                streamIn.Close();
				
            }
            catch (Exception ex)
            {
				_Logger.Error("ValidateIPNRequest", ex);
            }
        
            return strResponse;
        }

        #endregion

        public string PayWithPayPal(int memberBookingId, 
                                    double receiverAmount, 
                                    double workiFee, 
                                    string returnUrl, 
                                    string cancelUrl, 
                                    string ipnUrl, 
                                    string senderEmail, 
                                    string receiverEmail, 
                                    string workiAccountEmail)
        {
            HttpWebRequest request;
            WebResponse response = null;
            StreamReader reader = null;

            string resultUrl = null;

            try
            {
                request = CreatePaypalRequest(PayPalConstants.MessageFormat.NameValue, PayPalConstants.MessageFormat.Xml);
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";

                string postData = "&actionType=PAY";
                postData += "&senderMail=" + senderEmail;
                postData += "&cancelUrl=" + cancelUrl;
                postData += "&ipnNotificationUrl=" + ipnUrl;
                postData += "&currencyCode=" + "EUR";
                postData += "&receiverList.receiver(0).amount=" + receiverAmount.ToString("F");
                postData += "&receiverList.receiver(0).email=" + receiverEmail;
                postData += "&feesPayer=" + PayPalConstants.FeePayer.EachReceiver;   
              
                if (workiFee > 0)
                {
                    postData += "&receiverList.receiver(1).amount=" + workiFee.ToString("F");
                    postData += "&receiverList.receiver(1).email=" + workiAccountEmail;
                }

                postData += "&requestEnvelope.errorLanguage=" + "en_US";
                postData += "&returnUrl=" + returnUrl;

                byte[] byteArray = Encoding.UTF8.GetBytes(postData);
                request.ContentLength = byteArray.Length;

                Stream dataStream = request.GetRequestStream();
                dataStream.Write(byteArray, 0, byteArray.Length);
                dataStream.Close();

                response = request.GetResponse();

                dataStream = response.GetResponseStream();

                reader = new StreamReader(dataStream);
                string responseFromServer = reader.ReadToEnd();

                XmlDocument xdoc = new XmlDocument();
                xdoc.LoadXml(responseFromServer);

                XmlNodeList nodes = xdoc.GetElementsByTagName("ack");

                if (nodes.Count > 0)
                {
                    string ackMessage = nodes[0].InnerText;

                    if (!string.IsNullOrEmpty(ackMessage))
                    {
                        nodes = xdoc.GetElementsByTagName("payKey");

                        if (nodes.Count > 0)
                        {
                            string payKey = nodes[0].InnerText.Trim();

                            if (ackMessage.ToLower() == "success")
                            {
                                if (CreateTransactions(memberBookingId, payKey, receiverAmount, workiFee))
                                {
                                    resultUrl = PayPalConstants.ApprovalUrl + payKey;
                                }
                            }
                            else
                            {
                                _Logger.Error("Paypal: API call failure\r\n" + responseFromServer);
                            }
                        }
                        else
                        {
                            throw new Exception("Paypal: invalid API response XML format");
                        }
                    }
                    else
                    {
                        throw new Exception("Paypal: invalid API response XML format");
                    }
                }
                else
                {
                    throw new Exception("Paypal: invalid API response XML format");
                }
            }
            catch (Exception ex)
            {
                _Logger.Error(ex.Message);
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                }

                if (response != null)
                {
                    response.Close();
                }
            }

            return resultUrl;
        }

		public List<string> ProcessPaypalIPNMessage(HttpRequestBase paypalRequest, out string status, out string requestId)
		{
			List<string> errors = new List<string>();

			string strResponse = ValidateIPNRequest(paypalRequest);
			status = string.Empty;
			requestId = string.Empty;

			try
			{
				if (strResponse != null)
				{
					if (strResponse == "VERIFIED")
					{
						status = paypalRequest.Form["status"];

						if (!string.IsNullOrEmpty(status))
						{
							status = status.ToUpper();
							requestId = paypalRequest.Form["pay_key"];

							string ownerTransactionId = paypalRequest["transaction[0].id"];
							string eworkyTransactionId = paypalRequest["transaction[1].id"];
                            //Check it...
                            double ownerAmount = double.Parse(paypalRequest["transaction[0].amount"], NumberStyles.Currency);
                            double eworkyAmount = double.Parse(paypalRequest["transaction[1].amount"], NumberStyles.Currency);
                            //string ownerAmountStr = paypalRequest["transaction[0].amount"].Split()[1];
                            //string eworkyAmountStr = paypalRequest["transaction[1].amount"].Split()[1];
                            //var ownerAmount = double.Parse(ownerAmountStr);
                            //var eworkyAmount = double.Parse(eworkyAmountStr);

							switch (status)
							{
								case "COMPLETED":
									CompleteTransactions(requestId, ownerTransactionId, eworkyTransactionId, ownerAmount, eworkyAmount);
									break;
								case "INCOMPLETE":
								case "ERROR":
								case "PROCESSING":  // Do something?
								case "PENDING":     // Do something?
								default: break;
							}
						}
						else
						{
							string message = "Paypal: no status found in IPN message\r\n";
							message += "Client IP : " + paypalRequest.UserHostAddress + "\r\n";
							message += "Request   : " + paypalRequest.RawUrl;
							_Logger.Error(message);
						}
					}
					else
					{
						string message = "Paypal: invalid Paypal IPN message\r\n";
						message += "Client IP : " + paypalRequest.UserHostAddress + "\r\n";
						message += "Request   : " + paypalRequest.RawUrl;
						_Logger.Error(message);
					}
				}
				else
				{
					string message = "Paypal : cannot validate IPN request";
					errors.Add(message);
					_Logger.Error(message);
				}
			}
			catch (Exception ex)
			{
				errors.Add(ex.Message);
				_Logger.Error("ProcessPaypalIPNMessage", ex);
			}

			return errors;
		}

		public void GetAmounts(double totalAmount, out double ownerAmount, out double eworkyAmount)
		{
			ownerAmount = (1 - PayPalConstants.Commission.BookingCom) * totalAmount;
			eworkyAmount = PayPalConstants.Commission.BookingCom * totalAmount;
		}
    }
}
