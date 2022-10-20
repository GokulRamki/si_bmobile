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

namespace si_bmobile.DAL
{
    public class KYCRepository : IKYCRepository, IDisposable
    {
        UnitOfWork UOW;
        bmtestshopDBEntities _db;
        IUtilityRepository _Utility_Repo;
        private string idNumber;
        private string idType;
        private string countryCode;
        private string mtitle = ConfigurationManager.AppSettings["Mtitle"].ToString();
        private string ftitle = ConfigurationManager.AppSettings["Ftitle"].ToString();
        public KYCRepository()
        {
            this.UOW = new UnitOfWork();
            this._db = new bmtestshopDBEntities();
            this._Utility_Repo = new UtilityRepository();
            this.idNumber = ConfigurationManager.AppSettings["idNumber"].ToString();
            this.idType = ConfigurationManager.AppSettings["idType"].ToString();
            this.countryCode = ConfigurationManager.AppSettings["countryCode"].ToString();

        }
       

        #region KYC Customer List for grid
        public List<kyc_grid_list> getKycGridValue()
        {
            List<kyc_grid_list> kyc_grid = new List<kyc_grid_list>();

            List<tbl_kyc_customer> ll = new List<tbl_kyc_customer>();

                ll=UOW.kyc_Customer_Repo.Get().ToList();

                kyc_grid = (from k in _db.kyc_customer_Details.AsEnumerable()
                            join p in _db.kyc_customer_Purchase on k.cust_id equals p.cust_id
                            join a in _db.kyc_customer_acknowledgement on k.cust_id equals a.cust_id
                            join i in _db.kyc_customer_Identification on k.cust_id equals i.cust_id
                            join pr in _db.province_codes on k.province equals pr.CODE
                            join d in _db.district_codes on k.town equals d.DISTRICT_C
                            where k.is_active==true && k.is_deleted==false
                            select new kyc_grid_list
                            {
                                id = k.cust_id,
                                Name = k.name,
                                Surname=k.surname,
                                Sex = k.sex,
                                Is_employed=(k.is_employed==true)?"Yes":"No",
                                pobox = k.pobox,
                                town = d.DISTRICT_N,
                                province = pr.PROVINCE,
                                suburb = k.suburb,
                                email = k.email,
                                Purchase_date=k.kycdate.ToShortDateString(),
                                Customersignature = (k.signature == true) ? "Signed" : "Unsigned",
                                Is_nameMatched=(a.is_name_matched==true)?"Yes":"No",
                                Is_documentGenuine=(a.is_doc_genuine==true)?"Yes":"No",
                                is_officerSignature = (a.is_officer_sign == true) ? "Signed" : "Unsigned",
                                Acknowledge_date = a.ack_date.ToShortDateString(),
                                drv_lic=i.drv_lic,
                                student_id=i.student_id,
                                emp_id=i.emp_id,
                                nasfund_id=i.nasfund_id,
                                nbw_super_id=i.nbw_super_id,
                                other_id=i.other_id,
                                cust_status=p.cust_status,
                                cust_handset_model1=p.cust_handset_model1,
                                cust_handset_model2=p.cust_handset_model2,
                                cust_handset_model3=p.cust_handset_model3,
                                cust_imei_no1=p.cust_imei_no1,
                                cust_imei_no2=p.cust_imei_no2,
                                cust_imei_no3=p.cust_imei_no3,
                                cust_mobile_no1=p.cust_mobile_no1,
                                cust_mobile_no2=p.cust_mobile_no2,
                                cust_mobile_no3=p.cust_mobile_no3,
                                mobile_no = p.cust_mobile_no1,
                                dob=k.dob,
                                kyc_sim_log_list = Get_Sim_Log_for_customer_detail_list(p.cust_mobile_no1,p.cust_mobile_no2,p.cust_mobile_no3),
                                lt_ref=k.lt_ref
                            }).ToList();
            return kyc_grid;
        }
        #endregion

        #region Get Kyc Customer Details
        public kyc_Custome_Detail_Model getKYCCustomerDetailsById(long id)
        {
            kyc_Custome_Detail_Model kyc_CustomerDetail_obj = new kyc_Custome_Detail_Model();

            kyc_CustomerDetail_obj.kyc_cust_purchase = UOW.kyc_Cust_Purchase_Repo.Get(filter: p => p.cust_id == id).FirstOrDefault();
            kyc_CustomerDetail_obj.kyc_cust_Details = UOW.kyc_Customer_Repo.Get(filter: c => c.cust_id == id).FirstOrDefault();
            kyc_CustomerDetail_obj.kyc_cust_Details.dob = Convert.ToDateTime(kyc_CustomerDetail_obj.kyc_cust_Details.dob).ToShortDateString();
            kyc_CustomerDetail_obj.kyc_cust_identification = UOW.kyc_Cust_identification_Repo.Get(filter: i => i.cust_id == id).FirstOrDefault();
            kyc_CustomerDetail_obj.kyc_cust_ack = UOW.kyc_Cust_acknowledge_Repo.Get(filter: a => a.cust_id == id).FirstOrDefault();

            return kyc_CustomerDetail_obj;
        }
        #endregion

        #region Check any one identification Present in customer identification
                public int CheckCustomerIdentity(kyc_Custome_Detail_Model obj)
                {
                    if (obj.kyc_cust_identification != null)
                    {
                        if(((obj.kyc_cust_identification.is_drv_lic==true) && (!string.IsNullOrEmpty( obj.kyc_cust_identification.drv_lic))) || ((obj.kyc_cust_identification.is_student_id==true) && (!string.IsNullOrEmpty(obj.kyc_cust_identification.student_id))) || ((obj.kyc_cust_identification.is_emp_id==true) && (!string.IsNullOrEmpty(obj.kyc_cust_identification.emp_id))) || ((obj.kyc_cust_identification.is_nasfund_id==true) && (!string.IsNullOrEmpty(obj.kyc_cust_identification.nasfund_id))) || ((obj.kyc_cust_identification.is_nbw_super_id==true) && (!string.IsNullOrEmpty(obj.kyc_cust_identification.nbw_super_id))) || ((!string.IsNullOrEmpty(obj.kyc_cust_identification.other_id)) && (!string.IsNullOrEmpty(obj.kyc_cust_identification.other_id_name))) )
                        {
                            return 0;
                        }
                    }
                    return 1;
                }
        #endregion

