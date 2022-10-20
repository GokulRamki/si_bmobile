using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using si_bmobile.Models;
using si_bmobile.Utils;
using System.Configuration;
using System.Web.Script.Serialization;
using beLib.beRef;
using Newtonsoft.Json;
using si_bmobile.siCCBundleRef;

using System.Web.Mvc;

namespace si_bmobile.DAL
{
    public class PlanRepository : IPlanRepository
    {

        SelfCare proxy;
        IUtilityRepository _util_repo;
        bool bflag;
        string header;
        int iRet;
        UnitOfWork UOW;
        bemobileselfcareEntities _dbselfcare;
        private IBundlePlanSI _siCCBPlan;
        private string sfSvcUid;
        private string sfSvcPwd;

        public PlanRepository()
        {
            this.proxy = new SelfCare();
            this._util_repo = new UtilityRepository();
            this.header = "2013";
            this.bflag = false;
            this.iRet = 0;
            this.UOW = new UnitOfWork();
            this._dbselfcare = new bemobileselfcareEntities();
            this._siCCBPlan = new BundlePlanSIClient();
            this.sfSvcUid = ConfigurationManager.AppSettings["svc_uid"];
            this.sfSvcPwd = ConfigurationManager.AppSettings["svc_pwd"];
        }

        #region bundles

        public List<BundleModel> get_all_bundles()
        {
            //var obj_bundles = _dbselfcare.bundles.Select(n =>
            //    new BundleModel
            //    {

            //        orderby = n.orderby,
            //        //_sPrice = n.Price),
            //        Description = n.Description,
            //        Id = n.Id,
            //        isActive = (bool)n.isActive,
            //        isDeleted = (bool)n.isDelete,
            //        isPostpaid = (bool)n.isPostpaid,
            //        isVoice = n.isVoice,
            //        PlanName = n.PlanName,
            //        Price = n.Price,
            //        Size = n.Size,
            //        Validity = (int)n.Validity,
            //        AccountType=n.AccountType,
            //        sStatus = (n.isActive == true ? "Active" : "InActive"),
            //        sType = (n.isVoice == true ? "Voice" : "Data"),
            //        isChecked = false,
            //        validity_txt=n.validity_txt
            //    }).ToList();

            List<BundleModel> obj = new List<BundleModel>();
            var bnd = _dbselfcare.bundles.Where(x => x.isDelete == false).Select(n=>new BundleModel {
                Id=n.Id,
                PlanName=n.PlanName,
                Description=n.Description,
                Price=n.Price,
                Size=n.Size,
                validity_txt=n.validity_txt,
                isActive=(bool)n.isActive,
                isVoice=n.isVoice,
                orderby=n.orderby
            }).ToList();

            if (bnd != null)
            {
                obj = (List<BundleModel>)bnd;            
            }
            return obj;
        }

        public BundleModel get_bundles_byId(long bundle_id)
        {
            
            var bundles = _dbselfcare.bundles.Where(b => b.isDelete == false && b.Id == bundle_id).Select(n =>
                 new BundleModel
                 {
                     orderby = n.orderby,
                     //_sPrice = n.Price),
                     Description = n.Description,
                     Id = n.Id,
                     isActive = n.isActive??false,
                     isDeleted = n.isDelete??false,
                     isPostpaid = n.isPostpaid??false,
                     isVoice = n.isVoice,
                     PlanName = n.PlanName,
                     Price = n.Price,
                     Size = n.Size,
                     voiceSize=n.voiceSize,
                     smsCount=n.smsCount,
                     isCorporate=n.isCorporate??false,
                     //Validity = (int)n.Validity,
                     AccountType=n.AccountType,
                     //sStatus = (n.isActive == true ? "Active" : "InActive"),
                     //sType = (n.isVoice == true ? "Voice" : "Data"),
                     isChecked = false,
                     SmsAccountType=n.SmsAccountType,
                     VoiceAccountType=n.VoiceAccountType,
                     IddAccountType=n.IddAccountType,
                     isOnlyData = n.isOnlyData??false,
                     isSelfcare = n.isSelfCare??false,      //add selfcare
                     validity_txt=n.validity_txt,
                     romingDataAccountType = n.romingDataAccountType,
                     roamingVoiceAccountType = n.roamingVoiceAccountType,
                     roamingSmsAccountType = n.roamingSmsAccountType
                 }).FirstOrDefault();

            if (bundles.isCorporate == true)
            {
                #region get bundle type from cctopup using web service
                
                string enc_sfSvcUid = _util_repo.AES_JENC(sfSvcUid);
                string enc_sfSvcPwd = _util_repo.AES_JENC(sfSvcPwd);
                string sdata = JsonConvert.SerializeObject(bundles.Id);
                string btype = _siCCBPlan.GetBundleType(enc_sfSvcUid, enc_sfSvcPwd, sdata);
                if (btype != null)
                {
                    getBundleTypeModel btid = JsonConvert.DeserializeObject<getBundleTypeModel>(btype);
                    if (btid != null && btid.resultCode == "0")
                    {
                            bundles.bundle_type_id = btid.bdleTypeId;                      
                    }
                }
                #endregion
            }
            return bundles;
        }

