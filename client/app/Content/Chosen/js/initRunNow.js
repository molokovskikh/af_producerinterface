$(function () {
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
    }).on('changeDate', function () {
        if (dateTo)
            dateTo.setStartDate(dateFrom.getDate());
    }).data('datepicker');

	$('#MailTo').chosen({ width: '100%' });

    // при клике кнопки Добавить почту - добавляем
	$('#addBtn').on('click', function () {
	    var newMail = $('#MailTo_addMail');
	    var mailList = $('#MailTo');
	    if (!newMail)
	        return;
	    var op = $('<option selected></option>')
					.text(newMail.val())
					.val(newMail.val());
	    mailList.append(op);
	    newMail.val('');
	    mailList.trigger("chosen:updated");
	});


});

