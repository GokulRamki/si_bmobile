using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using si_bmobile.Models;
using System.Configuration;
using beLib.beRef;
using System.IO;
using System.Web.Security;
using System.Xml;
using System.Xml.Linq;
using si_bmobile.Utils;
using Newtonsoft.Json;
using si_bmobile.dokuRef;
using bmdoku.bmkuRef;

namespace si_bmobile.DAL
{
    public class careRepository : IcareRepository, IDisposable
    {
        SelfCare proxy;
        Utils.General oGen;
        string sEncKey;
        IUtilityRepository _util_repo;
        string sCode;
        private string _Hide;
        private string _Show;
        bemobileselfcareEntities _dbselfcare;
        private EasyRecharge _easyRecharge;
        private string kad_user_name = ConfigurationManager.AppSettings["kad_uname"];
        private string kad_password = ConfigurationManager.AppSettings["kad_pwd"];
        private string kad_merchant_id = ConfigurationManager.AppSettings["kad_merch_id"];
        private string kad_keycode = ConfigurationManager.AppSettings["kad_Keycode"];
        UnitOfWork UOW;
        public careRepository()
        {
            this._easyRecharge = new EasyRecharge();
            this.proxy = new SelfCare();
            this.oGen = new Utils.General();
            this.sEncKey = ConfigurationManager.AppSettings["encKey"].ToString();
            this._util_repo = new UtilityRepository();
            this.sCode = "2013";
            this._Hide = "display:none;";
            this._Show = "display:block;";
            this._dbselfcare = new bemobileselfcareEntities();
            this.UOW = new UnitOfWork();
        }



        public string RedeemFBPromotion_TopupVoucher(RedeemFBPromotionsModel RedeemFB)
        {
            string _iRes = string.Empty;

            _iRes = proxy.topupVoucher(AES_ENC(RedeemFB.msisdn_no), RedeemFB.pin_number, RedeemFB.msisdn_no);

            return _iRes;
        }

        #region TripleDES Enc/DEC
        private string TrENC(string _input)
        {
            TripleDESImplementation _encryTrip = new TripleDESImplementation();
            string sENC = _encryTrip.Encrypt(_input);
            return sENC;
        }

        private string TrDEC(string _input)
        {
            TripleDESImplementation _decryTrip = new TripleDESImplementation();
            string sDEC = _decryTrip.Decrypt(_input);
            return sDEC;
        }

        #endregion

        #region AES WithReplaceString Enc/Dec
        /// <summary>
        /// Encryption
        /// </summary>
        /// <param name="_input"></param>
        /// <returns>string</returns>
        public string AES_ENC(string _input)
        {
            Encryptor encryptor = new Encryptor(sEncKey);
            string sEnc = encryptor.Encrypt<AesEncryptor>(_input);
            sEnc = _util_repo.ReplaceStr(sEnc);
            return sEnc;
        }

        /// <summary>
        /// Decryption
        /// </summary>
        /// <param name="_input"></param>
        /// <returns>string</returns>
        public string AES_DEC(string _input)
        {
            Encryptor encryptor = new Encryptor(sEncKey);
            string sDec = encryptor.Decrypt<AesEncryptor>(_input);
            return sDec;
        }
        #endregion

        #region Register User

        /// <summary>
        /// Register User in DB
        /// </summary>
        /// <param name="oRM"></param>
        /// <param name="iRet"></param>
        /// <param name="bflag"></param>
        public void Register_User(RegistrationModel oRM, out int iRet, out bool bflag)
        {
            iRet = 0;
            bflag = false;
            string srandomPwd = "";

            Registration oReg = new Registration();
            Random r = new Random();
            oReg.userName = oRM.Email;
            srandomPwd = oGen.RandomString(r, 6);
            oReg.password = AES_ENC(srandomPwd);
            oReg.dPassword = srandomPwd;
            oReg.firstName = oRM.FirstName;
            oReg.middleName = oRM.MiddleName;
            oReg.lastName = oRM.LastName;
            oReg.company = oRM.Company;
            oReg.addr1 = oRM.Address1;
            oReg.addr2 = oRM.Address2;
            //  oReg.mobileNumber = "677" + oRM.ContactNo;
            oReg.parish = "";
            oReg.homeNumber = "";
            oReg.email = oRM.Email;
            oReg.msisdnNumber = "677" + oRM.BroadBandNo;
            //oReg.accountNumber = oRM.account_number;
            oReg.mother_maiden_name = oRM.mother_maiden_name;
            oReg.favourite_colour = oRM.favourite_color;
            oReg.pets_name = oRM.pets_name;
            proxy.RegisterUser(AES_ENC(oReg.email), oReg, out iRet, out bflag);
            
        }



        /// <summary>
        /// Check existence of msidn (Shop to Care)
        /// </summary>
        /// <param name="msisdn"></param>
        /// <param name="sEmail"></param>
        /// <param name="bRes"></param>
        public void GetEMailID_MSISDN(string msisdn, out string sEmail, out bool bRes)
        {
            sEmail = "";
            bRes = false;
            sEmail = proxy.isMsisdnExist(AES_ENC(msisdn), msisdn);
            if (sEmail == "1")
                bRes = false;
            else
                bRes = true;
        }


        public void VerifyCode(string Msisdn, string vcode, out bool bRet)
        {
            bool bflag = false;
            bRet = false;
            proxy.shoppingVerification(AES_ENC(Msisdn), Msisdn, vcode, out bflag, out bflag);
            bRet = bflag;

        }

        #endregion

        #region Update User

        /// <summary>
        /// Update User in DB
        /// </summary>
        /// <param name="oRM"></param>
        /// <param name="iRet"></param>
        /// <param name="bflag"></param>
        public void Update_User(RegistrationModel oRM, out int iRet, out bool bflag)
        {
            iRet = 0;
            bflag = false;
            Registration oReg = new Registration();
            oReg.userName = oRM.Email;
            //oReg.password = AES_DEC(oRM.Password);
            oReg.firstName = oRM.FirstName;
            oReg.middleName = oRM.MiddleName;
            oReg.lastName = oRM.LastName;
            oReg.company = oRM.Company;
            oReg.addr1 = oRM.Address1;
            oReg.addr2 = oRM.Address2;
            //  oReg.mobileNumber =  oRM.ContactNo;
            oReg.parish = "";
            oReg.homeNumber = "";
            oReg.email = oRM.Email;
            oReg.msisdnNumber = oRM.BroadBandNo;
            oReg.userId = oRM.UserId.ToString();
            oReg.accountNumber = oRM.account_number;
            oReg.mother_maiden_name = oRM.mother_maiden_name;
            oReg.favourite_colour = oRM.favourite_color;
            oReg.pets_name = oRM.pets_name;
            proxy.UpdateUser(AES_ENC(oRM.UserId.ToString()), Convert.ToInt32(oRM.UserId), true, oReg, out iRet, out bflag);
        }

