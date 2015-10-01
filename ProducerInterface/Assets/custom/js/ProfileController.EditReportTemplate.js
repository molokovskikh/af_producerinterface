//Добавление значений, полученных от сервера в список <select> (будет вызываться при вбивании пользователем значений в инпут фильтра)
var appendValuesToSelect = function(list, arr) {
    //Вносим новые значения в селект
    var txt = "";
    for (var i = 0; i < arr.length; i++)
        txt += "<option value='" + arr[i].Value + "'>" + arr[i].Name + "</option>";
    $(list).html(txt);
    //В этой верстке селекты заменены на кастомные, поэтому надо обновить их
    $(list).selectBox('refresh');
    //Убираем отображение дропдауна, так как вместо него у нас селект
    var el = this.getElement();
    $(el).css("display", "none");
};
//Получение значений в селектах
var getOptionValues = function (select) {
    var options = $(select).find("option").toArray();
    var arr = [];
    for (var i = 0; i < options.length; i++) {
        var val = $(options[i]).html();
        arr.push(val);
    }
    var result = arr.join(",");
    return result;
};

//Фильтрация значений в списке, при помощи дропдауна с подстказками
//подсказки мы скрываем функцией appendValuesToSelect и отображаем их в селекте
//также нам необходимо выслать дополнительную информацию о уже включенных объектах
var regionSearch = new InputSearchDropdown($(".findRegions").get(0), "profile/findRegions");
regionSearch.onRequestSend(function (data) {
    data.regions = getOptionValues($("select.regionList").get(0));
});
regionSearch.onChange(appendValuesToSelect.bind(regionSearch, $("select.availableRegions").get(0)));

var drugSearch = new InputSearchDropdown($(".findDrugs").get(0), "profile/findDrugs");
drugSearch.onRequestSend(function (data) {
    data.drugs = getOptionValues($("select.drugList").get(0));
});
drugSearch.onChange(appendValuesToSelect.bind(drugSearch, $("select.availableDrugs").get(0)));
var supplierSearch = new InputSearchDropdown($(".findSuppliers").get(0), "profile/findSuppliers");
supplierSearch.onChange(appendValuesToSelect.bind(supplierSearch, $("select.availableSuppliers").get(0)));
supplierSearch.onRequestSend(function (data) {
    data.regions = getOptionValues($("select.regionList").get(0));
    data.suppliers = getOptionValues($("select.supplierList").get(0));
});

//Функция, которая выправляет поведение перемещателя значений между селектами
//дело в том, что в данном проекте все селекты заменяются библиотекой jquery.selectBox и их надо вручную обновлять
var updateSelectBoxes = function () {
    console.log("Update lists");
    var mover = this;
    var fromSelect = mover.getFromContainer();
    var toSelect = mover.getToContainer();
    $(fromSelect).selectBox('refresh');
    $(toSelect).selectBox('refresh');
    //обновляем список поставщиков
}
//Создаем перемещатели значений из одного слеекта в другой
var regionMover = new MultipleSelectValueMover($(".availableRegions").get(0), $(".regionList").get(0), $(".addRegion").get(0), $(".removeRegion").get(0));
regionMover.onChange(function() {
    updateSelectBoxes.call(this);
    //когда добавляем регион еще нужно обновить список поставщиков
    supplierSearch.search();
});

var drugMover = new MultipleSelectValueMover($(".availableDrugs").get(0), $(".drugList").get(0), $(".addDrug").get(0), $(".removeDrug").get(0));
drugMover.onChange(updateSelectBoxes);
var supplierMover = new MultipleSelectValueMover($(".availableSuppliers").get(0), $(".supplierList").get(0), $(".addSupplier").get(0), $(".removeSupplier").get(0));
supplierMover.onChange(updateSelectBoxes);

//Если следующую строчку не добавить, то значения все будут в фокусе, а нам этого не нужно
console.log("cleaning focuses from selects", $("li").removeClass("selectBox-selected"));
$("li").removeClass("selectBox-selected");