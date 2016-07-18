function OnDeleteSucces(id_email) {
    var elementID = "#email_item_" + id_email;
    $(elementID).css("display", "none");
    $("#listemailview").show();
}
function AddEmail() {
    var url_ = document.location.pathname;
    var url__ = url_.toLowerCase().replace("index", "addemail")
    var SendItem = $('#addemail').val();
    $.ajax({
        type: 'POST',
        url: url__,
        data: 'Mail=' + SendItem,
        success: function (data) {
            $('#accountemaillist').append(data);
        }
    });
}