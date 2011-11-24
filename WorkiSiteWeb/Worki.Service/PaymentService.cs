using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Web;
using System.IO;
using System.Xml;
using Worki.Infrastructure.Logging;


namespace Worki.Service
{
    public interface IPaymentService
    {
        List<string> ProcessPaypalIPNMessage(HttpRequestBase req);

        string PayWithPayPal(double receiverAmount, double workiFee, string returnUrl, string cancelUrl, string ipnUrl, string senderEmail, string receiverEmail, string workiAccountEmail);
    }

    public class PaymentService : IPaymentService
    {
        #region private

        ILogger _Logger;

        private const string PaymentSandBoxUrl = "https://svcs.sandbox.paypal.com/AdaptivePayments/Pay";
        private const string PaymentProductionUrl = "https://svcs.paypal.com/AdaptivePayments/API_operation";
        private const string ApprovalUrl = "https://www.paypal.com/webscr?cmd=_ap-payment&paykey=";
        private const string ApprovalSandBoxUrl = "https://www.sandbox.paypal.com/webscr?cmd=_ap-payment&paykey=";

        private const string ApiUsername = "ulysse_1321039527_biz_api1.hotmail.com";
        private const string ApiPassword = "1321039578";
        private const string ApiSignature = "A3C9t4rj9cNZUxlBZAHngM9kfX.pAKAzUNijx8cmFgCIdqwAfHO4rpLR";
        private const string ApiTestApplicationId = "APP-80W284485P519543T";

        #endregion



        public PaymentService(ILogger logger)
        {
            _Logger = logger;
        }




        private HttpWebRequest CreatePaypalRequest(string dataFormat)
        {
            // TODO Ajuster
            string url = PaymentSandBoxUrl;
            string applicationId = ApiTestApplicationId;
            string ip = HttpContext.Current.Request.ServerVariables["LOCAL_ADDR"];

            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
            request.Headers.Add("X-PAYPAL-SECURITY-USERID", ApiUsername);
            request.Headers.Add("X-PAYPAL-SECURITY-PASSWORD", ApiPassword);
            request.Headers.Add("X-PAYPAL-SECURITY-SIGNATURE", ApiSignature);
            request.Headers.Add("X-PAYPAL-DEVICE-IPADDRESS", ip);
            request.Headers.Add("X-PAYPAL-REQUEST-DATA-FORMAT", dataFormat);
            request.Headers.Add("X-PAYPAL-RESPONSE-DATA-FORMAT", "XML");
            request.Headers.Add("X-PAYPAL-APPLICATION-ID", applicationId);


            
            return request;
        }




        /// <summary>
        /// Checks if the provided IPN request is valid and really comes from PayPal. If all's OK process the message
        /// </summary>
        /// <param name="paypalRequest"></param>
        /// <returns>A string list of errors, empty list if none</returns>
        public List<string> ProcessPaypalIPNMessage(HttpRequestBase paypalRequest)
        {
            List<string> errors = new List<string>();

            //Post back to either sandbox or live
            string strSandbox = "https://www.sandbox.paypal.com/cgi-bin/webscr";
            string strLive = "https://www.paypal.com/cgi-bin/webscr";
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(strSandbox);

            //Set values for the request back
            req.Method = "POST";
            req.ContentType = "application/x-www-form-urlencoded";
            byte[] param = paypalRequest.BinaryRead(HttpContext.Current.Request.ContentLength);
            string strRequest = Encoding.ASCII.GetString(param);
            strRequest += "&cmd=_notify-validate";
            req.ContentLength = strRequest.Length;

            //for proxy
            //WebProxy proxy = new WebProxy(new Uri("http://url:port#"));
            //req.Proxy = proxy;

            //Send the request to PayPal and get the response
            StreamWriter streamOut = new StreamWriter(req.GetRequestStream(), System.Text.Encoding.ASCII);
            streamOut.Write(strRequest);
            streamOut.Close();
            StreamReader streamIn = new StreamReader(req.GetResponse().GetResponseStream());
            string strResponse = streamIn.ReadToEnd();
            streamIn.Close();

            if (strResponse == "VERIFIED")
            {
                string paymentStatus = paypalRequest["payment_status"];
                string transactionId = paypalRequest["txn_id"];                






                //check the payment_status is Completed
                //check that txn_id has not been previously processed
                //process payment
            }
            else
            {
                string message = "Invalid Paypal IPN Message\r\n";
                message += "Client IP : " + paypalRequest.UserHostAddress + "\r\n";
                message += "Request   : " + paypalRequest.RawUrl;
                _Logger.Error(message);

                errors.Add("Invalide PayPal IPN message");
            }

            return errors;
        }






        /// <summary>
        /// Send a payment request to Paypals API
        /// </summary>
        /// <param name="receiverAmount">Payment amount</param>
        /// <param name="workiFee">Worki fee for this transaction</param>
        /// <param name="returnUrl">Return url if the payment is accepted by Paypal</param>
        /// <param name="ipnUrl">Return url if the payment is accepted by Paypal</param>
        /// <param name="cancelUrl">Cancel url if the payment is cancelled by the customer</param>
        /// <param name="senderEmail">The buyer email</param>
        /// <param name="receiverEmail">The receiver email</param>
        /// <param name="workiAccountEmail">Worki's Paypal account email</param>
        /// <returns>The customer Paypal approval url, null if an error occurred</returns>
        public string PayWithPayPal(double receiverAmount, double workiFee, string returnUrl, string cancelUrl, string ipnUrl, string senderEmail, string receiverEmail, string workiAccountEmail)
        {
            HttpWebRequest request;
            WebResponse response = null;
            StreamReader reader = null;

            string resultUrl = null;

            try
            {
                request = CreatePaypalRequest("NV");
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";

                string postData = "&actionType=PAY";
                postData += "&senderMail=" + senderEmail;
                postData += "&cancelUrl=" + cancelUrl;
                postData += "&ipnNotificationUrl=" + ipnUrl;
                postData += "&currencyCode=" + "EUR";
                postData += "&receiverList.receiver(0).amount=" + receiverAmount.ToString("F");
                postData += "&receiverList.receiver(0).email=" + receiverEmail;
              
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

                    if (!string.IsNullOrEmpty(ackMessage) && ackMessage.ToLower() == "success")
                    {
                        nodes = xdoc.GetElementsByTagName("payKey");

                        if (nodes.Count > 0)
                        {
                            resultUrl = ApprovalSandBoxUrl + nodes[0].InnerText;
                        }
                        else
                        {
                            throw new Exception("Paypal response XML format changed!");
                        }
                    }
                }
                else
                {
                    throw new Exception("Paypal response XML format changed!");
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
    }
}
