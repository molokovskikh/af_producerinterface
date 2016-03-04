$(function () {
    $('#Producer').chosen({ width: '100%' });

    var dateTo = $('#RunTo').datepicker({
        format: 'dd.mm.yyyy',
        language: 'ru',
        autoclose: true,
        weekStart: 1,
        endDate: '-0d'
    }).data('datepicker');

    var dateFrom = $('#RunFrom').datepicker({
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