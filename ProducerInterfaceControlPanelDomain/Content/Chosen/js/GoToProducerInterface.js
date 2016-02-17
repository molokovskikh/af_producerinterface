$('#idproducer').on('change', function () {
    var sup = $('#produceruserid');
    var prm = $(this).serialize() + "&" + sup.serialize();

    var stringPathName = document.location.pathname;
    stringPathName = (stringPathName.substring(0, stringPathName.length));
    stringPathName = stringPathName + '/GetListUser';

    $.getJSON(stringPathName, prm, function (data) {      
        $.each(data, function (index, item) {

            //$("#selectList").append(new Option("option text", "value"));
            //sup.append(new Option(text(item.text), value(item.value)));
            sup.children().remove();
            var NewOption = $('<option></option>').text(item.text).val(item.value);
            sup.append(NewOption);

        });
     /*   sup.trigger("chosen:updated");*/
    });
});