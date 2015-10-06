//Отображение статуса отчета на странице

/**
 * Получение статуса генерального отчета от сервера
 * @param Int reportId Идентификатор отчета
 * @param Function callback Функция обратного вызова, которой будет передан объект статуса
 */
var getGeneralReportStatus = function (reportId, lastExecutionDate, callback) {
    console.log("Обновляем состояние отчета",reportId);
    $.ajax({
        url: cli.getParam("baseurl") + "GeneralReports/GetGeneralReportSatus",
        data: { Id: reportId, LastExecutionDate: lastExecutionDate },
        type: 'POST',
        dataType: "json",
        success: function(data) {
            console.log("Получен статус отчета", data);
            callback(data);
        }
    });
}

/**
 * Точка входа текущего файла, постоянно узнает статус запущенного отчета и отображает его пользователю
 * @param Object status Объект статуса отчета. Необязательный праметр для рекурсивного вызова. Возвращается функцией getGeneralReportStatus
 */
var refreshGeneralReportSatus = function (status) {
    if (status) {
        var stop = processReportStatus(status);
        if (stop)
            return;
    }
    var reportId = $(".reportId").html();
    var lastExecutionDate = $(".lastExecutionDate").html();
    getGeneralReportStatus(reportId,lastExecutionDate, refreshGeneralReportSatus);
};

/**
 * Вывод статуса отчета на экран.
 * @param  Object statusData Объект статуса отчета.
 * @returns Bool Возвращает True, если полученный статус является конечным и больше обновлять его не надо.
 */
var processReportStatus = function (statusData) {
    var result = false;
    var code = statusData.Status;
    if (code != 1) {
        $("button.run").prop("disabled", false);
        renewExecutionStatistic();
        result = true;
    }

    //Обновляем статус
    var color = "orange";
    color = code == 0 ? "green" : color;
    color = code == 2 ? "red" : color;
    var newHtml = "<b style='color: " + color + ";'>" + statusData.Message + "<b>";
    $(".status").html(newHtml);
    return result;
}

var renewExecutionStatistic = function () {
    $.ajax({
        url: window.location.href,
        type: 'Get',
        success: function (data) {
            var page = $("<div>");
            page.html(data);
            var table = $(page).find(".executions");
            $(".executions").html(table.html());
            console.log("Обновляем статистику запусков");
        }
    });
}
window.setTimeout(refreshGeneralReportSatus, 10000);