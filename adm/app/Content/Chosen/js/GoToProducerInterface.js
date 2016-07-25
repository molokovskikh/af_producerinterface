$(function () {
	$('#idproducer').on('change', function () {
		var sup = $('#produceruserid');
		var prm = $(this).serialize() + "&" + sup.serialize();

		var stringPathName = document.location.pathname;
		stringPathName = (stringPathName.substring(0, stringPathName.length));
		stringPathName = stringPathName + '/GetListUser';

		$.getJSON(stringPathName, prm, function (data) {
			sup.children().remove();
			$.each(data, function (index, item) {
				var NewOption = $('<option></option>').text(item.text).val(item.value);
				sup.append(NewOption);
			});

		});
	});
	$('#idproducer').change();
});