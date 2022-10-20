using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Net;
using System.Net.Mail;
using System.Drawing;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;
/// <suobjMailMsgary>
/// SuobjMailMsgary description for SendMail
/// </suobjMailMsgary>
/// 
namespace si_bmobile.Utils
{
    public class General
    {
       
        public string RandomString(Random r, int len)
        {
            string str
            = "1234567890";
            StringBuilder sb = new StringBuilder();

            while ((len--) > 0)
                sb.Append(str[(int)(r.NextDouble() * str.Length)]);

            return sb.ToString();
        }       

        public void Mailcode(string sSubject, string toAddr, string sMessage,out bool bSend)
        {
            bSend = false;
            string sFrom = ConfigurationSettings.AppSettings["From"];
            string sCC = ConfigurationSettings.AppSettings["EmailCCTo"];
            string smtp_client = ConfigurationSettings.AppSettings["Smtp_Client"];
            string uname = ConfigurationSettings.AppSettings["UName"];
            string pwd = ConfigurationSettings.AppSettings["Password"];
            MailMessage myMail = new MailMessage();
            myMail.From = new MailAddress(sFrom);
            myMail.Subject = sSubject;
            MailAddressCollection myMailTo = new MailAddressCollection();
            myMail.To.Add(toAddr);
            if (!string.IsNullOrEmpty(sCC))
                myMail.Bcc.Add(sCC);
            StringBuilder sb = new StringBuilder();
            sb.Append(sMessage);
            string strBody = sb.ToString();
            myMail.Body = strBody;
            myMail.IsBodyHtml = true;
            SmtpClient smtp = new SmtpClient(smtp_client);
            smtp.Credentials = new NetworkCredential(uname, pwd);
            try
            {
                smtp.Send(myMail);
                bSend = true;
                myMail = null;
            }
            catch (System.Exception ex)
            {
                bSend = false;
            }
        }


        public byte[] Downloadfile(string spath, string sfilename)
        {

            HttpContext.Current.Response.ContentType = "application/pdf";
            HttpContext.Current.Response.AppendHeader("content-disposition", "attachment; filename=" + sfilename);
            FileStream sourceFile = new FileStream(spath, FileMode.Open);
            long FileSize;
            FileSize = sourceFile.Length;
            byte[] getContent = new byte[(int)FileSize];
            sourceFile.Read(getContent, 0, (int)sourceFile.Length);
            sourceFile.Close();
            HttpContext.Current.Response.BinaryWrite(getContent);
            return getContent;
        }

        public bool CheckHTMLdata(string sdata)
        {
            List<string> SpecChars = new List<string>();
            SpecChars.Add("<");
            SpecChars.Add(">");        

            bool bRet = true;

            foreach (string spec_char in SpecChars)
            {
                if (sdata.Contains(spec_char))
                    bRet = false;
            }

            return bRet;
        }              

        //public bool CheckContactNo(string C_No)
        //{
        //    bool IsNumeric = false;
        //    long bres=0;
        //    IsNumeric = long.TryParse(C_No, out bres);
        //    if (IsNumeric == true)
        //    {
        //            if (C_No.Length >= 6 && C_No.Length <= 15)
        //            {

        //                string fmt1 = C_No.Substring(0, 2);
        //                string fmt2 = C_No.Substring(0, 4);
        //                if (fmt1 == "87" || fmt2 == "2015" || fmt1 == "75" || fmt1 == "84")
        //                {
        //                    return true;
        //                }
        //                else
        //                    return false;
        //            }
        //            else
        //            {
        //                //please enter valid number
        //                return false;
        //            }
        //    }
        //    else
        //    {
        //    //please enter valid number
        //    return false;
        //    }
        //}

        public bool CheckContactNo(string C_No)
        {
            bool IsNumeric = false;
            long bres = 0;
            IsNumeric = long.TryParse(C_No, out bres);
            if (IsNumeric == true)
           {
                string msisdn_prefix1 = ConfigurationSettings.AppSettings["msisdn_prefix1"];
                List<string> prefix1 = msisdn_prefix1.Split(',').ToList();

                string msisdn_prefix2 = ConfigurationSettings.AppSettings["msisdn_prefix2"];
                List<string> prefix2 = msisdn_prefix2.Split(',').ToList();

                int msisdn_min_len = Convert.ToInt32(ConfigurationSettings.AppSettings["msisdn_min_len"]);
                int msisdn_max_len = Convert.ToInt32(ConfigurationSettings.AppSettings["msisdn_max_len"]);

                if (C_No.Length >= msisdn_min_len && C_No.Length <= msisdn_max_len)
                {

                    string fmt1 = C_No.Substring(0, 2);
                    string fmt2 = C_No.Substring(0, 4);
                    if (prefix1.Contains(fmt1) || prefix2.Contains(fmt2))
                    {
                        return true;
                    }
                    else
                        return false;
                }
                else
                {
                    //please enter valid number
                    return false;
                }
            }
            else
            {
                //please enter valid number
                return false;
            }
        }

        #region Error Log
        /// <summary>
        /// Log the error in txt file
        /// </summary>
        /// <param name="sPathName"></param>
        /// <param name="sErrMsg"></param>
        public void ErrorLog_Txt(string sPathName, string sErrMsg, string stackTrace)
        {
            //HttpContext.Current.Session.Clear();
            //sLogFormat used to create log files format :
            // dd/mm/yyyy hh:mm:ss AM/PM ==> Log Message
            string sLogFormat = DateTime.Now.ToShortDateString().ToString() + " " + DateTime.Now.ToLongTimeString().ToString() + " ==> ";

            //this variable used to create log filename format "
            //for example filename : ErrorLogYYYYMMDD
            string sYear = DateTime.Now.Year.ToString();
            string sMonth = DateTime.Now.Month.ToString();
            string sDay = DateTime.Now.Day.ToString();
            string sErrorTime = sYear + sMonth + sDay;

            StreamWriter sw = new StreamWriter(sPathName + sErrorTime + ".txt", true);
            sw.WriteLine(sLogFormat + sErrMsg);
            sw.WriteLine(stackTrace);
            sw.Flush();
            sw.Close();
            //sw.Dispose();
        }
        #endregion
    }
}
