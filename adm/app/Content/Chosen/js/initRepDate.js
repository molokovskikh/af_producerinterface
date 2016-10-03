$(function() {
	// http://www.eyecon.ro/bootstrap-datepicker/
	var dateTo = $('#DateToUi').datepicker({
		format: 'dd.mm.yyyy',
		language: 'ru',
		autoclose: true,
		weekStart: 1,
		endDate: '-0d'
	}).data('datepicker');

	var dateFrom = $('#DateFrom').datepicker({
		format: 'dd.mm.yyyy',
		language: 'ru',
		weekStart: 1,
		autoclose: true,
		endDate: '-0d'
	}).on('changeDate', function() {
		if (dateTo)
			dateTo.setStartDate(dateFrom.getDate());
	}).data('datepicker');
});