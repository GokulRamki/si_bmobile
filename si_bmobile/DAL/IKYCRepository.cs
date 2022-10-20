using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using si_bmobile.Models;
using System.Web.Mvc;

namespace si_bmobile.DAL
{
    interface IKYCRepository
    {
        List<kyc_grid_list> getKycGridValue();

        kyc_Custome_Detail_Model getKYCCustomerDetailsById(long id);

        int UpdateKyc_CustomerDetails(kyc_Custome_Detail_Model obj);

        List<PROVINCIAL_CODES> GetProvince();

        List<DISTRICT_CODES> GetDistrictByProvinceID(int id);

        List<DISTRICT_CODES> GetDistrict();

        string GetProvinceNamebyProvinceID(int id);

        string GetDistrictNamebyDistrictID(int id);

        List<tbl_kyc_simactivation_log> get_Sim_Activation_Log_Details();

        int Create_KYC_Version(tbl_kyc_version_update obj_kyc_version);

        int Update_KYC_Version(tbl_kyc_version_update obj_kyc_version);

        List<tbl_kyc_version_update> Get_All_KYC_version_Details();

        tbl_kyc_version_update Get_KYC_version_Details_By_ID(long ID);

        bool Check_KYC_Version(decimal version_No, long ID);

        int delete_kyc_version(string id);

        tbl_kyc_customer Get_KYC_Customer_Details_By_ID(long id);





        string SaveKYCDetails(string Details, out long cust_id);

        string GetProvinceWithCode();

        string sGetDistrictByProvinceID(int id);

        void KYC_simRegisterationLog(string msisdn, string puk_code, string sim_no, string activatedby, string refno, bool is_sim_activated);

        string Get_KYC_Version_Update(decimal kyc_version);

        string Get_Mobile_Model();

        string Get_JSON_String_for_LT(string details);

       // List<LTModel> Get_null_LT_reference();

        //int Update_LT_Reference_for_bulk(string msisdn, string lt_ref);

        long Update_LT_Reference(long cust_id, string lt_ref);

        string UpdateKyc_CustomerDetails(string Details, out long cust_id);

        string Get_JSON_String_for_LT_service4(string details);

        List<tbl_spc_promo_cust> getSpecialPromotion();

    }
}