        public long create_bundle(BundleModel obj_bundle, out string Msg)
        {
            Msg = "Failed to create bundle!";
            long lRes = 0;
            if (obj_bundle != null)
            {
                #region selfcare Bundle Insert
                bundle oBM = new bundle();
                oBM.Id = obj_bundle.Id;
                oBM.isActive = obj_bundle.isActive;
                oBM.isPostpaid = obj_bundle.isPostpaid;
                oBM.isVoice = obj_bundle.isVoice;
                oBM.PlanName = obj_bundle.PlanName;
                oBM.Price = obj_bundle.Price;
                oBM.Size = obj_bundle.Size;
                //oBM.Validity = obj_bundle.Validity;
                oBM.AccountType = obj_bundle.AccountType;
                oBM.Created_at = DateTime.Now;
                //oBM.Modified_on = DateTime.Now;
                oBM.isSelfCare = obj_bundle.isSelfcare;    //add selfcare
                oBM.isCorporate = obj_bundle.isCorporate;
                oBM.Description = obj_bundle.Description;
                oBM.orderby = obj_bundle.orderby;
                oBM.isDelete = false;
                oBM.validity_txt = obj_bundle.validity_txt;
                oBM.romingDataAccountType = obj_bundle.romingDataAccountType;
                oBM.roamingVoiceAccountType = obj_bundle.roamingVoiceAccountType;
                oBM.roamingSmsAccountType = obj_bundle.roamingSmsAccountType;
                oBM.SmsAccountType = obj_bundle.SmsAccountType;
                oBM.VoiceAccountType = obj_bundle.VoiceAccountType;
                oBM.IddAccountType = obj_bundle.IddAccountType;
                oBM.isOnlyData = obj_bundle.isOnlyData;
                oBM.voiceSize = obj_bundle.voiceSize;
                oBM.smsCount = obj_bundle.smsCount;                

                _dbselfcare.bundles.Add(oBM);
                _dbselfcare.SaveChanges();

                lRes = oBM.Id;

                if (lRes > 0)
                    Msg = " Bundle created successfully";

                #endregion
            }
            if(obj_bundle.isCorporate==true)
            {
                #region CCtopup BundlePlan Insert using webservice
                string wsMsg=string.Empty;
                string enc_sfSvcUid = _util_repo.AES_JENC(sfSvcUid);
                string enc_sfSvcPwd = _util_repo.AES_JENC(sfSvcPwd);

                BundleplanModel objbp = new BundleplanModel();
                objbp.bundle_id = obj_bundle.Id;
                objbp.bundle_Name = obj_bundle.PlanName;
                objbp.desc = obj_bundle.Description;
                objbp.Price = Convert.ToDecimal(obj_bundle.Price);               
                objbp.isActive = obj_bundle.isActive;
                objbp.bundle_type_id = obj_bundle.bundle_type_id;
                string temp = obj_bundle.validity_txt;
                for (int i = 0; i < temp.Length; i++)
                {

                    if (Char.IsDigit(temp[i]))
                        objbp.Validity += temp[i];
                }

                string sData = JsonConvert.SerializeObject(objbp);
                string sRes = _siCCBPlan.CreateorEdit_BunldePlanSICc(enc_sfSvcUid, enc_sfSvcPwd, sData);
                if (!string.IsNullOrEmpty(sRes))
                {
                    ReturnModel bundleOP = JsonConvert.DeserializeObject<ReturnModel>(sRes);
                    if (bundleOP != null)
                    {
                        if (bundleOP.resultCode == "0")
                        {
                            lRes = 1;
                            wsMsg = "Corporate Bundle Plan created successfully";
                            Msg = wsMsg;
                        }
                        //else if (bundleOP.resultCode == "-111")
                        //{
                        //    wsMsg = "Corporate Bundle Plan creation failed: Bundle ID already exist!";
                        //    Msg = wsMsg;
                        //}
                        else
                        {
                            wsMsg = "ErrorCode:("+ bundleOP.resultCode + "), Corporate Bundle Plan creation failed!";
                            Msg = wsMsg;
                        }
                    }
                }
                #endregion
            }
            return lRes;
        }

        public long update_bundle(BundleModel obj_bundle, out string Msg)
        {
            long Ret = 0;
            Msg = "Bundle Update Failed!";
            var bundles = _dbselfcare.bundles.Where(m => m.Id == obj_bundle.Id).FirstOrDefault();
            if (bundles != null)
            {
                #region selfcare Bundle Update
                //bundles.Id = obj_bundle.Id;
                bundles.Description = obj_bundle.Description;
                bundles.isActive = obj_bundle.isActive;
                bundles.isDelete = false;
                bundles.isPostpaid = obj_bundle.isPostpaid;
                bundles.isVoice = obj_bundle.isVoice;
                bundles.Modified_on = DateTime.Now;
                bundles.orderby = obj_bundle.orderby;
                bundles.PlanName = obj_bundle.PlanName;
                bundles.Price = obj_bundle.Price;
                bundles.Size = obj_bundle.Size;
                //bundles.Validity = obj_bundle.Validity;
                bundles.AccountType = obj_bundle.AccountType;
                bundles.SmsAccountType = obj_bundle.SmsAccountType;
                bundles.VoiceAccountType = obj_bundle.VoiceAccountType;
                bundles.IddAccountType = obj_bundle.IddAccountType;
                bundles.isOnlyData = obj_bundle.isOnlyData;
                bundles.isSelfCare = obj_bundle.isSelfcare;       //add selfcare
                bundles.validity_txt = obj_bundle.validity_txt;
                bundles.isCorporate = obj_bundle.isCorporate;
                bundles.romingDataAccountType = obj_bundle.romingDataAccountType;
                bundles.roamingVoiceAccountType = obj_bundle.roamingVoiceAccountType;
                bundles.roamingSmsAccountType = obj_bundle.roamingSmsAccountType;
                bundles.voiceSize = obj_bundle.voiceSize;
                bundles.smsCount = obj_bundle.smsCount;

               _dbselfcare.SaveChanges();

                Ret = bundles.Id;
                Msg = "Bundle Updated Successfully";
                #endregion
            }
            if (obj_bundle.isCorporate == true)
            {
                #region CCtopup BundlePlan Update using webservice
                string wsMsg = string.Empty;
                string enc_sfSvcUid = _util_repo.AES_JENC(sfSvcUid);
                string enc_sfSvcPwd = _util_repo.AES_JENC(sfSvcPwd);

                BundleplanModel objbp = new BundleplanModel();
                objbp.bundle_id = obj_bundle.Id;
                objbp.bundle_Name = obj_bundle.PlanName;
                objbp.desc = obj_bundle.Description;
                objbp.Price = Convert.ToDecimal(obj_bundle.Price);
                objbp.isActive = obj_bundle.isActive;
                objbp.bundle_type_id = obj_bundle.bundle_type_id;
                string temp = obj_bundle.validity_txt;
                for (int i = 0; i < temp.Length; i++)
                {

                    if (Char.IsDigit(temp[i]))
                        objbp.Validity += temp[i];
                }

                string sData = JsonConvert.SerializeObject(objbp);
                string sRes = _siCCBPlan.CreateorEdit_BunldePlanSICc(enc_sfSvcUid, enc_sfSvcPwd, sData);
                if (!string.IsNullOrEmpty(sRes))
                {
                    ReturnModel bundleOP = JsonConvert.DeserializeObject<ReturnModel>(sRes);
                    if (bundleOP != null)
                    {
                        if (bundleOP.resultCode == "0")
                        {
                            Ret = 1;
                            wsMsg = "Corporate Bundle Plan updated successfully";
                            Msg =  wsMsg;
                        }
                        //else if (bundleOP.resultCode == "-111")
                        //{
                        //    wsMsg = "Corporate Bundle Plan update failed: Bundle ID doesn't exist!";
                        //    Msg = wsMsg;
                        //}
                        else
                        {
                            wsMsg = "ErrorCode:(" + bundleOP.resultCode + "), Corporate Bundle Plan update failed!";
                            Msg = wsMsg;
                        }
                    }
                }
                #endregion
            }

                return Ret;
        }

