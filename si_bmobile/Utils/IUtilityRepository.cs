using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace si_bmobile.Utils
{
    interface IUtilityRepository : IDisposable
    {

        string AES_ENC(string _input);

        string AES_DEC(string _input);

        string AES_ENCWR(string _input);
        string AES_DECWR(string _input);

        string AES_JENC(string _input);

        string AES_JDEC(string _input);

        void ErrorLog_Txt(string sErrMsg, string stackTrace);
        void ErrorLog_Txt(Exception ex);

        void SendEmailMessage(string toAddr, string sSubject, string sMessage);

        void SendEmailMessage(string toAddr, string toCC, string sSubject, string sMessage);

        void SendEmailMessageFROMGMAIL(string to, string sSubject, string body);



        string ReplaceStr(string sString);

        string GetRandomNumber(int maxSize);

        string GetRandomString(int maxSize);

        string CalculateSha1(string text, Encoding enc);

        Bitmap Resize_Image(Stream streamImage, int maxWidth, int maxHeight);

        Bitmap ResizeImage(Stream streamImage, int resizeWidth, int resizeHeight);

        string validation_Resize_File(HttpPostedFileBase file, int targeWidth, int targetHeight, out Image NFileName);

        string ToOrdinalString(int number);

    }
}
