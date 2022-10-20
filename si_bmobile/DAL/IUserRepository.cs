using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using si_bmobile.Models;
using System.Configuration;


namespace si_bmobile.DAL
{
    interface IUserRepository: IDisposable
    {

        IEnumerable<all_user_details> get_all_users();

        bool shop_user_email_activation(string Enc_Id);

        web_tbl_shopping_user_contact shop_user_login(LoginModel logindetails);

        bool is_email_available(string email_id);

        bool is_msisdn_available(string msisdn);

        long change_password(ChangePwdModel details);

        long reset_password(ResetModel details);

        long forgot_password(string email, out string pwd);


        IEnumerable<Menu_Model> get_all_menu();

        List<Menu_Model> GetCareUserMenuList(long? U_id);

        List<MainMenuModel> GetUserMenu(long U_id);

        bool checkMenuAccess(long user_id, int role_id, string viewname);

        IList<TransactionReportModel> get_list();

        string get_MenuListByUserId(long id, int role);
    }
}
