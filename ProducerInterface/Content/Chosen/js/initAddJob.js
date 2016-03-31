$(function () {

	$('#RegionCodeEqual').chosen({ width: '95%' });
	$('#CatalogIdEqual').chosen({ width: '95%' });
	$('#SupplierIdNonEqual').chosen({ width: '95%', placeholder_text_multiple: 'Выберите поставщиков, если вы хотите исключить их из отчета' });
	$('#SupplierIdEqual').chosen({ width: '95%', placeholder_text_multiple: 'Выберите поставщиков, если вы хотите включить в отчёт только их. Иначе будут включены все' });

	var aa = $('input[name="Var"]');
	var hc = $('#HideCatalog');

	if (aa.length) {
        if (aa.filter(':checked').val() == '3') {
            hc.show();
        } else
            hc.hide();

        aa.click(function() {
            if (aa.filter(':checked').val() == '3') {
                hc.show();
            } else
                hc.hide();
        });
    }

    var ac = $('#AllCatalog');
    if (ac.length) {
        if (ac.prop('checked')) {
            hc.hide();
        } else
            hc.show();

        ac.click(function() {
            if (ac.prop('checked')) {
                hc.hide();
            } else
                hc.show();
        });
    }


    // при изменении регионов загружаем список поставщиков из этих регионов
	$('#RegionCodeEqual').on('change', function () {
        // для установки выбранных регионов по алфавиту
	    $(this).trigger("chosen:updated");
	    var supY = $('#SupplierIdEqual');
	    var supN = $('#SupplierIdNonEqual');
	    if (!supY.length && !supN.length)
	        return;
		var prm = $(this).serialize() + "&" + supN.serialize();
	    var supurl = $('#supurl').val();
	    $.getJSON(supurl, prm, function (data) {
	        AppendToChosen(supY, data);
	        AppendToChosen(supN, data);
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

function AppendToChosen(el, data) {
    if (!el.length)
        return;
    el.children().remove();
    $.each(data.results, function (index, item) {
        var op = $('<option></option>')
            .text(item.text)
            .val(item.value);
        if (item.selected)
            op.attr("selected", "selected");
        el.append(op);
    });
    el.trigger("chosen:updated");
}
