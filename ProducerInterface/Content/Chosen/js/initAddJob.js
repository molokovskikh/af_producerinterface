$(function () {

	$('#RegionCodeEqual').chosen({ width: '95%' });
	$('#CatalogIdEqual').chosen({ width: '95%' });
	$('#SupplierIdNonEqual').chosen({ width: '95%' });
	$('#CatalogNamesId').chosen({ width: '95%' });
	SetChosen("CatalogNamesId", "/Report/GetCatalogDragFamalyNames");

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

function SetChosen(elId, url) {
    var selector = '#' + elId;
    var el = $(selector);

    var TextSearch = '/' + ('#CatalogNamesId').val;

 //   загружаем сохранённые элементы
    $.getJSON(url + TextSearch, el.serialize(), function (data) {
        //el.children().remove();
        $.each(data, function (index, item) {
            var op = $('<option selected></option>')
				.text(item.text)
				.value(item.val);
            el.append(op);
        });
      
        // https://github.com/meltingice/ajax-chosen
        el.ajaxChosen({
            type: 'GET',
            url: url + '',
            dataType: 'json',
            minTermLength: 5,		
            afterTypeDelay: 0,
            keepTypingMsg: "Введите два или больше символов для поиска",
            lookingForMsg: "Поиск"
        }, function (data) {  
            return data;
        },
      { width: '95%' }    
      );
    });

}

