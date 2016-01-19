function EditToggle(field) {
	var selectorTxt = '#' + field + 'Txt';
	var txt = $(selectorTxt);
	var selectorEditor = '#' + field + 'Editor';
	var editor = $(selectorEditor);
	if (editor.is(':hidden')) {
		editor.show();
		txt.hide();
	} else {
		editor.hide();
		txt.show();
	}
};

function EditEntity(field) {
	var selector = '#' + field;
	var selectorTxt = '#' + field + 'Txt';

	var param = 'familyId=' + $('#familyId').val() + '&field=' + field + '&value=' + $(selector).val();
	var url = $('#url').val();

	$.post(url, param, function (data) {
		var txt = $(selectorTxt).find("span").first();
		txt.text(data.value);
		EditToggle(field);
	}, 'json');

};