        /// <summary>
        /// Get Plan purchase User History
        /// </summary>
        /// <returns></returns>
        //public List<User_History> GetUserHistory()
        //{
        //    List<User_History> oUM = new List<User_History>();
        //    string history = proxy.getUserHistory(AES_ENC(sCode));
        //    if (history != null)
        //    {
        //        for (int i = 0; i < history.Length; i++)
        //        {
        //            User_History h = new User_History();
        //            h.id = history[i].id;
        //            h.planId = history[i].planId;
        //            h.userId = history[i].userId.ToString();
        //            h.createdDate =history[i].createdDate;
        //            h.modifyDate = history[i].modifyDate;
        //            oUM.Add(h);
        //        }
        //    }
        //    return oUM;
        //}


        #endregion

        #region Authenticate
        /// <summary>
        /// authenticate for login
        /// </summary>
        /// <param name="oLM"></param>
        /// <param name="iRet"></param>
        /// <param name="bFlag"></param>
        public void Authenticate_User(LoginModel oLM, out int iRet, out bool bFlag)
        {
            iRet = 0;
            bFlag = false;
            //proxy.authenticateUser(AES_ENC(oLM.Email), oLM.Email,AES_ENC(oLM.Pwd), out iRet, out bFlag);
            proxy.authenicateUserbyMSDN("677" + oLM.MSISDN.Trim(), AES_ENC("677" + oLM.MSISDN.Trim()), AES_ENC(oLM.Pwd.Trim()), out iRet, out bFlag);
        }
        #endregion

        #region Lock Account
        public void LockAccount(long user_id)
        {
            int iRet = -1;
            bool bflag = false;
            proxy.disableUser(AES_ENC(user_id.ToString()), user_id, true, out iRet, out bflag);
        }
        #endregion