        #region Update Kyc Customer Details
        public int UpdateKyc_CustomerDetails(kyc_Custome_Detail_Model obj)
        {
            tbl_kyc_customer kyc_cust = new tbl_kyc_customer();
            int result = 1;
            if (obj.kyc_cust_Details != null)
            {
                if (CheckCustomerIdentity(obj) == 0)
                {

                    kyc_cust = UOW.kyc_Customer_Repo.Get(filter: g => g.cust_id == obj.kyc_cust_Details.cust_id).FirstOrDefault();
                    kyc_cust.name = obj.kyc_cust_Details.name;
                    kyc_cust.surname = obj.kyc_cust_Details.surname;
                    kyc_cust.sex = obj.kyc_cust_Details.sex;
                    kyc_cust.is_employed = obj.kyc_cust_Details.is_employed;
                    kyc_cust.pobox = obj.kyc_cust_Details.pobox;
                    kyc_cust.town = obj.kyc_cust_Details.town;
                    kyc_cust.province = obj.kyc_cust_Details.province;
                    kyc_cust.suburb = obj.kyc_cust_Details.suburb;
                    kyc_cust.email = obj.kyc_cust_Details.email;
                    kyc_cust.signature = obj.kyc_cust_Details.signature;
                    kyc_cust.kycdate = obj.kyc_cust_Details.kycdate;
                    kyc_cust.modified_on = DateTime.Now;
                    kyc_cust.dob = obj.kyc_cust_Details.dob;

                    UOW.kyc_Customer_Repo.Update(kyc_cust);
                    UOW.Save();

                    tbl_kyc_customer_acknowlegement kyc_ack = new tbl_kyc_customer_acknowlegement();
                    kyc_ack = UOW.kyc_Cust_acknowledge_Repo.Get(filter: g => g.ack_id == obj.kyc_cust_ack.ack_id).FirstOrDefault();
                    kyc_ack.ack_date = obj.kyc_cust_ack.ack_date;
                    kyc_ack.is_doc_genuine = obj.kyc_cust_ack.is_doc_genuine;
                    kyc_ack.is_name_matched = obj.kyc_cust_ack.is_name_matched;
                    kyc_ack.is_officer_sign = obj.kyc_cust_ack.is_officer_sign;

                    UOW.kyc_Cust_acknowledge_Repo.Update(kyc_ack);
                    UOW.Save();

                    tbl_kyc_customer_identification kyc_idnt = new tbl_kyc_customer_identification();
                    kyc_idnt = UOW.kyc_Cust_identification_Repo.Get(filter: g => g.cust_identity_id == obj.kyc_cust_identification.cust_identity_id).FirstOrDefault();
                    kyc_idnt.drv_lic = obj.kyc_cust_identification.drv_lic;
                    kyc_idnt.emp_id = obj.kyc_cust_identification.emp_id;
                    kyc_idnt.is_drv_lic = obj.kyc_cust_identification.is_drv_lic;
                    kyc_idnt.is_emp_id = obj.kyc_cust_identification.is_emp_id;
                    kyc_idnt.is_nasfund_id = obj.kyc_cust_identification.is_nasfund_id;
                    kyc_idnt.is_nbw_super_id = obj.kyc_cust_identification.is_nbw_super_id;

                    kyc_idnt.is_student_id = obj.kyc_cust_identification.is_student_id;
                    kyc_idnt.nasfund_id = obj.kyc_cust_identification.nasfund_id;
                    kyc_idnt.nbw_super_id = obj.kyc_cust_identification.nbw_super_id;

                    if (obj.kyc_cust_identification.other_id != null || obj.kyc_cust_identification.other_id_name != null)
                    {
                        kyc_idnt.is_other_id = true;
                        kyc_idnt.other_id = obj.kyc_cust_identification.other_id;
                        kyc_idnt.other_id_name = obj.kyc_cust_identification.other_id_name;
                    }
                    else
                    {
                        kyc_idnt.is_other_id = false;
                    }

                    kyc_idnt.student_id = obj.kyc_cust_identification.student_id;

                    UOW.kyc_Cust_identification_Repo.Update(kyc_idnt);
                    UOW.Save();

                    tbl_kyc_customer_purchase kyc_purchase = new tbl_kyc_customer_purchase();
                    kyc_purchase = UOW.kyc_Cust_Purchase_Repo.Get(filter: g => g.cust_pur_id == obj.kyc_cust_purchase.cust_pur_id).FirstOrDefault();
                    kyc_purchase.cust_handset_model1 = obj.kyc_cust_purchase.cust_handset_model1;
                    kyc_purchase.cust_handset_model2 = obj.kyc_cust_purchase.cust_handset_model2;
                    kyc_purchase.cust_handset_model3 = obj.kyc_cust_purchase.cust_handset_model3;
                    kyc_purchase.cust_imei_no1 = obj.kyc_cust_purchase.cust_imei_no1;
                    kyc_purchase.cust_imei_no2 = obj.kyc_cust_purchase.cust_imei_no2;
                    kyc_purchase.cust_imei_no3 = obj.kyc_cust_purchase.cust_imei_no3;
                    kyc_purchase.cust_mobile_no1 = obj.kyc_cust_purchase.cust_mobile_no1;
                    kyc_purchase.cust_mobile_no2 = obj.kyc_cust_purchase.cust_mobile_no2;
                    kyc_purchase.cust_mobile_no3 = obj.kyc_cust_purchase.cust_mobile_no3;
                    kyc_purchase.cust_status = obj.kyc_cust_purchase.cust_status;

                    UOW.kyc_Cust_Purchase_Repo.Update(kyc_purchase);
                    UOW.Save();

                    result = 0;
                }
                else
                {
                    result = 2;
                }
            }
            return result;

        }

