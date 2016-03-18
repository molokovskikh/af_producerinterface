$(document).ready(function () {
    ko.applyBindings(Model);
    var Company = $('#CompanyNames').val();
    Model.FormValidation(false);
    Model.FormValidation.valueHasMutated();
    Model.CompanyName(Company);
    Model.CompanyName.valueHasMutated();
});
var Model =
     {
         FIO: ko.observable(""),
         CompanyName: ko.observable(),
         Phone: ko.observable(""),
         Email: ko.observable(""),

         FormValidation: ko.observable(),

         ErrorFIO: ko.observable(),
         ErrorPhone: ko.observable(),
         ErrorEmail: ko.observable(),

         Event_Submit: function () {
             Model.FormValidation(true);
             return FormValidate();
         },
         Event_Change: function ()
         {
             FormValidate();
         }        

     }
Model.FullText = ko.computed(
    function () {
        return "Я, " + Model.FIO() + ", являясь сотрудником компании " + Model.CompanyName() + ", использую в своей деятельности E-mail: " + Model.Email() + " Однако система не позволяет мне зарегистрироваться с этим E-mail. Прошу решить возникшую  проблему. Телефон для связи: " + Model.Phone();
    }, Model);

function FormValidate() {

    if (!Model.FormValidation())
    {
        return false;
    }

    var SubmitSuccess = true;
    Model.ErrorFIO("");
    Model.ErrorEmail("");
    Model.ErrorPhone("");

    if (Model.FIO().length == 0) {
        SubmitSuccess = false;
        Model.ErrorFIO("Заполните поле ФИО");
    }
    if (Model.Email().length == 0) {
        SubmitSuccess = false;
        Model.ErrorEmail("Заполните поле Email");
    }
    else {
        if (!isEmail(Model.Email())) {
            Model.ErrorEmail("Некорректно указан Email");
        }
    }

    if (Model.Phone().length == 0) {
        SubmitSuccess = false;
        Model.ErrorPhone("Заполните поле Телефон");
    }
    else
    {
        var index_ = Model.Phone().indexOf('_');
        if (index_ > 0)
        {
            Model.ErrorPhone("Заполните поле Телефон");
        }
    }

    Model.ErrorEmail.valueHasMutated();
    Model.ErrorFIO.valueHasMutated();
    Model.ErrorPhone.valueHasMutated();

    return SubmitSuccess;
}
function isEmail(email) {
    var re = /^(([^<>()\[\]\\.,;:\s@"]+(\.[^<>()\[\]\\.,;:\s@"]+)*)|(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/;
    var regex = new RegExp(re);
    return regex.test(email);
}