        public int delete_bundle(long id, out string Msg)
        {
            int Ret = 0;
            Msg = "Bundle Delete is Failed!";
            var bundles = _dbselfcare.bundles.Where(m => m.Id == id && m.isDelete == false).FirstOrDefault();
            if (bundles != null)
            {
                #region Selfcare Bundle Delete
                bundles.isDelete = true;
                bundles.isActive = false;
                _dbselfcare.SaveChanges();

                if (bundles.isDelete == true)
                {
                    Ret = 1;
                    Msg = "Bundle deleted Successfully";

                }
                #endregion
            }
                if (bundles.isCorporate == true)
                {
                    #region CCtopup BundlePlan Delete using webservice
                    string wsMsg = string.Empty;
                    string enc_sfSvcUid = _util_repo.AES_JENC(sfSvcUid);
                    string enc_sfSvcPwd = _util_repo.AES_JENC(sfSvcPwd);

                    string b_id = bundles.Id.ToString();

                    string sData = JsonConvert.SerializeObject(b_id);
                    string sRes = _siCCBPlan.DeleteBunldePlanSICc(enc_sfSvcUid, enc_sfSvcPwd, sData);
                    if (!string.IsNullOrEmpty(sRes))
                    {
                        ReturnModel bundleOP = JsonConvert.DeserializeObject<ReturnModel>(sRes);
                        if (bundleOP != null)
                        {
                            if (bundleOP.resultCode == "0")
                            {
                            Ret = 1;
                            wsMsg = "Corporate Bundle Plan updated successfully";
                                Msg = wsMsg;
                            }
                            else if (bundleOP.resultCode == "-111")
                            {
                                wsMsg = "Corporate Bundle Plan update failed: Bundle ID doesn't exist!";
                                Msg = wsMsg;
                            }
                            else
                            {
                                wsMsg = "ErrorCode:(" + bundleOP.resultCode + "), Corporate Bundle Plan update failed!";
                                Msg = wsMsg;
                            }
                        }
                    }
                    #endregion
                }
            
            return Ret;
        }

        #endregion

        #region plan type

        public List<PlanTypeModel> get_yplan_types()
        {
            var obj_plans = _dbselfcare.plan_type.Select(m => new PlanTypeModel
            {
                id = m.id,
                name = m.name,
            }).ToList();

            return obj_plans;
        }

        public List<SelectListItem> getplan_andplantype()
        {
            var obj_plans = (from p in _dbselfcare.plans.AsEnumerable()
                             join pt in _dbselfcare.plan_type.AsEnumerable() on p.plan_type_id equals pt.id
                             where p.isDeleted == false && p.isActive == true
                             select new SelectListItem
                             {
                                 Text = p.name + "(" + pt.name + ")",
                                 Value = p.Id.ToString()
                             }).ToList();

            return obj_plans;
        }

        public List<SelectListItem> getdenoms_andplantype()
        {
            var obj_plans = (from p in _dbselfcare.plans
                             join pt in _dbselfcare.plan_type on p.plan_type_id equals pt.id
                             join ypd in _dbselfcare.plan_denomination on p.Id equals ypd.plan_id
                             join ypdt in _dbselfcare.denomination_type on ypd.denomination_type_id equals ypdt.Id
                             where ypd.isDeleted == false && ypdt.isDeleted == false && p.isDeleted == false
                             select new plansModel
                             {
                                 name = p.name + "(" + pt.name + ")",
                                 id = p.Id
                             }).Distinct().ToList();


            var ret_items = (from p in obj_plans
                             select new SelectListItem
                             {
                                 Text = p.name,
                                 Value = p.id.ToString()
                             }).ToList();

            return ret_items;
        }

        public List<plan_denominationModel> getdenom_anddenomtype()
        {
            var obj_plans = (from d in _dbselfcare.plan_denomination.AsEnumerable()
                             join dt in _dbselfcare.denomination_type.AsEnumerable() on d.denomination_type_id equals dt.Id
                             where d.isDeleted == false && d.isActive == true && dt.isDeleted == false && dt.isActive == true
                             select new plan_denominationModel
                             {
                                 denomination = d.denomination + " " + dt.type,
                                 id = d.id,
                                 plan_id = d.plan_id
                             }).ToList();

            return obj_plans;
        }


        #endregion

        #region your plans

        public List<plansModel> get_yplans()
        {
            var obj_plans = _dbselfcare.plans.Where(m => m.isDeleted == false).Select(m => new plansModel
            {
                account_type = m.account_type,
                id = m.Id,
                isActive = m.isActive,
                isDeleted = m.isDeleted,
                Minmeasureid = (long)m.minmeasureid,
                name = m.name,
                plan_type_id = (long)m.plan_type_id
            }).ToList();

            return obj_plans;
        }

        public plansModel get_yplan_byId(long yplan_id)
        {
            var plans = _dbselfcare.plans.Where(b => b.isDeleted == false && b.Id == yplan_id).Select(m =>
                 new plansModel
                 {
                     account_type = m.account_type,
                     id = m.Id,
                     isActive = m.isActive,
                     isDeleted = m.isDeleted,
                     Minmeasureid = (long)m.minmeasureid,
                     name = m.name,
                     plan_type_id = (long)m.plan_type_id
                 }).FirstOrDefault();

            return plans;
        }