        #endregion

        #region Dropdown bind for Province and District
        public List<PROVINCIAL_CODES> GetProvince()
        {
            List<PROVINCIAL_CODES> province = new List<PROVINCIAL_CODES>();
            province = UOW.Province_codes_Repo.Get().OrderBy(n => n.PROVINCE).ToList();
            return province;
        }
        public List<DISTRICT_CODES> GetDistrictByProvinceID(int id)
        {
            List<DISTRICT_CODES> district = new List<DISTRICT_CODES>();
            district = UOW.District_Codes_Repo.Get(filter: g => g.PROVINCE_C == id).OrderBy(n => n.DISTRICT_N).ToList();
            return district;
        }
      
        public List<DISTRICT_CODES> GetDistrict()
        {
            List<DISTRICT_CODES> district = new List<DISTRICT_CODES>();
            district = UOW.District_Codes_Repo.Get().OrderBy(n=>n.DISTRICT_N).ToList();
            return district;
        }

        public string GetProvinceNamebyProvinceID(int id)
        {
            string result = UOW.Province_codes_Repo.Get(filter: n => n.CODE == id).Select(n=>n.PROVINCE).FirstOrDefault();
            return result;
        }
        public string GetDistrictNamebyDistrictID(int id)
        {
            string result = UOW.District_Codes_Repo.Get(filter: n => n.DISTRICT_C == id).Select(n => n.DISTRICT_N).FirstOrDefault();
            return result;
        }
        #endregion

        #region get sim_activation_log
        public List<tbl_kyc_simactivation_log> get_Sim_Activation_Log_Details()
        {
            List<tbl_kyc_simactivation_log> obj_Sim_Activation = new List<tbl_kyc_simactivation_log>();
            obj_Sim_Activation = UOW.kyc_Sim_Activition_log_Repo.Get().ToList();
            return obj_Sim_Activation;
        }
        #endregion

        #region Create KYC Version 
        public int Create_KYC_Version(tbl_kyc_version_update obj_kyc_version)
        {
            int int_result=1;
            tbl_kyc_version_update obj_kyc_version_insert = new tbl_kyc_version_update();
            obj_kyc_version_insert=UOW.kyc_version_update_Repo.Get(filter:k=>k.kyc_version==obj_kyc_version.kyc_version).FirstOrDefault();

            if (obj_kyc_version_insert == null)
            {
                obj_kyc_version_insert = new tbl_kyc_version_update();
                obj_kyc_version_insert.apk_url = obj_kyc_version.apk_url;
                obj_kyc_version_insert.created_on = DateTime.Now;
                obj_kyc_version_insert.description = obj_kyc_version.description;
                obj_kyc_version_insert.is_active = obj_kyc_version.is_active;
                obj_kyc_version_insert.kyc_version = obj_kyc_version.kyc_version;
                obj_kyc_version_insert.is_deleted = false;
                UOW.kyc_version_update_Repo.Insert(obj_kyc_version_insert);
                UOW.Save();

                if (obj_kyc_version_insert.Id > 0)
                    int_result = 0;
            }
            else
                int_result = 2;

            return int_result;
        }
        #endregion

        #region Update KYC Version
        public int Update_KYC_Version(tbl_kyc_version_update obj_kyc_version)
        {
            int int_result = 1;
            tbl_kyc_version_update obj_kyc_version_insert = new tbl_kyc_version_update();

            obj_kyc_version_insert = UOW.kyc_version_update_Repo.Get(filter: k => k.Id == obj_kyc_version.Id).FirstOrDefault();

            obj_kyc_version_insert.apk_url = obj_kyc_version.apk_url;            
            obj_kyc_version_insert.description = obj_kyc_version.description;
            obj_kyc_version_insert.is_active = obj_kyc_version.is_active;
            

            UOW.kyc_version_update_Repo.Update(obj_kyc_version_insert);
            UOW.Save();

            if (obj_kyc_version_insert.Id > 0)
                int_result = 0;

            return int_result;
        }
        #endregion

        #region Get All KYC Version Details
        public List<tbl_kyc_version_update> Get_All_KYC_version_Details()
        {

            List<tbl_kyc_version_update> obj_kyc_version_list = new List<tbl_kyc_version_update>();

            obj_kyc_version_list = UOW.kyc_version_update_Repo.Get(filter:k=>k.is_deleted==false).OrderBy(k => k.kyc_version).ToList();

            return obj_kyc_version_list;
        }
        #endregion

        #region Get  KYC Version Details By ID
        public tbl_kyc_version_update Get_KYC_version_Details_By_ID(long ID)
        {

            tbl_kyc_version_update obj_kyc_version = new tbl_kyc_version_update();

            obj_kyc_version = UOW.kyc_version_update_Repo.Get(filter:k=>k.Id==ID).FirstOrDefault();

            return obj_kyc_version;
        }
        #endregion

        #region check kyc version already exist
        public bool Check_KYC_Version(decimal version_No,long ID)
        {
            bool bool_res = false;

            tbl_kyc_version_update obj_version_exist = new tbl_kyc_version_update();

            if (version_No == 0 && version_No == null)
                obj_version_exist = UOW.kyc_version_update_Repo.Get(filter: (m => m.kyc_version == version_No && m.is_deleted == false)).FirstOrDefault();
            else
                obj_version_exist = UOW.kyc_version_update_Repo.Get(filter: (m => m.kyc_version == version_No && m.is_deleted == false && m.Id!=ID )).FirstOrDefault();

            if (obj_version_exist == null)
                bool_res = true;

            return bool_res;
        }
        #endregion

