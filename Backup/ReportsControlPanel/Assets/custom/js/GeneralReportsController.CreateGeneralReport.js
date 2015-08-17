//Изменение плательщика отчета при помощи ajax. Поиск производится при помощи поля, к которому приложен выпадающий список с подсказками
var payerSearchInput = $(".findPayer").get(0);
var payerIdInput = $(".payerId").get(0);
var dropdown = new InputSearchDropdown(payerSearchInput, "GeneralReports/FindPayersByName", function (e) {
		dropdown.disable();
		var payerId = e.Value;
		$(payerIdInput).val(payerId);
		console.log("Подставляем id модели ", payerId, " в инпут", payerIdInput);
});
