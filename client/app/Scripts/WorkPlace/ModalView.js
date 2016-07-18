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
    $('#PhoneNum').each(function () {
        $(this).mask("(999) 999-99-99");
    });

    $('#PhoneNumber').each(function () {
            $(this).mask("(999) 999-99-99");
    });  
}

function Focusable(Id_Elem)
{
    var elem = document.getElementById(Id_Elem);
    elem.focus();  
}

function getCookie(name) {
    var pattern = RegExp(name + "=.[^;]*")
    matched = document.cookie.match(pattern)
    if (matched) {
        var cookie = matched[0].split('=')
        return cookie[1]
    }
    return false
}
