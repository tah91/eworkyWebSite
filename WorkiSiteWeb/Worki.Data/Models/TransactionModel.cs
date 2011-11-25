using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Worki.Data.Models
{
    public partial class Transaction
    {
        public enum Status
        {
            Created,
            Completed,
            Cancelled
        }


        public enum Payment
        {
            PayPal
        }
    }
}
