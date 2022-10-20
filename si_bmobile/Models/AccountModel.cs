using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace si_bmobile.Models
{
    public class AccountModel
    {
        public RegistrationModel Reg { get; set; }
        public List<SubscriberModel> Subr { get; set; }
       
    }
    public class PostpaidModel {
        public string cheque_num { get; set; }
        public string payment_amount { get; set; }
        public string entity_id { get; set; }
    }

    public class SubscriberModel
    {
        public int balanceResultCode { get; set; }

        public long resultCode { get; set; }

        public bool topupAllowed { get; set; }

        public List<planDetailsModel> planDetails { get; set; }

        public List<planDetailsModel> DataplanDetails { get; set; }
        public List<planDetailsModel> VoiceplanDetails { get; set; }

        public List<yourPlans> YPlans { get; set; }

        public bool purchasedAllowed { get; set; }

        public double balance { get; set; }

        public string _sbalance { get; set; }

        public long active { get; set; }

        public long dataBalance { get; set; }

        public string _sdataBalance { get; set; }

        public bool isExpired { get; set; }

        public int isMinSize { get; set; }

        public string sCreditExpirydate { get; set; }

        public string sDataExpirydate { get; set; }

        public string _MSISDNNumber { get; set; }

        public bool planAvl { get; set; }

        public bool yourPlanAvl { get; set; }

        public string yDisp { get; set; }

        public string paidtype { get; set; }

        #region for display
        public string dPlans { get; set; }
        public string dataPlans { get; set; }
        public string voicePlans { get; set; }
        public string dDataBal { get; set; }
        public string dNoPlans { get; set; }
        public string dplanexp { get; set; }
        public string dtopup { get; set; }
        public string dCC { get; set; }
        public string menuTopup { get; set; }

        public bool isPrimary { get; set; }
        #endregion

        public string IDDBalance { get; set; }

        public string NightPlanDataBalance { get; set; }

        public string accountNo { get; set; }
        public DateTime statementDate { get; set; }
        public DateTime paymentDueDate { get; set; }
        public string transactionNo { get; set; }
        public string totalAmount { get; set; }
        public string stotalAmount { get; set; }
        public string PaymentDue { get; set; }

        public string sSMSbalance { get; set; }
        public string sVoicebalance { get; set; }

        public bool isExistAccountNo { get; set; }
        public List<FilesModel> _ppbills { get; set; }
        public string Disp_pbillStatus { get; set; }
        public string Disp_nbillStatus { get; set; }

        public string ulDataBalance { get; set; }

        public string totDataPercentage { get; set; }
        public string totDataSize { get; set; }
        public string totDataUsed { get; set; }

        public string totSmsPercentage { get; set; }
        public string totSmsCount { get; set; }
        public string totSmsUsed { get; set; }

        public string totVoicePercentage { get; set; }
        public string totVoiceSize { get; set; }
        public string totVoiceUsed { get; set; }

        public string totIddBalance { get; set; }
        public string totIddPercentage { get; set; }
        public string totIddSize { get; set; }
        public string totIddUsed { get; set; }

        public string description { get; set; }

        #region for roaming plans

        public string totRoamingDataBalance { get; set; }
        public string totRoamingDataPercentage { get; set; }
        public string totRoamingDataSize { get; set; }
        public string totRoamingDataUsed { get; set; }

        public string totRoamingVoiceBalance { get; set; }
        public string totRoamingVoicePercentage { get; set; }
        public string totRoamingVoiceSize { get; set; }
        public string totRoamingVoiceUsed { get; set; }

        public string totRoamingSmsBalance { get; set; }
        public string totRoamingSmsPercentage { get; set; }
        public string totRoamingSmsSize { get; set; }
        public string totRoamingSmsUsed { get; set; }
        #endregion
    }

    public class planDetailsModel
    {
        public string expiryDate { get; set; }

        public string effectiveDate { get; set; }

        public string planId { get; set; }	
	
		public string planName { get; set; }
		
		public int planValidity { get; set; }
			
		public string totalUsage { get; set; }

        public bool isVoice { get; set; }

        public string _planprice { get; set; }

        public double planprice { get; set; }

        public bool PurchaseAllowed { get; set; }
    }

    public class yourPlans
    {
        public string balance { get; set; }
        public string expiry { get; set; }
        public string planName { get; set; }
    }

    public class PaidTypeModel {
        public string email { get; set; }
        public string paidType { get; set; }
        public string msisdn { get; set; }
    }

    public class ResultModel
    {
        public string rcode { get; set; }
        public string rURL { get; set; }
    }
}