        #region delete kyc version
        public int delete_kyc_version(string id)
        {
            int int_result = 1;
            if (!string.IsNullOrEmpty(id))
            {
                long int_id = Convert.ToInt64(id);

                tbl_kyc_version_update obj_kyc_version_delete = new tbl_kyc_version_update();
                obj_kyc_version_delete = UOW.kyc_version_update_Repo.Get(filter: n => n.Id == int_id).FirstOrDefault() ;
                if (obj_kyc_version_delete != null)
                {
                    obj_kyc_version_delete.is_deleted = true;
                    UOW.kyc_version_update_Repo.Update(obj_kyc_version_delete);
                    UOW.Save();
                    int_result = 0;

                }

            }
            return int_result;
        }
        #endregion

        #region get kyc customer photo by id

        public tbl_kyc_customer Get_KYC_Customer_Details_By_ID(long id)
        {
            tbl_kyc_customer obj_kyc_customer = new tbl_kyc_customer();
            obj_kyc_customer = UOW.kyc_Customer_Repo.Get(filter: k => k.cust_id == id).FirstOrDefault();
            return obj_kyc_customer;        
        }

        #endregion

        #region kyc sim log for customer details list
        private List<tbl_kyc_simactivation_log> Get_Sim_Log_for_customer_detail_list(string mobile1,string mobile2,string mobile3)
        {
            List<tbl_kyc_simactivation_log> obj_kyc_sim_log = new List<tbl_kyc_simactivation_log>();
            List<tbl_kyc_simactivation_log> obj_kyc_sim_log_return = new List<tbl_kyc_simactivation_log>();
            if (!string.IsNullOrEmpty(mobile1))
            {
                obj_kyc_sim_log = UOW.kyc_Sim_Activition_log_Repo.Get(filter: k => k.msisdn_no == mobile1).ToList();
                if (obj_kyc_sim_log != null && obj_kyc_sim_log.Count > 0)
                {
                    obj_kyc_sim_log_return.AddRange(obj_kyc_sim_log);

                }
            }
            if (!string.IsNullOrEmpty(mobile2))
            {
                obj_kyc_sim_log = UOW.kyc_Sim_Activition_log_Repo.Get(filter: k => k.msisdn_no == mobile2).ToList();
                if (obj_kyc_sim_log != null && obj_kyc_sim_log.Count > 0)
                {
                    obj_kyc_sim_log_return.AddRange(obj_kyc_sim_log);

                }
            }
            if (!string.IsNullOrEmpty(mobile3))
            {
                obj_kyc_sim_log = UOW.kyc_Sim_Activition_log_Repo.Get(filter: k => k.msisdn_no == mobile3).ToList();
                if (obj_kyc_sim_log != null && obj_kyc_sim_log.Count > 0)
                {
                    obj_kyc_sim_log_return.AddRange(obj_kyc_sim_log);

                }
            }
            return obj_kyc_sim_log_return;
        }
        #endregion






        

        #region Save KYC Details
        public string SaveKYCDetails(string Details, out long cust_id)
        {
            cust_id = 0;
            Custome_Detail_Model cust_det_obj = new Custome_Detail_Model();
            number_exist_model obj_number_check = new number_exist_model();
            //resultModel res = new resultModel();
            string result = "";
           
                //-222 login check
                if (!string.IsNullOrEmpty(Details))
                {
                    cust_det_obj = JsonConvert.DeserializeObject<Custome_Detail_Model>(Details);
                    if (cust_det_obj != null)
                    {
                        if (cust_det_obj.cust_Details != null && cust_det_obj.cust_purchase != null)
                        {

                            result = chek_mobile_number_exist(cust_det_obj.cust_purchase.cust_mobile_no1, cust_det_obj.cust_purchase.cust_mobile_no2, cust_det_obj.cust_purchase.cust_mobile_no3);
                            if (!string.IsNullOrEmpty(result))
                                return result;

                            cust_det_obj.cust_Details.created_on = DateTime.Now;
                            cust_det_obj.cust_Details.is_active = true;
                            cust_det_obj.cust_Details.is_deleted = false;

                            if (!string.IsNullOrEmpty(cust_det_obj.photo))
                            {
                                cust_det_obj.cust_Details.photo_image = Convert.FromBase64String(cust_det_obj.photo);
                            }

                            UOW.kyc_Customer_Repo.Insert(cust_det_obj.cust_Details);
                            UOW.Save();

                            if ((cust_det_obj.cust_Details.cust_id > 0) && (cust_det_obj.cust_purchase != null))
                            {
                                cust_det_obj.cust_purchase.cust_id = cust_det_obj.cust_Details.cust_id;
                                UOW.kyc_Cust_Purchase_Repo.Insert(cust_det_obj.cust_purchase);
                                UOW.Save();
                                if (cust_det_obj.cust_purchase.cust_pur_id > 0)
                                {

                                    if ((cust_det_obj.cust_Details.cust_id > 0) && (cust_det_obj.cust_identification != null))
                                    {
                                        cust_det_obj.cust_identification.cust_id = cust_det_obj.cust_Details.cust_id;
                                        if (!string.IsNullOrEmpty(cust_det_obj.cust_identification.other_id_name))
                                        {
                                            cust_det_obj.cust_identification.is_other_id = true;

                                        }
                                        UOW.kyc_Cust_identification_Repo.Insert(cust_det_obj.cust_identification);
                                        UOW.Save();

                                        if (cust_det_obj.cust_ack != null)
                                        {
                                            if (cust_det_obj.cust_identification.cust_identity_id > 0)
                                            {


                                                cust_det_obj.cust_ack.cust_id = cust_det_obj.cust_Details.cust_id;
                                                UOW.kyc_Cust_acknowledge_Repo.Insert(cust_det_obj.cust_ack);
                                                UOW.Save();
                                                if (cust_det_obj.cust_ack.ack_id > 0)
                                                {
                                                    cust_id = cust_det_obj.cust_Details.cust_id;
                                                    obj_number_check.code = "0";
                                                    obj_number_check.msisdn_number = "";
                                                    result = JsonConvert.SerializeObject(obj_number_check);
                                                }

                                            }
                                        }

                                    }
                                }

                            }
                            else
                            {
                                obj_number_check.code = "1";
                                obj_number_check.msisdn_number = "";
                                result = JsonConvert.SerializeObject(obj_number_check);

                            }

                        }
                        else
                        {
                            obj_number_check.code = "-444";
                            obj_number_check.msisdn_number = "";
                            result = JsonConvert.SerializeObject(obj_number_check);
                        }
                    }
                    else
                    {
                        obj_number_check.code = "-333";
                        obj_number_check.msisdn_number = "";
                        result = JsonConvert.SerializeObject(obj_number_check);
                    }

                }
                else
                {
                    obj_number_check.code = "-555";
                    obj_number_check.msisdn_number = "";
                    result = JsonConvert.SerializeObject(obj_number_check);

                }

          
            return result;
        }
        #endregion

