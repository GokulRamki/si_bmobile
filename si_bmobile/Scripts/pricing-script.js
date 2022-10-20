
$(function () {

    LoadVoiceSlider();
    LoadSMSSlider();
    LoadDataSlider();

    var voicevalue = $("#voice_denom_id").val();
    var smsvalue = $("#sms_denom_id").val();
    var datavalue = $("#data_denom_id").val();

    SetSliderValue(voicevalue, smsvalue, datavalue);
    GetPlanPrice();

    $(".alert-message").delegate("a.close", "click", function (event) {
        event.preventDefault();
        $(this).closest(".alert-message").fadeOut(function (event) {
            $(this).remove();
        });
    });

    $('.cell').removeClass('cell-act');
    $('#lnkPopular').parents('.cell').addClass('cell-act');
    $('#div_mobile img').removeClass('bg')
    $('#div_mobile span').removeClass('bg')
});

$(document).ajaxStart(function () {
    $("#div_loading").css("display", "block");
});
$(document).ajaxComplete(function () {
    $("#div_loading").css("display", "none");
});

// voice slider
function LoadVoiceSlider() {

    var ddlvoice = $("#ddlvoice");
    var length = $('#ddlvoice option').length;

    var sliderVoice = $("#sliderVoice").slider({
        min: 0,
        max: length - 1,
        step: 1,
        range: "min",
        value: ddlvoice[0].selectedIndex,
        slide: function (event, ui) {
            ddlvoice[0].selectedIndex = ui.value;
            GetPlanPrice();
        }
    });
    $("#ddlvoice").change(function () {
        sliderVoice.slider("value", this.selectedIndex);
    });

    var labellist = [];
    $("#ddlvoice option").each(function () {
        labellist.push($(this).text());
    });

    sliderVoice.slider("pips", {
        rest: 'label',
        labels: labellist
    });
}

// SMS slider
function LoadSMSSlider() {

    var ddlSMS = $("#ddlSMS");
    var length = $('#ddlSMS option').length;
    var sliderSMS = $("#sliderSMS").slider({
        min: 0,
        max: length - 1,
        step: 1,
        range: "min",
        value: ddlSMS[0].selectedIndex,
        slide: function (event, ui) {
            ddlSMS[0].selectedIndex = ui.value;
            GetPlanPrice();
        }
    });

    $("#ddlSMS").change(function () {
        sliderSMS.slider("value", this.selectedIndex);
    });

    var SMSlabellist = [];
    $("#ddlSMS option").each(function () {
        SMSlabellist.push($(this).text());
    });

    sliderSMS.slider("pips", {
        rest: 'label',
        labels: SMSlabellist
    });
}

// Data slider
function LoadDataSlider() {
    var ddlData = $("#ddlData");
    var length = $('#ddlData option').length;
    var sliderData = $("#sliderData").slider({
        min: 0,
        max: length - 1,
        step: 1,
        range: "min",
        value: ddlData[0].selectedIndex,
        slide: function (event, ui) {
            ddlData[0].selectedIndex = ui.value;
            GetPlanPrice();
        }
    });

    $("#ddlData").change(function () {
        sliderData.slider("value", this.selectedIndex);
    });

    var Datalabellist = [];
    $("#ddlData option").each(function () {
        Datalabellist.push($(this).text());
    });

    sliderData.slider("pips", {
        rest: 'label',
        labels: Datalabellist
    });
}

//get price 
function GetPlanPrice() {

    var voice_denom_id = $("#ddlvoice option:selected").val();
    var sms_denom_id = $("#ddlSMS option:selected").val();
    var data_denom_id = $("#ddlData option:selected").val();

    $("#voice_denom_id").val(voice_denom_id);
    $("#sms_denom_id").val(sms_denom_id);
    $("#data_denom_id").val(data_denom_id);

    if (voice_denom_id == '')
        voice_denom_id = '0';

    if (sms_denom_id == '')
        sms_denom_id = '0';

    if (data_denom_id == '')
        data_denom_id = '0';

    $.ajax({
        url: '@Url.Action("GetPlanPrice", "Care")',
        //url: "/Care/GetPlanPrice",
        data: { VoiceDenomId: voice_denom_id, SMSDenomId: sms_denom_id, DataDenomId: data_denom_id },
        type: "POST",
        success: function (data) {

            $("#tot_amt").val(data.Status);

            $('.span-price').html(data.Status);
        },
        error: function (response) {
            //alert("PlanPrice: " + response.d);
        }
    });
}