        public long create_yplan(plansModel obj_yplan)
        {
            long lRes = 0;
            if (obj_yplan != null)
            {
                plan _plan = new plan();
                _plan.account_type = obj_yplan.account_type;
                _plan.Id = obj_yplan.id;
                _plan.isActive = obj_yplan.isActive;
                _plan.isDeleted = obj_yplan.isDeleted;
                _plan.minmeasureid = Convert.ToInt32(obj_yplan.Minmeasureid);
                _plan.plan_type_id = obj_yplan.plan_type_id;
                _plan.name = obj_yplan.name;
                _dbselfcare.plans.Add(_plan);
                _dbselfcare.SaveChanges();
                lRes = _plan.Id;

            }
            return lRes;
        }

        public long update_yplan(plansModel obj_yplan)
        {
            long Ret = 0;
            var _plan = _dbselfcare.plans.Where(m => m.Id == obj_yplan.id).FirstOrDefault();
            if (_plan != null)
            {
                _plan.account_type = obj_yplan.account_type;
                _plan.Id = obj_yplan.id;
                _plan.isActive = obj_yplan.isActive;
                _plan.isDeleted = obj_yplan.isDeleted;
                _plan.minmeasureid = (int)obj_yplan.Minmeasureid;
                _plan.plan_type_id = obj_yplan.plan_type_id;
                _plan.name = obj_yplan.name;
                _dbselfcare.SaveChanges();
                Ret = _plan.Id;

            }
            return Ret;
        }

        public bool delete_yplan(long id)
        {
            bool Ret = false;

            var _plan = _dbselfcare.plans.Where(m => m.Id == id).FirstOrDefault();
            if (_plan != null)
            {
                _plan.isDeleted = true;
                _dbselfcare.SaveChanges();

                Ret = true;
            }
            return Ret;
        }

        #endregion

        #region your plan price

        public List<YPriceModel> get_yplanprice()
        {
            List<YPriceModel> obj_price = new List<YPriceModel>();

            obj_price = (from price in _dbselfcare.plan_price
                         join yp in _dbselfcare.plans on price.plan_id equals yp.Id
                         join ypd in _dbselfcare.plan_denomination on price.denom_id equals ypd.id
                         join ypdt in _dbselfcare.denomination_type on ypd.denomination_type_id equals ypdt.Id
                         join ypt in _dbselfcare.plan_type on yp.plan_type_id equals ypt.id
                         where
                         yp.isDeleted == false && yp.isActive == true
                         && ypd.isDeleted == false && ypd.isActive == true
                         && ypdt.isDeleted == false && ypdt.isActive == true
                        && price.isDeleted == false && price.isActive == true
                         select new YPriceModel
                         {
                             id = price.id,
                             plan_id = yp.Id,
                             planName = yp.name,
                             price = price.price,
                             denomination = ypd.denomination,
                             Denomination_id = ypd.id,
                             denomination_name = ypdt.type,
                             denominationtypeid = ypdt.Id,
                             plan_type_id = ypt.id,
                             play_type_name = ypt.name,
                             isActive = price.isActive,
                             isDeleted = price.isDeleted,

                         }).OrderBy(p => p.id).ToList();
            return obj_price;

        }

        public plan_priceModel get_yplanprice_byId(long price_id)
        {
            var plans =(from pp in _dbselfcare.plan_price
                            join pd in _dbselfcare.plan_denomination on pp.denom_id equals pd.id
                            
                            select 
                 new plan_priceModel
                 {
                     id = pp.id,
                     isActive = pp.isActive,
                     isDeleted = pp.isDeleted,
                     denom_id = pp.denom_id,
                     plan_id = pp.plan_id,
                     price = pp.price,
                     plan_type_id=pd.plan_type_id
                     
                 }).Where(b => b.isDeleted == false && b.id == price_id).FirstOrDefault();

            return plans;
        }

        public long create_yplanprice(plan_priceModel obj_price)
        {
            long lRes = 0;
            if (obj_price != null)
            {
                plan_price _price = new plan_price();
                _price.id = obj_price.id;
                _price.isActive = obj_price.isActive;
                _price.isDeleted = obj_price.isDeleted;
                _price.denom_id = obj_price.denom_id;
                _price.plan_id = obj_price.plan_id;
                _price.price = Convert.ToInt64(obj_price.price);
                _dbselfcare.plan_price.Add(_price);
                _dbselfcare.SaveChanges();
                lRes = _price.id;

            }
            return lRes;
        }

        public long update_yplanprice(plan_priceModel obj_price)
        {
            long Ret = 0;

            var price_id = _dbselfcare.plan_price.Where(m => m.denom_id == obj_price.denom_id).FirstOrDefault();
            if (price_id != null)
            {
                var _price = _dbselfcare.plan_price.Where(m => m.id == price_id.id && m.isDeleted == false).FirstOrDefault();
                if (_price != null)
                {
                   
                    _price.isActive = obj_price.isActive;
                    _price.isDeleted = obj_price.isDeleted;
                    _price.denom_id = obj_price.denom_id;
                    _price.plan_id = obj_price.plan_id;
                    _price.price = Convert.ToInt64(obj_price.price);
                    _dbselfcare.SaveChanges();
                    Ret = _price.id;

                }
            }
            return Ret;
        }

        public bool delete_yplanprice(long id)
        {
            bool Ret = false;

            var _price = _dbselfcare.plan_price.Where(m => m.id == id).FirstOrDefault();
            if (_price != null)
            {
                _price.isDeleted = true;
                _dbselfcare.SaveChanges();
                Ret = true;
            }
            return Ret;
        }

        #endregion

        #region your plan denoms

        public List<plan_denominationModel> get_yplandenominations()
        {
            var obj_plans = (from ypd in _dbselfcare.plan_denomination
                             join ypdt in _dbselfcare.denomination_type on ypd.denomination_type_id equals ypdt.Id
                             join yp in _dbselfcare.plans on ypd.plan_id equals yp.Id
                             join ypt in _dbselfcare.plan_type on ypd.plan_type_id equals ypt.id
                             where yp.isDeleted == false && ypd.isDeleted == false && ypdt.isDeleted == false
                             select new plan_denominationModel
                             {
                                 denomination = ypd.denomination + " " + ypdt.type,
                                 denomination_type_id = (long)ypd.denomination_type_id,
                                 denominationtypename = ypdt.type,
                                 id = ypd.id,
                                 isActive = ypd.isActive,
                                 isDeleted = ypd.isDeleted,
                                 plan_id = ypd.plan_id,
                                 plan_type_id = yp.plan_type_id,
                                 unit = (long)ypd.units,
                                 plan_name = yp.name,
                                 plan_type_name = ypt.name

                             }).ToList();

            return obj_plans;
        }

