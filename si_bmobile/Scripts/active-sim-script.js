

//--------------------------- Activate SIM Begin ---------------------------------------
$(document).ready(function () {

    $('#msidn_number,#sim_number,#puk_code').keypress(function (e) {
        if (e.which != 8 && e.which != 0 && (e.which < 48 || e.which > 57)) {
           
            return false;
        }
    });

    $('#btnVerify_sim').click(function () {

        var msidn_no = $('#msidn_number').val();
        var sim_no = $('#sim_number').val();
        var puk_code = $('#puk_code').val();
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

        VerifySIM(msidn_no, sim_no, puk_code);        
    });

    $('#btnActive_sim').click(function () {

        var fname = $('#first_name').val();
        var lname = $('#last_name').val();
        var email = $('#email').val();
        var address1 = $('#address1').val();
        $('#spanFName').html('');
        $('#spanLName').html('');
        $('#spanEmail').html('');
        $('#spanAddress1').html('');

        var res = true;
        if (fname == '') {
            $('#spanFName').html('Please enter First Name');
            res = false;
        }

        if (lname == '') {
            $('#spanLName').html('Please enter Last Name');
            res = false;
        }

        if (email == '') {
            $('#spanEmail').html('Please enter E-mail');
            res = false;
        }

        if (address1 == '') {
            $('#spanAddress1').html('Please enterAddress1');
            res = false;
        }

        if (res == false)
            return false;
      
        var active_sim =
           {
               "sim_number": $('#sim_number').val(),
               "msidn_number": $('#msidn_number').val(),
               "puk_code": $('#puk_code').val(),
               "ref_number": $('#ref_number').val(),
               "first_name": fname,
               "last_name": lname,
               "email": email,
               "address1": address1,
               "address2": $('#address2').val(),
               "driving_lic": $('#driving_lic').val(),
               "status": $('#status').val()
           };

        $('#div_active_process').show();

        $.ajax({
            url: "/Care/ActivateSIM",
            data: JSON.stringify(active_sim),
            type: "POST",
            contentType: 'application/json; charset=utf-8',
            success: function (data) {
                if (data.Status == "0") {
                    Activatesuccess();
                }
                else if (data.Status == "702")
                    ShowActiveSIMAlert('Postpaid Not allowed');
                else if (data.Status == "703")
                    ShowActiveSIMAlert('This service can not be processed when the prepaid subscriber is in the Active state.');
                else if (data.Status == "704")
                    ShowActiveSIMAlert('Invalid SIM details');
                else {
                    ShowActiveSIMAlert('Invalid SIM details');
                }

                $('#div_active_process').hide();
            },
            error: function (response) {
                alert("ActivateSIM: " + response.d);
                $('#div_active_process').hide();
            }
        });        

    });
});

function VerifySIM(msidn_no, sim_no, puk_code) {
    $('#div_verify_process').show();
    $.ajax({
        url: "/Care/VerifySIM",
        data: { msidnno: msidn_no, simno: sim_no, pukcode: puk_code },
        type: "POST",
        success: function (data) {
            if (data.Status == "0") {
                ActivateSIM(msidn_no,sim_no, data.RefNo);
            }
            else if (data.Status == "702")
                ShowActiveSIMAlert('Postpaid Not allowed');
            else if (data.Status == "703")
                ShowActiveSIMAlert('This service can not be processed when the prepaid subscriber is in the Active state.');
            else if (data.Status == "704")
                ShowActiveSIMAlert('Invalid SIM details');
            else {
                ShowActiveSIMAlert('Invalid SIM details');
            }
            $('#div_verify_process').hide();
        },
        error: function (response) {
            alert("VerifySIM: " + response.d);
            $('#div_verify_process').hide();
        }
    });
}

function ActivateSIM(msidn_no,sim_no, ref_no) {

    $('#div_sim_info').hide();
    $('#img_sim_info').attr('src', '/Images/plus_icon_20x20.png');
    var div_header1 = $('#img_sim_info').parents('.div-header')
    div_header1.addClass("div-header act");

    $('#div_sim_contact').show();
    $('#img_sim_contact').attr('src', '/Images/minus_icon_20x20.png');
    var div_header2 = $('#img_sim_contact').parents('.div-header')
    div_header2.removeClass("act");

    $('#span_sim_number').html(sim_no);   
    $('#ref_number').val(ref_no);
}

function Activatesuccess() {

    //$('#div_sim_contact').hide();
    //$('#img_sim_contact').attr('src', '/Images/plus_icon_20x20.png');
    //var div_header2 = $('#img_sim_contact').parents('.div-header')
    //div_header2.addClass("div-header act");

    $('.div-pan').hide();

    $('#success_msg_box').show();
}

function DisplaySimImg(sim_det) {

    var sim_img = $('#sim_img');
    sim_img.show();

    if (sim_det == 'msdn_no') {
        sim_img.attr('src', '/images/sim_demo/bm_sim_msidn_no.jpg');
    }
    else if (sim_det == 'sim_no') {
        sim_img.attr('src', '/images/sim_demo/bm_sim_no.jpg');
    }
    else if (sim_det == 'puk_no') {
        sim_img.attr('src', '/images/sim_demo/bm_sim_puk_code.jpg');
    }
    else {
        sim_img.hide();
    }
}

//function ShowActiveSIMAlert(msg) {

//    $("#alert_dialog").html(msg);
//    $("#alert_dialog").dialog({
//        title: 'Message',
//        buttons: [
//            {
//                text: "OK",
//                click: function () {
//                    $(this).dialog("close");
//                    $('#Activate_SIM').dialog('close');
//                }
//            }
//        ],
//        dialogClass: 'dialog_css',
//        width: 350,
//        autoOpen: true,
//        closeOnEscape: false,
//        draggable: false,
//        resizable: false,
//        modal: true
//    });
//}
//--------------------------- Activate SIM End ---------------------------------------