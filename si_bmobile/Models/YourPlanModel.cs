using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
namespace si_bmobile.Models
{
    public class YourPlanModel
    {
    }

    public class plansModel
    {

        public long id { get; set; }

        [Required(ErrorMessage = "Plan name required!")]
        [Remote("CheckPlanNameExists", "Admin", AdditionalFields = "name,plan_type_id", ErrorMessage = "Plan name already exits!")]
        public string name { get; set; }

        [Required(ErrorMessage = "Account type required!")]
        public long account_type { get; set; }

        [Required(ErrorMessage = "Min measure id required!")]
        public long Minmeasureid { get; set; }

        [Required(ErrorMessage = "Plan type required!")]
        public long plan_type_id { get; set; }

        public bool isActive { get; set; }

        public bool isDeleted { get; set; }

        public List<SelectListItem> lstPlanType { get; set; }

    }

    public class plan_priceModel
    {

        public long id { get; set; }

        [Required(ErrorMessage = "Plan")]
        public long plan_id { get; set; }

        [Required(ErrorMessage = "Please select plan denomination")]
        public long denom_id { get; set; }

        [Required(ErrorMessage = "Price required")]
        public long price { get; set; }

        public bool isActive { get; set; }

        public bool isDeleted { get; set; }

        [Required(ErrorMessage = "Please select a plan type")]
        public long? plan_type_id { get; set; }

        public List<SelectListItem> lstPlantypes { get; set; }
    }

    public class PurchasePlanModel
    {
        //public long voice_denom_id { get; set; }

        //public long sms_denom_id { get; set; }

        //public long data_denom_id { get; set; }

        public List<Purchasedenomination> pDenomIds { get; set; }

        public List<Purchasedpromotion> PromotionIds { get; set; }

        public decimal tot_amt { get; set; }

        public string from_msisdn { get; set; }

        public string purchase_msisdn { get; set; }

        public string isMobile { get; set; }

        //public long promotion_id { get; set; }

        public bool isTopUp { get; set; }
    }

    public class Purchasedenomination
    {
        public long denomination_id { get; set; }
        public string plan_name { get; set; }
        public double price { get; set; }
    }

    public class Purchasedpromotion
    {
        public long promotion_id { get; set; }
        public string plan_name { get; set; }
        public double price { get; set; }
    }


    public class plan_denominationModel
    {

        public long id { get; set; }

        [Required(ErrorMessage = "Please select plan")]
        public long plan_id { get; set; }

        [Required(ErrorMessage = "Please select a plan type")]
        public long? plan_type_id { get; set; }

        [Required(ErrorMessage = "please enter plan denomination")]
        public string denomination { get; set; }

        public string plan_name { get; set; }

        [Required(ErrorMessage = "Please select plan denomination type")]
        public long denomination_type_id { get; set; }

        public string denominationtypename { get; set; }

        public bool isActive { get; set; }

        public bool isDeleted { get; set; }

        public long unit { get; set; }

        public long denom_id { get; set; }

        

        public string plan_type_name { get; set; }

        public List<SelectListItem> lstPlantypes { get; set; }

        public long price_id { get; set; }


    }

    public class YPriceModel
    {

        public long id { get; set; }

        public long plan_id { get; set; }

        public long Denomination_id { get; set; }

        public long denominationtypeid { get; set; }

        public string denomination { get; set; }

        public string denomination_name { get; set; }

        public long plan_type_id { get; set; }

        public string play_type_name { get; set; }

        public string planName { get; set; }

        public long price { get; set; }

        public bool isActive { get; set; }

        public bool isDeleted { get; set; }

    }

    public class TotalPriceModel
    {
        public decimal Tot_Price { get; set; }

    }

    public class DenominationModel
    {

        public long id { get; set; }

        public long plan_id { get; set; }

        public long denomination_type_id { get; set; }

        public string plan_name { get; set; }

        public string denomination { get; set; }

        public bool IsActive { get; set; }

        public bool IsDeleted { get; set; }



    }