        public plan_denominationModel get_yplandenominationbyId(long ypd_id)
        {
            var plans = _dbselfcare.plan_denomination.Where(b => b.isDeleted == false && b.id == ypd_id).Select(ypd =>
                 new plan_denominationModel
                 {
                     denomination = ypd.denomination,
                     denomination_type_id = (long)ypd.denomination_type_id,
                     id = ypd.id,
                     isActive = ypd.isActive,
                     isDeleted = ypd.isDeleted,
                     plan_id = ypd.plan_id,
                     plan_type_id = ypd.plan_type_id,
                     unit = (long)ypd.units,
                     denom_id=(long)ypd.id

                 }).FirstOrDefault();

            return plans;
        }

        public long create_yplandenomination(plan_denominationModel obj_yplan)
        {
            long lRes = 0;
            if (obj_yplan != null)
            {
                plan_denomination _plan = new plan_denomination();
                _plan.id = obj_yplan.id;
                _plan.plan_id = obj_yplan.plan_id;
                _plan.units = obj_yplan.unit;
                _plan.denomination = obj_yplan.denomination;
                _plan.denomination_type_id = obj_yplan.denomination_type_id;
                _plan.isActive = obj_yplan.isActive;
                _plan.isDeleted = obj_yplan.isDeleted;
                _plan.plan_type_id = obj_yplan.plan_type_id;
                _dbselfcare.plan_denomination.Add(_plan);
                _dbselfcare.SaveChanges();
                lRes = _plan.id;

            }
            return lRes;
        }

        public long calc_price(long plan_id, string denomination)
        {
            long result = 0;
            if (plan_id > 0 && !string.IsNullOrEmpty(denomination))
            {
                var retail = _dbselfcare.plans.Where(p => p.isActive == true && p.isDeleted == false && p.Id == plan_id).FirstOrDefault();
                if (retail != null)
                {
                    double dRetail = (Convert.ToDouble(denomination) * Convert.ToDouble(retail.plan_price)) * 100;
                    result = Convert.ToInt64(dRetail);
                }
            }
            return result;
        }

        public long update_yplandenomination(plan_denominationModel obj_yplan)
        {
            long Ret = 0;
            var _plan_denom = _dbselfcare.plan_denomination.Where(b => b.isDeleted == false && b.id == obj_yplan.id).FirstOrDefault();
            if (_plan_denom != null)
            {
                _plan_denom.id = obj_yplan.id;
                _plan_denom.plan_id = obj_yplan.plan_id;
                _plan_denom.units = obj_yplan.unit;
                _plan_denom.denomination = obj_yplan.denomination;
                _plan_denom.denomination_type_id = obj_yplan.denomination_type_id;
                _plan_denom.isActive = obj_yplan.isActive;
                _plan_denom.isDeleted = obj_yplan.isDeleted;
                _plan_denom.plan_type_id = obj_yplan.plan_type_id;
                _dbselfcare.SaveChanges();
                Ret = _plan_denom.id;

            }
            return Ret;
        }

        public bool delete_yplandenomination(long id)
        {
            bool Ret = false;

            var _plan_denom = _dbselfcare.plan_denomination.Where(b => b.id == id).FirstOrDefault();
            if (_plan_denom != null)
            {
                _plan_denom.isDeleted = true;
                _dbselfcare.SaveChanges();

                Ret = true;
            }
            return Ret;
        }

        #endregion

        #region your plan denom types

        public List<DenominationTypeModel> get_yplandenom_types()
        {
            var obj_denom_type = (from ypdt in _dbselfcare.denomination_type
                                  join yp in _dbselfcare.plans on ypdt.plan_id equals yp.Id
                                  join ypt in _dbselfcare.plan_type on yp.plan_type_id equals ypt.id
                                  where yp.isDeleted == false && ypdt.isDeleted == false
                                  select new DenominationTypeModel
                                  {
                                      type = ypdt.type,
                                      id = ypdt.Id,
                                      isActive = ypdt.isActive??false,
                                      isDeleted = ypdt.isDeleted??false,
                                      plan_id = ypdt.plan_id,
                                      unit = ypdt.unit??0,
                                      PlanName = yp.name,
                                      plan_type_id = ypt.id,
                                      PlanTypeName = ypt.name
                                  }).ToList();

            return obj_denom_type;
        }

        public DenominationTypeModel get_yplandenom_typebyId(long ypdt_id)
        {
            var obj_denom_type = (from d in _dbselfcare.denomination_type
                                  join p in _dbselfcare.plans on d.plan_id equals p.Id
                                  join t in _dbselfcare.plan_type on p.plan_type_id equals t.id
                                  where d.isDeleted == false && d.Id == ypdt_id
                                  select new DenominationTypeModel
                                  {
                                      type = d.type,
                                      id = d.Id,
                                      isActive = d.isActive??false,
                                      isDeleted = d.isDeleted??false,
                                      plan_id = d.plan_id,
                                      unit = d.unit??0,
                                      plan_type_id = t.id,
                                      PlanTypeName = t.name,
                                      PlanName = p.name
                                  }).FirstOrDefault();

            return obj_denom_type;
        }

        public long create_yplandenom_type(DenominationTypeModel obj_denom_type)
        {
            long lRes = 0;
            if (obj_denom_type != null)
            {
                denomination_type _plan_dy = new denomination_type();
                _plan_dy.Id = obj_denom_type.id;
                _plan_dy.plan_id = Convert.ToInt64(obj_denom_type.plan_id);
                _plan_dy.unit = obj_denom_type.unit;
                _plan_dy.type = obj_denom_type.type;
                _plan_dy.isActive = obj_denom_type.isActive;
                _plan_dy.isDeleted = obj_denom_type.isDeleted;
                _dbselfcare.denomination_type.Add(_plan_dy);
                _dbselfcare.SaveChanges();
                lRes = _plan_dy.Id;

            }
            return lRes;
        }