        #region update KYC details
        public string UpdateKyc_CustomerDetails(string Details, out long cust_id)
        {
            long custid = 0; string result = ""; cust_id = 0;
            Custome_Detail_Model cust_det_obj = new Custome_Detail_Model();
            number_exist_model obj_number_check = new number_exist_model();
           
                if (!string.IsNullOrEmpty(Details))
                {
                    cust_det_obj = JsonConvert.DeserializeObject<Custome_Detail_Model>(Details);
                    if (cust_det_obj != null)
                    {
                        if (cust_det_obj.cust_Details != null && cust_det_obj.cust_purchase != null)
                        {
                            
                                tbl_kyc_customer_purchase kyc_purchase = new tbl_kyc_customer_purchase();
                                kyc_purchase = UOW.kyc_Cust_Purchase_Repo.Get(filter: g => g.cust_mobile_no1 == cust_det_obj.cust_purchase.cust_mobile_no1).FirstOrDefault();
                                kyc_purchase.cust_handset_model1 = cust_det_obj.cust_purchase.cust_handset_model1;
                                kyc_purchase.cust_handset_model2 = cust_det_obj.cust_purchase.cust_handset_model2;
                                kyc_purchase.cust_handset_model3 = cust_det_obj.cust_purchase.cust_handset_model3;
                                kyc_purchase.cust_imei_no1 = cust_det_obj.cust_purchase.cust_imei_no1;
                                kyc_purchase.cust_imei_no2 = cust_det_obj.cust_purchase.cust_imei_no2;
                                kyc_purchase.cust_imei_no3 = cust_det_obj.cust_purchase.cust_imei_no3;
                                kyc_purchase.cust_status = cust_det_obj.cust_purchase.cust_status;
                                kyc_purchase.cust_mobile_no2 = cust_det_obj.cust_purchase.cust_mobile_no2;
                                kyc_purchase.cust_mobile_no3 = cust_det_obj.cust_purchase.cust_mobile_no3;

                                custid = kyc_purchase.cust_id;


                                if (custid > 0)
                                {
                                    UOW.kyc_Cust_Purchase_Repo.Update(kyc_purchase);
                                    UOW.Save();

                                    tbl_kyc_customer kyc_cust = new tbl_kyc_customer();
                                    kyc_cust = UOW.kyc_Customer_Repo.Get(filter: g => g.cust_id == custid).FirstOrDefault();
                                    kyc_cust.name = cust_det_obj.cust_Details.name;
                                    kyc_cust.surname = cust_det_obj.cust_Details.surname;
                                    kyc_cust.sex = cust_det_obj.cust_Details.sex;
                                    kyc_cust.is_employed = cust_det_obj.cust_Details.is_employed;
                                    kyc_cust.pobox = cust_det_obj.cust_Details.pobox;
                                    kyc_cust.town = cust_det_obj.cust_Details.town;
                                    kyc_cust.province = cust_det_obj.cust_Details.province;
                                    kyc_cust.suburb = cust_det_obj.cust_Details.suburb;
                                    kyc_cust.email = cust_det_obj.cust_Details.email;
                                    kyc_cust.signature = cust_det_obj.cust_Details.signature;
                                    kyc_cust.kycdate = cust_det_obj.cust_Details.kycdate;
                                    kyc_cust.modified_on = DateTime.Now;
                                    kyc_cust.dob = cust_det_obj.cust_Details.dob;
                                    if (!string.IsNullOrEmpty(cust_det_obj.photo))
                                    {
                                        kyc_cust.photo_image = Convert.FromBase64String(cust_det_obj.photo);
                                    }
                                    UOW.kyc_Customer_Repo.Update(kyc_cust);
                                    UOW.Save();
                               

                                    tbl_kyc_customer_acknowlegement kyc_ack = new tbl_kyc_customer_acknowlegement();
                                    kyc_ack = UOW.kyc_Cust_acknowledge_Repo.Get(filter: g => g.cust_id == custid).FirstOrDefault();
                                    kyc_ack.ack_date = cust_det_obj.cust_ack.ack_date;
                                    kyc_ack.is_doc_genuine = cust_det_obj.cust_ack.is_doc_genuine;
                                    kyc_ack.is_name_matched = cust_det_obj.cust_ack.is_name_matched;
                                    kyc_ack.is_officer_sign = cust_det_obj.cust_ack.is_officer_sign;

                                    UOW.kyc_Cust_acknowledge_Repo.Update(kyc_ack);
                                    UOW.Save();

                                    tbl_kyc_customer_identification kyc_idnt = new tbl_kyc_customer_identification();
                                    kyc_idnt = UOW.kyc_Cust_identification_Repo.Get(filter: g => g.cust_id == custid).FirstOrDefault();
                                    kyc_idnt.drv_lic = cust_det_obj.cust_identification.drv_lic;
                                    kyc_idnt.emp_id = cust_det_obj.cust_identification.emp_id;
                                    kyc_idnt.is_drv_lic = cust_det_obj.cust_identification.is_drv_lic;
                                    kyc_idnt.is_emp_id = cust_det_obj.cust_identification.is_emp_id;
                                    kyc_idnt.is_nasfund_id = cust_det_obj.cust_identification.is_nasfund_id;
                                    kyc_idnt.is_nbw_super_id = cust_det_obj.cust_identification.is_nbw_super_id;

                                    kyc_idnt.is_student_id = cust_det_obj.cust_identification.is_student_id;
                                    kyc_idnt.nasfund_id = cust_det_obj.cust_identification.nasfund_id;
                                    kyc_idnt.nbw_super_id = cust_det_obj.cust_identification.nbw_super_id;

                                    if (cust_det_obj.cust_identification.other_id != null || cust_det_obj.cust_identification.other_id_name != null)
                                    {
                                        kyc_idnt.is_other_id = true;
                                        kyc_idnt.other_id = cust_det_obj.cust_identification.other_id;
                                        kyc_idnt.other_id_name = cust_det_obj.cust_identification.other_id_name;
                                    }
                                    else
                                    {
                                        kyc_idnt.is_other_id = false;
                                    }

                                    kyc_idnt.student_id = cust_det_obj.cust_identification.student_id;

                                    UOW.kyc_Cust_identification_Repo.Update(kyc_idnt);
                                    UOW.Save();
                                    cust_id = custid;
                                    obj_number_check.code = "0";
                                    obj_number_check.msisdn_number = "Success";
                                    result = JsonConvert.SerializeObject(obj_number_check);
                                

                               
                            }
                        }

                        else
                        {
                            obj_number_check.code = "-444";
                            obj_number_check.msisdn_number = "";
                            result = JsonConvert.SerializeObject(obj_number_check);
                        }


                    }
                    else
                    {
                        obj_number_check.code = "-333";
                        obj_number_check.msisdn_number = "";
                        result = JsonConvert.SerializeObject(obj_number_check);
                    }
                }
                else
                {
                    obj_number_check.code = "-555";
                    obj_number_check.msisdn_number = "";
                    result = JsonConvert.SerializeObject(obj_number_check);

                }
           
            return result;

        }
        #endregion