        #region Get Multiple Accounts
        public AccountModel GetMultipleSubscribers(string sMSISDN)
        {
            AccountModel oAM = new AccountModel();

            Profile oProfile = new Profile();
            oProfile = proxy.getMultiSubscriberDetailsByMsisdn(AES_ENC(sMSISDN), sMSISDN);
            if (oProfile != null)
            {
                Registration[] regis = oProfile.registration;
                RegistrationModel oRM = new RegistrationModel();
                if (regis != null)
                {
                    foreach (var ritem in regis)
                    {
                        oRM.Address1 = ritem.addr1;
                        oRM.Address2 = ritem.addr2;
                        oRM.BroadBandNo = ritem.msisdnNumber;
                        oRM.Company = ritem.company;
                        //oRM.ContactNo = ritem.mobileNumber;
                        oRM.dPassword = ritem.dPassword;
                        oRM.Email = ritem.email;
                        oRM.FirstName = ritem.firstName;
                        oRM.FormattedBroadBandNo = oRM.BroadBandNo.Substring(0, 3) + " " + oRM.BroadBandNo.Substring(3);
                        oRM.LastName = ritem.lastName;
                        oRM.MiddleName = ritem.middleName;
                        oRM.paidType = ritem.paidType;
                        oRM.Password = ritem.password;
                        oRM.UserId = Convert.ToInt64(ritem.id);
                        oRM.Username = ritem.userName;
                        oRM.account_number = ritem.accountNumber;
                        oRM.mother_maiden_name = ritem.mother_maiden_name;
                        oRM.favourite_color = ritem.favourite_colour;
                        oRM.pets_name = ritem.pets_name;
                    }
                    oAM.Reg = oRM;
                }
                SubscriberDetails[] subs = oProfile.all_subcriberdetails;
                List<SubscriberModel> oSMs = new List<SubscriberModel>();

                if (subs.Length > 0)
                {
                    foreach (var item in subs)
                    {
                        SubscriberModel oSM = new SubscriberModel();

                        oSM.balance = item.balance;
                        oSM._sbalance = ToKina(oSM.balance);
                        oSM.balanceResultCode = item.balanceResultCode;
                        oSM.purchasedAllowed = item.purchasedAllowed;
                        oSM.resultCode = item.resultCode;
                        oSM.topupAllowed = item.topupAllowed;
                        oSM.isExpired = item.isExpired;
                        oSM.isMinSize = item.isMinSize;
                        oSM.dataBalance = item.dataBalance;
                        oSM._sdataBalance = ToMB_GB(item.dataBalance.ToString());
                        oSM.active = item.active;
                        oSM.sCreditExpirydate = item.creditExpiry;
                        oSM.sDataExpirydate = item.dataExpiry;
                        oSM._MSISDNNumber = item.msisdn;
                        oSM.isPrimary = item.primary;
                        oSM.planAvl = item.planAvl;
                        oSM.yourPlanAvl = item.yourPlanAvl;
                        oSM.paidtype = item.paidType.ToString();
                        oSM.IDDBalance = ToMin(item.iDDBalance);
                        oSM.accountNo = item.accountNumber;

                        if (item.nightPLanDataBalance == 0)
                            oSM.NightPlanDataBalance = item.nightPLanDataBalance.ToString();
                        else
                            oSM.NightPlanDataBalance = ToMB_GB(item.nightPLanDataBalance.ToString());
                        if (item.balanceResultCode == 208)
                            oSM.dtopup = _Show;
                        else
                            oSM.dtopup = _Hide;
                        if (item.resultCode == 602)
                        {
                            oSM.dCC = _Hide;
                            oSM.dplanexp = _Hide;
                        }
                        else
                        {
                            oSM.dCC = _Hide;
                            oSM.dplanexp = _Hide;
                        }
                        if (item.topupAllowed == true)
                            oSM.menuTopup = _Show;
                        else
                            oSM.menuTopup = _Hide;

                        oSM.transactionNo = item.lastInvoiceNum;

                        if (!string.IsNullOrEmpty(item.lastInvoiceDate))
                            oSM.statementDate = Convert.ToDateTime(item.lastInvoiceDate);

                        if (!string.IsNullOrEmpty(item.lastInvoiceDueDate))
                            oSM.paymentDueDate = Convert.ToDateTime(item.lastInvoiceDueDate);

                        if (item.totalInvoiceAmt > 0)
                        {
                            oSM.stotalAmount = item.totalInvoiceAmt.ToString();
                            oSM.totalAmount = ToKina(Convert.ToDouble(item.totalInvoiceAmt));
                        }
                        if (item.newInvoiceAmt > 0)
                        {
                            oSM.PaymentDue = item.newInvoiceAmt.ToString();
                            oSM.PaymentDue = ToKina(Convert.ToDouble(item.newInvoiceAmt));
                        }
                        else
                            oSM.PaymentDue = ToKina(0);

                        if (!string.IsNullOrEmpty(item.smsCount) && item.voiceBalance != "0")
                            oSM.sSMSbalance = item.smsCount.ToString() + " Counts";
                        else
                            oSM.sSMSbalance = item.smsCount;
                        if (!string.IsNullOrEmpty(item.voiceBalance) && item.voiceBalance != "0")
                            oSM.sVoicebalance = ToMin(item.voiceBalance.ToString());
                        else
                            oSM.sVoicebalance = item.voiceBalance;
                        if (!string.IsNullOrEmpty(item.ulDataBalance) && item.ulDataBalance != "0")
                            oSM.ulDataBalance = ToMB_GB(item.ulDataBalance);
                        else
                            oSM.ulDataBalance = item.ulDataBalance;

                        if (!string.IsNullOrEmpty(item.totRoamingDataBalance) && item.totRoamingDataBalance != "0")
                            oSM.totRoamingDataBalance = ToMB_GB(item.totRoamingDataBalance);
                        else
                            oSM.totRoamingDataBalance = item.totRoamingDataBalance;

                        if (!string.IsNullOrEmpty(item.totRoamingSmsBalance) && item.totRoamingSmsBalance != "0")
                            oSM.totRoamingSmsBalance = item.totRoamingSmsBalance.ToString() + " Counts";
                        else
                            oSM.totRoamingSmsBalance = item.totRoamingSmsBalance;

                        if (!string.IsNullOrEmpty(item.totRoamingVoiceBalance) && item.totRoamingVoiceBalance != "0")
                            oSM.totRoamingVoiceBalance = item.totRoamingVoiceBalance.ToString() + " Minutes";
                        else
                            oSM.totRoamingVoiceBalance = item.totRoamingVoiceBalance;

                        #region Plans stuff
                        List<planDetailsModel> oPDM = new List<planDetailsModel>();
                        PlanDetails[] plans = item.planDetails;
                        if (oSM.planAvl == true)
                        {
                            if (plans != null)
                            {
                                if (plans.Length > 0 && plans.Count() > 0)
                                {
                                    foreach (var plan in plans)
                                    {
                                        planDetailsModel oP = new planDetailsModel();
                                        oP.planId = plan.planId;
                                        oP.planName = plan.planName;
                                        oP.planValidity = plan.planValidity;
                                        oP.expiryDate = plan.expiryDate;
                                        oP.totalUsage = plan.totalUsage;
                                        oP.effectiveDate = plan.effectiveDate;
                                        oP.isVoice = plan.voice;
                                        oP.PurchaseAllowed = plan.purchaseAllowed;
                                        oPDM.Add(oP);

                                    }

                                    oPDM = oPDM.GroupBy(m => new { m.planName, m.expiryDate, m.effectiveDate }).Select(n => n.First()).ToList();
                                  
                                    oSM.planDetails =oPDM;
                                    oSM.DataplanDetails = oPDM.Where(d => d.isVoice == false).ToList();
                                    oSM.VoiceplanDetails = oPDM.Where(v => v.isVoice == true).ToList();


                                    if (item.purchasedAllowed == true)
                                    {
                                        if (item.planDetails != null)
                                        {
                                            if (item.planDetails.Length > 0)
                                            {
                                                oSM.dPlans = _Show;
                                                oSM.dNoPlans = _Hide;

                                                if (oSM.DataplanDetails.Count > 0)
                                                {
                                                    oSM.dataPlans = _Show;
                                                    oSM.dDataBal = _Show;
                                                }
                                                else
                                                {
                                                    oSM.dataPlans = _Hide;
                                                    oSM.dDataBal = _Hide;
                                                }
                                                if (oSM.VoiceplanDetails.Count > 0)
                                                    oSM.voicePlans = _Show;
                                                else
                                                    oSM.voicePlans = _Hide;

                                            }
                                            else
                                            {
                                                oSM.dPlans = _Hide;
                                                oSM.dNoPlans = _Show;
                                            }
                                        }
                                        else
                                        {
                                            oSM.dPlans = _Hide;
                                            oSM.dNoPlans = _Show;
                                        }


                                    }
                                    else
                                    {
                                        oSM.dPlans = _Show;

                                    }


                                }
                            }
                        }
                        #endregion

                        #region yplan stuffs

                        List<yourPlans> oYP = new List<yourPlans>();
                        YourPlan[] yp = item.yourPlans;

                        if (oSM.yourPlanAvl == true)
                        {
                            if (yp != null)
                            {
                                if (yp.Length > 0 && yp.Count() > 0)
                                {
                                    oSM.yDisp = _Show;
                                    foreach (var plan in yp)
                                    {
                                        yourPlans oP = new yourPlans();
                                        oP.balance = plan.balance;
                                        // DateTime.ParseExact(sDate, "MM/dd/yyyy", null); 
                                        oP.expiry = DateTime.ParseExact(plan.expiry, "yyyyMMddHHmmss", null).ToString("dd/MM/yyyy");
                                        oP.planName = plan.planName;
                                        oYP.Add(oP);
                                    }
                                    oSM.YPlans = oYP;
                                }
                            }
                        }
                        else
                        {
                            oSM.yDisp = _Hide;
                        }

                        #endregion

                        oSM.totDataPercentage = item.totDataPercentage;
                        oSM.totDataSize = item.totDataSize;
                        oSM.totDataUsed = item.totDataUsed;
                        oSM.totSmsPercentage = item.totSmsPercentage;
                        oSM.totSmsCount = item.totSmsCount;
                        oSM.totSmsUsed = item.totSmsUsed;
                        oSM.totVoicePercentage = item.totVoicePercentage;
                        oSM.totVoiceSize = item.totVoiceSize;
                        oSM.totVoiceUsed = item.totVoiceUsed;
                        oSM.totIddBalance = item.totIddBalance + " Minutes";
                        oSM.totIddPercentage = item.totIddPercentage;
                        oSM.totIddSize = item.totIddSize;
                        oSM.totIddUsed = item.totIddUsed;

                        oSM.description = item.simDescription;

                        oSM.totRoamingDataPercentage = item.totRoamingDataPercentage;
                        oSM.totRoamingDataSize = item.totRoamingDataSize;
                        oSM.totRoamingDataUsed = item.totRoamingDataUsed;
                        oSM.totRoamingSmsPercentage = item.totRoamingSmsPercentage;
                        oSM.totRoamingSmsSize = item.totRoamingSmsSize;
                        oSM.totRoamingSmsUsed = item.totRoamingSmsUsed;
                        oSM.totRoamingVoicePercentage = item.totRoamingVoicePercentage;
                        oSM.totRoamingVoiceSize = item.totRoamingVoiceSize;
                        oSM.totRoamingVoiceUsed = item.totRoamingVoiceUsed;

                        if ( !string.IsNullOrEmpty(oSM.accountNo))
                        {

                            if (oSM.paidtype == "502")
                            {
                                int post_paid_count = subs.Where(p => p.accountNumber == oSM.accountNo).Count();

                                if (post_paid_count > 1)
                                    oSM.isExistAccountNo = true;
                            }

                            string sAcno = "";
                            List<FilesModel> fm = new List<FilesModel>();
                            if (oSM.accountNo != "" && oSM.accountNo != null)
                            {
                                sAcno = oSM.accountNo;

                                string acpath = "";
                                string pattern = sAcno + "*.pdf";
                                string fpath = System.Configuration.ConfigurationManager.AppSettings["fpathpdf"].ToString();
                                var bills = Directory.GetFiles(fpath, pattern, SearchOption.AllDirectories);
                                if (Directory.Exists(fpath))
                                {
                                    // acpath = bills.FirstOrDefault().ToString();
                                    foreach (string spdfname in bills)
                                    {
                                        FilesModel ofm = new FilesModel();
                                        ofm._file_loc = Directory.GetDirectoryRoot(spdfname);
                                        ofm._file_name = spdfname.Split('\\').Last();
                                        ofm._doc_name = sMonthname(ofm._file_name);
                                        ofm._dfiledate = dFileDate(ofm._file_name);
                                        fm.Add(ofm);
                                    }
                                }
                            }

                            oSM._ppbills = fm.OrderByDescending(f => f._dfiledate).ToList();

                            if (fm.Count > 0)
                            {
                                oSM.Disp_pbillStatus = "";
                                oSM.Disp_nbillStatus = "display:none;";
                            }
                            else
                            {
                                oSM.Disp_pbillStatus = "display:none;";
                                oSM.Disp_nbillStatus = "";
                            }
                        }

                        oSMs.Add(oSM);

                    }
                    oAM.Subr = oSMs;
                }
                return oAM;
            }




            return oAM;
        }


