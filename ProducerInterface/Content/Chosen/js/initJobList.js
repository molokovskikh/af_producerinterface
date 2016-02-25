$(function () {
	// запрос статуса каждые 10 сек
	setInterval(ajaxCall, 10000);
});

function ajaxCall() {
	var pr = $('td.processed');
	$.each(pr, function () {
		var el = $(this);
		var url = el.data('url');
		$.ajax({
		    url: url,
		    data: null,
		    type: "POST",
		    success: function (data) {
		        switch (data.status) {
		            case 0:
		                el.text('Не запускался');
		                break;
		            case 1:
		                el.text('Отчет готовится');
		                break;
		            case 2:
		                location.reload();
		                //el.html('<a href="' + data.url + '">Посмотреть отчет</a>');
		                //el.removeClass('processed');
		                break;
		            case 3:
		                el.text('Нет данных для построения отчета');
		                break;
		            case 4:
		                el.text('В процессе подготовки произошла ошибка');
		                break;
		            default:
		                el.text('Неизвестный статус');
		        }
		    }
		});

		//$.getJSON(url, null, function (data) {
		//	switch (data.status) {
		//		case 0:
		//			el.text('Не запускался');
		//			break;
		//		case 1:
		//	  	el.text('Отчет готовится');
		//	  	break;
		//		case 2:
		//			el.html('<a href="' + data.url + '">Посмотреть отчет</a>');
		//			el.removeClass('processed');
		//			break;
		//		case 3:
		//			el.text('Нет данных для построения отчета');
		//			break;
		//		case 4:
		//			el.text('В процессе подготовки произошла ошибка');
		//			break;
		//		default:
		//			el.text('Неизвестный статус');
		//	}		
		//});
	});
};





