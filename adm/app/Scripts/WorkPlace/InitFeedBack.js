$(function () {

    var dateTo = $('#DateEnd').datepicker({
        format: 'dd.mm.yyyy',
        language: 'ru',
        autoclose: true,
        weekStart: 1,
        endDate: '-0d'
    }).data('datepicker');

    var dateFrom = $('#DateBegin').datepicker({
        format: 'dd.mm.yyyy',
        language: 'ru',
        autoclose: true,
        weekStart: 1,
        endDate: '-0d'
    }).on('changeDate', function () {
        dateTo.setStartDate(dateFrom.getDate());
    }).data('datepicker');
});


function getPage(id) {
    $('#CurrentPageIndex').val(id);
    $('#sform').submit();
}

function getSearch() {
    $('#CurrentPageIndex').val('');
    $('#sform').submit();
}