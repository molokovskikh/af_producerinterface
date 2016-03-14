$(function () {     
    var elem = document.getElementById("PromotionEditDiv");
    ko.applyBindings(Promotion, elem);
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
    /*{'Id1':'2'}*/

    var JsonSendData =  "{'Id'" +":"+ "'"+ SendId + "'}";

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
    $('#SuppierRegions').dropdown('clear');
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
        $('#SuppierRegions').dropdown('destroy').dropdown();
        $('#RegionList').dropdown("onHide").dropdown();
    }
}

function bindModel(JsonModel)
{
    Promotion.Title(JsonModel.Title);
    Promotion.Title.valueHasMutated();

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

    $('.ui.dropdown').dropdown();  
    InitDatePicker();
}

var Promotion =
    {
        Title: ko.observable(),
        Id: ko.observable(),
        Name: ko.observable(""),
        Annotation: ko.observable(""),
        Event_Annotation: function (data, event)
        {
            if (Promotion.Annotation() != null)
            {
                Promotion.AnnotationError("");
                Promotion.AnnotationError.valueHasMutated();
            }
            if (this.Annotation == null) {                
                AnnotationLightOstalos("Количество оставшихся для ввода знаков 500");
                Promotion.AnnotationLightOstalos.valueHasMutated();
            }
            else {
                var OST = "Количество оставшихся для ввода знаков " + ( 500 - this.Annotation().length);
                Promotion.AnnotationLightOstalos(OST);
                Promotion.AnnotationLightOstalos.valueHasMutated();
                if (this.Annotation().length >= 500) {
                    var keyCode_ = event.which || event.keyCode;
                    if (keyCode_ != 8) {
                        var newstr = this.Annotation().substring(0, 499);
                        Promotion.Annotation(newstr);
                        Promotion.Annotation.valueHasMutated();
                    }
                    else { return true;}
                }
                else { return true;}
            }
        },
        AnnotationLightOstalos: ko.observable(),
        Begin: ko.observable(),
        End: ko.observable(),
        File: ko.observable(),

        DrugList: ko.observableArray(), /* List Long */
        Event_DrudList: function () {
            if (Promotion.DrugList() != null) {
                Promotion.DrugListError("");
                Promotion.DrugListError.valueHasMutated();
            }
        },

        RegionList: ko.observableArray(), /* List Long */
        Event_RegionList: function()
        {        
            if (Promotion.RegionList() != null) {
                $('#RegionList').dropdown('hide').dropdown();
                Promotion.RegionListError("");
                Promotion.RegionListError.valueHasMutated();                
                setTimeout(UpdateSupplierList, 10);                
            }           
        },

        SuppierRegions: ko.observableArray(), /* List Long */
        Event_SuppierRegions: function () {
            if (SuppierRegions.RegionList() != null) {
                Promotion.SuppierRegionsError("");
                Promotion.SuppierRegionsError.valueHasMutated();
            }
        },        

        DrugCatalogList: ko.observableArray(), /* List OptionElement */
        RegionGlobalList: ko.observableArray(), /* List OptionElement */
        SuppierRegionsList: ko.observableArray(), /* List OptionElement */

        AllSupplier:ko.observable(),   
        Event_AllSupplier: function () {
            setTimeout(CheckAllSupplier, 1000);
        },

        PromotionFileId: ko.observable(),
        PromotionFileName: ko.observable(),
        PromotionFileUrl: ko.observable(),

        Validation: function()
        {
            var ret = SubmitValidationForm();        
            return ret;
        },

        NameError: ko.observable(),
        AnnotationError: ko.observable(),
        DrugListError: ko.observable(),
        RegionListError: ko.observable(),
        SuppierRegionsError: ko.observable(),
        BeginError: ko.observable(),
        EndError: ko.observable(),
        FileError: ko.observable()
    }

function SubmitValidationForm()
{
    var ValidReturn = true;

    if (Promotion.Name().length == 0)
    {
        Promotion.NameError("Заполните описание промоакции");
        Promotion.NameError.valueHasMutated();
        ValidReturn = false;
    }

    if (Promotion.Annotation().length == 0)
    {
        Promotion.AnnotationError("Заполните аннотацию промоакции");
        Promotion.AnnotationError.valueHasMutated();
        ValidReturn = false;
    }


    if (Promotion.DrugList() == null)
    {
        Promotion.DrugListError("Выберите лекарства участвующие в акции");
        Promotion.DrugListError.valueHasMutated();
        ValidReturn = false;
    }

    if (Promotion.RegionList() == null) {
        Promotion.RegionListError("Выберите регионы");
        Promotion.RegionListError.valueHasMutated();
        ValidReturn = false;
    }

    if (Promotion.SuppierRegions() == null) {
        Promotion.SuppierRegionsError("Выберите поставщиков");
        Promotion.SuppierRegionsError.valueHasMutated();
        ValidReturn = false;
    }



    return ValidReturn;
}

function CheckAllSupplier()
{
    var X = Promotion.AllSupplier();
    if (X) {
        $('#SuppierRegions').dropdown('clear');     
        Promotion.SuppierRegions([0]);
        Promotion.SuppierRegions.valueHasMutated();
        $('#SuppierRegions').dropdown('destroy').dropdown();      
    }
    else {
        $('#SuppierRegions').dropdown('clear');
        $('#SuppierRegions').dropdown('destroy').dropdown();    
    }
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

GetIndex = function (value, strict) {
    for (var i = 0; i < strict.length; i++) {
        if (strict[i] === value) return value;
        }
        return -1;  
};