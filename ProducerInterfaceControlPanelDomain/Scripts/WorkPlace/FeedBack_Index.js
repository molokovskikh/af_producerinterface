$(function () {
    InitDatePicker();
    ServerGetResult(0);

    var elem1 = document.getElementById("FeedBackForm");
    var elem2 = document.getElementById("filterModal")
    var elem3 = document.getElementById("FeedBackModal");
    
    ko.applyBindings(FeedBackListModel, elem1);  /* запуск отслеживания изменений в модели обращений пользователей */ 
    ko.applyBindings(FilterModel, elem2); /* запуск отслеживания изменений в модели фильтра */
    ko.applyBindings(FeedBackOneModel, elem3); /* запуск отслеживания изменений в модели одного обращения пользователя */
})

/* инициализация датепикеров */
function InitDatePicker() {
    var dateFrom = $('#DateBeginer').datepicker({
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

    var dateTo = $('#DateEnder').datepicker({
        format: 'dd.mm.yyyy',
        autoclose: true,
        language: 'ru',
        weekStart: 1,
        onRender: function (date) {
            return date.valueOf() > Date.now() ? 'disabled' : '';
        }
    }).data('datepicker');
}

function ServerGetResult(iii)
{
    if (iii == 0) /*модель не заполнена*/
    {
        var Url = "GetTopHundredList";
        AjaxServer(Url, null, iii);
    }
    if (iii == 1)
    {
        var Url = "FeedbackFilter";
        var JsonData = ko.toJSON(FilterModel);
        AjaxServer(Url, JsonData, iii);
    }    
}
 
/* отправка запросов серверу */
function AjaxServer(Url_, JsonSendData, Type)
{
    FeedBackListModel.MessageView("block");
    FeedBackListModel.ErrorMessage("Ожидайте");
    FilterModel.ErrorMessage("Ожидайте");
    if (Type == 0) {
        var url = Url_; /*'GetTopHundredList';*/
        $.ajax({
            url: url,
            cache: false,
            contentType: 'application/json',
            type: "POST",
            success: function (result) {
                bindJsonToModel(result, 0);
                FeedBackListModel.ErrorMessage("");
                FilterModel.ErrorMessage("");
                FeedBackListModel.MessageView("none");
            },
            error: function (jqXHR) {
                $('#message').html(jqXHR.statusText);
            }
        });
    }
    else
    {             
        $.ajax({
            url: Url_, /* FeedbackFilter */
            cache: false,
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            data: JsonSendData,
            type: "POST",
            success: function (result) {
                if (result == "0")
                {                    
                    FeedBackListModel.ErrorMessage("Список результатов пуст");
                    FilterModel.ErrorMessage("Список результатов пуст");
                    FeedBackListModel.ErrorMessage.valueHasMutated();                   
                }
                else
                {
                    $('#filterModal').modal('hide');
                    bindJsonToModel(result, 1);
                 //   bindDataFilter(result);
                    FeedBackListModel.ErrorMessage("");
                    FilterModel.ErrorMessage("");
                    FeedBackListModel.MessageView("none");
                }
            },
            error: function (jqXHR) {
                $('#message').html(jqXHR.statusText);
            }
        });
    }
}
