using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using si_bmobile.Models;
using bmdoku.bmkuRef;
using Newtonsoft.Json;
using si_bmobile.Utils;
using System.Net;

namespace si_bmobile.DAL
{
    class StaffsTopup_Repo : IStaffsTopup_Repo, IDisposable
    {
        #region Repo

        private IUtilityRepository _util_repo;
        private UnitOfWork _uow;
        private EasyRecharge _easyRecharge;

        private string dk_encry_pwd;
        private string dk_plain_pwd;
        private string dk_merchantid;
        private string dk_username;
        private string dk_keycode;
        public StaffsTopup_Repo()
        {
            this._uow = new UnitOfWork();
            this._util_repo = new UtilityRepository();
            this._easyRecharge = new EasyRecharge();
            this.dk_merchantid = ConfigurationManager.AppSettings["sf_merchantid"].ToString();
            this.dk_username = ConfigurationManager.AppSettings["sf_username"].ToString();
            this.dk_plain_pwd = ConfigurationManager.AppSettings["sf_plain_pwd"].ToString();
            this.dk_encry_pwd = ConfigurationManager.AppSettings["sf_encry_pwd"].ToString();
            this.dk_keycode = ConfigurationManager.AppSettings["sf_keycode"].ToString();
        }

        #endregion

        #region SendScheduledEmail

        public bool SendStaffsTopupEmail()
        {
            bool bRes = false;
            try
            {

                var objStaffs = _uow.staffs_topup_repo.Get(filter: S => S.is_recharged == false && S.reason == null && S.is_active == true && S.is_deleted == false).ToList();

                if (objStaffs.Count > 0)
                {
                    //_util_repo.ErrorLog_Txt("Begin", "Data Loaded");
                    string EmailTempFilePath = ConfigurationSettings.AppSettings["EmailTempPath"];
                    XElement doc = XElement.Load(@EmailTempFilePath + "email_template.xml");
                    XElement emailsubj = doc.Element("ReminderEmail_Subj");
                    XElement emailBody = doc.Element("ReminderEmail_Body");




                    foreach (var item in objStaffs)
                    {
                        bm_staffs_trans obj_sf_trans = new bm_staffs_trans();
                        bool res = false;

                        obj_sf_trans.trans_date = DateTime.Now;
                        obj_sf_trans.ip_address = Dns.GetHostByName(Dns.GetHostName()).AddressList[0].ToString();
                        obj_sf_trans.amount = item.amount;
                        obj_sf_trans.email = item.email;
                        obj_sf_trans.invoice_number = item.invoice;
                        obj_sf_trans.msisdn_number = item.msisdn_number;

                        if (item.amount <= 500)
                        {
                            string ValidateMerch = validate_merchant(item.msisdn_number, item.amount.ToString());
                            if (ValidateMerch != "")
                            {
                                string Recharge = recharge_msisdn(item.msisdn_number, item.amount.ToString(), item.invoice, ValidateMerch);
                                var opRes = JsonConvert.DeserializeObject<recharge_OP_model>(Recharge);
                                if (opRes.resultCode == 0)
                                {
                                    item.is_recharged = true;
                                    item.recharged_on = DateTime.Now;
                                    _uow.staffs_topup_repo.Update(item);
                                    _uow.Save();

                                    obj_sf_trans.is_recharged = true;
                                    obj_sf_trans.trans_desc = "Topup Success";
                                    _uow.staff_trans_repo.Insert(obj_sf_trans);
                                    _uow.Save();

                                    res = true;

                                    if (!string.IsNullOrEmpty(item.email))
                                    {
                                        string sBody = emailBody.Value.Replace("#StaffName#", item.first_name + " " + item.last_name).Replace("#Amount#", item.amount.ToString());
                                        sBody = sBody.Replace("#MSISDN#", item.msisdn_number).Replace("#Date#", DateTime.Now.ToString("dd/MM/yyyy"));

                                        _util_repo.SendEmailMessage(item.email, emailsubj.Value.Trim(), sBody);
                                    }
                                }
                                else
                                {
                                    item.reason = "Result Code: " + opRes.resultCode.ToString();
                                    res = false;

                                    obj_sf_trans.trans_desc = "Failed and Result Code: " + opRes.resultCode.ToString();
                                }
                            }
                            else
                            {
                                item.reason = "Validate Merchant Failed";
                                res = false;

                                obj_sf_trans.trans_desc = "Validate Merchant Failed";
                            }
                        }
                        else
                        {
                            item.reason = "Amount limit exceeded";
                            item.is_deleted = true;
                            res = false;

                            obj_sf_trans.trans_desc = "Amount limit exceeded";
                        }

                        if (!res)
                        {
                            item.is_recharged = false;
                            item.recharged_on = DateTime.Now;
                            _uow.staffs_topup_repo.Update(item);
                            _uow.Save();

                            obj_sf_trans.is_recharged = false;
                            _uow.staff_trans_repo.Insert(obj_sf_trans);
                            _uow.Save();
                        }
                    }

                    bRes = true;
                }
            }
            catch (Exception ex)
            {
                _util_repo.ErrorLog_Txt(ex.Message, ex.StackTrace);
            }
            return bRes;
        }

        #endregion

        #region For Recharge APIs