        #region Get Province
        public string GetProvinceWithCode()
        {
            string result = "";
            List<Province_Model> province_obj = new List<Province_Model>();
            province_obj = (from g in _db.province_codes
                            select new Province_Model
                            {
                                Province_name = g.PROVINCE,
                                Province_code = g.CODE
                            }).ToList();
            result = JsonConvert.SerializeObject(province_obj);
            return result;
        }
        #endregion

        #region Get District
        public string sGetDistrictByProvinceID(int id)
        {
            string result = "";
            List<District_Model> District_obj = new List<District_Model>();
            District_obj = (from g in _db.district_codes
                            where g.PROVINCE_C == id
                            select new District_Model
                            {
                                District_name = g.DISTRICT_N,
                                District_code = g.DISTRICT_C
                            }).ToList();
            result = JsonConvert.SerializeObject(District_obj);
            return result;

        }


        #endregion

        #region simregisteration Log
        public void KYC_simRegisterationLog(string msisdn, string puk_code, string sim_no, string activatedby, string refno, bool is_sim_activated)
        {
           
                if (is_sim_activated == false)
                {
                    tbl_kyc_simactivation_log obj_sim_ack = new tbl_kyc_simactivation_log();
                    obj_sim_ack.created_on = DateTime.Now;
                    obj_sim_ack.msisdn_no = msisdn;
                    obj_sim_ack.puk_code = puk_code;
                    obj_sim_ack.sim_no = sim_no;
                    obj_sim_ack.activatedby = activatedby;
                    obj_sim_ack.ref_no = refno;
                    UOW.kyc_Sim_Activition_log_Repo.Insert(obj_sim_ack);
                    UOW.Save();
                }
                if (is_sim_activated == true)
                {
                    if (!string.IsNullOrEmpty(msisdn) && !string.IsNullOrEmpty(refno))
                    {
                        tbl_kyc_simactivation_log obj_sim_ack = new tbl_kyc_simactivation_log();
                        obj_sim_ack = UOW.kyc_Sim_Activition_log_Repo.Get(filter: g => g.ref_no == refno && g.msisdn_no == msisdn).FirstOrDefault();
                        obj_sim_ack.activated_on = DateTime.Now;
                        obj_sim_ack.is_sim_active = is_sim_activated;
                        UOW.kyc_Sim_Activition_log_Repo.Update(obj_sim_ack);
                        UOW.Save();
                    }
                }
            

        }
        #endregion

        #region KYC Version Update
        public string Get_KYC_Version_Update(decimal kyc_version)
        {
            string apk_url = "1";
            tbl_kyc_version_update obj_kyc_version_update = new tbl_kyc_version_update();
            obj_kyc_version_update = UOW.kyc_version_update_Repo.Get(filter: v => v.is_active == true && v.is_deleted == false && v.kyc_version > kyc_version).OrderByDescending(q => q.kyc_version).FirstOrDefault();
            if (obj_kyc_version_update != null)
                apk_url = obj_kyc_version_update.apk_url;

            return apk_url;
        }
        #endregion

