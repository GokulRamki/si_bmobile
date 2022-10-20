using si_bmobile.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace si_bmobile.DAL
{
    interface IStaffsTopup_Repo : IDisposable
    {
        bool SendStaffsTopupEmail();

        bool staff_deduction(bm_staffs_topup item);
    }
}