        public long update_yplandenom_type(DenominationTypeModel obj_denom_type)
        {
            long Ret = 0;
            var _plan_denom_type = _dbselfcare.denomination_type.Where(b => b.isDeleted == false && b.Id == obj_denom_type.id).FirstOrDefault();
            if (_plan_denom_type != null)
            {
                _plan_denom_type.Id = obj_denom_type.id;
                _plan_denom_type.plan_id = Convert.ToInt64(obj_denom_type.plan_id);
                _plan_denom_type.unit = obj_denom_type.unit;
                _plan_denom_type.type = obj_denom_type.type;
                _plan_denom_type.isActive = obj_denom_type.isActive;
                _plan_denom_type.isDeleted = obj_denom_type.isDeleted;
                _dbselfcare.SaveChanges();
                Ret = _plan_denom_type.Id;
            }
            return Ret;
        }

        public bool delete_yplandenom_type(long id)
        {
            bool Ret = false;

            var _plan_denom_type = _dbselfcare.denomination_type.Where(b => b.Id == id).FirstOrDefault();
            if (_plan_denom_type != null)
            {
                _plan_denom_type.isDeleted = true;
                _dbselfcare.SaveChanges();

                Ret = true;
            }
            return Ret;
        }

        #endregion

        public List<YPlanUsageModel> get_yourplan_usage(string msisdn_no)
        {
            var data = proxy.getYourPlanUsage(_util_repo.AES_ENC(msisdn_no), msisdn_no);

            var objYU = new List<YPlanUsageModel>();

            if (data != "801")
                objYU = JsonConvert.DeserializeObject<List<YPlanUsageModel>>(data);

            return objYU;
        }

        public int purchase_plan(PurchasePlanModel PurchasePlan)
        {
            string jsonPurchanseplan = JsonConvert.SerializeObject(PurchasePlan);
            proxy.purchaseTriple(_util_repo.AES_ENC(header), jsonPurchanseplan, out iRet, out bflag);
            return iRet;
        }

        public List<plandetails> GetFavouritePlan(long user_id, List<YPriceModel> allplans)
        {
           // var data = proxy.getFavoritePlan(_util_repo.AES_ENC(user_id.ToString()), user_id, true);
            List<plandetails> objdenom_plan = new List<plandetails>();

            var favs = (from f in _dbselfcare.favorite_plans

                        where f.user == user_id && f.isActive == true && f.isDelete == false
                        select new plandetails
                        {
                            plan_type_id = (long)f.plan_type_id,
                            plan_id = (long)f.plan_id,
                            deno_id = (long)f.denom_id,
                        }).ToList();
            if (favs.Count > 0)
            {
                var vallplans = (from p in allplans select new { p.plan_id, p.planName, p.plan_type_id }).Distinct().ToList();
                if (vallplans.Count > 0)
                {
                    foreach (var vp in vallplans)
                    {
                        plandetails obj = new plandetails();

                        var favPlan = (from n in favs where n.plan_id == vp.plan_id select n).FirstOrDefault();

                        if(favPlan != null)
                        {
                            obj.deno_id = favPlan.deno_id;
                            obj.plan_id = favPlan.plan_id;
                            obj.plan_name = vp.planName;
                            obj.denomination_name = favPlan.denomination_name;
                            obj.plan_type_id = vp.plan_type_id;
                            objdenom_plan.Add(obj);
                        }
                        else
                        {
                            obj.deno_id = 0;
                            obj.plan_id = vp.plan_id;
                            obj.plan_name = vp.planName;
                            obj.denomination_name = "";
                            obj.plan_type_id = vp.plan_type_id;
                            objdenom_plan.Add(obj);
                        }
                        //foreach (var fp in favs)
                        //{
                        //    if (fp.plan_id == vp.plan_id)
                        //    {
                        //        obj.deno_id = fp.deno_id;
                        //        obj.plan_id = fp.plan_id;
                        //        obj.plan_name = fp.plan_name;
                        //        obj.denomination_name = fp.denomination_name;
                        //        obj.plan_type_id = fp.plan_type_id;
                        //        objdenom_plan.Add(obj);
                        //    }
                        //    else
                        //    {
                        //        obj.deno_id = 0;
                        //        obj.plan_id = vp.plan_id;
                        //        obj.plan_name = vp.planName;
                        //        obj.denomination_name = "";
                        //        obj.plan_type_id = vp.plan_type_id;
                        //        objdenom_plan.Add(obj);
                        //    }
                        //}
                    }
                }

            }
           
          
            objdenom_plan = objdenom_plan.OrderBy(p => p.plan_id).ToList();
            return objdenom_plan;
        }

        public int AddFavouritePlan(long user_id, List<plandetails> objdenom)
        {
            int iRet = -1;
            //string objplansPrice = JsonConvert.SerializeObject(objdenom);

            DelFavPlans(user_id);
            long retval = SaveFavPlans(user_id, objdenom);
            if (retval > 0)
                iRet = 0;
            //proxy.addFavoritePlan(_util_repo.AES_ENC(user_id.ToString()), user_id, true, objplansPrice, out iRet, out bflag);
            return iRet;
        }
        
        private void DelFavPlans(long user_id)
        {
            var userfav = _dbselfcare.favorite_plans.Where(f => f.user == user_id).ToList();
            if (userfav.Count > 0)
            {
                foreach (var user in userfav)
                {
                    favorite_plans fp = new favorite_plans();
                    fp = _dbselfcare.favorite_plans.Where(u => u.Id == user.Id).FirstOrDefault();
                    fp.isDelete = true;
                    _dbselfcare.SaveChanges();

                }
            }
        }

        private long SaveFavPlans(long user_id, List<plandetails> objdenom)
        {
            long retVal = 0;
            if (objdenom.Count > 0)
            {
                foreach (var item in objdenom)
                {

                    favorite_plans fpn = new favorite_plans();
                    fpn.Id = 0;
                    fpn.isActive = true;
                    fpn.isDelete = false;
                    fpn.plan_id = item.plan_id;
                    fpn.plan_type_id = item.plan_type_id;
                    fpn.denom_id = item.deno_id;
                    fpn.date = DateTime.Now;
                    fpn.user = user_id;
                    _dbselfcare.favorite_plans.Add(fpn);
                    _dbselfcare.SaveChanges();
                    retVal = fpn.Id;
                }
            }
            return retVal;
        }

