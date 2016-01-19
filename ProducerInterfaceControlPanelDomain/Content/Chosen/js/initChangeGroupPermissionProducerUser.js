$(function () {

    //$('#RegionCodeEqual').chosen({ width: '95%' });
    //$('#CatalogIdEqual').chosen({ width: '95%' });
    //$('#SupplierIdNonEqual').chosen({ width: '95%' });
    //$('#MailTo').chosen({ width: '95%' });

    var PermissionList = $('#ListSelectedPermission');
    PermissionList.chosen({ width: '100%' });
});
//    // при чеке За предыдущий месяц гасим выбор интервала
//    var bm = $('#ByPreviousMonth');
//    if (bm.prop('checked'))
//        interval.prop('disabled', true).trigger("chosen:updated");

//    bm.click(function () {
//        if ($(this).prop('checked'))
//            interval.prop('disabled', true).trigger("chosen:updated");
//        else
//            interval.removeProp('disabled').trigger("chosen:updated");
//    });

//    // при изменении регионов загружаем список поставщиков из этих регионов
//    $('#RegionCodeEqual').on('change', function () {
//        var sup = $('#SupplierIdNonEqual');
//        var prm = $(this).serialize() + "&" + sup.serialize();
//        $.getJSON('/ProducerInterfaceControlPanel/Report/GetSupplierJson', prm, function (data) {
//            sup.children().remove();
//            $.each(data.results, function (index, item) {
//                var op = $('<option></option>')
//					.text(item.text)
//					.val(item.value);
//                if (item.selected)
//                    op.attr("selected", "selected");
//                sup.append(op);
//            });
//            sup.trigger("chosen:updated");
//        });
//    });

//    // при клике кнопки Добавить почту - добавляем
//    $('#addBtn').on('click', function () {
//        var newMail = $('#MailTo_addMail');
//        var mailList = $('#MailTo');
//        if (!newMail)
//            return;
//        var op = $('<option selected></option>')
//					.text(newMail.val())
//					.val(newMail.val());
//        mailList.append(op);
//        newMail.val('');
//        mailList.trigger("chosen:updated");
//    });

//});




////function SetChosen(elId, url) {
////	var selector = '#' + elId;
////	var el = $(selector);
////	// загружаем сохранённые элементы
////	$.getJSON(url + 'ById', el.serialize(), function (data) {
////		el.children().remove();
////		$.each(data.results, function (index, item) {
////			var op = $('<option selected></option>')
////				.text(item.text)
////				.val(item.value);
////			el.append(op);
////		});
////	  // https://github.com/meltingice/ajax-chosen
////		el.ajaxChosen({
////		  type: 'GET',
////		  url: url + 'ByTerm',
////		  dataType: 'json',
////		  minTermLength: 2,
////		  afterTypeDelay: 250,
////		  keepTypingMsg: "Введите два или больше символов для поиска",
////		  lookingForMsg: "Поиск"
////		}, function (data) {
////		  return data.results;
////		},
////      { width: '95%' });
////	});

////}
