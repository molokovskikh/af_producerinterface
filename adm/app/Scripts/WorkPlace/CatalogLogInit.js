var details = $("#ModalDetailsItem");
var currentCollapsingElement = {};
$(function() {
	// отслеживание изменений в модели выбранного обращения
	ko.applyBindings(DetailsItem, details[0]);
	$('.collapse').collapse({
		toggle: false
	}).on('shown.bs.collapse', function() {
		var obj = $("a[href='" + $(currentCollapsingElement).attr("href") + "']").parent().find("div");
		var _maxHeight = 0;
		obj.each(function() { _maxHeight = _maxHeight <= $(this).height() ? $(this).height() : _maxHeight; });
		obj.css("height", String(_maxHeight));
	});
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
	DetailsItem.Id(result.Id);
	DetailsItem.LogTimeUi(result.LogTimeUi);
	DetailsItem.OperatorHost(result.OperatorHost);
	DetailsItem.ProducerName(result.ProducerName);
	DetailsItem.Login(result.Login);
	DetailsItem.UserName(result.UserName);
	DetailsItem.DateEditUi(result.DateEditUi);
	details.modal('show');
}

function ChangeArrowIcon(obj) {
	currentCollapsingElement = obj;
	obj = $("a[href='" + $(obj).attr("href") + "'] span");
	obj.each(function() {
		if ($(this).hasClass("glyphicon-arrow-up")) {
			$(this).removeClass("glyphicon-arrow-up").addClass("glyphicon-arrow-down");
		} else {
			$(this).removeClass("glyphicon-arrow-down").addClass("glyphicon-arrow-up");
		}
	});
}