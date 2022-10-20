executeOnServer = function (model, url) {
    $.ajax({
        url: url,
        async: false,
        type: 'POST',
        data: ko.mapping.toJSON(model),
        dataType: "json",
        contentType: "application/json; charset=utf-8",
        success: function (data) {
            ko.mapping.fromJS(data, model);
            //alert(JSON.stringify(data));
        },
        error: function (error) {
            // alert(error.responseText);
        }
    });

};