        #region Get Mobile Model
        public string Get_Mobile_Model()
        {
            string result = "";
            List<tbl_kyc_taccode> obj_Tac_Code = new List<tbl_kyc_taccode>();
            obj_Tac_Code = UOW.tbl_tac_code_Repo.Get().ToList();
            result = JsonConvert.SerializeObject(obj_Tac_Code);
            return result;
        }
        #endregion

        #region check mobile number exist
        public string chek_mobile_number_exist(string mobile1, string mobile2, string mobile3)
        {
            string str_mobile_number = "";

            number_exist_model obj_number_exist_model = new number_exist_model();
            tbl_kyc_customer_purchase obj_kyc_cust_purchase = new tbl_kyc_customer_purchase();

            obj_kyc_cust_purchase = UOW.kyc_Cust_Purchase_Repo.Get(filter: K => K.cust_mobile_no1 == mobile1 || K.cust_mobile_no2 == mobile1 || K.cust_mobile_no3 == mobile1).FirstOrDefault();
            if (obj_kyc_cust_purchase != null)
            {
                obj_number_exist_model.msisdn_number = mobile1;
                obj_number_exist_model.code = "-102";
                str_mobile_number = JsonConvert.SerializeObject(obj_number_exist_model);
                return str_mobile_number;
            }

            obj_kyc_cust_purchase = UOW.kyc_Cust_Purchase_Repo.Get(filter: K => K.cust_mobile_no1 == mobile2 || K.cust_mobile_no2 == mobile2 || K.cust_mobile_no3 == mobile2).FirstOrDefault();
            if (obj_kyc_cust_purchase != null)
            {
                obj_number_exist_model.msisdn_number = mobile2;
                obj_number_exist_model.code = "-102";
                str_mobile_number = JsonConvert.SerializeObject(obj_number_exist_model);
                return str_mobile_number;
            }

            obj_kyc_cust_purchase = UOW.kyc_Cust_Purchase_Repo.Get(filter: K => K.cust_mobile_no1 == mobile3 || K.cust_mobile_no2 == mobile3 || K.cust_mobile_no3 == mobile3).FirstOrDefault();
            if (obj_kyc_cust_purchase != null)
            {
                obj_number_exist_model.msisdn_number = mobile3;
                obj_number_exist_model.code = "-102";
                str_mobile_number = JsonConvert.SerializeObject(obj_number_exist_model);
                return str_mobile_number;
            }

            return str_mobile_number;

        }
        #endregion

        #region JSON String for LT and address
        public string Get_JSON_String_for_LT(string details)
        {

            string sreturn_Json = "";
            if (!string.IsNullOrEmpty(details))
            {
                Custome_Detail_Model cust_det_obj = new Custome_Detail_Model();
                LTModel obj_LT = new LTModel();

                cust_det_obj = JsonConvert.DeserializeObject<Custome_Detail_Model>(details);
                if (cust_det_obj != null)
                {
                    if ((cust_det_obj.cust_Details != null) && (cust_det_obj.cust_purchase != null))
                    {
                        string district_code, state_code;
                        district_code = UOW.Province_codes_Repo.Get(filter: k => k.id == cust_det_obj.cust_Details.province).Select(k => k.DISTRICT_CODE).FirstOrDefault().ToString();
                        state_code = UOW.Province_codes_Repo.Get(filter: k => k.id == cust_det_obj.cust_Details.province).Select(k => k.STATE_CODE).FirstOrDefault().ToString();
                        DateTime dob = Convert.ToDateTime(cust_det_obj.cust_Details.dob);
                        obj_LT.dob = dob.ToString("dd/MM/yyyy");
                        obj_LT.email = cust_det_obj.cust_Details.email;
                        obj_LT.firstName = cust_det_obj.cust_Details.name;

                        if (cust_det_obj.cust_Details.sex == "Male")
                        {
                            obj_LT.gender = "M";
                            obj_LT.title = mtitle;
                        }
                        else
                        {
                            obj_LT.gender = "F";
                            obj_LT.title = ftitle;
                        }
                        obj_LT.idNumber = idNumber;
                        obj_LT.idType = idType;
                        obj_LT.lastName = cust_det_obj.cust_Details.surname;
                        obj_LT.msisdn = cust_det_obj.cust_purchase.cust_mobile_no1;
                        obj_LT.address1 = cust_det_obj.cust_Details.pobox;
                        obj_LT.address2 = cust_det_obj.cust_Details.suburb;
                        obj_LT.countryCode = countryCode;

                        obj_LT.districtCode = district_code;
                        obj_LT.postBoxNo = cust_det_obj.cust_Details.pobox;
                        obj_LT.stateCode = state_code;

                        sreturn_Json = JsonConvert.SerializeObject(obj_LT);
                    }

                }

            }


            //iLTreturnerrorcode = 0;
            return sreturn_Json;
        }


        #endregion

        #region JSON String for LT and address
        public string Get_JSON_String_for_LT_service4(string details)
        {

            string sreturn_Json = "";
            if (!string.IsNullOrEmpty(details))
            {
                Custome_Detail_Model cust_det_obj = new Custome_Detail_Model();
                LTService4Model obj_LT = new LTService4Model();

                cust_det_obj = JsonConvert.DeserializeObject<Custome_Detail_Model>(details);
                if (cust_det_obj != null)
                {
                    if ((cust_det_obj.cust_Details != null) && (cust_det_obj.cust_purchase != null))
                    {
                        string district_code, state_code;
                        district_code = UOW.Province_codes_Repo.Get(filter: k => k.id == cust_det_obj.cust_Details.province).Select(k => k.DISTRICT_CODE).FirstOrDefault().ToString();
                        state_code = UOW.Province_codes_Repo.Get(filter: k => k.id == cust_det_obj.cust_Details.province).Select(k => k.STATE_CODE).FirstOrDefault().ToString();
                        DateTime dob = Convert.ToDateTime(cust_det_obj.cust_Details.dob);
                        obj_LT.dob = dob.ToString("dd/MM/yyyy");
                        obj_LT.email = cust_det_obj.cust_Details.email;
                        obj_LT.firstName = cust_det_obj.cust_Details.name;

                        if (cust_det_obj.cust_Details.sex == "Male")
                        {
                            obj_LT.gender = "M";
                            obj_LT.title = mtitle;
                        }
                        else
                        {
                            obj_LT.gender = "F";
                            obj_LT.title = ftitle;
                        }
                        obj_LT.idNumber = idNumber;
                        obj_LT.idType = idType;
                        obj_LT.lastName = cust_det_obj.cust_Details.surname;
                        obj_LT.msisdn = cust_det_obj.cust_purchase.cust_mobile_no1;

                        sreturn_Json = JsonConvert.SerializeObject(obj_LT);
                    }

                }

            }


            //iLTreturnerrorcode = 0;
            return sreturn_Json;
        }