        private string sMonthname(string sFilename)
        {
            string sRes = "";
            if (sFilename != "")
            {
                string[] sfn = sFilename.Split('_');
                if (sfn.Length > 0 && sfn[1] != null)
                {
                    string[] sDate = sfn[1].Split('.');
                    if (sDate.Length > 0 && sDate[0] != null)
                    {
                        DateTime dt = DateTime.ParseExact(sDate[0], "yyyyMMdd", null);
                        sRes = dt.ToString("MMMM-yyyy");
                    }
                }
            }
            return sRes;
        }

        private DateTime dFileDate(string sFilename)
        {
            DateTime dRes = new DateTime();
            if (sFilename != "")
            {
                string[] sfn = sFilename.Split('_');
                if (sfn.Length > 0 && sfn[1] != null)
                {
                    string[] sDate = sfn[1].Split('.');
                    if (sDate.Length > 0 && sDate[0] != null)
                    {
                        DateTime dt = DateTime.ParseExact(sDate[0], "yyyyMMdd", null);
                        dRes = dt;
                    }
                }
            }
            return dRes;
        }

        public PaidTypeModel GetEmailFromMSISDN(string msisdn)
        {
            PaidTypeModel oPT = new PaidTypeModel();
            string sRes = "";
            sRes = proxy.getEmailFromMsisdnNew(AES_ENC(msisdn), msisdn);
            var ptype = JsonConvert.DeserializeObject<PaidTypeModel>(sRes);
            if (ptype != null)
            {
                oPT.email = ptype.email;
                oPT.msisdn = ptype.msisdn;
                oPT.paidType = ptype.paidType;

            }
            return oPT;
        }
        #endregion

        #region Reset Pwd


        /// <summary>
        /// First time Reset password
        /// </summary>
        /// <param name="oRSM"></param>
        /// <param name="iRet"></param>
        /// <param name="bFlag"></param>
        public void ResetPwd(ResetModel oRSM, out int iRet, out bool bFlag)
        {
            iRet = 0;
            bFlag = false;
            bool bUserID = true;
            proxy.ResetPassword(AES_ENC(oRSM.UserId), Convert.ToInt32(oRSM.UserId), bUserID, AES_ENC(oRSM.OldPassword), AES_ENC(oRSM.NewPassword), out iRet, out bFlag);

        }
        #endregion

        #region Forgot pwd
        public Dictionary<string, string> TempPassEmail(FPModel oFP)
        {
            Dictionary<string, string> oDic = new Dictionary<string, string>();
            object oRes = null;
            if (oFP.Email != "")
            {
                oRes = proxy.getTempPass(AES_ENC(oFP.Email), oFP.Email);
                if (oRes != null)
                {
                    oDic.Add("ResultCode", ((VerificationCode)(oRes)).result_code.ToString());
                    oDic.Add("TempPwd", ((VerificationCode)(oRes)).temporary_password);
                    oDic.Add("RegId", AES_ENC(((VerificationCode)(oRes)).registration_id.ToString()));
                }
            }
            return oDic;
        }

        public bool EmailTemppwd(Dictionary<string, string> oDic)
        {
            bool bFlag = false;
            if (oDic.Count > 0)
            {
                XElement doc = XElement.Load(System.Web.Hosting.HostingEnvironment.MapPath("~/EmailTemplate/email_template.xml"));
                XElement emailsubj = doc.Element("forgotpwd_subject");
                XElement emailBody = doc.Element("forgotpwd_body");

                string sData = emailBody.Value;
                sData = sData.Replace("##temppwd##", oDic["TempPwd"]).Replace("##resetlink##", oDic["ResetLink"]);
                Utils.General ogen = new Utils.General();
                ogen.Mailcode(emailsubj.Value, oDic["ToEmail"], sData, out bFlag);
            }
            return bFlag;
        }

        public void VerifyUser(TPModel oTP, out bool bRetVerify)
        {
            bool bVerify = false;
             bRetVerify = false;
            proxy.verifyTempPass(oTP.Id, oTP.TempPwd, out bRetVerify, out bVerify);
        }

        public void UpdatePassword(TPModel oTP, out int iRet)
        {
            iRet = -1;
            bool bflag = false;
            proxy.updatePasswordbyTempPass(oTP.Id, AES_ENC(oTP.NewPwd), out iRet, out bflag);
        }
        #endregion

        #region Conversions
        private string ToKina(double _dval)
        {
            double dRes = _dval / 100;
            string sRes = "$ " + dRes.ToString("#0.00");
            return sRes;
        }

