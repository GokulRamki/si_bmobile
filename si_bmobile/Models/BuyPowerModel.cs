using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace si_bmobile.Models
{
    public class GetPowerModelIP
    {
        public string sUserName { get; set; }
        public string sPassword { get; set; }
        public string sMeterNo { get; set; }
        public long dAmount { get; set; }
        public string sMSISDN { get; set; }
        public string sRef1 { get; set; }
        public string sRef2 { get; set; }
        public string sKeyCode { get; set; }
        public string sIPAddress { get; set; }
        public int iProcessMode { get; set; }
        public string responseXML { get; set; }
        public int user_id { get; set; }
    }

}