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
    ProducerId: ko.observable(),
    ProducerName: ko.observable(),
    Login: ko.observable(),
    UserId: ko.observable(),
    UserName: ko.observable(),
    DateEditUi: ko.observable(),
    AdminLogin: ko.observable()
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

	if (result.DateEditUi !== undefined && result.DateEditUi !== "") {
		result.DateEditUi = "<a href='" + $("input[name='LongChangesUrl']").val()+"/" + result.NameId + "'>" + result.DateEditUi + "</a>";
	}
    DetailsItem.DateEditUi(result.DateEditUi);
    DetailsItem.DateEditUi.valueHasMutated();

    DetailsItem.AdminLogin(result.AdminLogin);
    DetailsItem.AdminLogin.valueHasMutated();

    details.modal('show');
}

function getPage(id) {
    $('#CurrentPageIndex').val(id);
    $('#sform').submit();
}

function getSearch() {
    $('#CurrentPageIndex').val('');
    $('#sform').submit();
}

function applyChange(id) {
    $('#ApplyId').val(id);
    $('#sform').submit();
}

function getComment(id) {
    $('#RejectId2').val(id);
    $('#commentModal').modal('show');
}

function rejectChange() {
    var rejectId = $('#RejectId2').val();
    var rejectComment = $('#RejectComment2').val();
    $('#RejectId').val(rejectId);
    $('#RejectComment').val(rejectComment);
    $('#sform').submit();
}


