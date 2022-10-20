
function ShowAlert(title, msg) {

    $("#alert_dialog").html(msg);
    $("#alert_dialog").dialog({
        title: title,
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

function ShowAlertAndRedirect(title, msg, sURL) {

    $("#alert_dialog").html(msg);
    $("#alert_dialog").dialog({
        title: title,
        buttons: [
            {
                text: "OK",
                click: function () {
                    $(this).dialog("close");
                    window.location.href = sURL;
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

function ShowConfirmAlertAndRedirect(msg, sURL) {

    $("#alert_dialog").html(msg);
    $("#alert_dialog").dialog({
        title: "Confirmation",
        buttons: {
            Continue: function () {               
                $(this).dialog('close');
                window.location.href = sURL;
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

//--------------------------- Activate SIM Begin ---------------------------------------
$(document).ready(function () {   

    $('#lnkActivateSIM').click(function () {

        $('.img_box').hide();
        $('#txtMSISDNNo').val('');
        $('#txtSIMNo').val('');
        $('#txtPUKCode').val('');

        $('#Activate_SIM').dialog({
            autoOpen: true,
            width: 'auto',
            dialogClass: 'dialog_css',
            resizable: false,
            title: 'Activate your SIM',
            closeOnEscape: false,
            draggable: false,
            modal: true,
            create: function (event, ui) {
                $(".ui-widget-header").css('font-size', '16px');            
            },
        });

        //$("#Activate_SIM").parent(".ui-dialog").css('top', '200px');
    });

    $('#btnCloseActivateSIM').click(function () {
        $('#Activate_SIM').dialog('close');
    });

    $('#txtMSISDNNo,#txtSIMNo,#txtPUKCode').keypress(function (e) {
        if (e.which != 8 && e.which != 0 && (e.which < 48 || e.which > 57)) {
           
            return false;
        }
    });

    $('#btnActivateSIM').click(function () {      
       
        var msidn_no = $('#txtMSISDNNo').val();
        var sim_no = $('#txtSIMNo').val();
        var puk_code = $('#txtPUKCode').val();        
        $('#spanMSISDNNo').html('');
        $('#spanSIMNo').html('');
        $('#spanPUKCode').html('');
        
        var res = true;
        if (msidn_no == '') {
            $('#spanMSISDNNo').html('Please enter Mobile Number');
            res = false;
        }

        if (sim_no == '') {
            $('#spanSIMNo').html('Please enter SIM Number');
            res = false;
        }

        if (puk_code == '') {
            $('#spanPUKCode').html('Please enter PUK1 Number');
            res = false;
        }

        if (res == false)
            return false;

        Verify_SIM(msidn_no, sim_no, puk_code);        
    });   
});

function Verify_SIM(msidn_no, sim_no, puk_code) {
    
    $('#divSIMProcess').show();
    $('#SIMbtnPanel').hide();

    $.ajax({
        url: "/Care/VerifySIM",
        data: { msidnno: msidn_no, simno: sim_no, pukcode: puk_code },
        type: "POST",
        success: function (data) {           
            if (data.Status == "0") {
                Activate_SIM(msidn_no, data.RefNo);
            }
            else if (data.Status == "702") {
                ShowActiveSIMAlert('Postpaid Not allowed');
                $('#Activate_SIM').dialog('close');
            }
            else if (data.Status == "703")
                ShowActiveSIMAlert('This service can not be processed when the prepaid subscriber is in the Active state.');
            else if (data.Status == "704")
                ShowActiveSIMAlert('Invalid SIM details');
            else {
                ShowActiveSIMAlert('Invalid SIM details');
            }
            $('#divSIMProcess').hide();
            $('#SIMbtnPanel').show();
        },
        error: function (response) {
            //alert("VerifySIM: " + response.d);
            $('#divSIMProcess').hide();
            $('#SIMbtnPanel').show();
        }
    });
}

function Activate_SIM(msidn_no, ref_no) {

    $('#divSIMProcess').show();
    $('#SIMbtnPanel').hide();

    var msg = 'Are you sure, want to Activate ' + msidn_no;
    $("#alert_dialog").html(msg);
    $("#alert_dialog").dialog({
        title: "Confirmation",
        buttons: {
            Continue: function () {
                $(this).dialog('close');
                
                $.ajax({
                    url: "/Care/MembersActivateSIM",
                    data: { msidnno: msidn_no, refno: ref_no },
                    type: "POST",
                    success: function (data) {
                        if (data.Status == "0") {
                            ShowActiveSIMAlert('SIM activated successfully');

                        }
                        else if (data.Status == "702") {
                            ShowActiveSIMAlert('Postpaid Not allowed');
                            $('#Activate_SIM').dialog('close');
                        }
                        else if (data.Status == "703")
                            ShowActiveSIMAlert('This service can not be processed when the prepaid subscriber is in the Active state.');
                        else if (data.Status == "704")
                            ShowActiveSIMAlert('Invalid SIM details');
                        else {
                            ShowActiveSIMAlert('Invalid SIM details');
                        }
                        $('#divSIMProcess').hide();
                        $('#SIMbtnPanel').show();
                    },
                    error: function (response) {
                        //alert("ActivateSIM: " + response.d);
                        $('#divSIMProcess').hide();
                        $('#SIMbtnPanel').show();
                    }
                });
            },
            Close: function () {
                $(this).dialog('close');
                $('#Activate_SIM').dialog('close');
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
}

function ViewSimImage(sim_det) {

    var img_sim = $('#img_sim');
    var img_box = $('.img_box');
    img_box.show();

    if (sim_det == 'msdn_no') {
        img_sim.attr('src', '/images/sim_demo/bm_sim_msidn_no.jpg');
    }
    else if (sim_det == 'sim_no') {
        img_sim.attr('src', '/images/sim_demo/bm_sim_no.jpg');
    }
    else if (sim_det == 'puk_no') {
        img_sim.attr('src', '/images/sim_demo/bm_sim_puk_code.jpg');
    }
    else {
        img_box.hide();
    }
}

function ShowActiveSIMAlert(msg) {

    $("#alert_dialog").html(msg);
    $("#alert_dialog").dialog({
        title: 'Message',
        buttons: [
            {
                text: "OK",
                click: function () {
                    $(this).dialog("close");
                    $('#Activate_SIM').dialog('close');
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
//--------------------------- Activate SIM End ---------------------------------------

//--------------------------- Credit Me Begin  -----------------------------------------

$(document).ready(function () {

    $('#lnkTransferTo').click(function () {

        $('#txtMSDNNo').val('');
        $('#txtAmt').val('');

        $('#CreditMe').dialog({
            autoOpen: true,
            width: 'auto',
            dialogClass: 'dialog_css',
            resizable: false,
            title: 'Transfer To Others',
            closeOnEscape: false,
            draggable: false,
            modal: true
        });
    });

    $('#btnCloseCreditMe').click(function () {
        $('#CreditMe').dialog('close');
    });

    $('#txtMSDNNo,#txtAmt').keypress(function (e) {
        if (e.which != 8 && e.which != 0 && (e.which < 48 || e.which > 57)) {

            return false;
        }
    });

    $('#btnCreditMe').click(function () {

        var msidn_no = $('#txtMSDNNo').val();
        var amt = $('#txtAmt').val();
        $('#spanMSDNNo').html('');
        $('#spanAmt').html('');

        if (msidn_no == '') {
            $('#spanMSDNNo').html('Please enter Mobile Number');
            return false;
        }

        if (amt == '') {
            $('#spanAmt').html('Please enter Amount');
            return false;
        }

            $('#Conf_box').dialog({
                autoOpen: true,
                width: 'auto',
                dialogClass: 'dialog_css',
                resizable: false,
                title: 'Confirm',
                closeOnEscape: false,
                draggable: false,
                modal: true,
                buttons: {
                    "Yes": function () {
                        $(this).dialog("close");
                        $('#divProcess').show();
                        $.ajax({
                            url: "/Care/CreditMeCheck",
                            data: { msidn_no: msidn_no, amount: amt },
                            type: "POST",
                            success: function (data) {

                                $('#divProcess').hide();

                                if (data.rmsg == "success") {
                                    process_creditme(msidn_no, amt);
                                }
                                else {
                                    ShowCreditMeAlert(data.rmsg);
                                }
                            },
                            error: function (response) {
                                $('#divProcess').hide();
                                alert("CreditMe: " + response.d);
                            }
                        });
                    },
                    "No": function () {
                        $(this).dialog("close");
                    }
                }
            });
           
     });
});
function process_creditme(msidn_no, amt) {

    $.ajax({
        url: "/Care/CreditMeProcess",
        data: { msidn_no: msidn_no, amount: amt },
        type: "POST",
        success: function (data) {
            if (data.rmsg == "0") {
              //  window.location = data.rurl;
                ShowActiveSIMAlert('Successfully Transfer');
            }
            else {
                ShowActiveSIMAlert('Invalid Number!');
            }
            $("#CreditMe").dialog('close');
        },
        error: function (response) {
            $('#CreditMe').hide();
            alert("CreditMe: " + response.d);
        }
    });
}
function ShowCreditMeAlert(msg) {

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

//--------------------------- Credit Me End  -----------------------------------------