        public List<plandetails> GetPlans(List<YPriceModel> allplans)
        {
            var dplan = (from d in allplans.ToList() select new { d.plan_id, d.planName, d.plan_type_id }).Distinct();

            List<plandetails> plans = new List<plandetails>();
            foreach (var item in dplan)
            {
                plandetails obj = new plandetails();
                obj.plan_id = item.plan_id;
                obj.plan_name = item.planName;
                obj.plan_type_id = item.plan_type_id;
                plans.Add(obj);
            }
            return plans;
        }

        #region TEMP PLANPRICING

        public tempDenominationModel GetTempPlans(string sSessID)
        {
            temp_yplans tPlans = new temp_yplans();
            tPlans = UOW.temp_yplans_repo.Get(filter: t => t.Session_Id == sSessID).LastOrDefault();

            tempDenominationModel oTmp = new tempDenominationModel();

            if (tPlans != null)
            {
                oTmp.sesstion_id = tPlans.Session_Id;
                oTmp.denomination_Id = tPlans.Denomination_Ids;
                oTmp.Created_on = tPlans.CreatedOn;
            }

            return oTmp;

        }

        public bool IsypExpired(string sMsisdn)
        {
            bool bflag = false;
            string Res = proxy.checkYourPlanExpired(_util_repo.AES_ENC(sMsisdn), sMsisdn);
            if (Res.ToLower() == "true")
                bflag = true;
            return bflag;

        }

        #endregion

        #region Plan Details w.r.t denom_id

        public plandetails GetPlanDetailsbyDenm_Id(long deno_id)
        {
            plandetails oPD = new plandetails();
            var Result = proxy.getPlanDenomination(_util_repo.AES_ENC(header), deno_id.ToString());
            if (Result != null)
                oPD = JsonConvert.DeserializeObject<plandetails>(Result);
            return oPD;
        }

        #endregion

        public List<YPriceModel> get_allyplanprice()
        {
            List<YPriceModel> obj_price = new List<YPriceModel>();

            obj_price = (from price in _dbselfcare.plan_price
                         join yp in _dbselfcare.plans on price.plan_id equals yp.Id
                         join ypd in _dbselfcare.plan_denomination on price.denom_id equals ypd.id
                         join ypdt in _dbselfcare.denomination_type on ypd.denomination_type_id equals ypdt.Id
                         join ypt in _dbselfcare.plan_type on yp.plan_type_id equals ypt.id
                         where
                         yp.isDeleted == false
                         && ypd.isDeleted == false
                         && ypdt.isDeleted == false
                        && price.isDeleted == false
                         select new YPriceModel
                         {
                             id = price.id,
                             plan_id = yp.Id,
                             planName = yp.name,
                             price = price.price,
                             denomination = ypd.denomination,
                             Denomination_id = ypd.id,
                             denomination_name = ypdt.type,
                             denominationtypeid = ypdt.Id,
                             plan_type_id = ypt.id,
                             play_type_name = ypt.name,
                             isActive = price.isActive,
                             isDeleted = price.isDeleted,

                         }).OrderBy(p => p.id).ToList();
            return obj_price;

        }

        #region for Data Plan

        public List<bundle> GetBunldePlans()
        {
            List<bundle> objBundles = _dbselfcare.bundles.Where(x => x.isDelete == false).ToList();

            return objBundles;
        }

        //public string CreateBunldePlan(string sData)
        //{
        //    string Res = "1";

        //    if (!string.IsNullOrEmpty(sData))
        //    {
        //        BunldePlanServiceModel objPlan = JsonConvert.DeserializeObject<BunldePlanServiceModel>(sData);
        //        if (objPlan != null)
        //        {
        //            bundle objBundle = _dbselfcare.bundles.Where(x => x.Id == objPlan.bundleId && x.isDelete == false).FirstOrDefault();
                   

        //            if (objBundle == null)
        //            {
        //                bundle obj = new bundle();
        //                obj.Id = objPlan.bundleId;
        //                obj.PlanName = objPlan.planName;
        //                obj.Description = objPlan.Description;
        //                obj.Price = objPlan.Price;
        //                obj.Size = objPlan.Size;
        //                obj.Validity = objPlan.Validity;
        //                obj.isPostpaid = objPlan.isPostpaid;
        //                obj.isActive = objPlan.isActive;
        //                obj.isVoice = objPlan.isVoice;
        //                obj.Created_at = DateTime.Now;
        //                //obj.Modified_on = objPlan.Modified_on;
        //                obj.isDelete = false;
        //                obj.AccountType = objPlan.AccountType;
        //                obj.SmsAccountType = objPlan.SmsAccountType;
        //                obj.VoiceAccountType = objPlan.VoiceAccountType;
        //                obj.IddAccountType = objPlan.IddAccountType;
        //                obj.UlAccountType = objPlan.UlAccountType;
        //                obj.voiceSize = objPlan.voiceSize;
        //                obj.smsCount = objPlan.smsCount;
        //                obj.isOnlyData = objPlan.isOnlyData;

        //                _dbselfcare.bundles.Add(obj);
        //                _dbselfcare.SaveChanges();

        //                Res = "0";
        //            }
        //            else
        //                Res = "-111";
        //        }
        //    }

        //    return Res;
        //}


        //public string UpdateBunldePlan(string sData)
        //{
        //    string Res = "1";

        //    if (!string.IsNullOrEmpty(sData))
        //    {
        //        BunldePlanServiceModel objPlan = JsonConvert.DeserializeObject<BunldePlanServiceModel>(sData);
        //        if (objPlan != null)
        //        {
        //            bundle obj = _dbselfcare.bundles.Where(x => x.Id == objPlan.bundleId).FirstOrDefault();
        //            if (obj != null)
        //            {
        //                //obj.Id = objPlan.bundleId;
        //                obj.PlanName = objPlan.planName;
        //                obj.Description = objPlan.Description;
        //                obj.Price = objPlan.Price;
        //                obj.Size = objPlan.Size;
        //                obj.Validity = objPlan.Validity;
        //                obj.isPostpaid = objPlan.isPostpaid;
        //                obj.isActive = objPlan.isActive;
        //                obj.isVoice = objPlan.isVoice;
        //                obj.Modified_on = DateTime.Now;
        //                obj.AccountType = objPlan.AccountType;
        //                obj.SmsAccountType = objPlan.SmsAccountType;
        //                obj.VoiceAccountType = objPlan.VoiceAccountType;
        //                obj.IddAccountType = objPlan.IddAccountType;
        //                obj.UlAccountType = objPlan.UlAccountType;
        //                obj.voiceSize = objPlan.voiceSize;
        //                obj.smsCount = objPlan.smsCount;
        //                obj.isOnlyData = objPlan.isOnlyData;

