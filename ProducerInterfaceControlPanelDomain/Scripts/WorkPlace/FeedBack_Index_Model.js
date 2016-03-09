var FeedBackOneModel =
{
    FeedBackOneId: ko.observable(),
    FeedBackOneDescription: ko.observable(),
    FeedBackDate: ko.observable(),
    FeedbackType: ko.observable(),
    FeedBackContact: ko.observable()
}
/* модель для поиска и пагинатор */
var FilterModel =
    {
        DateBegin: ko.observable(),
        DateEnd: ko.observable(),
        PagerIndex: ko.observable(),
        ProducerId: ko.observable(),
        LoginId: ko.observable(),
        Pager: ko.observable(),

        /*массив линков пагинатора*/
        PaginatorLinks: ko.observableArray([]),      

        /* сообщение для пользователя */
        ErrorMessage: ko.observable(),

        /*Список пользователей для выбора в поиске*/
        AccountList: ko.observableArray([]),

        /*Список производителей для выбора в поиске*/
        ProducerList: ko.observableArray([]),

        /* список с количеством отображаемых страниц */
        PagerCountList: ko.observableArray([]),

        /*Событие выбора Производителя*/
        Event_SetProducer: function () {
            if (FilterModel.ProducerId() != 0) {
                FilterModel.LoginId(0);
                FilterModel.LoginId.valueHasMutated();
            }
        },
        /*Событие выбора Пользователя*/
        Event_SetAccount: function () {
            if (FilterModel.LoginId() != 0) {
                FilterModel.ProducerId(0);
                FilterModel.ProducerId.valueHasMutated();
            }
        },
        /* Событие Пагинатора */
        Event_Pager_Search: function (data, event) {
            var PI = FilterModel.PagerIndex();/*Текущая страница пейджера*/
            var PC = data.Counter; /* Страница по которой кликнули */
            if (PC == PI) {
                return false;
            }
            else {
                FilterModel.PagerIndex(PC);
                ServerGetResult(1);
            }
        },
        /* Событие поиск в модальном окне*/
        Event_ModalSearch: function () {
            FilterModel.PagerIndex(0);          
            ServerGetResult(1);
        }
    };

/* Модель представления запросов пользователей */
var FeedBackListModel = {
    /*Отображение фильтра   - CSS для показа по каким столбикам произведена фильтрация */
    SortTime: ko.observable(),
    SortProducerName: ko.observable(),
    SortAccountName: ko.observable(),
    SortType: ko.observable(),
    SortStatus: ko.observable(),

    /*список обращений*/
    Feedback: ko.observableArray([]),

    /* текущий пейджер*/
    PagerIndex: ko.observable(),

    /*максимально возможный пейджер*/
    MaxPageCount: ko.observable(),

    /* сообщения для пользователя */
    ErrorMessage: ko.observable(),

    /* block - none    display css styly для сообщения пользователю */
    MessageView: ko.observable(),

    Open: function(Feedback_){ 
        bindDataFeedBackOne(Feedback_);  
        $('#FeedBackModal').modal('show');  
    }
};
