$(function () {
  
    var characters = 500;
    $("#counter").append(characters);

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
  //  var element_ = $('#annotation');
  //  var CountVal = element_.val().length;
    


    // counter

}


//    $("#annotation").keyup(function () {

//        var element_ = document.getElementById('annotation');
//        var CountVal = element_.val().trim();
        
//        var txt = $('#annotation').val().trim();
//        var value = $(this).attr('value');
//        var remaining = characters - $('#annotation').innerText.length;
//        document.getElementById('#counter').append(remaining);
//     //   $("#counter").innerHTML = remaining.toString();     
//        if (remaining <= 999) {
//            $("#counter").css("color", "red");
//        }
//        else {
//            $("#counter").css("color", "black");
//        }
//    });
//}
