$(function () {
    var DrugList = $('#DrugList');
    DrugList.chosen({ width: '100%' });

    var RegionList = $('#RegionList');
    RegionList.chosen({ width: '100%' });

    var SuppierRegionsList = $('#SuppierRegions');
    SuppierRegionsList.chosen({ width: '100%' });
});


$('#RegionList').on('change', function () {
    var sup = $('#SuppierRegions');
    var prm = $(this).serialize() + "&" + sup.serialize();
    var stringPathName = document.location.pathname;
    stringPathName = stringPathName.replace("Manage", "GetSupplierJson");
  
    $.getJSON(stringPathName, prm, function (data) {
        sup.children().remove();
        $.each(data.results, function (index, item) {
            var op = $('<option></option>')
                .text(item.text)
                .val(item.value);
            if (item.selected)
                op.attr("selected", "selected");
            sup.append(op);
        });
        sup.trigger("chosen:updated");
        var sup2 = $('#RegionList');
        sup2.trigger("chosen:updated");
    });
});

function SelectAll(listID) {

    var boolSelect = document.getElementById("AllRegionSelected").checked;
 
    var listbox = document.getElementById("SuppierRegions");

    if (boolSelect)
    {
        for (var count = 0; count < listbox.options.length; count++) {
            listbox.options[count].selected = false;
        }

        var opt = document.createElement('option');
        opt.value = 0;
        opt.selected = true;
        opt.innerHTML = "Все поставщики в выбранных регионах";
        listbox.appendChild(opt);
        var sup = $('#SuppierRegions');
        sup.trigger("chosen:updated");
        return;
    }  

    for (var count = 0; count < listbox.options.length; count++) {
        listbox.options[count].selected = boolSelect;
    }

    $("#SuppierRegions option[value='0']").remove();

    var sup = $('#SuppierRegions');
    sup.trigger("chosen:updated");
}
