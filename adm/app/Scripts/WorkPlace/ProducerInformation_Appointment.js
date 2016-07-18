
function ActivateDeactivete(i) // i=0 deactivate  i=1 activate
{
    var sendParam = "";
    var stringPathName = document.location.pathname;
    stringPathName = stringPathName.replace("AppointmentNameList", "AppointmentEnable");

    var sendParamId = "";

    if (i == 0) {
        sendParamId = $("#active").val();
    }
    if (i == 1) {
        sendParamId = $("#inactive").val();
    }

    sendParam = "Id=" + sendParamId + "&enabled=" + i;

    AjaxPost(stringPathName, sendParam, "replacecontent")
}

function GetListAppointmentNamesThisProducer() {
    var IdProducer = $('#IdSelectProducer').val();
    if (IdProducer != 0) {
        var stringPathName = document.location.pathname;
        stringPathName = stringPathName.replace("AppointmentNameList", "GetAppointmentProducer");
        var sendParam = "Id=" + IdProducer;

        AjaxPost(stringPathName, sendParam, "replacecontent2");
    }
}

function DeleteProducerAppointment() {
    var IdAppointment = $('#producerappointmentlist').val();
    var IdProducer = $('#AccountCompanyId').val();
    var stringPathName = document.location.pathname;
    stringPathName = stringPathName.replace("AppointmentNameList", "DeleteProducerAppointment");
    var sendParam = "Id=" + IdAppointment + "&IdCompany=" + IdProducer;

    AjaxPost(stringPathName, sendParam, "replacecontent2");
}

function AddProducerAppointment() {
    var IdProducer = $('#AccountCompanyId').val();
    var NameAppointment = $('#addProducerAppointment').val();
    var stringPathName = document.location.pathname;
    stringPathName = stringPathName.replace("AppointmentNameList", "AddProducerAppointment");
    var sendParam = "Name=" + NameAppointment + "&Id=" + IdProducer;

    AjaxPost(stringPathName, sendParam, "replacecontent2");
}

function AjaxPost(url_, sendData_, idUpdate) {
    var ElemUpdate = '#' + idUpdate;
    $.ajax({
        url: url_,
        data: sendData_,
        type: "POST",
        success: function (data) {
            $(ElemUpdate).html(data);
        }
    });
}
