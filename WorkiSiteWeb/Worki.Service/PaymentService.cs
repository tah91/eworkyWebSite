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
        /// <param name="id">id of the eworky product</param>
		/// <param name="returnUrl">Return url if the payment is accepted by Paypal</param>
		/// <param name="ipnUrl">Return url if the payment is accepted by Paypal</param>
		/// <param name="cancelUrl">Cancel url if the payment is cancelled by the customer</param>
		/// <param name="senderEmail">The buyer email</param>
        /// <param name="payments">list of payments</param>
        /// <param name="paymentHandler">payment handler to create transactions</param>
		/// <returns>The customer Paypal approval url, null if an error occurred</returns>
        string PayWithPayPal(int id, string returnUrl, string cancelUrl, string ipnUrl, string senderEmail, IEnumerable<PaymentItem> payments, IPaymentHandler paymentHandler, PaymentConstants configuration);

		/// <summary>
		/// Checks if the provided IPN request is valid and really comes from PayPal. If all's OK process the message
		/// </summary>
		/// <param name="req">paypal request</param>
		/// <param name="status">payment status</param>
		/// <param name="requestId">paypal request id</param>
		/// <returns>A string list of errors, empty list if none</returns>
        List<string> ProcessPaypalIPNMessage(HttpRequestBase req, out string status, out string requestId, PaymentConstants config);
    }

    #region Constants for Paypal

    public class PayPalConstants
    {
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
    }

    #endregion

    public class PaymentService : IPaymentService
    {
        #region private

        ILogger _Logger;

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
        private HttpWebRequest CreatePaypalRequest(string requestFormat, string responseFormat, PaymentConstants config)
        {
            string url = config.PaymentUrl;
            string applicationId = config.ApiTestApplicationId;
            string ip = HttpContext.Current.Request.ServerVariables["LOCAL_ADDR"];

            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
            request.Headers.Add("X-PAYPAL-SECURITY-USERID", config.ApiUsername);
            request.Headers.Add("X-PAYPAL-SECURITY-PASSWORD", config.ApiPassword);
            request.Headers.Add("X-PAYPAL-SECURITY-SIGNATURE", config.ApiSignature);
            request.Headers.Add("X-PAYPAL-DEVICE-IPADDRESS", ip);
            request.Headers.Add("X-PAYPAL-REQUEST-DATA-FORMAT", requestFormat);
            request.Headers.Add("X-PAYPAL-RESPONSE-DATA-FORMAT", responseFormat);
            request.Headers.Add("X-PAYPAL-APPLICATION-ID", applicationId);
            
            return request;
        }

        /// <summary>
        /// Validate the paypal request via postbackurl
        /// </summary>
        /// <param name="paypalRequest">the request to validate</param>
        /// <returns>string telling if paypalrequest is valid</returns>
        private string ValidateIPNRequest(HttpRequestBase paypalRequest, PaymentConstants config)
        {
            string strResponse = null;

            try
            {
				HttpWebRequest req = (HttpWebRequest)WebRequest.Create(config.PostbackUrl);
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

        public string PayWithPayPal(int id, 
                                    string returnUrl, 
                                    string cancelUrl, 
                                    string ipnUrl, 
                                    string senderEmail, 
                                    IEnumerable<PaymentItem> payments,
                                    IPaymentHandler paymentHandler,
                                    PaymentConstants config)
        {
            HttpWebRequest request;
            WebResponse response = null;
            StreamReader reader = null;

            string resultUrl = null;

            try
            {
                request = CreatePaypalRequest(PayPalConstants.MessageFormat.NameValue, PayPalConstants.MessageFormat.Xml, config);
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";

                string postData = "&actionType=PAY";
                postData += "&senderMail=" + senderEmail;
                postData += "&cancelUrl=" + cancelUrl;
                postData += "&ipnNotificationUrl=" + ipnUrl;
                postData += "&currencyCode=" + "EUR";
                postData += "&feesPayer=" + PayPalConstants.FeePayer.EachReceiver;

                foreach (var payment in payments)
                {
                    postData += "&receiverList.receiver(" + payment.Index.ToString() + ").amount=" + payment.Amount.ToString("F");
                    postData += "&receiverList.receiver(" + payment.Index.ToString() + ").email=" + payment.Email;
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
                                if (paymentHandler.CreateTransactions(id, payKey, payments))
                                {
                                    resultUrl = config.ApprovalUrl + payKey;
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

        public List<string> ProcessPaypalIPNMessage(HttpRequestBase paypalRequest, out string status, out string requestId, PaymentConstants config)
		{
			List<string> errors = new List<string>();

            string strResponse = ValidateIPNRequest(paypalRequest, config);
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

                            string tr1 = paypalRequest["transaction[0].id"];
                            string tr2 = paypalRequest["transaction[1].id"];
                            //Check it...
                            decimal tr1Amount, tr2Amount;
                            decimal.TryParse(paypalRequest["transaction[0].amount"], NumberStyles.Currency, null, out tr1Amount);
                            decimal.TryParse(paypalRequest["transaction[1].amount"], NumberStyles.Currency, null, out tr2Amount);
                            //string ownerAmountStr = paypalRequest["transaction[0].amount"].Split()[1];
                            //string eworkyAmountStr = paypalRequest["transaction[1].amount"].Split()[1];
                            //var ownerAmount = double.Parse(ownerAmountStr);
                            //var eworkyAmount = double.Parse(eworkyAmountStr);

                            var payments = new List<PaymentItem>
                            {
                                new PaymentItem{  Index = 0, Amount = tr1Amount, TransactionId = tr1},
                                new PaymentItem{  Index = 1, Amount = tr2Amount, TransactionId = tr2},
                            };

							var context = ModelFactory.GetUnitOfWork();
							var tRepo = ModelFactory.GetRepository<ITransactionRepository>(context);

							var type = tRepo.GetHandlerType(requestId);
							var paymentHandler = PaymentHandlerFactory.GetHandler(type);

							switch (status)
							{
								case "COMPLETED":
                                    paymentHandler.CompleteTransactions(requestId, payments);
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
    }
}
