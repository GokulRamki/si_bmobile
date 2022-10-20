

$(document).ready(function () {

    $('#BuyPowerdialog').dialog({
        autoOpen: false,
        width: 450,
        //height: 150,
        dialogClass: 'dialog_css',
        resizable: false,
        title: 'Get Pawa',
        modal: true,
        create: function (event, ui) {
            $(".ui-widget-header").css('font-size', '16px');
            //$(event.target).parent().css('position', 'fixed');
        },
        open: function (event, ui) {           
            url = "/Care/GetPower";
            $(this).load(url);
        }
    });  
});

$('#lnkBuyPower,#lnkBuyPower2').live("click", function () {
    $('#BuyPowerdialog').dialog('open');
});
$('#lnkBuyPowerPre').live("click", function () {
    $('#BuyPowerdialog').dialog('open');
});

function submitBuyPower() {
    var meter_no = $('#txt_MeterNo').val();
    var amt = $('#txt_Amt').val();
    if (meter_no == '') {
        $('#span_meterno').html('Please enter meter number');
        return false;
    }

    if (amt == '') {
        $('#span_amt').html('Please enter amount');
        return false;
    }

    $('#divGetPowerProcess').show();

    $.ajax({
        url: "/Care/GetPowerCheck",
        data: { meterno: meter_no, amount: amt },
        type: "POST",
        success: function (data) {

            $('#divGetPowerProcess').hide();            

            if (data.rcode == "0") {
                $('#BuyPowerdialog').dialog('close');
                show_confirm(meter_no, amt, data.rmsg);
            }
            else if (data.rcode == "1") {
                ShowGetPowerAlert('This meter number has blocked');
            }
            else if (data.rcode == "-1") {
                ShowGetPowerAlert(data.rmsg);
            }
            else if (data.rcode == "-76") {
                ShowActiveSIMAlert('Decimal places not allowed!');
            }
            else if (data.rcode == "-77") {
                ShowActiveSIMAlert('Invalid Amount!');
            }
            else {
                ShowGetPowerAlert('Invalid meter number!');
            }            
        },
        error: function (response) {
            $('#divGetPowerProcess').hide();
            //alert("BuyPower: " + response.d);            
        }
    });
}

function show_confirm(_meterno, _amount, msg) {

    $("#GetPower_Confirm_msg").html(msg);
    $("#GetPower_Confirm_dialog").dialog({
        title: "Confirmation",
        buttons: {
            Continue : function () {                
                process_meter(_meterno, _amount);
            },
            Close: function () {
                $(this).dialog('close');
            }
        },
        dialogClass: 'dialog_css',
        width: 350,
        autoOpen: true,
        closeOnEscape: false,
        draggable: false,
        resizable: false,
        modal: true
    });
}

function process_meter(meter_no, amt) {

    $("#GetPower_Confirm_img").show();

    $.ajax({
        url: "/Care/GetPowerProcess",
        data: { meterno: meter_no, amount: amt },
        type: "POST",
        success: function (data) {
            if (data.rcode == "0") {
                //ShowGetPowerAlert(data.rmsg);
                window.location = data.rurl;
            }
           
            else {
                ShowActiveSIMAlert('Invalid Meter Number!');
            }

            $('#GetPower_Confirm_img').hide();
            $("#GetPower_Confirm_dialog").dialog('close');
        },
        error: function (response) {
            $('#GetPower_Confirm_img').hide();
            //alert("BuyPower: " + response.d);
        }
    });
}

function closeBuyPower() {
    $('#BuyPowerdialog').dialog('close');
}

function ShowGetPowerAlert(msg) {

    $("#alert_dialog").html(msg);
    $("#alert_dialog").dialog({
        title: 'Message',
        buttons: [
            {
                text: "OK",
                click: function () {
                    $(this).dialog("close");                    
                }
            }
        ],
        dialogClass: 'dialog_css',
        width: 350,
        autoOpen: true,
        closeOnEscape: false,
        draggable: false,
        resizable: false,
        modal: true
    });
}