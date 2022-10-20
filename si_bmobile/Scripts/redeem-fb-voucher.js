
$(document).ready(function () {

    $('#Redeem_FB_Voucher').dialog({
        autoOpen: false,
        width: 450,
        dialogClass: 'dialog_css',
        resizable: false,
        title: 'Redeem FB Voucher',
        modal: true,
        create: function (event, ui) {
            $(".ui-widget-header").css('font-size', '16px');
            //$(event.target).parent().css('position', 'fixed');
        },
        open: function (event, ui) {
            var url = "/Care/RedeemFBPromotions";
            $(this).load(url);
        }
    });
});

function RedeemFBVoucher() {

    $('#Redeem_FB_Voucher').html('<div style="text-align: center; padding: 50px 0px 50px 0px;"><img src="/Content/themes/base/images/loading.gif" /></div>');

    $('#Redeem_FB_Voucher').dialog('open');
}

$('#btnSubmitRedeemFBVoucher').live("click", function () {

    var msisdn_no = $('.ddl_fb_msisdn').val();
    var fb_voucher = $('#fb_voucher').val();
    $('#span_msisdn').html('');
    $('#span_fbvoucher').html('');
    $('#Div_Msg').html('');

    var res = true;
    if (msisdn_no == '' || msisdn_no == null) {
        $('#span_msisdn').text('Please select Mobile Number');
        res = false;
    }

    if (fb_voucher == '') {
        $('#span_fbvoucher').html('Please enter FB Voucher');
        res = false;
    }

    if (res == false)
        return false;

    $('#divFBProcess').show();
    $('#fb_btn_pan').hide();

    $.ajax({
        url: "/Care/RedeemFBPromotions2",
        data: { fbvoucher: fb_voucher, msisdnno: msisdn_no },
        type: "GET",
        contentType: 'application/json; charset=utf-8',
        success: function (data) {
            if (data.Status == "Success") {
                $('#div_Success_Msg').html('Your voucher of $ ' + data.Amount + ' has been redeemed successfully.');
                $('.fb_popup').hide();
                $('.fb_success').show();
            }
            else if (data.Status == "Fail") {
                $('#Div_Msg').html('Invalid voucher number!');                
            }
            else if (data.Status == "AttemptFail") {
                $('#Div_Msg').html('Invalid voucher number!');
                $('#btnSubmitRedeemFBVoucher').prop('disabled', true);
                $('#btnSubmitRedeemFBVoucher').css('background-color', '#ddd');
            }
            else {
                $('#Div_Msg').html(data.Status);
                
            }

            $('#divFBProcess').hide();
            $('#fb_btn_pan').show();
        },
        error: function (response) {
            //alert("FB Voucher: " + response.d);
            $('#Redeem_FB_Voucher').dialog('close');
            $('#divFBProcess').hide();
        }
    });

});

$('#btnCancelRedeemFBVoucher').live("click", function () {
    $('#Redeem_FB_Voucher').dialog('close');
});

function CloseFBPopup() {
    $('#Redeem_FB_Voucher').dialog('close');
    window.location = '/Care/RefreshACC';
}