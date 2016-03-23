$(function () {

	$('#RegionCodeEqual').chosen({ width: '95%' });
	$('#CatalogIdEqual').chosen({ width: '95%' });
	$('#SupplierIdNonEqual').chosen({ width: '95%', placeholder_text_multiple: 'Выберите поставщиков, если вы хотите исключить их из отчета' });

	var aa = $('input[name="Var"]');
	var hc = $('#HideCatalog');

	if (aa.filter(':checked').val() == '3') {
	    hc.show();
	} else
	    hc.hide();

	aa.click(function () {
	    if (aa.filter(':checked').val() == '3') {
	        hc.show();
	    } else
	        hc.hide();
	});

	var ac = $('#AllCatalog');
	if (ac.prop('checked')) {
	    hc.hide();
	} else
	    hc.show();

	ac.click(function () {
	    if (ac.prop('checked')) {
	        hc.hide();
	    } else
	        hc.show();
	});


	// при изменении регионов загружаем список поставщиков из этих регионов
	$('#RegionCodeEqual').on('change', function () {
        // для установки выбранных регионов по алфавиту
	    $(this).trigger("chosen:updated");
	    var sup = $('#SupplierIdNonEqual');
	    if (!sup.length)
	        return;
		var prm = $(this).serialize() + "&" + sup.serialize();
	    var supurl = $('#supurl').val();
	    $.getJSON(supurl, prm, function (data) {
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

    // https://github.com/meltingice/ajax-chosen
	var caturl = $('#caturl').val();
	$('#CatalogIdEqual2').ajaxChosen({
	    type: 'GET',
	    url: caturl,
	    dataType: 'json',
	    minTermLength: 2,
	    afterTypeDelay: 300,
	    keepTypingMsg: "Введите два или больше символов для поиска",
	    lookingForMsg: "Поиск"
	}, null,
    { width: '95%', placeholder_text_multiple: 'Введите два или больше символов для поиска' }
  );


});
