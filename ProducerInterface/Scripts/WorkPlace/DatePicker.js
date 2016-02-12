$(function () {
    // http://www.eyecon.ro/bootstrap-datepicker/
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

function UpKey()
{
    var characters = 1000;
    $("#counter").append(characters);
    $("#annotation").keyup(function () {
        if ($(this).val().length > characters) {
            $(this).val($(this).val().substr(0, characters));
        }
        var remaining = characters - $(this).val().length;
        document.getElementById('#annotation').innerHTML = remaining;
     //   $("#counter").innerHTML = remaining.toString();     
        if (remaining <= 999) {
            $("#counter").css("color", "red");
        }
        else {
            $("#counter").css("color", "black");
        }
    });
}

$(function () {
    var characters = 1000;
    $("#counter").append(characters);
    $("#annotation").keyup(function () {
        if ($(this).val().length > characters) {
            $(this).val($(this).val().substr(0, characters));
        }
        var remaining = characters - $(this).val().length;
        $("#counter").html(remaining);
        alert(remaining);
        if (remaining <= 999) {
            $("#counter").css("color", "red");
        }
        else {
            $("#counter").css("color", "black");
        }
    });
});
