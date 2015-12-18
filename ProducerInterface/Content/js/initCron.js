$(function () {

	var crExp = $('#CronExpression');
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

	// cron human text insert
	$('#btn').click(function () {
		$('#CronHumanText').val(cr.getHumanText());
	});

});