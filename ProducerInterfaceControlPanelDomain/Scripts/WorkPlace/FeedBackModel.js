$(function() {
    // инициализация датепикеров
    InitDatePicker();
    var elem1 = document.getElementById("FeedBackListForm");
    var elem2 = document.getElementById("FeedBackPaginationForm");
    var elem3 = document.getElementById("ModalFilterForm");
    var elem4 = document.getElementById("ModalFeedBackItem");

    // отслеживание изменений в модели списка обращений пользователей
    ko.applyBindings(FeedBackList, elem1);
    // отслеживание изменений в модели пагинатора
    ko.applyBindings(FeedBackPagination, elem2);
    // отслеживание изменений в модели фильтра
    ko.applyBindings(FeedBackFilter, elem3);
    // отслеживаниt изменений в модели выбранного обращения
    ko.applyBindings(FeedBackItem, elem4);

    AjaxGetFilter();
});

// модель фильтра
var FeedBackFilter =
{
    DateBegin: ko.observable(),
    DateEnd: ko.observable(),
    ListProducer: ko.observableArray(),
    ListAccount: ko.observableArray(),

    ProducerId: ko.observable(),
    AccountId: ko.observable(),

    PageIndex: ko.observable(),

    PageCountList: ko.observableArray(),
    PageCount: ko.observable(),

    Event_SelectProducer: function() {

    },
    Event_SelectAccount: function() {

    },
    Event_Search: function() {
        FeedBackFilter.PageIndex(0);
        FeedBackFilter.PageIndex.valueHasMutated();
        AjaxSearch();
    }
}

// модель пагинатора
var FeedBackPagination =
{
    PageIndex: ko.observable(),
    PageList: ko.observableArray(),

    Event_SelectedPage: function(data) {
        var pageIndex = FeedBackFilter.PageIndex();
        var counter = data.Counter;

        if (pageIndex != counter) {
            FeedBackFilter.PageIndex(data.Counter);
            FeedBackFilter.PageIndex.valueHasMutated();
            AjaxSearch();
        }
    }
}

// модель выбранного сообщения
var FeedBackItem =
{
    Id: ko.observable(),
    Description: ko.observable(),
    //Date: ko.observable(),
    //Type: ko.observable(),
    //Contact: ko.observable(),
    Status: ko.observable(),
    StatusList: ko.observableArray(),
    Comment: ko.observable(),

    Event_ChangeStatus: function() {
        AjaxAddComment();
    }
}

// модель списка сообщений
var FeedBackList =
{
    /*список обращений*/
    FeedList: ko.observableArray(),

    /*выбранное обращение*/
    FeedSelectedItem: ko.observable(),

    /*сообщение для пользователя об ошибках/ожидании/обновлении и загрузке информации*/
    Message: ko.observable(),
    MessageView: ko.observable(), /*block or none   css display param*/

    /* Для отображения информации об фильтрации запроса */
    SortTime: ko.observable(),
    SortProducerName: ko.observable(),
    SortAccountName: ko.observable(),
    SortType: ko.observable(),
    SortStatus: ko.observable(),

    Event_SelectItem: function(data) {
        var JsonSendData = "{\"Id\":\"" + data.Id + "\"}";
        AjaxGetOneFeedBack("FeedBackGetItem", JsonSendData);
    }
}

// получение фильтра
function AjaxGetFilter()
{
    $.ajax({
        url: "GetFilter", 
        cache: false,
        contentType: "application/json; charset=utf-8",
        dataType: "json",      
        type: "POST",
        success: function (result) {
            if (result == "0") {
                FeedBackList.Message("При выполнении запроса возникла ошибка");          
                FeedBackList.Message.valueHasMutated();
            }
            else {
                bindFilter(result);               
            }
        },
        error: function (jqXHR) {
            FeedBackList.Message("При выполнении запроса возникла ошибка" + jqXHR);
            FeedBackList.Message.valueHasMutated();
        }
    });

}

