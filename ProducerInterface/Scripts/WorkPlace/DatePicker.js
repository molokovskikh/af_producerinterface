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
});

function KyUp() {
    var stringText = $('#Annotation').val();
    var count = stringText.length; // кол-во уже введенных символов
    var num = 500 - count; // кол-во символов, которое еще можно ввести
    if (num > 0) {
        // если не достигнут лимит символов         
        $('#counter').text('Количество оставшихся для ввода знаков: ' + num);
    } else {
        // если достигнут лимит символов         
        $('#counter').text('Достигнут лимит символов');
        return false;
    }
}
