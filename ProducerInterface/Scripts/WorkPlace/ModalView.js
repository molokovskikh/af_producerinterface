function SendMessage(context) {
    //  $('#myModal').append("<div class='modal-footer' id='footer_model_message' style='opacity:0;'><div id='SendMessage'><p>Отпарвка сообщения серверу</p></div></div>");
    $("#footer_model_message").css("opacity", "1");
    $("#footer_model_message").css("display", "block");
    $("#SendMessage").css("opacity", "1");
}

function ServerOkMessage(context) {
    $("#SendMessage").css("opacity", "0");
    $("#SendMessageOk").css("opacity", "1");
}

function serverMessageCleanForm() {
    $('#Id').val('');
    $("#footer_model_message").css("display", "none");
}

