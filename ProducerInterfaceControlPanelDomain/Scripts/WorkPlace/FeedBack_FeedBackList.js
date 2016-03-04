$(function () {
    InitDatePicker();
    ServerGetResult(0);
    ko.applyBindings(FeedBackListModel);
    ko.applyBindings(FilterModel);   
})

var FilterModel = 
    {
        DateBegin: ko.observable(),
        DateEnd: ko.observable(),
        PagerIndex: ko.observable(),
        ProducerId: ko.observable(),
        LoginId: ko.observable(),
        Pager: ko.observable(),
      
        Event_SetProducer: function () /*Событие выбора Производителя*/ {       
            if (FilterModel.ProducerId() != 0) {
                FilterModel.LoginId(0);
                FilterModel.LoginId.valueHasMutated();
            }
        },
        Event_SetAccount: function () /*Событие выбора Пользователя*/ {          
            if (FilterModel.LoginId() != 0) {
                FilterModel.ProducerId(0);
                FilterModel.ProducerId.valueHasMutated();
            }
        },
        Event_Pager_Search: function (data, event) /*Клик по странице*/ {
            var PI = FeedBackListModel.PageIndex();/*Текущая страница пейджера*/
            var PC = data.Counter; /*Страница по которой кликнули*/
            if (PC == PI) {
                return false;
            }
            else {
                FilterModel.PagerIndex(PC);
                ServerGetResult(1);
            }
        },

        Event_ModalSearch: function () /*Событие поиск в модальном окне*/ {
            ServerGetResult(1);
        }
    }

// Модель представления
var FeedBackListModel = {
   /*Отображение фильтра*/
    SortTime: ko.observable(),
    SortProducerName: ko.observable(),
    SortAccountName: ko.observable(),
    SortType: ko.observable(),
    SortStatus: ko.observable(),

    /*Модель отправляемая на сервер для фильтрации*/
         
    /*список обращений*/
    Feedback: ko.observableArray([]),

    /*текущий пайджер*/
    PageIndex: ko.observable(),

    /*максимально возможный пейджер*/
    MaxPageCount: ko.observable(),

    /*массив линков пейджера*/
    PaginatorLinks: ko.observableArray([]),

    /*Список пользователей для выбора в поиске*/
    AccountList: ko.observableArray([]),

    /*Список производителей для выбора в поиске*/
    ProducerList: ko.observableArray([]),
 
    /* список с количеством отображаемых страниц */
    PagerCountList: ko.observableArray([]),
   
    ErrorMessage: ko.observable()    
};

function ServerGetResult(iii)
{
    if (iii == 0) /*модель не заполнена*/
    {
        var Url = "GetTopHundredList";
        AjaxServer(Url, null, iii);
    }
    if (iii == 1)
    {
        var Url2 = "FeedbackFilter";
        var JsonData = ko.toJSON(FilterModel);
        AjaxServer(Url2, JsonData, iii);
    }    
}
    
function AjaxServer(Url_, JsonSendData, Type)
{
    FeedBackListModel.ErrorMessage("Поиск");
    if (Type == 0) {
        var url = Url_; /*'GetTopHundredList';*/
        $.ajax({
            url: url,
            cache: false,
            contentType: 'application/json',
            type: "POST",
            success: function (result) {
                bindDataFeedBack(result);
                bindDataFilter(result);
                FeedBackListModel.ErrorMessage("");
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
                    FeedBackListModel.ErrorMessage.valueHasMutated();                   
                }
                else
                {
                    $('#filterModal').modal('hide');
                    bindDataFeedBack(result);
                    bindDataFilter(result);
                    FeedBackListModel.ErrorMessage("");
                }
            },
            error: function (jqXHR) {
                $('#message').html(jqXHR.statusText);
            }
        });
    }
}

function InitDatePicker()
{
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

formattedDate = function (dateToFormat) {
    var dateObject = new Date(dateToFormat);
    var day = dateObject.getDate();
    var month = dateObject.getMonth() + 1;
    var year = dateObject.getFullYear();
    day = day < 10 ? "0" + day : day;
    month = month < 10 ? "0" + month : month;
    var formattedDate = day + "." + month + "." + year;
    return formattedDate;
}

bindDataFilter = function (ModelResult)
{
    var ModelFeedBackFilter = ModelResult.FeedBackFilter;

    var beginDate = formattedDate(ModelFeedBackFilter.DateBegin);
    var endDate = formattedDate(ModelFeedBackFilter.DateEnd);

    FilterModel.DateBegin(beginDate);
    FilterModel.DateEnd(endDate);
    FilterModel.LoginId(ModelFeedBackFilter.LoginId);
    FilterModel.Pager(ModelFeedBackFilter.Pager);
    FilterModel.ProducerId(ModelFeedBackFilter.ProducerId);
    FilterModel.LoginId(ModelFeedBackFilter.LoginId);

    FilterModel.DateBegin.valueHasMutated();
    FilterModel.DateEnd.valueHasMutated();
    FilterModel.LoginId.valueHasMutated();
    FilterModel.Pager.valueHasMutated();
    FilterModel.ProducerId.valueHasMutated();
    FilterModel.LoginId.valueHasMutated();
}

bindDataFeedBack = function (ModelResult) {

    /* фильтр модель выбора количества страниц */
    FeedBackListModel.PagerCountList(ModelResult.PageCount);
    FeedBackListModel.PagerCountList.valueHasMutated();

    /* список обращений */
    FeedBackListModel.Feedback(ModelResult.FeedBackList);
    FeedBackListModel.Feedback.valueHasMutated();

    /* добаление css  для отображения по каким столбца произведена фильтрация*/
    FeedBackListModel.SortAccountName(ModelResult.SortAccountName);
    FeedBackListModel.SortAccountName.valueHasMutated();
    FeedBackListModel.SortProducerName(ModelResult.SortProducerName);
    FeedBackListModel.SortProducerName.valueHasMutated();
    FeedBackListModel.SortStatus(ModelResult.SortStatus);
    FeedBackListModel.SortStatus.valueHasMutated();
    FeedBackListModel.SortTime(ModelResult.SortTime);
    FeedBackListModel.SortTime.valueHasMutated();
    FeedBackListModel.SortType(ModelResult.SortType);
    FeedBackListModel.SortType.valueHasMutated();

    /* Модель пагинатора */
    FeedBackListModel.PaginatorLinks(ModelResult.PaginatorLinks);
    FeedBackListModel.PaginatorLinks.valueHasMutated();

    /* Текущая страница пагинатора */
    FeedBackListModel.PageIndex(ModelResult.PageIndex);
    FeedBackListModel.PageIndex.valueHasMutated();

    /* максимальное количество страниц */
    FeedBackListModel.MaxPageCount(ModelResult.MaxPageCount);
    FeedBackListModel.MaxPageCount.valueHasMutated();

    /* список для фильтрации количества отображаемых страк на странице пайджера */
    FeedBackListModel.PagerCountList(ModelResult.PageCount);
    FeedBackListModel.PagerCountList.valueHasMutated();

    /* список производителей для фильтра */
    FeedBackListModel.ProducerList(ModelResult.ProducerList);
    FeedBackListModel.ProducerList.valueHasMutated();

    /* список пользователей для фильтра */
    FeedBackListModel.AccountList(ModelResult.AccountList);
    FeedBackListModel.AccountList.valueHasMutated();    
}