        private string KinaFormat(double _dval)
        {
            string sRes = "$ " + string.Format("{0:0.##}", _dval); //_dval.ToString("#0.00");
            return sRes;
        }

        private string ToMin(string sSecs)
        {
            string sRes = "";
            double seconds = 0;
            int iMins = 0;
            bool bRes = double.TryParse(sSecs, out seconds);
            if (bRes == true)
            {
                //var timespan = TimeSpan.FromSeconds(seconds);
                //sRes = timespan.ToString(@"hh\:mm\:ss");
                iMins = (Convert.ToInt32(seconds) / 60);
                sRes = iMins + " Minutes";
            }
            return sRes;
        }
        private string ToMB_GB(string _val)
        {
            string sTotUsage = "";
            bool bIsDouble = false;
            double dTotUsage;
            //double dRes = 0;
            bIsDouble = double.TryParse(_val, out dTotUsage);
            if (bIsDouble)
            {
                //dTotUsage = double.Parse(_val);
                if (dTotUsage > 0)
                {
                    double data_GB = (((dTotUsage / 1024) / 1024) / 1024);
                    double data_MB = ((dTotUsage / 1024) / 1024);                    
                    if (data_GB >= 1)
                    {
                        //dRes = ((dTotUsage / 1024) / 1024) / 1024;
                        sTotUsage = string.Format("{0:0.##}", data_GB) + "GB";
                    }
                    else if (data_MB >= 1)
                    {
                        //dRes = (dTotUsage / 1024) / 1024;
                        sTotUsage = string.Format("{0:0.##}", data_MB) + "MB";
                    }
                    else
                    {
                        double data_KB = (dTotUsage / 1024);
                        sTotUsage = string.Format("{0:0.##}", data_KB) + "KB";
                    }
                }
                else { sTotUsage = "0 MB"; }
            }
            return sTotUsage;
        }
        #endregion


        #region Bundles/Purchase
        /// <summary>
        /// GET ALL Bundles to display in Admin
        /// </summary>
        /// <returns></returns>
        public List<BundleModel> GetAllBundles()
        {

            List<BundleModel> bundles = new List<BundleModel>();
            //Bundles[] oBundles = null;
            var oBundles = _dbselfcare.bundles.Where(b => b.isDelete == false && b.isPostpaid == false).OrderBy(p => p.Price).ToList();
            //oBundles = proxy.getPlans(AES_ENC(sCode));
            if (oBundles.Count > 0)
            {
                for (int i = 0; i < oBundles.Count; i++)
                {
                    BundleModel ob = new BundleModel();
                    //ob.htmlAttributes = new Dictionary<string, object>();
                    ob.Id = Convert.ToInt64(oBundles[i].Id);
                    ob.isActive = Convert.ToBoolean(oBundles[i].isActive);
                    ob.isPostpaid = Convert.ToBoolean(oBundles[i].isPostpaid);
                    ob.PlanName = oBundles[i].PlanName;
                    ob.Price = Convert.ToDouble(oBundles[i].Price);
                    ob._sPrice = KinaFormat(Convert.ToDouble(oBundles[i].Price));
                    ob.Size = (oBundles[i]).Size;
                    ob.Validity = Convert.ToInt32(oBundles[i].Validity);
                    ob.Description = oBundles[i].Description;
                    ob.isVoice = Convert.ToBoolean(oBundles[i].isVoice);
                    ob.orderby = oBundles[i].orderby;
                    ob.isChecked = false;
                    ob.isDeleted = Convert.ToBoolean(oBundles[i].isDelete);
                    if (ob.isActive == true)
                        ob.sStatus = "Active";
                    else
                        ob.sStatus = "Inactive";
                    if (ob.isVoice == true)
                        ob.sType = "Voice";
                    else
                        ob.sType = "Data";
                    bundles.Add(ob);

                }
            }
            return bundles;
        }


        /// <summary>
        /// Get all bundles
        /// </summary>
        /// <param name="oSubscriber"></param>
        /// <returns></returns>
        public List<BundleModel> GetBundles(string sUserId, double dBalance)
        {
            List<BundleModel> bundles = new List<BundleModel>();
            // Bundles[] oBundles = null;
            var oBundles = _dbselfcare.bundles.Where(b => b.isDelete == false && b.isPostpaid == false).OrderBy(p => p.Price).ToList();
            if (oBundles.Count > 0)
            {
                for (int i = 0; i < oBundles.Count; i++)
                {                   
                    BundleModel ob = new BundleModel();
                    ob.Id = Convert.ToInt64(oBundles[i].Id);
                    ob.isActive = Convert.ToBoolean(oBundles[i].isActive);
                    ob.isPostpaid = Convert.ToBoolean(oBundles[i].isPostpaid);
                    ob.PlanName = oBundles[i].PlanName;
                    ob.Price = Convert.ToDouble(oBundles[i].Price);
                    ob._sPrice = KinaFormat(Convert.ToDouble(oBundles[i].Price));
                    ob.Size = ToMB_Bytes(oBundles[i].Size);
                    ob.Validity = Convert.ToInt32(oBundles[i].Validity);
                    ob.Description = oBundles[i].Description;
                    ob.isVoice = Convert.ToBoolean(oBundles[i].isVoice);
                    if ((dBalance / 100) >= ob.Price)
                        ob.htmlAttributes = "enabled";
                    else
                        ob.htmlAttributes = "disabled";

                    ob.isChecked = false;
                    ob.AccountType = oBundles[i].AccountType;
                    ob.SmsAccountType = oBundles[i].SmsAccountType;
                    ob.VoiceAccountType = oBundles[i].VoiceAccountType;
                    ob.IddAccountType = oBundles[i].IddAccountType;
                    ob.voiceSize = (!string.IsNullOrEmpty(oBundles[i].voiceSize) ? (Convert.ToDecimal(oBundles[i].voiceSize) / 60).ToString() + " Mins" : "n/a");
                    ob.smsCount = oBundles[i].smsCount;

                    ob.voice_desc = oBundles[i].voice_desc;
                    ob.sms_desc = oBundles[i].sms_desc;
                    ob.validity_txt = oBundles[i].validity_txt;

                    ob.romingDataAccountType = oBundles[i].romingDataAccountType;
                    //ob.roamingVoiceAccountType = (!string.IsNullOrEmpty(oBundles[i].roamingVoiceAccountType) ? (Convert.ToDecimal(oBundles[i].roamingVoiceAccountType) / 60).ToString() + "Mins" : "n/a");
                    ob.roamingVoiceAccountType = oBundles[i].roamingVoiceAccountType;
                    ob.roamingSmsAccountType = oBundles[i].roamingSmsAccountType;

                    bundles.Add(ob);
                }
            }
            return bundles;
        }


