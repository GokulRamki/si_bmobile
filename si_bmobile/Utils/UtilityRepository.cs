using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Security.Cryptography;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Collections;
namespace si_bmobile.Utils
{
    public class UtilityRepository : IUtilityRepository, IDisposable
    {
        public string sEncKey = ConfigurationManager.AppSettings["encKey"];
        string error_log_path = ConfigurationManager.AppSettings["error_log_path"];
        string sBugemailTo = ConfigurationManager.AppSettings["bugEmailTo"];
        //string ErrPath = ConfigurationManager.AppSettings["ErrPath"];
        string sJEnckey = ConfigurationManager.AppSettings["dk_enc_key"];
        private string bugmailSubject = ConfigurationSettings.AppSettings["bugmailSubject"];

        public string AES_ENC(string _input)
        {
            Encryptor encryptor = new Encryptor(sEncKey);
            string sEnc = encryptor.Encrypt<AesEncryptor>(_input);
            sEnc = ReplaceStr(sEnc);
            return sEnc;
        }
        public string AES_DEC(string _input)
        {
            string sdecstring =ReplaceStr(_input);
            Encryptor encryptor = new Encryptor(sEncKey);
            string sDec = encryptor.Decrypt<AesEncryptor>(sdecstring);
            return sDec;
        }

        public string AES_ENCWR(string _input)
        {
            Encryptor encryptor = new Encryptor(sEncKey);
            string sEnc = encryptor.Encrypt<AesEncryptor>(_input);
            return sEnc;
        }

        public string AES_DECWR(string _input)
        {
            Encryptor encryptor = new Encryptor(sEncKey);
            string sDec = encryptor.Decrypt<AesEncryptor>(_input);
            return sDec;
        }
         public string AES_JDEC(string _input)
        {
            Encryptor encryptor = new Encryptor(sJEnckey);
            string sDec = encryptor.Decrypt<AesEncryptor>(_input);
            return sDec;
        }
         public string AES_JENC(string _input)
         {
             Encryptor encryptor = new Encryptor(sJEnckey);
             string sEnc = encryptor.Encrypt<AesEncryptor>(_input);
             return sEnc;
         }

        public string ReplaceStr(string sString)
        {
            string sRes = sString.Replace(":", "");
            sRes = sRes.Replace("&", "");
            sRes = sRes.Replace(" ", "-");
            sRes = sRes.Replace("'", "-");
            sRes = sRes.Replace(".", "");
            sRes = sRes.Replace("/", "-");
            sRes = sRes.Replace("%", "-");
            sRes = sRes.Replace("+", "-");
            return sRes;
        }

        private string sLogFormat;
        private string sErrorTime;

        public void ErrorLog_Txt(string sErrMsg, string stackTrace)
        {
            //HttpContext.Current.Session.Clear();
            //sLogFormat used to create log files format :
            // dd/mm/yyyy hh:mm:ss AM/PM ==> Log Message
            sLogFormat = DateTime.Now.ToShortDateString().ToString() + " " + DateTime.Now.ToLongTimeString().ToString() + " ==> ";

            //this variable used to create log filename format "
            //for example filename : ErrorLogYYYYMMDD
            string sYear = DateTime.Now.Year.ToString();
            string sMonth = DateTime.Now.Month.ToString();
            string sDay = DateTime.Now.Day.ToString();
            sErrorTime = sYear + sMonth + sDay;

            StreamWriter sw = new StreamWriter(error_log_path + sErrorTime + ".txt", true);
            sw.WriteLine(sLogFormat + sErrMsg);
            sw.WriteLine(stackTrace);
            sw.Flush();
            sw.Close();
            SendEmailMessageFROMGMAIL(sBugemailTo, bugmailSubject, sLogFormat + sErrMsg + stackTrace);

        }

