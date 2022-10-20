$(document).ready(function () {

});

function VoiceCallActDeact() {
    //$('#VoiceCall_ActDeactPopup').dialog('open');
    $('#VoiceCall_ActDeactPopup').html('<div style="text-align: center; padding: 50px 0px 50px 0px;"><img src="/Content/themes/base/images/loading.gif" /></div>');

    $('#VoiceCall_ActDeactPopup').dialog({
        autoOpen: true,
        width: 450,
        dialogClass: 'dialog_css',
        resizable: false,
        title: 'Voice Mail Call Activation/Deactivation',
        modal: true,
        create: function (event, ui) {
            $(".ui-widget-header").css('font-size', '16px');
            //$(event.target).parent().css('position', 'fixed');
        },
        open: function (event, ui) {
            var url = "/Care/GetVoiceCallActDeact";
            $(this).load(url);
        }
    });
}

$('#btnVCActivation').live("click", function () {
    var msisdn_no = $('.ddl_vc_msisdn').val();
    $('#span_msisdn').html('');
    
    if (msisdn_no == '' || msisdn_no == null) {
        $('#span_msisdn').text('Please select Mobile Number');       
        return false;
    }
    else {       
        vc_act_deact('act', msisdn_no);
    }    
});

$('#btnVCDeactivation').live("click", function () {
    var msisdn_no = $('.ddl_vc_msisdn').val();
    $('#span_msisdn').html('');

    if (msisdn_no == '') {
        $('#span_msisdn').text('Please select Mobile Number');
        return false;
    }
    else {
        vc_act_deact('deact', msisdn_no);
    }
});

function vc_act_deact(type, msisdn_no) {         

    $('#divVoiceCallProcess').show();
    $('#vc_btn_pan').hide();

    $.ajax({
        url: "/Care/VoiceCallActDeact",
        data: { msisdnno: msisdn_no, vc_type: type },
        type: "GET",
        contentType: 'application/json; charset=utf-8',
        success: function (data) {
            if (data.Status == "0") {
                if (type == 'act') {
                    $('#div_VC_Success_Msg').html('Voice mail call activated successfully');
                }
                else if (type == 'deact') {
                    $('#div_VC_Success_Msg').html('Voice mail call deactivated successfully');
                }

                $('.vc_popup').hide();
                $('#div_vc_success').show();
            }
            else {
                if (type == 'act') {
                    $('#Div_VC_Msg').html('Voice mail call already activated');
                }
                else if (type == 'deact') {
                    $('#Div_VC_Msg').html('Voice mail call already deactivated');
                }
            }

            $('#divVoiceCallProcess').hide();
            $('#vc_btn_pan').show();
        },
        error: function (response) {
            //alert("Voice call: " + response.d);
            $('#VoiceCall_ActDeactPopup').dialog('close');
        }
    });
}

$('#btnVCClose,#btnVCClose2').live("click", function () {
    $('#VoiceCall_ActDeactPopup').dialog('close');
});