        private string ToMB_Bytes(string size_mb)
        {
            string Res = "n/a";            
            if(!string.IsNullOrEmpty(size_mb))
            {
                double data_MB;
                bool s_res = double.TryParse(size_mb, out data_MB);
                if(s_res)
                {
                    double data_Bytes = ((data_MB * 1024)*1024);
                    Res = ToMB_GB(data_Bytes.ToString());
                }
            }

            return Res;
        }

        public List<BundleModel> GetVoicePlans(List<BundleModel> bundles, List<planDetailsModel> v_plan, double dbalance)
        {
            List<BundleModel> vbundles = new List<BundleModel>();
            //var voicebundles = bundles.Where(b => b.isVoice == true).ToList();
            var voicebundles = bundles.Where(b => b.VoiceAccountType == "0" && b.AccountType == "0" && b.SmsAccountType == "0" && b.IddAccountType != "0" && b.romingDataAccountType =="0" && b.roamingVoiceAccountType == "0" && b.roamingSmsAccountType == "0").ToList();
            if (v_plan != null)
                v_plan = v_plan.Where(p => p.isVoice == true).ToList();

            foreach (var item in voicebundles)
            {
                BundleModel ob = new BundleModel();
                //ob.htmlAttributes = new Dictionary<string, object>();
                ob.Id = item.Id;
                ob.isActive = item.isActive;
                ob.isPostpaid = item.isPostpaid;
                ob.PlanName = item.PlanName;
                ob.Price = item.Price;
                ob._sPrice = KinaFormat(item.Price);
                ob.Size = item.Size;
                ob.Validity = item.Validity;
                ob.Description = item.Description;
                ob.isVoice = item.isVoice;
                ob.isPlanActive = false;
                ob.validity_txt = item.validity_txt;
                //ob.isVoice = item.voic;
                string splanId = item.Id.ToString();

                if (v_plan != null)
                {
                    var vDis = v_plan.Where(p => p.planId == splanId && p.PurchaseAllowed == false).ToList();
                    if (vDis.Count > 0 || (dbalance / 100) <= ob.Price)
                    {
                        ob.htmlAttributes = "disabled";
                        ob.isPlanActive = true;
                        //ob.htmlAttributes.Add("disabled", "disabled");
                    }
                    else
                        ob.htmlAttributes = "enabled";
                    //ob.htmlAttributes.Add("enabled", "enabled");
                }
                else
                {
                    if ((dbalance / 100) <= ob.Price)
                        ob.htmlAttributes = "disabled";//ob.htmlAttributes.Add("disabled", "disabled");
                    else
                        ob.htmlAttributes = "enabled";//ob.htmlAttributes.Add("enabled", "enabled");

                }
                ob.isChecked = false;
                ob.voiceSize = item.voiceSize;
                vbundles.Add(ob);
            }

            return vbundles;

        }





        /// <summary>
        /// Purchase a bundle plan for the give msisdn
        /// </summary>
        /// <param name="oCM"></param>
        /// <returns></returns>
        public string UpdateSubscription(ChangeTypeModel oCM)
        {
            string sRet = "";
            if (oCM.MobileNo != "" && oCM.MobileNo != null)
            {
                sRet = proxy.updateSubscriptionPlan(AES_ENC(oCM.MobileNo), oCM.MobileNo, oCM.PlanId, true);
            }
            return sRet;
        }
        #endregion

        #region Top up Stuffs
        /// <summary>
        /// Topup voucher
        /// </summary>
        /// <param name="oTM"></param>
        /// <returns></returns>
        public string Voucher_Topup(TopupModel oTM)
        {
            string sRes = "";
            sRes = proxy.topupVoucher(AES_ENC(oTM.MSISDN_Number), oTM.VoucherNumber, oTM.MSISDN_Number);
            return sRes;
        }
        #endregion

        #region Transaction/Application Logs

        public List<UserHistoryLogs> get_user_history()
        {
            List<UserHistoryLogs> oUM = new List<UserHistoryLogs>();


            oUM = (from UH in _dbselfcare.userhistories
                   join B in _dbselfcare.bundles on UH.PlanId equals B.Id
                   join U in _dbselfcare.registrations on UH.UserId equals U.Id
                   where UH.IsDeleted == false
                   orderby UH.CreatedDate descending
                   select new UserHistoryLogs
                   {
                       createdDate = UH.CreatedDate,
                       id = UH.id,
                       isDeleted = UH.IsDeleted,
                       modifyDate = UH.ModifyDate,
                       planId = UH.PlanId,
                       userId = UH.UserId,
                       planName = B.PlanName,
                       userName = U.FirstName + " " + U.LastName,
                   }).ToList();



            return oUM;
        }

        public List<YourPlanTransModel> get_your_plan_translog()
        {
            var plantrans = (from t in _dbselfcare.your_plan_transactions
                             select new YourPlanTransModel
                             {
                                 date = t.date,
                                 expiry_date = t.expiry_date,
                                 from_msisdn = t.from_msisdn,
                                 Id = t.Id,
                                 isExpired = t.isExpired,
                                 to_msisdn = t.to_msisdn,
                                 total_amt = t.total_amt
                             }).ToList();

            return plantrans;
        }

        public List<Application_Transaction> get_application_translogs()
        {
            var oAppTrans = (from trans in _dbselfcare.application_transaction
                             orderby trans.date descending
                             select new Application_Transaction
                             {
                                 id = trans.id,
                                 date = trans.date,
                                 msisdn = trans.msisdn,
                                 reference = trans.reference,
                                 status = trans.status,
                                 log = (from trans_log in _dbselfcare.application_transaction_log
                                        where trans_log.app_tx_id == trans.id
                                        select new Transaction_LogDetail { appTxId = trans_log.app_tx_id, date = trans_log.date, details = trans_log.details, id = trans_log.id, status = trans_log.status, type = trans_log.type }).FirstOrDefault(),
                             }).ToList();



            return oAppTrans;
        }


        #endregion

        #region Postpaid stuffs
        public byte[] DwldPostpaidbill(string filepath, string sFilename)
        {
            General obj = new General();
            byte[] result = obj.Downloadfile(filepath, sFilename);
            return result;
        }

        public int CheckPaidType(string sPostpaidnumber)
        {
            bool bPPRes = false;
            int iRet = 0;
            if (sPostpaidnumber != "" && sPostpaidnumber != null)
            {
                proxy.accountPaidTypeByMsisdn(AES_ENC(sPostpaidnumber), sPostpaidnumber, out iRet, out bPPRes);
            }
            return iRet;
        }

