using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using si_bmobile.Models;

namespace si_bmobile.DAL
{
    public interface IcareRepository : IDisposable
    {
        List<UserHistoryLogs> get_user_history();

        List<YourPlanTransModel> get_your_plan_translog();

        List<Application_Transaction> get_application_translogs();

        string AES_ENC(string _input);

        string AES_DEC(string _input);

        string RedeemFBPromotion_TopupVoucher(RedeemFBPromotionsModel RedeemFB);

        void Register_User(RegistrationModel oRM, out int iRet, out bool bflag);

        void GetEMailID_MSISDN(string msisdn, out string sEmail, out bool bRes);

        void LockAccount(long user_id);

        void VerifyCode(string Msisdn, string vcode, out bool bflag);

        void Update_User(RegistrationModel oRM, out int iRet, out bool bflag);

        void Authenticate_User(LoginModel oLM, out int iRet, out bool bFlag);

        void ResetPwd(ResetModel oRSM, out int iRet, out bool bFlag);

        Dictionary<string, string> TempPassEmail(FPModel oFP);

        bool EmailTemppwd(Dictionary<string, string> oDic);

        void VerifyUser(TPModel oTP, out bool bVerify);

        void UpdatePassword(TPModel oTP, out int iRet);

        List<BundleModel> GetAllBundles();

        List<BundleModel> GetBundles(string sUserId, double dBalance);

        string UpdateSubscription(ChangeTypeModel oCM);

        string Voucher_Topup(TopupModel oTM);

        List<BundleModel> GetVoicePlans(List<BundleModel> bundles, List<planDetailsModel> v_plan, double dbalance);

        byte[] DwldPostpaidbill(string filepath, string sFilename);

        string PurchasePostpaid(PostpaidModel oP);

        RegistrationModel GetRegDetails(string sMSISDN);

        int SendSimVerification_Res(long regId, string msisdn, out int iRet, out bool bflag);

        int AddMulitSIM_Res(long regId, string msisdn, string vcode, string description, out int iRet, out bool bflag);

        int RemoveSIM_Res(long regId, string msisdn);

        AccountModel GetMultipleSubscribers(string sMSISDN);

        bool VerifyMSISN(long regId, string sMsisdn);

        PaidTypeModel GetEmailFromMSISDN(string msisdn);

        string VerifySIMActivation(string msidn_no, string sim_no, string puk_code, string activated_by);

        int ActivateSIM(string msidn_no, string ref_no);

        int CheckPaidType(string sPostpaidnumber);
        
        string RedeemFBPromotion_GetVoucher(RedeemFBPromotionsModel RedeemFB);

        string VoiceCallActivationDeactivation(VoiceCallActDeactModel objVoiceCall);

        #region top kad
        int insert_top_kad_log(top_kad_verify_model obj_top_kad);

        List<tbl_top_kad_log> get_top_kad_Log_List(string UserId);

        string get_json_for_top_Kad_service(tbl_top_kad_log obj_top_kad);

        string verify_topkad_serial(tbl_top_kad_log obj_top_kad);

        string top_up_topkad_serial(tbl_top_kad_log obj_top_kad);

        #endregion

        bool Update_MSISDN_Description(long reg_id, string msisdn, string desc);

        bool Delete_FB_Voucher(long id);

        int GetMsisdnStatus(string msisdn_no);

        List<RegistrationReportModel> GetRegistrationDetails();

        tbl_web_plan_purchase_trans CreatePlanPurchaseTransLog(ChangeTypeModel oCM);
    }
}