        #endregion

        #region get null lt reference
        public List<LTModel> Get_null_LT_reference()
        {


            List<LTModel> obj_lt = new List<LTModel>();
            obj_lt = (from c in _db.kyc_customer_Details.AsEnumerable()
                      join p in _db.kyc_customer_Purchase on c.cust_id equals p.cust_id
                      where c.lt_ref == null
                      select new LTModel
                      {
                          dob = Convert.ToDateTime(c.dob).ToShortDateString(),
                          email = c.email,
                          firstName = c.name,
                          gender = (c.sex == "Male") ? "M" : "F",
                          idNumber = "DUMMY",
                          idType = "DL",
                          lastName = c.surname,
                          msisdn = p.cust_mobile_no1,
                          title = (c.sex == "Male") ? "MR" : "MS"

                      }).ToList();
            //LTModel obj_l=new LTModel();
            //obj_l.dob = "12/04/2015";
            //obj_l.email = "asd@asd.com";
            //obj_l.firstName = "kumar";
            //obj_l.gender = "M";
            //obj_l.idNumber = "DUMMY";
            //obj_l.idType = "DL";
            //obj_l.lastName = "twinkle";
            //obj_l.msisdn = "76342468";
            //obj_l.title = "MR";
            //obj_lt.Add(obj_l);

            //obj_l = new LTModel();
            //obj_l.dob = "12/04/2015";
            //obj_l.email = "asd@asd.com";
            //obj_l.firstName = "kumar";
            //obj_l.gender = "M";
            //obj_l.idNumber = "DUMMY";
            //obj_l.idType = "DL";
            //obj_l.lastName = "twinkle";
            //obj_l.msisdn = "76404912";
            //obj_l.title = "MR";

            //obj_lt.Add(obj_l);

            return obj_lt;

        }
        #endregion

        //#region update lt reference with msisdn
        //public int Update_LT_Reference_for_bulk(string msisdn, string lt_ref)
        //{
        //    int ireturn = 1;
        //    long cust_id = (from c in _db.customer_Details
        //                    join p in _db.customer_Purchase on c.cust_id equals p.cust_id
        //                    where p.cust_mobile_no1 == msisdn && c.lt_ref == null
        //                    select p.cust_id).FirstOrDefault();
        //    if (cust_id > 0)
        //    {
        //        tbl_kyc_customer obj_cust = _UOW.kyc_Customer_Repo.Get(filter: k => k.cust_id == cust_id).FirstOrDefault();

        //        obj_cust.lt_ref = lt_ref;
        //        obj_cust.modified_on = DateTime.Now;
        //        _UOW.kyc_Customer_Repo.Update(obj_cust);
        //        _UOW.Save();
        //        ireturn = 0;//return custid
        //    }
        //    return ireturn;
        //}
        //#endregion

        #region update lt reference by cust id
        public long Update_LT_Reference(long cust_id, string lt_ref)
        {
            long ireturn = 0;
            if (cust_id > 0)
            {
                tbl_kyc_customer obj_cust = UOW.kyc_Customer_Repo.Get(filter: k => k.cust_id == cust_id).FirstOrDefault();

                obj_cust.lt_ref = lt_ref;
                obj_cust.modified_on = DateTime.Now;
                UOW.kyc_Customer_Repo.Update(obj_cust);
                UOW.Save();
                ireturn = obj_cust.cust_id;
            }
            return ireturn;
        }
        #endregion






        #region LT model
        public class LTModel
        {
            public string title { get; set; }

            public string firstName { get; set; }

            public string lastName { get; set; }

            public string gender { get; set; }

            public string dob { get; set; }

            public string idType { get; set; }

            public string idNumber { get; set; }

            public string msisdn { get; set; }

            public string email { get; set; }

            public string postBoxNo { get; set; }

            public string countryCode { get; set; }

            public string stateCode { get; set; }

            public string districtCode { get; set; }

            public string address1 { get; set; }

            public string address2 { get; set; }

        }


        #endregion

        #region service4 LT model
        public class LTService4Model
        {
            public string title { get; set; }

            public string firstName { get; set; }

            public string lastName { get; set; }

            public string gender { get; set; }

            public string dob { get; set; }

            public string idType { get; set; }

            public string idNumber { get; set; }

            public string msisdn { get; set; }

            public string email { get; set; }


        }
        #endregion

        #region LT return model
        public class LTReturnModel
        {
            public string resultCode { get; set; }

            public string reference { get; set; }

        }
        #endregion

        #region special Promotion
        public List<tbl_spc_promo_cust> getSpecialPromotion()
        {
            List<tbl_spc_promo_cust> obj_spc = new List<tbl_spc_promo_cust>();
            obj_spc = _db.spc_promo_cust.AsQueryable().Where(m => m.is_deleted == false).OrderByDescending(o => o.created_on).ToList();
            return obj_spc;
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
                    UOW.Dispose();
                    _db.Dispose();
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