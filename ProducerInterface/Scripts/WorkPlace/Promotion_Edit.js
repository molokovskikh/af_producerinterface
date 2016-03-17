$(function () {     
    var elem = document.getElementById("PromotionEditDiv");
    ko.applyBindings(Promotion, elem);
    $("form").hide();
    Promotion.Title("Загрузка информации, ожидайте");
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
        url: "GetUpdateSupplierList",
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
    var SendId = $('#PromotionId').val();
   
    var JsonSendData = "{'IdKey'" + ":" + "'" + SendId + "'}";

    $.ajax({
        url: "EditGetPromotion",
        cache: false,
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        data: JsonSendData,
        type: "POST",
        success: function (result) {
            if (result == "0") {

            }
            else {
                bindModel(result);
            }
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

            if (X > 0) {
                NewRegions.push(X);
            }
        }

        var EEE = NewRegions;
        Promotion.SuppierRegions(EEE);
        Promotion.SuppierRegions.valueHasMutated();
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
 
    Promotion.PromotionFileName(JsonModel.PromotionFileName);
    Promotion.PromotionFileName.valueHasMutated();

    Promotion.PromotionFileUrl(JsonModel.PromotionFileUrl);
    Promotion.PromotionFileUrl.valueHasMutated();
         
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
            //return date.valueOf() <= dateFrom.date.valueOf() ? 'disabled' : '';
            return date.valueOf() > Date.now() ? 'disabled' : '';
        }
    }).data('datepicker');
    dateFrom.setStartDate(Date().toLocaleString());
    dateTo.setStartDate(Date().toLocaleString());
}

