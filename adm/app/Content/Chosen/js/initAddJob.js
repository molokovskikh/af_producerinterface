$(function () {

	$('#RegionCodeEqual').chosen({ width: '95%' });
	$('#CatalogIdEqual').chosen({ width: '95%' });
	$('#SupplierIdNonEqual').chosen({ width: '95%' });
    
	// при изменении регионов загружаем список поставщиков из этих регионов
	$('#RegionCodeEqual').on('change', function () {
		var sup = $('#SupplierIdNonEqual');
		var prm = $(this).serialize() + "&" + sup.serialize();
		var stringPathName = document.location.pathname;
		stringPathName = (stringPathName.substring(0, stringPathName.length - 4));
		if (stringPathName.substring(stringPathName.length - 2, stringPathName.length) == 'Ad') {
		    stringPathName = stringPathName.substring(0, stringPathName.length - 2) + '/GetSupplierJson';
		}
		else
		{
		    stringPathName = stringPathName + '/GetSupplierJson';
		}
		$.getJSON(stringPathName, prm, function (data) {
			sup.children().remove();
			$.each(data.results, function(index, item) {
				var op = $('<option></option>')
					.text(item.text)
					.val(item.value);
				if (item.selected)
					op.attr("selected", "selected");
				sup.append(op);
			});
			sup.trigger("chosen:updated");
		});
	});

});

