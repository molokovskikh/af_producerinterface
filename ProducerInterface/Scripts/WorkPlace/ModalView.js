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

function SetLoginNames()
{
   /* setTimeout(setLogin_1000,250);   */
}

function setLogin_1000()
{
    var element = document.getElementById('autentificationForm');
     
    if (!element) { }
    else {

        var Login = getCookie("AccountName");
        if (Login == false)
        { }
        else
        {
            var elemLogin = document.getElementById('login');
            $("#login").val(Login);       
            var elemPass = document.getElementById('password');

            setTimeout(Focusable, 100, "login");
            setTimeout(UI_Tab_key_Click, 250, "login");
            setTimeout(UI_Tab_key_Click, 500, "login");


        }        
    }
}

function Focusable(Id_Elem)
{
    var elem = document.getElementById(Id_Elem);
    elem.focus();  

    //if (window.KeyEvent) // Для FF
    //{
    //    var o = document.createEvent('KeyEvents');
    //    o.initKeyEvent('keyup', true, true, window, false, false, false, false, 9, 0);
    //}
    //else // Для остальных браузеров
    //{
    //    var o = document.createEvent('UIEvents');
    //    o.initUIEvent('keyup', true, true, window, 1);
    //    o.keyCode = 9; // Указываем дополнительный параметр, так как initUIEvent его не принимает
    //}
    UI_Tab_key_Click("password");
}

function UI_Tab_key_Click(Id_Elem)
{
    /* в работе */

    //var elem = document.getElementById(Id_Elem);
    //var mousedownEvent = document.createEvent("MouseEvent");
    //mousedownEvent.initMouseEvent("mousedown", true, true, window, 0,
    //                              0, 0, 0, 0,
    //                               0, 0, 0, 0,
    //                               0, null);
    //elem.target.dispatchEvent(mousedownEvent);

  //  var elem = document.getElementById(Id_Elem);  
  //  if (window.KeyEvent) // Для FF
  //  {
  //      var o = document.createEvent('KeyEvents');
  //      o.initKeyEvent('keyup', true, true, window, false, false, false, false, 9, 0);
  //  }
  //  else // Для остальных браузеров
  //  {
  //      var o = document.createEvent('UIEvents');
  //      o.initUIEvent('keyup', true, true, window, 1);
  //      o.keyCode = 9; // Указываем дополнительный параметр, так как initUIEvent его не принимает
  //  }
  //  //elem.fireEvent(o);
  //  //var evt = document.createEventObject();
  ////  document.body.AttachEvent(o);
  //  document.body.fireEvent('keyup', o);
  //  alert(o);

    //var element = document.getElementById('login'); // Получаем объект необходимого элемента
    //var o = document.createEvent('MouseEvents');  // Создаём объект события, выбран модуль событий мыши
    //o.initMouseEvent('click', true, true, window, 1, 12, 345, 7, 220, false, false, true, false, 0, null); // Инициализируем объект события
    //element.dispatchEvent(o);  // Запускаем событие на элементе

    //var element2 = document.getElementById('password'); // Получаем объект необходимого элемента
    //var o2 = document.createEvent('MouseEvents');  // Создаём объект события, выбран модуль событий мыши
    //o2.initMouseEvent('click', true, true, window, 1, 12, 345, 7, 220, false, false, true, false, 0, null); // Инициализируем объект события
    //element2.dispatchEvent(o2);  // Запускаем событие на элементе

    //var keyboardEvent = document.createEvent("KeyboardEvent");
    //var initMethod = typeof keyboardEvent.initKeyboardEvent !== 'undefined' ? "initKeyboardEvent" : "initKeyEvent";


    //keyboardEvent[initMethod](
    //                   "keyup", // event type : keydown, keyup, keypress
    //                    true, // bubbles
    //                    true, // cancelable
    //                    window, // viewArg: should be window
    //                    false, // ctrlKeyArg
    //                    false, // altKeyArg
    //                    false, // shiftKeyArg
    //                    false, // metaKeyArg
    //                    9, // keyCodeArg : unsigned long the virtual key code, else 0
    //                    9 // charCodeArgs : unsigned long the Unicode character associated with the depressed key, else 0
    //);
    //document.dispatchEvent(keyboardEvent);

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

function CookiseGetLogin(name)
{
    var arg = name + "=";
    var alen = arg.length;
    var clen = document.cookie.length;
    var i = 0;
    while (i < clen) {
        var j = i + alen;
        if (document.cookie.substring(i, j) == arg) {
            return getCookieVal(j);
        }
        i = document.cookie.indexOf(" ", i) + 1;
        if (i == 0) break;
    }
    return null;
}