        /// <summary>
        /// For Extended Log info
        /// </summary>
        /// <param name="sErrMsg"></param>
        /// <param name="stackTrace"></param>
        public void ErrorLog_Txt(Exception ex)
        {
            //HttpContext.Current.Session.Clear();
            //sLogFormat used to create log files format :
            // dd/mm/yyyy hh:mm:ss AM/PM ==> Log Message
            sLogFormat = DateTime.Now.ToShortDateString().ToString() + " " + DateTime.Now.ToLongTimeString().ToString() + " ==> ";

            //this variable used to create log filename format "
            //for example filename : ErrorLogYYYYMMDD
            string sYear = DateTime.Now.Year.ToString();
            string sMonth = DateTime.Now.Month.ToString();
            string sDay = DateTime.Now.Day.ToString();
            sErrorTime = sYear + sMonth + sDay;

            StreamWriter sw = new StreamWriter(error_log_path + sErrorTime + ".txt", true);

            string sExMsg = "";
            if (!string.IsNullOrEmpty(ex.Message))
            {

                sExMsg = sLogFormat + ex.Message;

                sw.WriteLine(sExMsg);
                if (ex.InnerException != null)
                {
                    sExMsg += string.Format("\n  Begin Inner Exception  n" + ex.InnerException.Message + "\n");
                    sw.WriteLine(sExMsg);
                    if (ex.InnerException.InnerException != null)
                    {
                        if (ex.InnerException.Message != null)
                        {
                            if (ex.InnerException.InnerException.Message != null)
                                sExMsg += string.Format("Inner Message details :") + ex.InnerException.InnerException.Message + string.Format("\n\n  End Inner Exception ");
                            sw.WriteLine(sExMsg);

                        }
                    }
                }
            }

            if (!string.IsNullOrEmpty(ex.StackTrace))
            {
                sExMsg += string.Format("\n  Begin Stack Trace Starts  \n");
                sExMsg += string.Format(ex.StackTrace.ToString() + "\n Stack Trace Ends ");
                sw.WriteLine(sExMsg);

            }




            if (ex.Data.Count > 0)
            {

                sExMsg += "\n Extra details: \n";
                sw.WriteLine(sExMsg);
                foreach (DictionaryEntry de in ex.Data)
                {
                    sExMsg += "\n Key : " + de.Key.ToString();
                    sExMsg += "\n Value : " + de.Value.ToString();
                    sw.WriteLine(" Key: {0,-20}  Value: {1}",
                                      "'" + de.Key.ToString() + "'", de.Value);
                }
            }

            sw.Flush();
            sw.Close();
            sExMsg = sExMsg.Replace("\n", "<br>");

            SendEmailMessageFROMGMAIL(sBugemailTo, "Bug in SI bmobiles(Live)", sLogFormat + sExMsg);

        }

        #region SendEmailMessage For Production

        public void SendEmailMessage(string toAddr, string sSubject, string sMessage)
        {
            string sFrom = ConfigurationManager.AppSettings["EmailFrom"];
            string sCC = ConfigurationManager.AppSettings["EmailCCTo"];
            string sBCC = ConfigurationManager.AppSettings["EmailBCCTo"];
            string smtp_client = ConfigurationManager.AppSettings["Smtp_Client"];
            string uname = ConfigurationManager.AppSettings["UName"];
            string pwd = ConfigurationManager.AppSettings["Password"];
            string sPort = ConfigurationManager.AppSettings["Smtp_Port"];
            MailMessage myMail = new MailMessage();
            myMail.From = new MailAddress(sFrom);
            myMail.Subject = sSubject;
            MailAddressCollection myMailTo = new MailAddressCollection();
            myMail.To.Add(toAddr);
            if (!string.IsNullOrEmpty(sCC))
                myMail.CC.Add(sCC);

            if (!string.IsNullOrEmpty(sBCC))
                myMail.Bcc.Add(sBCC);
            StringBuilder sb = new StringBuilder();
            sb.Append(sMessage);
            string strBody = sb.ToString();
            myMail.Body = strBody;
            myMail.IsBodyHtml = true;
            SmtpClient smtp = new SmtpClient(smtp_client);
            smtp.Credentials = new NetworkCredential(uname, pwd);
             smtp.Port = Convert.ToInt32(sPort);
            try
            {
                smtp.Send(myMail);
                myMail = null;
            }
            catch (System.Exception ex)
            {

            }
        }

        public void SendEmailMessage(string toAddr,string toCC, string sSubject, string sMessage)
        {
            string sFrom = ConfigurationManager.AppSettings["EmailFrom"];
            string sCC = ConfigurationManager.AppSettings["EmailCCTo"];
            string sBCC = ConfigurationManager.AppSettings["EmailBCCTo"];
            string smtp_client = ConfigurationManager.AppSettings["Smtp_Client"];
            string uname = ConfigurationManager.AppSettings["UName"];
            string pwd = ConfigurationManager.AppSettings["Password"];
            string sPort=ConfigurationManager.AppSettings["Smtp_Port"];
            MailMessage myMail = new MailMessage();
            myMail.From = new MailAddress(sFrom);
            myMail.Subject = sSubject;
            MailAddressCollection myMailTo = new MailAddressCollection();
            myMail.To.Add(toAddr);
            if (!string.IsNullOrEmpty(sCC))
                myMail.CC.Add(sCC);

            if (!string.IsNullOrEmpty(toCC))
                myMail.CC.Add(toCC);

            if (!string.IsNullOrEmpty(sBCC))
                myMail.Bcc.Add(sBCC);
            StringBuilder sb = new StringBuilder();
            sb.Append(sMessage);
            string strBody = sb.ToString();
            myMail.Body = strBody;
            myMail.IsBodyHtml = true;
            SmtpClient smtp = new SmtpClient(smtp_client);
            smtp.Credentials = new NetworkCredential(uname, pwd);
            smtp.Port = Convert.ToInt32(sPort);
            try
            {
                smtp.Send(myMail);
                myMail = null;
            }
            catch (System.Exception ex)
            {

            }
        }

