$(document).on("dialogopen", ".ui-dialog", function (event, ui) {
    $('.main').css('position', 'static');   
});

//// on window resize run function
//$(window).resize(function () {
//    fluidDialog();
//});

//// catch dialog if opened within a viewport smaller than the dialog width
//$(document).on("dialogopen", ".ui-dialog", function (event, ui) {
   
//});

// --------------------------- AddMsisdndialog -------------------------------------

$(document).ready(function () {

    //$(function () {
    //    setTimeout(function () {
    //        $(".alert-success").fadeOut(function (event) {
    //            $(".alert-success").remove();
    //        });
    //    }, 1500)
    //});

    $('#AddMsisdndialog').dialog({
        autoOpen: false,
        width: 400,        
        dialogClass: 'dialog_css',
        resizable: false,
        title: 'ADD ADDITIONAL MOBILE NUMBER',
        modal: true,
        autoReposition: true,
        create: function (event, ui) {
            $(".ui-widget-header").css('font-size', '16px');
            //$(event.target).parent().css('position', 'fixed');           
        },
        open: function (event, ui) {
            var p_id = $(this).data('id');
            url = "/Care/AddMsisdn";
            $(this).load(url);
        }
    });


    //Search

});

function ViewAddMSISDN() {
    $("#AddMsisdndialog").dialog('open');
}
function clmsisdn() {
    $("#AddMsisdndialog").dialog('close');
}
function clvcode() {
    $("#VerifyMsisdndialog").dialog('close');
}

// --------------------------- VerifyMsisdndialog -------------------------------------

$(document).ready(function () {

    $('#VerifyMsisdndialog').dialog({
        autoOpen: false,
        width: 400,
        height: 200,
        dialogClass: 'dialog_css',
        resizable: false,
        title: 'CODE VERIFICATION',
        modal: true,
        create: function (event, ui) {
            $(".ui-widget-header").css('font-size', '16px');
            $(event.target).parent().css('position', 'fixed');
        },
        open: function (event, ui) {
            var p_id = $(this).data('id');
            url = "/Care/VerifyMsisdn";
            $(this).load(url);
        }
    });

});


function ViewVCode() {
    $("#AddMsisdndialog").dialog('close');
    $("#VerifyMsisdndialog").dialog('open');
}

// --------------------------- others -------------------------------------

function DisplayProgress() {
    $("#divMProcess").css("display", "block");
    $("#divMSubmit").css("display", "none");
}

function HideProgressPanel() {
    $("#divMProcess").css("display", "none");
    $("#divMSubmit").css("display", "block");
}

function SendVerif() {
    DisplayProgress();
    var msdnno = $("#txtMSISDN").val();
    var description = $("#txtDescription").val();
    $.ajax({
        url: "/Care/SendSMSVerficationCode",
        data: { sMsisdn: msdnno },
        type: "POST",
        success: function (response) {
            HideProgressPanel();
            if (response == "116")
                ShowAlert('Message', 'Postpaid number not allowed!');
            else if (response == "1")
                ShowAlert('Message', 'Failed to Process!');
            else if (response == "114")
                ShowAlert('Message', 'Failed to send verification code!');
            else if (response == "0")
                ViewVCode();
            else if (response == "902")
                ShowAlert('Message', 'Mobile Number Already Exists!');
            else {
                $("#AddMsisdndialog").dialog('close');
                ShowAlert('Message', 'Failed to Process!');
            }
        },
        error: function (response) {
            alert("SendVerification: " + response.d);
        }
    });
}

//AddMultiSIM(string sMsisdn,string sVCode)

function DisplayProgressV() {
    $("#divVProcess").css("display", "block");
    $("#divVSubmit").css("display", "none");
}

function HideProgressPanelV() {
    $("#divVProcess").css("display", "none");
    $("#divVSubmit").css("display", "block");
}

function Multisim() {
    DisplayProgressV();
    var msdnno = "677" + $("#txtMSISDN").val();
    var vcode = $("#txtVCode").val();
    var description = $("#txtDescription").val();

    $.ajax({
        url: "/Care/AddMultiSIM",
        data: { sMsisdn: msdnno, sVCode: vcode, sDesc: description },
        type: "POST",
        success: function (response) {
            HideProgressPanelV();

            var result = $.parseJSON(response);

            if (result.rcode == "116")
                ShowAlert('Message', 'Postpaid number not allowed!');
            else if (result.rcode == "901")
                ShowAlert('Message', 'Invalid Request!');
            else if (result.rcode == "903")
                ShowAlert('Message', 'Invalid Verification Code!');
            else if (result.rcode == "904")
                ShowAlert('Message', 'Verification Code Expired!');
            else if (result.rcode == "0") {
                $("#VerifyMsisdndialog").dialog('close');
                var surl = "/Care/" + result.rURL;
                ShowAlertAndRedirect('Message', 'Mobile Number Successfully Added', surl);

            }
            else {
                ShowAlert('Message', 'Failed to Proceed!');
                $("#VerifyMsisdndialog").dialog('close');
            }
        },
        error: function (response) {
            //alert("MultiSim: " + response.d);
        }
    });
}

function RemoveSim(msdnno) {
    var msg = 'Are you sure, want to Remove ' + msdnno;

    $("#alert_dialog").html(msg);
    $("#alert_dialog").dialog({
        title: "Confirmation",
        buttons: {
            Continue: function () {
                $(this).dialog('close');
                $.ajax({
                    url: "/Care/RemoveSIM",
                    data: { sMsisdn: msdnno },
                    type: "POST",
                    success: function (response) {
                        var result = $.parseJSON(response);

                        if (result.rcode == "905")
                            ShowAlert('Message', 'Cannot delete primary number!');
                        else if (result.rcode == "1")
                            ShowAlert('Message', 'Process Failed!');
                        else if (result.rcode == "0") {
                            var msg = msdnno + " Mobile Number Successfully Removed";
                            var surl = "/Care/" + result.rURL;
                            ShowAlertAndRedirect('Message', msg, surl);
                        }
                        else {
                            ShowAlert('Message', 'Failed to Proceed!');
                        }
                    },
                    error: function (response) {
                        alert("RemoveSim: " + response.d);
                    }
                });
            },
            Close: function () {
                $(this).dialog('close');
                return false;
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

    return false;
}