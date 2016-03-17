$(function () {
    var dateFrom = $('#Begin').datepicker({
        format: 'dd.mm.yyyy',
        language: 'ru',
        weekStart: 1,
        autoclose: true,
        onRender: function (date) {
            return date.valueOf() > Date.now() ? 'disabled' : '';
        }
    }).on('changeDate', function (ev) {
        //$('#DateToUI')[0].focus();
    }).data('datepicker');

    var dateTo = $('#End').datepicker({
        format: 'dd.mm.yyyy',
        autoclose: true,
        language: 'ru',
        weekStart: 1,
        onRender: function (date) {
            //return date.valueOf() <= dateFrom.date.valueOf() ? 'disabled' : '';
            return date.valueOf() > Date.now() ? 'disabled' : '';
        }
    }).data('datepicker');

    ShowOrHideDateTime();
});

function LoadingResult()
{
    $('#loadingdata').css('display', 'block');
    $('#replacecontent').css('display', 'none');
}

function LoadingResultCompleted()
{
    $('#loadingdata').css('display', 'none');
    $('#replacecontent').css('display', 'block');

    $('#FilterModal').modal('hide');
}

function ShowOrHideDateTime() {
    var inputElements = document.getElementById('EnabledDateTime');

    if (inputElements.checked) {
        $('#datepickers').css("display", "none");
    }
    else {
        $('#datepickers').css("display", "block");
    }
}