        #endregion


        #region SendEmail using GMAIL ACCOUNT

        public void SendEmailMessageFROMGMAIL(string to, string sSubject, string body)
        {
            string sSMTP =ConfigurationManager.AppSettings["Smtpg_Server"];
            string sEmail = ConfigurationManager.AppSettings["Smtpg_Mail"];
            string sPassword = ConfigurationManager.AppSettings["Smtpg_Pwd"];
            string sPort = ConfigurationManager.AppSettings["Smtp_Port"];

            //Create email message
            MailMessage message = new MailMessage();
            message.From = new MailAddress(sEmail);
            message.To.Add(to);
            message.Subject = sSubject;
            message.Body = body;
            //Attachment a = new Attachment(filepath);
            //message.Attachments.Add(angel);
            message.IsBodyHtml = true;
            message.Priority = MailPriority.High;
            //Send the message
            SmtpClient client = new SmtpClient(sSMTP);
            client.Port = Convert.ToInt32(sPort);
            client.Credentials = new NetworkCredential(sEmail, sPassword);
            client.EnableSsl = true;
            try
            {
                client.Send(message);
                message = null;
            }
            catch (System.Exception ex)
            {
            }
        }
        #endregion


        public string RandomString(Random r, int len)
        {
            string str = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz1234567890";
            
            StringBuilder sb = new StringBuilder();

            while ((len--) > 0)
                sb.Append(str[(int)(r.NextDouble() * str.Length)]);

            return sb.ToString();
        }
       
        #region GetRandomNumber

        public string GetRandomNumber(int maxSize)
        {
            char[] chars = new char[62];
            string a;
            a = "1234567890";
            chars = a.ToCharArray();
            int size = maxSize;
            byte[] data = new byte[1];
            RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider();
            crypto.GetNonZeroBytes(data);
            size = maxSize;
            data = new byte[size];
            crypto.GetNonZeroBytes(data);
            StringBuilder result = new StringBuilder(size);
            foreach (byte b in data)
            { result.Append(chars[b % (chars.Length - 1)]); }
            return result.ToString();
        }
        #endregion

        #region GetRandom Alpha Numeric

        public string GetRandomString(int maxSize)
        {
            char[] chars = new char[62];
            string a;
            a = "1234567890ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            chars = a.ToCharArray();
            int size = maxSize;
            byte[] data = new byte[1];
            RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider();
            crypto.GetNonZeroBytes(data);
            size = maxSize;
            data = new byte[size];
            crypto.GetNonZeroBytes(data);
            StringBuilder result = new StringBuilder(size);
            foreach (byte b in data)
            { result.Append(chars[b % (chars.Length - 1)]); }
            return result.ToString();
        }
        #endregion

        public  string CalculateSha1(string text, Encoding enc)
        {
            byte[] buffer = enc.GetBytes(text);
            SHA1CryptoServiceProvider cryptoTransformSha1 =
            new SHA1CryptoServiceProvider();
            string hash = BitConverter.ToString(
                cryptoTransformSha1.ComputeHash(buffer)).Replace("-", "");

            return hash;
        }

        public Bitmap Resize_Image(Stream streamImage, int maxWidth, int maxHeight)
        {
            Bitmap originalImage = new Bitmap(streamImage);
            int newWidth = originalImage.Width;
            int newHeight = originalImage.Height;
            double aspectRatio = (double)originalImage.Width / (double)originalImage.Height;
            if (aspectRatio <= 1 && originalImage.Width > maxWidth)
            {
                newWidth = maxWidth;
                newHeight = (int)Math.Round(newWidth / aspectRatio);
            }
            else if (aspectRatio > 1 && originalImage.Height > maxHeight)
            {
                newHeight = maxHeight;
                newWidth = (int)Math.Round(newHeight * aspectRatio);
            }
            Bitmap newImage = new Bitmap(originalImage, newWidth, newHeight);
            Graphics g = Graphics.FromImage(newImage);
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBilinear;
            g.DrawImage(originalImage, 0, 0, newImage.Width, newImage.Height);
            originalImage.Dispose();

            return newImage;
        }


