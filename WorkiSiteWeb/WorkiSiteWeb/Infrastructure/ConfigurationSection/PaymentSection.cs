using System.Configuration;
using System;
using System.Web.Configuration;
using Worki.Infrastructure.Repository;

namespace Worki.Section
{
    public sealed class PaymentConfiguration : ConfigurationSection
    {
        public const string UserNameString = "userName";
        public const string PasswordString = "password";
        public const string SignatureString = "signature";
        public const string ApplicationIdString = "appId";

		public const string PaypalMailString = "paypalMail";

        [ConfigurationProperty(UserNameString, IsRequired = true)]
        public string UserName
        {
            get { return (string)base[UserNameString]; }
            set { base[UserNameString] = value; }
        }

        [ConfigurationProperty(PasswordString, IsRequired = true)]
        public string Password
        {
            get { return (string)base[PasswordString]; }
            set { base[PasswordString] = value; }
        }

        [ConfigurationProperty(SignatureString, IsRequired = true)]
        public string Signature
        {
            get { return (string)base[SignatureString]; }
            set { base[SignatureString] = value; }
        }

        [ConfigurationProperty(ApplicationIdString, IsRequired = true)]
        public string ApplicationId
        {
            get { return (string)base[ApplicationIdString]; }
            set { base[ApplicationIdString] = value; }
        }


		[ConfigurationProperty(PaypalMailString, IsRequired = true)]
		public string PaypalMail
		{
			get { return (string)base[PaypalMailString]; }
			set { base[PaypalMailString] = value; }
		}

        private static readonly Lazy<PaymentConfiguration> lazySection = new Lazy<PaymentConfiguration>(() => (PaymentConfiguration)WebConfigurationManager.GetSection("paymentSettings"));
        private static readonly Lazy<PaymentConstants> lazyConfiguration = new Lazy<PaymentConstants>(() => { return GetConstants(); });

        public static PaymentConfiguration Instance { get { return lazySection.Value; } }
        public static PaymentConstants Constants { get { return lazyConfiguration.Value; } }

        private const string _postbackSandboxUrl = "https://www.sandbox.paypal.com/cgi-bin/webscr";
        private const string _postbackProductionUrl = "https://www.paypal.com/cgi-bin/webscr";
        private const string _paymentSandBoxUrl = "https://svcs.sandbox.paypal.com/AdaptivePayments/Pay";
		private const string _paymentProductionUrl = "https://svcs.paypal.com/AdaptivePayments/Pay";
        private const string _approvalProductionUrl = "https://www.paypal.com/webscr?cmd=_ap-payment&paykey=";
        private const string _approvalSandBoxUrl = "https://www.sandbox.paypal.com/webscr?cmd=_ap-payment&paykey=";

        static bool _IsProduction
        {
            get
            {
				return !bool.Parse(ConfigurationManager.AppSettings["IsAzureDebug"]);
            }
        }

        static PaymentConstants GetConstants()
        {
            return new PaymentConstants
            {
                ApiUsername = Instance.UserName,
                ApiPassword = Instance.Password,
                ApiSignature = Instance.Signature,
                ApiTestApplicationId = Instance.ApplicationId,
                ApprovalUrl = _IsProduction ? _approvalProductionUrl : _approvalSandBoxUrl,
                PaymentUrl = _IsProduction ? _paymentProductionUrl : _paymentSandBoxUrl,
                PostbackUrl = _IsProduction ? _postbackProductionUrl : _postbackSandboxUrl
            };
        }
    }
}
