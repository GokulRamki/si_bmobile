using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using si_bmobile.Models;
using System.Web.Mvc;

namespace si_bmobile.DAL
{
    interface IPlanRepository : IDisposable
    {

        #region New

        //Bundles
        List<BundleModel> get_all_bundles();
        BundleModel get_bundles_byId(long bundle_id);
        long create_bundle(BundleModel obj_bundle, out string Msg);
        long update_bundle(BundleModel obj_bundle, out string Msg);
        int delete_bundle(long id, out string Msg);

        //Types
        List<PlanTypeModel> get_yplan_types();
        List<SelectListItem> getplan_andplantype();
        List<SelectListItem> getdenoms_andplantype();
        List<plan_denominationModel> getdenom_anddenomtype();

        //Your Plans
        List<plansModel> get_yplans();
        plansModel get_yplan_byId(long yplan_id);
        long create_yplan(plansModel obj_yplan);
        long update_yplan(plansModel obj_yplan);
        bool delete_yplan(long id);

        //Your Plan Price
        List<YPriceModel> get_yplanprice();
        List<YPriceModel> get_allyplanprice();
        plan_priceModel get_yplanprice_byId(long price_id);
        long create_yplanprice(plan_priceModel obj_price);
        long update_yplanprice(plan_priceModel obj_price);
        bool delete_yplanprice(long id);

        //Your Plan denomination
        List<plan_denominationModel> get_yplandenominations();
        plan_denominationModel get_yplandenominationbyId(long ypd_id);
        long create_yplandenomination(plan_denominationModel obj_yplan);
        long update_yplandenomination(plan_denominationModel obj_yplan);
        bool delete_yplandenomination(long id);

        //Your Plan denomination type
        List<DenominationTypeModel> get_yplandenom_types();
        DenominationTypeModel get_yplandenom_typebyId(long ypdt_id);
        long create_yplandenom_type(DenominationTypeModel obj_denom_type);
        long update_yplandenom_type(DenominationTypeModel obj_denom_type);
        bool delete_yplandenom_type(long id);


        #endregion

        List<YPlanUsageModel> get_yourplan_usage(string msisdn_no);

        int purchase_plan(PurchasePlanModel PurchasePlan);

    

        List<plandetails> GetPlans(List<YPriceModel> allplans);

        List<plandetails> GetFavouritePlan(long user_id, List<YPriceModel> allplans);

        int AddFavouritePlan(long user_id, List<plandetails> objdenom);

        plandetails GetPlanDetailsbyDenm_Id(long deno_id);

        tempDenominationModel GetTempPlans(string sSessID);

        bool IsypExpired(string sMsisdn);

        #region old



        //plan
        //List<plansModel> get_allplans();

        //bool add_plan(plansModel plan);

        //bool update_plan(plansModel plan);



        //plan denominations

        //List<plan_denominationModel> get_plan_denomination();

        //bool add_plan_denomination(plan_denominationModel pd);

        //bool update_plan_denomination(plan_denominationModel pd);

        //plan price

        //List<YPriceModel> GetAllPlans();


        //bool add_plan_price(plan_priceModel price);
        //bool update_plan_price(plan_priceModel price);
        //List<YPriceModel> GetActivePlans();


        //List<PlanTypeModel> GetAllPlanTypes();
        //List<plan_priceModel> GetPlanPrice_Edit(List<YPriceModel> price);

        //List<plan_denominationModel> GetAllPlanDenominations();

        //bool addDenominationType(DenominationTypeModel oDT);
        //bool updateDenominationType(DenominationTypeModel oDT);
        //List<DenominationTypeModel> GetAllDenominationType();




        #endregion

        long calc_price(long plan_id, string denomination);

        List<bundle> GetBunldePlans();

        //string CreateBunldePlan(string sData);

        //string UpdateBunldePlan(string sData);

        string CreateBundlePlanCCtpService(string objPlan);

        string UpdateBunldePlanCCtpService(string objPlan);

        string DeleteBunldePlanCCtpService(string sData);

        GetBunldePlansModel getBundlePlan(long id);

    }
}
