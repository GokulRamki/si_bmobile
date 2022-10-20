using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace si_bmobile.Models
{
    public class PaymentModel
    {

        public long payment_id { get; set; }

        public string payment_type { get; set; }

        public string payment_gateway { get; set; }

        public string payment_status { get; set; }

        public decimal payment_total { get; set; }

        public DateTime payment_datetime { get; set; }

        public string payment_transaction_number { get; set; }

        public string payment_response { get; set; }

        public long order_id { get; set; }

    }
}