// получение списка сообщений по фильтру
function AjaxSearch()
{
    var JsonSendData = ko.toJSON(FeedBackFilter);
    $.ajax({
        url: "FeedBackSearch",
        cache: false,
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        data: JsonSendData,
        type: "POST",
        success: function (result) {
            if (result == "0") {
                /* поиск результатов не дал */
                FeedBackList.Message("поиск результатов не дал");
            }
            else {
                bindFeedBackList(result);
                $("#ModalFilterForm").modal('hide');
            }
        },
        error: function (jqXHR) {
            $('#message').html(jqXHR.statusText);
        }
    });
}

// получение выбранного сообщения / изменение выбранного сообщения
function AjaxGetOneFeedBack(Url, JsonSendData)
{  
    $.ajax({
        url: Url,
        cache: false,
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        data: JsonSendData,
        type: "POST",
        success: function (result) {
            if (result == "0") {
                $("#ModalFeedBackItem").modal('hide');
            }
            else {
                bindFeddBackItem(result);              
            }
        },
        error: function (jqXHR) {
            $('#message').html(jqXHR.statusText);
        }
    });
}

// изменение выбранного сообщения
function AjaxAddComment()
{
    var JsonSendData = ko.toJSON(FeedBackItem);
    AjaxGetOneFeedBack("AddCommentToFeedBack", JsonSendData);
}

// биндинг фильтра
function bindFilter(result)
{
    FeedBackFilter.DateBegin(result.DateBegin);
    FeedBackFilter.DateEnd(result.DateEnd);
    FeedBackFilter.ListAccount(result.AccountList);
    FeedBackFilter.ListProducer(result.ProducerList);
    FeedBackFilter.ProducerId(result.ProducerId);
    FeedBackFilter.AccountId(result.AccountId);
    FeedBackFilter.PageIndex(result.PageIndex);
    FeedBackFilter.PageCountList(result.PageCountList);
    FeedBackFilter.PageCount(result.PageCount);

    FeedBackFilter.DateEnd.valueHasMutated();
    FeedBackFilter.DateBegin.valueHasMutated();
    FeedBackFilter.ListAccount.valueHasMutated();
    FeedBackFilter.ListProducer.valueHasMutated();
    FeedBackFilter.ProducerId.valueHasMutated();
    FeedBackFilter.AccountId.valueHasMutated();
    FeedBackFilter.PageIndex.valueHasMutated();
    FeedBackFilter.PageCountList.valueHasMutated();
    FeedBackFilter.PageCount.valueHasMutated();

    AjaxSearch();
}

// биндинг списка сообщений и пагинатора
function bindFeedBackList(result)
{
    /* биндим пагинатор */
    FeedBackPagination.PageList(result.PaginatorLinks);
    FeedBackPagination.PageIndex(result.PageIndex);
    FeedBackPagination.PageList.valueHasMutated();
    FeedBackPagination.PageIndex.valueHasMutated();

    /* биндим список обращений */
    FeedBackList.FeedList(result.FeedList);
    FeedBackList.FeedList.valueHasMutated();

    /* очищаем сообщение */
    FeedBackList.Message("");
    FeedBackList.Message.valueHasMutated();

    /* биндим фильтрацию столбцов */
    FeedBackList.SortTime(result.SortTime);
    FeedBackList.SortProducerName(result.SortProducerName); 
    FeedBackList.SortAccountName(result.SortProducerName); 
    FeedBackList.SortType(result.SortType); 
    FeedBackList.SortStatus(result.SortStatus);

    FeedBackList.SortTime.valueHasMutated();
    FeedBackList.SortProducerName.valueHasMutated();
    FeedBackList.SortAccountName.valueHasMutated();
    FeedBackList.SortType.valueHasMutated();
    FeedBackList.SortStatus.valueHasMutated();
}

// биндинг выбранного сообщения
function bindFeddBackItem(result)
{
    FeedBackItem.Id(result.Id);
    FeedBackItem.Id.valueHasMutated();

    FeedBackItem.Description(result.Description);
    FeedBackItem.Description.valueHasMutated();

    FeedBackItem.Status(result.Status);
    FeedBackItem.Status.valueHasMutated();

    FeedBackItem.StatusList(result.StatusList);
    FeedBackItem.StatusList.valueHasMutated();
    
    FeedBackItem.Comment(result.Comment);
    FeedBackItem.Comment.valueHasMutated();
    
    $("#ModalFeedBackItem").modal('show');
}














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