function ModalClose()
{
    $('#myModal').modal('hide');
}
$(document).ready()
{
    InitPhone();
}
function InitPhone()
{
    var PhoneNumElem = $('#PhoneNum');
    PhoneNumElem.mask("(999) 999-99-99");

    $('#PhoneNumber').each(function () {
            $(this).mask("(999) 999-99-99");
    });  
}