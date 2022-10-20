using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace bemobile.DAL
{
    public interface ISIRepository : IDisposable
    {
        string bmsi_validate_merchant(string msisdn, string amount);

        string bmsi_recharge_msisdn(string msisdn, string amount, string data);
    }
}