        public RegistrationModel GetRegDetails(string sMSISDN)
        {
            Registration[] regis = proxy.getSubscriptionProfileByMssidn(AES_ENC(sMSISDN), sMSISDN);

            RegistrationModel oRM = new RegistrationModel();
            if (regis != null)
            {
                foreach (var ritem in regis)
                {
                    oRM.Address1 = ritem.addr1;
                    oRM.Address2 = ritem.addr2;
                    oRM.BroadBandNo = ritem.msisdnNumber;
                    oRM.Company = ritem.company;
                    // oRM.ContactNo = ritem.mobileNumber;
                    oRM.dPassword = ritem.dPassword;
                    oRM.Email = ritem.email;
                    oRM.FirstName = ritem.firstName;
                    oRM.FormattedBroadBandNo = oRM.BroadBandNo.Substring(0, 3) + " " + oRM.BroadBandNo.Substring(3);
                    oRM.LastName = ritem.lastName;
                    oRM.MiddleName = ritem.middleName;
                    oRM.paidType = ritem.paidType;
                    oRM.Password = ritem.password;
                    oRM.UserId = Convert.ToInt64(ritem.id);
                    oRM.Username = ritem.userName;
                    oRM.account_number = ritem.accountNumber;
                }

            }
            return oRM;
        }

        public string PurchasePostpaid(PostpaidModel oP)
        {
            string sResult = "";
            string details = JsonConvert.SerializeObject(oP);
            if (!string.IsNullOrEmpty(details))
                sResult = proxy.postpaidPurchase(_util_repo.AES_ENC(sCode), details);
            return sResult;
        }

        #endregion


        #region SEND SIM Verification code

        public int SendSimVerification_Res(long regId, string msisdn, out int iRet, out bool bflag)
        {
            iRet = -1;
            bflag = true;
            proxy.sendSimVerificationCode(AES_ENC(sCode), regId, bflag, msisdn, out iRet, out bflag);
            return iRet;
        }


        public int AddMulitSIM_Res(long regId, string msisdn, string vcode, string description, out int iRet, out bool bflag)
        {
            iRet = -1;
            bflag = true;
            proxy.addMultipleSim(AES_ENC(sCode), regId, bflag, msisdn, vcode,description, out iRet, out bflag);
            return iRet;
        }
        public int RemoveSIM_Res(long regId, string msisdn)
        {
            int iRet = -1;
            bool bflag = true;
            proxy.deleteMsisdn(AES_ENC(regId.ToString()), regId, bflag, msisdn, out iRet, out bflag);
            return iRet;
        }
        public bool VerifyMSISN(long regId, string sMsisdn)
        {
            bool bflag = false;
            proxy.isRegisteredMsisdn(AES_ENC(regId.ToString()), regId, true, sMsisdn, out bflag, out bflag);
            return bflag;
        }

        #endregion

        #region SIM Activation

        public string VerifySIMActivation(string msidn_no, string sim_no, string puk_code, string activated_by)
        {
            string Res = proxy.verifySimActivation(AES_ENC(msidn_no), msidn_no, puk_code, sim_no, activated_by);
            return Res;
        }

        public int ActivateSIM(string msidn_no, string ref_no)
        {
            int Res = 0;
            bool bRes = false;
            proxy.activateSIM(AES_ENC(msidn_no), msidn_no, ref_no, out Res, out bRes);
            return Res;
        }
        #endregion


        public string RedeemFBPromotion_GetVoucher(RedeemFBPromotionsModel RedeemFB)
        {
            string _iRes = string.Empty;

            //FBPromotionInputModel model = new FBPromotionInputModel();
            //model.username = "bmselfcare";
            //model.password = "bmselfcare";
            //model.keycode = "45678";
            //model.serialNo = RedeemFB.serial_number;
            //model.amount = RedeemFB.amount.ToString();

            //var data = JsonConvert.SerializeObject(model);

            //_iRes = _easyRecharge.getVoucher("111", data);
            _iRes = "";//proxy.getVoucher(RedeemFB.serial_number);

            return _iRes;
        }

        public string VoiceCallActivationDeactivation(VoiceCallActDeactModel objVoiceCall)
        {
            string _iRes = string.Empty;

            //VCActDeactInputModel model = new VCActDeactInputModel();
            //model.username = "bmselfcare";
            //model.password = "bmselfcare";
            //model.keycode = "45678";
            //model.msisdn = objVoiceCall.msisdn_no;

            //var data = JsonConvert.SerializeObject(model);
            //string enc_json_detail = _util_repo.AES_ENCWR(data);

            if (objVoiceCall.vc_type == "act")
            {
                //_iRes = _easyRecharge.BarVoiceCall("111", data);
                _iRes = "";// proxy.BarVoiceCall(objVoiceCall.msisdn_no);

            }
            else if (objVoiceCall.vc_type == "deact")
            {
                //_iRes = _easyRecharge.UnBarVoiceCall("111", data);
                _iRes = "";// proxy.UnBarVoiceCall(objVoiceCall.msisdn_no);
            }

            //if (!string.IsNullOrEmpty(_iRes))
            //{                
            //    VCActDeactOutputModel VC_Out = JsonConvert.DeserializeObject<VCActDeactOutputModel>(_iRes);
            //    if (VC_Out != null)
            //        _iRes = VC_Out.resultcode;
            //}

            return _iRes;
        }

        #region top kad
        public int insert_top_kad_log(top_kad_verify_model obj_top_kad)
        {
            int iresult = 1;
            if (obj_top_kad != null)
            {
                tbl_top_kad_log obj_new_top_kad = new tbl_top_kad_log();
                obj_new_top_kad.invoice = obj_top_kad.invoice;
                obj_new_top_kad.is_recharged = obj_top_kad.is_recharged;
                obj_new_top_kad.msisdn = obj_top_kad.msisdn;
                obj_new_top_kad.recharge_amount = obj_top_kad.recharge_amount;
                obj_new_top_kad.serial_number = obj_top_kad.serial_number;
                obj_new_top_kad.server_reference = obj_top_kad.server_reference;
                obj_new_top_kad.server_response = obj_top_kad.server_response;
                obj_new_top_kad.sever_desc = obj_top_kad.sever_desc;
                obj_new_top_kad.updated_by = obj_top_kad.updated_by;

                obj_new_top_kad.created_on = DateTime.Now;
                UOW.top_kad_log_repo.Insert(obj_new_top_kad);
                UOW.Save();
                if (obj_top_kad.id > 0)
                    iresult = 0;
            }
            return iresult;
        }

