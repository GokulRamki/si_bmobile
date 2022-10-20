using si_bmobile.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace si_bmobile.DAL
{
    interface ISimRepository:IDisposable
    {
        bool save_sim_contact_details(ActiveSimModel active_sim);

        List<sim_reg_model> get_new_sim_customers();

        List<replace_sim_reg_model> get_replace_sim_customers();

        long save_sim_reg_cust(sim_reg_model Item);

        long save_sim_replace_reg_cust(replace_sim_reg_model Item);

        long update_sim_reg_cust(update_sim_reg_model Item);

        long update_sim_replace_reg_cust(update_replace_sim_reg_model Item);
    }
}
