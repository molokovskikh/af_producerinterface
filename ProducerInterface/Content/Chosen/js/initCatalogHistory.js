var details = $("#ModalDetailsItem");

$(function () {
    // отслеживание изменений в модели выбранного обращения
    ko.applyBindings(DetailsItem, details[0]);
});

// модель выбранного сообщения
var DetailsItem =
{
    ObjectReference: ko.observable(),
    Id: ko.observable(),
    LogTimeUi: ko.observable(),
    OperatorHost: ko.observable(),
    ProducerName: ko.observable(),
    Login: ko.observable(),
    UserName: ko.observable(),
    DateEditUi: ko.observable(),
}

// биндинг выбранного сообщения
function bindDetailsItem(result) {

    DetailsItem.ObjectReference(result.ObjectReference);
    DetailsItem.ObjectReference.valueHasMutated();

    DetailsItem.Id(result.Id);
    DetailsItem.Id.valueHasMutated();

    DetailsItem.LogTimeUi(result.LogTimeUi);
    DetailsItem.LogTimeUi.valueHasMutated();

    DetailsItem.OperatorHost(result.OperatorHost);
    DetailsItem.OperatorHost.valueHasMutated();

    DetailsItem.ProducerName(result.ProducerName);
    DetailsItem.ProducerName.valueHasMutated();
    
    DetailsItem.Login(result.Login);
    DetailsItem.Login.valueHasMutated();

    DetailsItem.UserName(result.UserName);
    DetailsItem.UserName.valueHasMutated();

    DetailsItem.DateEditUi(result.DateEditUi);
    DetailsItem.DateEditUi.valueHasMutated();

    details.modal('show');
}

