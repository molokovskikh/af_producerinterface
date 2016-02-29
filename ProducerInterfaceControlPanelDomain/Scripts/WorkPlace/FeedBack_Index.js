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

$(document).ready()
{
    $('#SearchWait').css('display', 'none');
    ViewProducer();
}

function ViewProducer() {

    var inputElements = document.getElementById('Producer');

    if (inputElements.checked) {
        $('#ProducerSelectView').css("display", "block");
    }
    else {
        $('#ProducerSelectView').css("display", "none");
    }
}

function ViewDate() {

    var inputElements = document.getElementById('DateTimeApply');

    if (inputElements.checked) {
        $('#DateSelectView').css("display", "block");
    }
    else {
        $('#DateSelectView').css("display", "none");
    }
}

function SearchWait(search_or_not) // 1-search begin / 2-success search
{
    if (search_or_not == 1) {
        $('#FeedBackSearchResult').css('display', 'none');
        $('#SearchWait').css('display', 'block');
    }
    else
    {
        $('#FeedBackSearchResult').css('display', 'block');
        $('#SearchWait').css('display', 'none');
    }
}