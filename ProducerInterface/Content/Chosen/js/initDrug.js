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

function EditDescription(field) {
	var selector = '#' + field;
	var selectorTxt = '#' + field + 'Txt';

	var param = 'id=' + $('#DescriptionId').val() + '&field=' + field + '&value=' + $(selector).val();
	var url = $('#url').val();

	$.post(url, param, function (data) {
		var txt = $(selectorTxt);
		var children = txt.children();
		txt.text(data.value);
		txt.append(children); 
		EditToggle(field);
	}, 'json');

	//$.getJSON(url + 'ById', el.serialize(), function(data) {
	//	el.children().remove();
	//	$.each(data.results, function(index, item) {
	//		var op = $('<option selected></option>')
	//			.text(item.text)
	//			.val(item.value);
	//		el.append(op);
	//	});
	//});


};