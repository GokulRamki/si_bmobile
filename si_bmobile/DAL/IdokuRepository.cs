using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using bemobile.Models;
namespace bemobile.DAL
{
    interface IdokuRepository:  IDisposable
    {
        long SaveSelfcareOrder(Payment_EntModel oTP);
        long SaveDokuSelfcareTrans(Payment_EntModel oTP, long order_id, int site_id);

        string validate_merchant(string msisdn, string amount);
        string recharge_msisdn(string msisdn, string amount, string data);

        string bmpng_validate_merchant(string msisdn, string amount);
        string bmpng_recharge_msisdn(string msisdn, string amount, string data);

        List<DokuCareModel> GetDokuOrderTransaction();
        DokuCareModel GetDokuOrder(long OrderId);
        List<DokuCareModel> GetCUGDokuOrderTransaction();
        DokuCareModel GetDokuOrderCUG(long OrderId);

        List<DokuShopModel> GetDokuShopOrderTransaction();
        DokuShopModel GetShopDokuOrder(long OrderId);

        List<DokuCareModel> GetSIDokuOrderTransaction();
        DokuCareModel GetDokuOrderSI(long OrderId);

        long SaveBYPOrder(Payment_EntModel oTP);
		
		string RedeemFBPromotion_GetVoucher(RedeemFBPromotionsModel RedeemFB);

        string SendSMS(string sMSISDN, string sMessage);
    }
}
