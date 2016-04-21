$(function() {
    var elem4 = document.getElementById("ModalDetailsItem");

    // отслеживаниt изменений в модели выбранного обращения
    ko.applyBindings(DetailsItem, elem4);

});


// модель выбранного сообщения
var DetailsItem =
{
    Id: ko.observable(),
    LogTime: ko.observable(),
    OperatorHost: ko.observable(),
    ProducerId: ko.observable(),
    ProducerName: ko.observable(),
    Login: ko.observable(),
    UserId: ko.observable(),
    UserName: ko.observable(),
    DateEdit: ko.observable(),

    Event_SelectItem: function (e) {
        var asd = e;
        //bindDetailsItem(result);
    }
}

// биндинг выбранного сообщения
function bindDetailsItem(result)
{
    DetailsItem.Id(result.Id);
    DetailsItem.Id.valueHasMutated();

    DetailsItem.LogTime(result.LogTime);
    DetailsItem.LogTime.valueHasMutated();

    DetailsItem.OperatorHost(result.OperatorHost);
    DetailsItem.OperatorHost.valueHasMutated();

    DetailsItem.ProducerId(result.ProducerId);
    DetailsItem.ProducerId.valueHasMutated();

    DetailsItem.ProducerName(result.ProducerName);
    DetailsItem.ProducerName.valueHasMutated();
    
    DetailsItem.Login(result.Login);
    DetailsItem.Login.valueHasMutated();

    DetailsItem.UserId(result.UserId);
    DetailsItem.UserId.valueHasMutated();

    DetailsItem.UserName(result.UserName);
    DetailsItem.UserName.valueHasMutated();

    DetailsItem.DateEdit(result.DateEdit);
    DetailsItem.DateEdit.valueHasMutated();

    $("#ModalDetailsItem").modal('show');
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