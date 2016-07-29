$(function () {
	$("form.auto-post select").change(function () {
		$(this).parent("form").submit();
	});
});