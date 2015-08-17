//Изменение плательщика отчета при помощи ajax. Поиск производится при помощи поля, к которому приложен выпадающий список с подсказками
$(".editPayer").on("click", function () {
	var wholeElement = $(this).parent().get(0);
	var textElement = $(parent).find("a");
	var reportId = $(parent).parent().parent().find("td").html();
	var editor = new InputTextEditor(wholeElement, textElement, function (e) {
		//Если мы закрыли редактор, то и дропдаун нам не нужен
		dropdown.disable();
	});
	var dropdown = new InputSearchDropdown(editor.getInput(), "GeneralReports/FindPayersByName", function (e) {
		console.log("Меняем плательщика на сервере");
		dropdown.disable();
		editor.disable(true);
		var payerId = e.Value;
		console.log("Плательщик", payerId, "Отчет", reportId);
		$.ajax({
			url: cli.getParam("baseurl") + "GeneralReports/ChangePayer",
			data : {payerId : payerId, Id : reportId },
			type: 'POST',
			dataType: "json",
			success: function(data) {
				console.log("Объект успешно изменен");
			}
		});
	});
});