    public class PriceServiceModel
    {
        public long id { get; set; }

        public long plan_id { get; set; }

        public long Denomination_id { get; set; }

        public string planName { get; set; }

        public string denomination { get; set; }

        public string denomination_name { get; set; }

        public long price { get; set; }

        public bool isActive { get; set; }

        public bool isDeleted { get; set; }
    }

    public class DenominationTypeModel
    {
        public long id { get; set; }

        [Required(ErrorMessage = "Type name required!")]
        [Remote("CheckDenoTypeExists", "Admin", ErrorMessage = "Type already exists!")]
        public string type { get; set; }

        [Required(ErrorMessage = "Please select a plan")]
        public long? plan_id { get; set; }

        [Required(ErrorMessage = "Unit required!")]
        public long unit { get; set; }

        public bool isActive { get; set; }
        public bool isDeleted { get; set; }

        public string PlanName { get; set; }

        public string PlanTypeName { get; set; }

        [Required(ErrorMessage = "Please select a plan type")]
        public long? plan_type_id { get; set; }

        public List<SelectListItem> lstPlantypes { get; set; }
    }

    public class PurchasePriceModel
    {

        public long voice_denom_id { get; set; }

        public long sms_denom_id { get; set; }

        public long data_denom_id { get; set; }

        public long int_voice_denom_id { get; set; }

        public decimal tot_amt { get; set; }

        public string from_msisdn { get; set; }

        public string purchase_msisdn { get; set; }

        public string voice_planname { get; set; }
        public string data_planname { get; set; }
        public string sms_planname { get; set; }

        public string type_name { get; set; }


    }

    public class NewPurchasePriceModel
    {
        public string from_msisdn { get; set; }

        public string purchase_msisdn { get; set; }

        public List<plandetails> denomination_ids { get; set; }

        public decimal tot_amt { get; set; }

        public string plan_denominations { get; set; }

        public long type_id { get; set; }

        public long UserId { get; set; }

        public TotalPriceModel tPriceModel { get; set; }

        public List<SelectListItem> YMSISDNlst { get; set; }

        public bool bfav { get; set; }


        public string jsonallplans { get; set; }

        public string ptype { get; set; }


        public bool IsypPuchase { get; set; }

        public string jsonallmsisdn { get; set; }

        public BundlesModel Bundles { get; set; }

        public string displayPromotion { get; set; }

        public int Payment_Id { get; set; }

        public List<YPlanUsageModel> yplanusagelist { get; set; }

        //public List<PaymentMode> PaymentModeList { get; set; }

        public string pr_type { get; set; }
        public string po_type { get; set; }

        public PaymentPGModel pay { get; set; }

        public string jsonSubs { get; set; }

        public bool isFixedbuy { get; set; }
    }

    public class PaymentMode
    {
        public int Payment_Id { get; set; }

        public string PName { get; set; }
    }

    //public class denominationIds 
    //{
    //    public long deno_id { get; set; }

    //    public long plan_id { get; set; }
    //}

    public class plandetails
    {
        public long plan_id { get; set; }
        public long deno_id { get; set; }
        public string denomination { get; set; }
        public string denomination_name { get; set; }
        public string plan_name { get; set; }
        public long plan_type_id { get; set; }
        public decimal price { get; set; }
    }

    public class PlanTypeModel
    {
        public long id { get; set; }
        public string name { get; set; }
    }

    public class tempDenominationModel
    {
        public long Id { get; set; }
        public string denomination_Id { get; set; }
        public string sesstion_id { get; set; }
        public Nullable<DateTime> Created_on { get; set; }

    }

    public class MSISDNBalanceModel
    {
        public string msisdn_no { get; set; }
        public string balance { get; set; }
        public List<yourPlans> YPlanList { get; set; }
    }

    public class YPlanUsageModel
    {
        public string planName { get; set; }
        public string balance { get; set; }
        public string expiry { get; set; }
        public string type { get; set; }
        public string denom_type { get; set; }
        public long usage { get; set; }
        public long total { get; set; }
    }
}