        //                _dbselfcare.SaveChanges();

        //                Res = "0";
        //            }
        //            else
        //                Res = "-111";
        //        }
        //    }

        //    return Res;
        //}

        #endregion

        #region create bundle plan using cctopup service

        public string CreateBundlePlanCCtpService(string sData)
        {
            string Res = "1";

            if (!string.IsNullOrEmpty(sData))
            {
                BunldePlanServiceModel objPlan = JsonConvert.DeserializeObject<BunldePlanServiceModel>(sData);

                if (objPlan != null)
                {
                    bundle objBundle = _dbselfcare.bundles.Where(x => x.Id == objPlan.bundleId && x.isDelete == false).FirstOrDefault();

                    if (objBundle == null)
                    {
                        bundle obj = new bundle();
                        obj.Id = objPlan.bundleId;
                        obj.isActive = objPlan.isActive;
                        obj.isPostpaid = objPlan.isPostpaid;
                        obj.isVoice = objPlan.isVoice;
                        obj.PlanName = objPlan.planName;
                        obj.Price = objPlan.Price;
                        obj.Size = objPlan.Size;
                        //obj.Validity = objPlan.Validity;
                        obj.AccountType = objPlan.AccountType;
                        obj.Created_at = DateTime.Now;
                        obj.isCorporate = true;
                        obj.Description = objPlan.Description;
                        obj.orderby = objPlan.orderby;
                        obj.isDelete = false;
                        obj.validity_txt = objPlan.Validity.ToString();
                        obj.romingDataAccountType = objPlan.romingDataAccountType;
                        obj.roamingVoiceAccountType = objPlan.roamingVoiceAccountType;
                        obj.roamingSmsAccountType = objPlan.roamingSmsAccountType;
                        obj.SmsAccountType = objPlan.SmsAccountType;
                        obj.VoiceAccountType = objPlan.VoiceAccountType;
                        obj.IddAccountType = objPlan.IddAccountType;
                        obj.isOnlyData = objPlan.isOnlyData;
                        obj.voiceSize = objPlan.voiceSize;
                        obj.smsCount = objPlan.smsCount;

                        _dbselfcare.bundles.Add(obj);
                        _dbselfcare.SaveChanges();

                        if (obj.Id > 0)
                        {
                            Res = "0";
                        }

                    }
                    else
                    {
                        Res = "-111";
                    }
                }
            
            }
            return Res;
        }

        #endregion

        #region update bundle plan using cctopup service
        public string UpdateBunldePlanCCtpService(string sData)
        {
            string Res = "1";
            long rtn = 0;
            if (!string.IsNullOrEmpty(sData))
            {
                BunldePlanServiceModel objPlan = JsonConvert.DeserializeObject<BunldePlanServiceModel>(sData);
                if (objPlan != null)
                {
                    bundle objBundle = _dbselfcare.bundles.Where(x => x.Id == objPlan.bundleId && x.isDelete == false).FirstOrDefault();
                    if (objBundle != null)
                    {
                        //obj.Id = objPlan.bundleId;
                        objBundle.Description = objPlan.Description;
                        objBundle.isActive = objPlan.isActive;
                        objBundle.isDelete = false;
                        objBundle.isPostpaid = objPlan.isPostpaid;
                        objBundle.isVoice = objPlan.isVoice;
                        objBundle.Modified_on = DateTime.Now;
                        objBundle.orderby = objPlan.orderby;
                        objBundle.PlanName = objPlan.planName;
                        objBundle.Price = objPlan.Price;
                        objBundle.Size = objPlan.Size;
                        //objBundle.Validity = objPlan.Validity;
                        objBundle.AccountType = objPlan.AccountType;
                        objBundle.SmsAccountType = objPlan.SmsAccountType;
                        objBundle.VoiceAccountType = objPlan.VoiceAccountType;
                        objBundle.IddAccountType = objPlan.IddAccountType;
                        objBundle.isOnlyData = objPlan.isOnlyData;
                        objBundle.validity_txt = objPlan.Validity.ToString();
                        objBundle.isCorporate = true;
                        objBundle.romingDataAccountType = objPlan.romingDataAccountType;
                        objBundle.roamingVoiceAccountType = objPlan.roamingVoiceAccountType;
                        objBundle.roamingSmsAccountType = objPlan.roamingSmsAccountType;
                        objBundle.voiceSize = objPlan.voiceSize;
                        objBundle.smsCount = objPlan.smsCount;

                        _dbselfcare.SaveChanges();
                        rtn = objBundle.Id;
                        Res = "0";
                    }
                    else
                        Res = "-111";
                }
            }

            return Res;
        }
        #endregion

        #region delete bundle plan using cctopup service
        public string DeleteBunldePlanCCtpService(string sData)
        {
            string Res = "1";

            if (!string.IsNullOrEmpty(sData))
            {
                long bid = JsonConvert.DeserializeObject<long>(sData);
                if (bid > 0)
                {
                    bundle obj = _dbselfcare.bundles.Where(x => x.Id == bid && x.isDelete == false).FirstOrDefault();
                    if (obj != null)
                    {

                        obj.isDelete = true;
                        obj.isActive = false;
                                               
                        _dbselfcare.SaveChanges();

                        Res = "0";
                    }
                    //else
                    //    Res = "-111";
                }
            }

            return Res;
        }
        #endregion

        #region get bundle plan details using cctopup service
        public GetBunldePlansModel getBundlePlan(long id)
        {
            GetBunldePlansModel objBund = new GetBunldePlansModel();
            if (id > 0)
            {
               
                objBund.resultCode = "1";
                objBund.bundles = _dbselfcare.bundles.Where(x => x.Id == id && x.isDelete == false).FirstOrDefault();

                if (objBund.bundles != null)
                {                    
                    objBund.resultCode = "0";
                }                      
            }
            return objBund;
        }
        #endregion

        #region Dispose Objects

        private bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    proxy.Dispose();
                    _util_repo.Dispose();
                    UOW.Dispose();
                    _dbselfcare.Dispose();
                }
            }
            this.disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}