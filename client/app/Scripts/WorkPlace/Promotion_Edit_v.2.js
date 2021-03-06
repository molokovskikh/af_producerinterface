﻿$(function () {
    var elem = document.getElementById("PromotionUiDiv");
    ko.applyBindings(Promotion, elem);
    $("form").hide();
    Promotion.Title("Загрузка ...");
    Promotion.Title.valueHasMutated();
    Promotion.LoadingImageVisible(1);
    Promotion.LoadingImageVisible.valueHasMutated();
    $('.drop').each(function () {
        $(this).chosen({ width: '100%' });
    });

    AjaxLoadModel();
});

function UpdateSupplierList() {
    var RegionListJson = ko.toJSON(Promotion.RegionList());
    $.ajax({
        url: "/Promotion/GetUpdateSupplierList",
        cache: false,
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        data: RegionListJson,
        type: "POST",
        success: function (result) {
            if (result == "0") {

            }
            else {
                bindSupplierList(result);
            }
        },
        error: function (jqXHR) {
            $('#message').html(jqXHR.statusText);
        }
    });

}

function AjaxLoadModel()
{
    $.ajax({
    	url: $("#LoadUrl").val(),
        cache: false,
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        type: "POST",
        success: function (result) {
					bindModel(result);
        },
        error: function (jqXHR) {
            $('#message').html(jqXHR.statusText);
        }
    });
}

function bindSupplierList(JsonList)
{
    var Array_ = Promotion.SuppierRegions();
    Promotion.SuppierRegions([]);
    Promotion.SuppierRegionsList(JsonList);
    Promotion.SuppierRegionsList.valueHasMutated();

    if (Array_ != null) {
        var X = 0;
        var NewRegions = new Array();

        for (var I = 0; I < JsonList.length; I++) {
            var SearchElem = JsonList[I].Value;

            X = GetIndex(SearchElem, Array_);

            if (X > -1) {
                NewRegions.push(X);
            }
        }

        var EEE = NewRegions;
        Promotion.SuppierRegions(EEE);
        Promotion.SuppierRegions.valueHasMutated();

        if (EEE.length == 0)
        {
            Promotion.AllSupplier(false);
            Promotion.AllSupplier.valueHasMutated();
        }

        DropdownReinit();
        SubmitValidationForm();
    }
}

function bindModel(JsonModel)
{
    Promotion.LoadingImageVisible(0);
    Promotion.LoadingImageVisible.valueHasMutated();

    Promotion.Title(JsonModel.Title);
    Promotion.Title.valueHasMutated();

    if (JsonModel.Title == "Новая промоакция") {
        Promotion.SubmitText("Добавить и отправить запрос на подтверждение");
    }
    else
    {
        Promotion.SubmitText("Сохранить изменения и отправить запрос на подтверждение");
    }

    Promotion.Id(JsonModel.Id);
    Promotion.Id.valueHasMutated();

    Promotion.SubmitText.valueHasMutated();
    Promotion.Name(JsonModel.Name);
    Promotion.Name.valueHasMutated();

    Promotion.Annotation(JsonModel.Annotation);
    Promotion.Annotation.valueHasMutated();

    Promotion.Begin(JsonModel.Begin);
    Promotion.Begin.valueHasMutated();

    Promotion.End(JsonModel.End);
    Promotion.End.valueHasMutated();

    Promotion.DrugCatalogList(JsonModel.DrugCatalogList);
    Promotion.DrugCatalogList.valueHasMutated();

    Promotion.DrugList(JsonModel.DrugList);
    Promotion.DrugList.valueHasMutated();

    Promotion.RegionGlobalList(JsonModel.RegionGlobalList);
    Promotion.RegionGlobalList.valueHasMutated();

    Promotion.RegionList(JsonModel.RegionList);
    Promotion.RegionList.valueHasMutated();

    Promotion.SuppierRegionsList(JsonModel.SuppierRegionsList);
    Promotion.SuppierRegionsList.valueHasMutated();

    Promotion.SuppierRegions(JsonModel.SuppierRegions);
    Promotion.SuppierRegions.valueHasMutated();

    if (JsonModel.PromotionFileName == null || JsonModel.PromotionFileName == "") {
        if (JsonModel.Id > 0) {
            Promotion.PromotionFileName("Файл ранее не добавляли");
            Promotion.PromotionFileName.valueHasMutated();
            Promotion.OldFileVisible(1);
            Promotion.OldFileVisible.valueHasMutated();
            Promotion.PromoFileClass("href_decoration_off");
            Promotion.PromoFileClass.valueHasMutated();

        }
        else
        {
            Promotion.OldFileVisible(0);
            Promotion.OldFileVisible.valueHasMutated();
        }
    }
    else
    {
        Promotion.PromotionFileName(JsonModel.PromotionFileName);
        Promotion.PromotionFileName.valueHasMutated();
        Promotion.PromotionFileId(JsonModel.PromotionFileId);
        Promotion.PromotionFileId.valueHasMutated();

        Promotion.PromotionFileUrl(JsonModel.PromotionFileUrl);
        Promotion.PromotionFileUrl.valueHasMutated();
        Promotion.OldFileVisible(1);
        Promotion.OldFileVisible.valueHasMutated();
    }

    Promotion.ValiedationOnOff(false);
    Promotion.ValiedationOnOff.valueHasMutated();

    Event_Name_Change();
    Event_Annotation_Change();

    var X = Promotion.RegionList();

    $("form").show();
    DropdownReinit();
    InitDatePicker();

}

function InitDatePicker() {
    var dateFrom = $('#Begin').datepicker({
        format: 'dd.mm.yyyy',
        language: 'ru',
        weekStart: 1,
        autoclose: true,
        onRender: function (date) {
            return date.valueOf() > Date.now() ? 'disabled' : '';
        }
    }).on('changeDate', function () {
        dateTo.setStartDate(dateFrom.getDate());
        if (dateFrom.getDate() > dateTo.getDate()){
            dateTo.setDate(dateFrom.getDate());
        }
    }).data('datepicker');

    var dateTo = $('#End').datepicker({
        format: 'dd.mm.yyyy',
        autoclose: true,
        language: 'ru',
        weekStart: 1,
        onRender: function (date) {
            return date.valueOf() > Date.now() ? 'disabled' : '';
        }
    }).data('datepicker');
    dateFrom.setStartDate(Date().toLocaleString());
    dateTo.setStartDate(Date().toLocaleString());
}

