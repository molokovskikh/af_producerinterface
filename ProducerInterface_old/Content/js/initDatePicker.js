$(function () {
  // http://www.eyecon.ro/bootstrap-datepicker/
	var dateFrom = $('#DateFrom').datepicker({
		format: 'dd.mm.yyyy',
		weekStart: 1,
		onRender: function (date) {
			return date.valueOf() > Date.now() ? 'disabled' : '';
		}
	}).on('changeDate', function (ev) {
	  //$('#DateToUI')[0].focus();
	}).data('datepicker');

	var dateTo = $('#DateToUi').datepicker({
	  format: 'dd.mm.yyyy',
	  weekStart: 1,
	  onRender: function (date) {
	    //return date.valueOf() <= dateFrom.date.valueOf() ? 'disabled' : '';
	    return date.valueOf() > Date.now() ? 'disabled' : '';
	  }
	}).data('datepicker');
});