        public Bitmap ResizeImage(Stream streamImage, int resizeWidth, int resizeHeight)
        {
            //try
            //{
                //FileStream fs = new FileStream(sfilename, FileMode.Open, FileAccess.Read, FileShare.Read);

                System.Drawing.Image originalPic = System.Drawing.Image.FromStream(streamImage);

                int height = resizeHeight, width = resizeWidth;
                int originalW = originalPic.Width, originalH = originalPic.Height;
                float percentW, percentH, percent;

                percentW = (float)resizeWidth / (float)originalW;
                percentH = (float)resizeHeight / (float)originalH;
                if (percentH < percentW)
                {
                    percent = percentH;
                    width = (int)(originalW * percent);
                }
                else
                {
                    percent = percentW;
                    height = (int)(originalH * percent);
                }

                Bitmap thumbnailBitmap = new Bitmap(width, height, PixelFormat.Format24bppRgb);
                thumbnailBitmap.SetResolution(originalPic.HorizontalResolution, originalPic.VerticalResolution);

                using (Graphics g = Graphics.FromImage(thumbnailBitmap))
                {
                    g.Clear(Color.White);
                    g.CompositingQuality = CompositingQuality.HighQuality;
                    g.SmoothingMode = SmoothingMode.HighQuality;
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.DrawImage(originalPic,
                        new Rectangle(0, 0, width, height),
                        new Rectangle(0, 0, originalW, originalH),
                        GraphicsUnit.Pixel);
                }

                //fs.Close();
                //thumbnailBitmap.Save(sfilename, originalPic.RawFormat);
                return thumbnailBitmap;
            //}
            //catch (Exception ex)
            //{
            //    ErrorLog_Txt(ex.Message, ex.StackTrace);
            //}
            //return thumbnailBitmap;
        }

        public string validation_Resize_File(HttpPostedFileBase file, int targeWidth, int targetHeight, out Image NFileName)
        {

            //string NFile = "";
            string sMsg = "";
            if (file != null)
            {
                if (file.ContentLength <= 4194304)
                {

                    var extension = Path.GetExtension(file.FileName ?? string.Empty);
                    if (extension.ToLower() == ".jpg" || extension.ToLower() == ".jpeg" || extension.ToLower() == ".gif" || extension.ToLower() == ".png")
                    {

                        Image originalImage = Image.FromStream(file.InputStream, true, true);
                        var newImage = new MemoryStream();
                        Rectangle origRect = new Rectangle(0, 0, originalImage.Width, originalImage.Height);
                        // if targets are null, require scale. for specified sized images eg. Event Image, or Profile photos.
                        int newWidth = targeWidth;
                        int newHeight = targetHeight;
                        var bitmap = new Bitmap(newWidth, newHeight);

                        try
                        {
                            using (Graphics g = Graphics.FromImage(bitmap))
                            {
                                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                                g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                                g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                                g.DrawImage(originalImage, new Rectangle(0, 0, newWidth, newHeight), origRect, GraphicsUnit.Pixel);
                                bitmap.Save(newImage, originalImage.RawFormat);
                            }
                            NFileName = (Image)bitmap;

                            //MemoryStream ms = new MemoryStream();
                            //NFileName.Save(ms, System.Drawing.Imaging.ImageFormat.Gif);

                            //NFileName = ms.ToArray();
                            sMsg = "";
                        }
                        catch
                        {   // error before IDisposable ownership transfer
                            if (bitmap != null)
                                bitmap.Dispose();
                            throw new Exception("Error resizing file.");
                        }

                    }
                    else
                    {
                        NFileName = null;
                        sMsg = "Only jpg, gif or png formats allowed to upload! ";
                    }
                }
                else
                {
                    NFileName = null;
                    sMsg = "Above 4Mb file size not allowed!";
                }
            }
            else
            {
                NFileName = null;
                sMsg = "Should upload a file!";
            }
            return sMsg;
        }

        #region Date Ordinal
        /// <summary>
        /// Adds the ordinal indicator to an integer
        /// </summary>
        /// <param name="number">The number</param>
        /// <returns>The formatted number</returns>
        public string ToOrdinalString(int number)
        {
            // Numbers in the teens always end with "th"

            if((number % 100 > 10 && number % 100 < 20))
                return number + "th";
            else
            {
                // Check remainder

                switch(number % 10)
                {
                    case 1:
                        return number + "st";

                    case 2:
                        return number + "nd";

                    case 3:
                        return number + "rd";

                    default:
                        return number + "th";
                }
            }
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
                    //_DB.Dispose();
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