        public List<tbl_top_kad_log> get_top_kad_Log_List(string UserId)
        {
            List<tbl_top_kad_log> obj_top_kad_log = new List<tbl_top_kad_log>();
            obj_top_kad_log = UOW.top_kad_log_repo.Get().ToList();

            if (!string.IsNullOrEmpty(UserId))
                obj_top_kad_log = obj_top_kad_log.Where(k => k.updated_by == UserId).ToList();

            obj_top_kad_log = (from k in obj_top_kad_log.AsEnumerable()
                               join u in get_care_user_list() on k.updated_by equals u.id.ToString()
                               select new tbl_top_kad_log
                               {
                                   created_on = k.created_on,
                                   id = k.id,
                                   invoice = k.invoice,
                                   is_recharged = k.is_recharged,
                                   msisdn = k.msisdn,
                                   recharge_amount = k.recharge_amount,
                                   serial_number = k.serial_number,
                                   server_reference = k.server_reference,
                                   server_response = k.server_response,
                                   sever_desc = k.sever_desc,
                                   updated_by = u.first_name + " " + u.last_name
                               }).ToList();

            return obj_top_kad_log;
        }
        private List<web_tbl_care_user> get_care_user_list()
        {
            List<web_tbl_care_user> obj_care_user = new List<web_tbl_care_user>();
            obj_care_user = UOW.care_user_repo.Get().ToList();
            return obj_care_user;
        }
        public string get_json_for_top_Kad_service(tbl_top_kad_log obj_top_kad)
        {
            string json_detail = "";
            top_kad_model obj_top_kad_model = new top_kad_model();
            obj_top_kad_model.keycode = kad_keycode;
            obj_top_kad_model.msisdn = obj_top_kad.msisdn;
            obj_top_kad_model.password = kad_password;
            obj_top_kad_model.serialNo = obj_top_kad.serial_number;
            obj_top_kad_model.username = kad_user_name;
            json_detail = JsonConvert.SerializeObject(obj_top_kad_model);
            return json_detail;
        }
        public string verify_topkad_serial(tbl_top_kad_log obj_top_kad)
        {
            string json_result = "";
            string json_detail = "", enc_json_detail = "", dec_json_result;

            top_kad_return_model obj_return = new top_kad_return_model();
            json_detail = get_json_for_top_Kad_service(obj_top_kad);
            enc_json_detail = _util_repo.AES_ENCWR(json_detail);

            json_result = _easyRecharge.validateVoucher(kad_merchant_id, enc_json_detail);
            dec_json_result = _util_repo.AES_DECWR(json_result);

            return dec_json_result;
        }

        public string top_up_topkad_serial(tbl_top_kad_log obj_top_kad)
        {
            string json_result = "";
            string json_detail = "", enc_json_detail = "", dec_json_result;
            top_kad_return_model obj_return = new top_kad_return_model();
            json_detail = get_json_for_top_Kad_service(obj_top_kad);
            enc_json_detail = _util_repo.AES_ENCWR(json_detail);

            json_result = _easyRecharge.topupVoucher(kad_merchant_id, enc_json_detail);
            dec_json_result = _util_repo.AES_DECWR(json_result);
            return dec_json_result;
        }
        #endregion

        public bool Update_MSISDN_Description(long reg_id, string msisdn, string desc)
        {
            bool bflag = false;

            string Res = ""; //proxy.updateAdditionalSim(reg_id, true, msisdn, desc);
            if (Res == "0")
                bflag = true;

            return bflag;
        }

        public bool Delete_FB_Voucher(long id)
        {
            bool Res = false;

            web_tbl_fb_promotions objFB = UOW.redeem_fb_promotions_repo.Get(filter: x => x.Id == id).FirstOrDefault();
            if (objFB != null)
            {
                objFB.is_deleted = true;

                UOW.redeem_fb_promotions_repo.Update(objFB);
                UOW.Save();

                Res = true;
            }

            return Res;
        }

        #region GetMsisdnStatus

        public int GetMsisdnStatus(string msisdn_no)
        {
            int Res = 0;

            CheckMsisdnInputModel model = new CheckMsisdnInputModel();
            model.username = kad_user_name;
            model.password = kad_password;
            model.keycode = kad_keycode;
            model.msisdn = msisdn_no;

            string jdata = JsonConvert.SerializeObject(model);
            string enc_jdata = _util_repo.AES_JENC(jdata);

            string enc_json_result = _easyRecharge.GetMsisdnStatus(kad_merchant_id, enc_jdata);
            string dec_json_result = _util_repo.AES_JDEC(enc_json_result);

            if (!string.IsNullOrEmpty(dec_json_result))
            {
                CheckMsisdnOutputModel OPRes = JsonConvert.DeserializeObject<CheckMsisdnOutputModel>(dec_json_result);
                if (OPRes != null)
                    Res = OPRes.msisdn_Status;
            }

            return Res;
        }
        #endregion

        #region GetRegistrationDetails

        public List<RegistrationReportModel> GetRegistrationDetails()
        {
            List<RegistrationReportModel> objRegDetail = new List<RegistrationReportModel>();

            List<multiplesim> objMultiSIM = _dbselfcare.multiplesims.Where(x => x.isDeleted == false).ToList();

            var obj_reg = (from reg in _dbselfcare.registrations
                           select reg).ToList();


            objRegDetail = (from reg in obj_reg.AsEnumerable()
                            select new RegistrationReportModel
                                {
                                    Reg = reg,
                                    MultipileSim = objMultiSIM.Where(x => x.RegistrationId == reg.Id).ToList()
                                }).ToList();

            return objRegDetail;
        }

        #endregion

        #region CreatePlanPurchaseTransLog
        public tbl_web_plan_purchase_trans CreatePlanPurchaseTransLog(ChangeTypeModel oCM)
        {
            tbl_web_plan_purchase_trans obj = new tbl_web_plan_purchase_trans();
            if (oCM != null)
            {
                obj.trans_number = GetUniqueTransNo();
                obj.msisdn = oCM.MobileNo;
                obj.plan_id = oCM.PlanId;
                obj.plan_name = oCM.planName;
                obj.plan_price = Convert.ToDecimal(oCM.dPlanPrice);
                obj.curr_bal = Convert.ToDecimal(oCM.balance);
                obj.user_id = oCM.UserId;
                obj.trans_status = "PENDING";
                obj.created_on = DateTime.Now;
                obj.is_processed = false;
                obj.type_id = 1;
                UOW.plan_purchase_trans_Repo.Insert(obj);
                UOW.Save();
            }
            return obj;
        }

        private string GetUniqueTransNo()
        {
            string transNo = _util_repo.GetRandomNumber(8);
            tbl_web_plan_purchase_trans chktransno = UOW.plan_purchase_trans_Repo.Get(filter: p => p.trans_number == transNo).FirstOrDefault();
            if (chktransno != null)
                transNo = GetUniqueTransNo();
            return transNo;
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
                    _dbselfcare.Dispose();
                    _util_repo.Dispose();
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