        /// <summary>
        /// Validate Merchant using the config details
        /// </summary>
        /// <param name="msisdn"></param>
        /// <param name="amount"></param>
        /// <returns>json format string</returns>
        public string validate_merchant(string msisdn, string amount)
        {
            string sRes = "";
            RechargeModel obj_regcharge = new RechargeModel();
            obj_regcharge.keycode = dk_keycode;
            obj_regcharge.username = dk_username;
            obj_regcharge.password = dk_plain_pwd;
            obj_regcharge.msisdn = msisdn;
            obj_regcharge.amount = amount;

            var data = JsonConvert.SerializeObject(obj_regcharge, Formatting.Indented);

            var resultmerchant = _util_repo.AES_JDEC(_easyRecharge.ValidateMerchant(dk_merchantid, _util_repo.AES_JENC(data)));

            if (!string.IsNullOrEmpty(resultmerchant))
                sRes = resultmerchant;

            return sRes;
        }

        /// <summary>
        /// Recharge MSISDN w.r.t amount
        /// </summary>
        /// <param name="msisdn"></param>
        /// <param name="amount"></param>
        /// <param name="data"></param>
        /// <returns>json format string</returns>
        public string recharge_msisdn(string msisdn, string amount, string invoice, string data)
        {
            string sRes = "";

            if (data != null)
            {
                var opRes = JsonConvert.DeserializeObject<validate_output_model>(data);

                if (opRes.resultcode == 0)
                {
                    recharge_msisdn_model obj_recharge = new recharge_msisdn_model();
                    obj_recharge.amount = amount;
                    obj_recharge.msisdn = msisdn;
                    obj_recharge.reference = opRes.reference;
                    obj_recharge.invoice = invoice;
                    string sRec_res = JsonConvert.SerializeObject(obj_recharge, Formatting.Indented);
                    string encry_data = _util_repo.AES_JENC(sRec_res);

                    var resultmerchant = _util_repo.AES_JDEC(_easyRecharge.RechargeMsisdn(dk_merchantid, encry_data));

                    if (!string.IsNullOrEmpty(resultmerchant))
                        sRes = resultmerchant;
                }
                else
                {
                    sRes = data;
                }
            }

            return sRes;
        }

        #endregion


        #region Staff Deductions

        public bool staff_deduction(bm_staffs_topup item)
        {
            bool bRes = false;
            try
            {
                bm_staffs_deduction_trans obj_sf_trans = new bm_staffs_deduction_trans();
                bool res = false;

                obj_sf_trans.trans_date = DateTime.Now;
                obj_sf_trans.amount = item.amount;
                //obj_sf_trans.email = item.email;
                //obj_sf_trans.invoice_number = item.invoice;
                obj_sf_trans.msisdn_number = item.msisdn_number;

                string deduction = deduction_msisdn(item.msisdn_number, item.amount.ToString());
                var opRes = JsonConvert.DeserializeObject<recharge_OP_model>(deduction);
                if (opRes.resultCode == 0)
                {
                    //item.is_recharged = true;
                    //item.recharged_on = DateTime.Now;
                    //_uow.staffs_topup_repo.Update(item);
                    //_uow.Save();

                    obj_sf_trans.is_deducted = true;
                    obj_sf_trans.trans_desc = "Success";
                    _uow.staffs_deduction_trans_repo.Insert(obj_sf_trans);
                    _uow.Save();
                    bRes = true;
                    res = true;
                }
                else
                {
                    item.reason = "Result Code: " + opRes.resultCode.ToString();
                    res = false;

                    obj_sf_trans.trans_desc = "Failed and Result Code: " + opRes.resultCode.ToString();
                    obj_sf_trans.is_deducted = false;
                    _uow.staffs_deduction_trans_repo.Insert(obj_sf_trans);
                    _uow.Save();

                }

                
            }
            catch (Exception ex)
            {
                _util_repo.ErrorLog_Txt(ex.Message, ex.StackTrace);
            }
            return bRes;
        }

        public string deduction_msisdn(string msisdn, string amount)
        {
            string sRes = "";

            if (msisdn != null)
            {
                RechargeModel obj_regcharge = new RechargeModel();
                obj_regcharge.keycode = dk_keycode;
                obj_regcharge.username = dk_username;
                obj_regcharge.password = dk_plain_pwd;
                obj_regcharge.msisdn = msisdn;
                obj_regcharge.amount = amount;

                var data = JsonConvert.SerializeObject(obj_regcharge, Formatting.Indented);

                var resultmerchant = _util_repo.AES_JDEC(_easyRecharge.oneOffDeduction(dk_merchantid, _util_repo.AES_JENC(data)));

                if (!string.IsNullOrEmpty(resultmerchant))
                    sRes = resultmerchant;

                return sRes;
            }

            return sRes;
        }

        #endregion

        #region Dispose Objects

        //private bool disposed = false;

        //protected virtual void Dispose(bool disposing)
        //{
        //    if (!this.disposed)
        //    {
        //        if (disposing)
        //        {
        //            //_context.Dispose();
        //        }
        //    }
        //    this.disposed = true;
        //}

        public void Dispose()
        {
            _uow.Dispose();
            _util_repo.Dispose();
            //Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
