//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace si_bmobile.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class application_transaction_log
    {
        public long id { get; set; }
        public long app_tx_id { get; set; }
        public System.DateTime date { get; set; }
        public string type { get; set; }
        public string status { get; set; }
        public string details { get; set; }
    }
}
