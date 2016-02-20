$(function () {

	var crExp = $('#CronExpressionUi');
	// cron control init
	var cr = $('#cron_selector').jqCron({
		bind_to: crExp,
		enabled_year: false,
		enabled_hour: false,
		numeric_zero_pad: true,
		multiple_dom: true,
		//multiple_month: true,
		//multiple_mins: true,
		multiple_dow: true,
		//multiple_time_hours: true,
		//multiple_time_minutes: true,
		default_period: 'week',
		default_value: crExp.val(), //'0 10 * * 1',
		no_reset_button: true,
		lang: 'ru'
	}).jqCronGetInstance();

	var interval = $('#Interval');
	interval.chosen({ width: '30%' });

	// при чеке За предыдущий месяц гасим выбор интервала
	var bm = $('#ByPreviousMonth');
	if (bm.prop('checked'))
		interval.prop('disabled', true).trigger("chosen:updated");

	bm.click(function () {
	    if ($(this).prop('checked')) {
	        interval.prop('disabled', true).trigger("chosen:updated");
	        $('#Interval_param').css("display", "none");
	    }
	    else
	    {
	        interval.removeProp('disabled').trigger("chosen:updated");
	        $('#Interval_param').css("display", "block");
	    }
	});

	// cron human text insert
	$('#btn').click(function () {
		$('#CronHumanText').val(cr.getHumanText());
	});

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