$('#lnkPopular').click(function () {

    $('.cell').removeClass('cell-act');
    $(this).parents('.cell').addClass('cell-act');

    SetSliderValue(1, 2, 1);
    GetPlanPrice();
});

$('#lnkBigTalker').click(function () {

    $('.cell').removeClass('cell-act');
    $(this).parents('.cell').addClass('cell-act');

    SetSliderValue(3, 1, 1);
    GetPlanPrice();
});

$('#lnkBigTxter').click(function () {

    $('.cell').removeClass('cell-act');
    $(this).parents('.cell').addClass('cell-act');

    SetSliderValue(1, 3, 1);
    GetPlanPrice();
});

$('#div_mobile').click(function () {

    $('.div-pn').show();
    $('.tabs-head').show();
    $('#div_mobile img').removeClass('bg')
    $('#div_tablet img').addClass('bg')
    $('#div_mobile span').removeClass('bg')
    $('#div_tablet span').addClass('bg')

    var voice_denom_id = $("#ddlvoice option:selected").val();
    var sms_denom_id = $("#ddlSMS option:selected").val();
    var data_denom_id = $("#ddlData option:selected").val();

    SetSliderValue(voicevalue, smsvalue, datavalue);
    GetPlanPrice();

});

$('#div_tablet').click(function () {

    $('#div_mobile img').addClass('bg')
    $('#div_tablet img').removeClass('bg')
    $('#div_mobile span').addClass('bg')
    $('#div_tablet span').removeClass('bg')

    $('.div-pn').hide();
    $('.tabs-head').hide();
    $('#div_data_pan').show();

    SetSliderValue(0, 0, 4);
    GetPlanPrice();
});

function SetSliderValue(voice_index, sms_index, data_index) {

    var ddlvoice = $("#ddlvoice");
    ddlvoice[0].selectedIndex = voice_index;
    $("#sliderVoice").slider("value", ddlvoice[0].selectedIndex);

    var ddlSMS = $("#ddlSMS");
    ddlSMS[0].selectedIndex = sms_index;
    $("#sliderSMS").slider("value", ddlSMS[0].selectedIndex);

    var ddlData = $("#ddlData");
    ddlData[0].selectedIndex = data_index;
    $("#sliderData").slider("value", ddlData[0].selectedIndex);

    $(".alert-message").hide();
}

function PurchasePlan() {

    var voice_denom_id = $("#ddlvoice option:selected").val();
    var sms_denom_id = $("#ddlSMS option:selected").val();
    var data_denom_id = $("#ddlData option:selected").val();
    var total = $(".span-price").text();
    var from_msisdn_id = $("#from_msisdn").val();
    var purchase_msisdn_id = $("#purchase_msisdn").val();


    if (voice_denom_id == '')
        voice_denom_id = '0';

    if (sms_denom_id == '')
        sms_denom_id = '0';

    if (data_denom_id == '')
        data_denom_id = '0';

    if (total == '')
        total = '0';

    $.ajax({
        url: "/Care/PurchasePlan",
        data: { VoiceDenomId: voice_denom_id, SMSDenomId: sms_denom_id, DataDenomId: data_denom_id, Total: total, FromMsisdn: from_msisdn_id, PurchaseMsisdn: purchase_msisdn_id },
        type: "POST",
        success: function (data) {
            if (data.Status == '0')
                show_success_notification('Plan purchased successfully');
            else if (data.Status == '111')
                window.location.href = "login";
            else if (data.Status == '1')
                show_warning_notification('Falied to purchase!');
            else if (data.Status == '951')
                show_warning_notification('Plan denomination not found!');
            else
                show_warning_notification('Falied to purchase!');
        },
        error: function (response) {
            alert("PurchasePlan: " + response.d);
        }
    });
}

function show_success_notification(msg) {
    $("#success_msg").html(msg);
    $(".alert-message.success").show();
    $(".plans_pan").hide();
}

function show_warning_notification(msg) {
    $("#warning_msg").html(msg);
    $(".alert-message.warning").show();
}