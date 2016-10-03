$(function() {

	var crExp = $('#CronExpressionUi');
	// cron control init
	var cr = $('#cron_selector').jqCron({
		bind_to: crExp,
		enabled_year: false,
		enabled_hour: false,
		numeric_zero_pad: true,
		multiple_dom: true,
		multiple_dow: true,
		default_period: 'week',
		default_value: crExp.val(), //'0 10 * * 1',
		no_reset_button: true,
		lang: 'ru'
	}).jqCronGetInstance();

	$('#IntervalType').change(function() {
		if ($('#IntervalType').val() == "1") {
			$('#Interval_param').css("display", "block");
		} else {
			$('#Interval_param').css("display", "none");
		}
	});
	$('#IntervalType').change();

	// cron human text insert
	$('#btn').click(function() {
		$('#CronHumanText').val(cr.getHumanText());
	});

	$('#MailTo').chosen({ width: '100%' });

	// при клике кнопки Добавить почту - добавляем
	$('#addBtn').on('click', function() {
		var newMail = $('#MailTo_addMail');
		var mailList = $('#MailTo');
		if (!newMail)
			return;
		if (newMail.val() === "")
			return;
		if (mailList.find("option[value='" + newMail.val() + "']").length > 0)
			return;
		var op = $('<option selected></option>')
			.text(newMail.val())
			.val(newMail.val());
		mailList.append(op);
		newMail.val('');
		mailList.trigger("chosen:updated");
	});

});