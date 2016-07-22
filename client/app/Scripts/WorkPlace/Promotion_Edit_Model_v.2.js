
var Promotion = {
	LoadingImageVisible: ko.observable(),
	ValiedationOnOff: ko.observable(),
	Title: ko.observable(),
	SubmitText: ko.observable(),
	Enabled: ko.observable(),
	Id: ko.observable(),
	Name: ko.observable(""),
	NameLight: ko.observable(),
	Event_Name: function (data, event) { Event_Name_Change(data, event); },
	Annotation: ko.observable(""),
	AnnotationLight: ko.observable(),
	Event_Annotation: function (data, event) { Event_Annotation_Change(data, event); },
	Begin: ko.observable(),
	End: ko.observable(),
	File: ko.observable(),

	DrugList: ko.observableArray(), /* List Long */
	Event_DrudList: function () {
			if (Promotion.DrugList() != null) {
					Promotion.DrugListError("");
					Promotion.DrugListError.valueHasMutated();
					DropdownReinit();
			}
			SubmitValidationForm();
	},

	RegionList: ko.observableArray(), /* List Long */
	Event_RegionList: function () {Event_RegionList_Change();},

	SuppierRegions: ko.observableArray(), /* List Long */
	Event_SuppierRegions: function () {Event_SupplierRegion_Change();},

	DrugCatalogList: ko.observableArray(), /* List OptionElement */
	RegionGlobalList: ko.observableArray(), /* List OptionElement */
	SuppierRegionsList: ko.observableArray(), /* List OptionElement */

	AllSupplier: ko.observable(),
	Event_AllSupplier: function () {
			setTimeout(Event_AllSupplier_Change, 50); /* задержка нужна для корректной работы во всех браузерах */
	},

	PromotionFileId: ko.observable(),
	PromotionFileName: ko.observable(),
	PromotionFileUrl: ko.observable(),
	OldFileVisible: ko.observable(),
	PromoFileClass: ko.observable(),

	Validation: function () {
			Promotion.ValiedationOnOff(true);
			Promotion.ValiedationOnOff.valueHasMutated();
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
	FileError: ko.observable(),
	selectAllRegions: function () {
		Promotion.RegionList.removeAll();
		for (var region of Promotion.RegionGlobalList()) {
			Promotion.RegionList.push(region.Value);
		}
		DropdownReinit();
	},
	clearRegions: function () {
		Promotion.RegionList.removeAll();
		DropdownReinit();
	},
}

function Event_SupplierRegion_Change()
{
    if (Promotion.SuppierRegions() != null) {
        Promotion.SuppierRegionsError("");
        Promotion.SuppierRegionsError.valueHasMutated();

        var _index = GetIndex("0", Promotion.SuppierRegions());

        if (_index != -1) {
            Promotion.SuppierRegions(["0"]);
            Promotion.SuppierRegions.valueHasMutated();
        }
    }

    if (Promotion.SuppierRegions().length == 0)
    {
        Promotion.AllSupplier(false);
        Promotion.AllSupplier.valueHasMutated();
    }

    DropdownReinit();
    SubmitValidationForm();
}

function Event_RegionList_Change()
{
    if (Promotion.RegionList() != null) {
        Promotion.RegionListError("");
        Promotion.RegionListError.valueHasMutated();
        setTimeout(UpdateSupplierList, 10);

        var _index = GetIndex("0", Promotion.RegionList());

        if (_index != -1) {
            Promotion.RegionList(["0"]);
            Promotion.RegionList.valueHasMutated();
        }
    }

    DropdownReinit();
    SubmitValidationForm();
}

function Event_Name_Change(data, event) {
    if (Promotion.Name() != null) {
        Promotion.NameError("");
        Promotion.NameError.valueHasMutated();
    }
    if (Promotion.Name == null) {
        Promotion.NameLight("Количество оставшихся для ввода знаков 150");
        Promotion.NameLight.valueHasMutated();
    }
    else {
        var OST = "Количество оставшихся для ввода знаков " + (150 - Promotion.Name().length);

        Promotion.NameLight(OST);
        Promotion.NameLight.valueHasMutated();
        if (event != null)
        {
            var keyCode_ = event.which || event.keyCode;
            if (Promotion.Name().length >= 150) {
                if (keyCode_ != 8) {
                    var newstr = Promotion.Name().substring(0, 149);
                    Promotion.Name(newstr);
                    Promotion.Name.valueHasMutated();
                }
                else { }
            }
            else { }
        }
    }
    setTimeout(SubmitValidationForm, 10);
}

function Event_Annotation_Change(data, event)
{
    if (Promotion.Annotation() != null) {
        Promotion.AnnotationError("");
        Promotion.AnnotationError.valueHasMutated();
    }
    if (Promotion.Annotation == null) {
        AnnotationLight("Количество оставшихся для ввода знаков 500");
        Promotion.AnnotationLight.valueHasMutated();
    }
    else {
        var OST = "Количество оставшихся для ввода знаков " + (500 - Promotion.Annotation().length);

        Promotion.AnnotationLight(OST);
        Promotion.AnnotationLight.valueHasMutated();
        if (event != null) {
            var keyCode_ = event.which || event.keyCode;
            if (Promotion.Annotation().length >= 500) {
                if (keyCode_ != 8) {
                    var newstr = Promotion.Annotation().substring(0, 499);
                    Promotion.Annotation(newstr);
                    Promotion.Annotation.valueHasMutated();
                }
                else { }
            }
            else { }
        }
    }
    setTimeout(SubmitValidationForm, 10);
}

function SubmitValidationForm() {
    var ValidReturn = true;

    if (Promotion.ValiedationOnOff() == false)
    {
        return ValidReturn;
    }

    Promotion.NameError("");
    Promotion.AnnotationError("");
    Promotion.DrugListError("");
    Promotion.RegionListError("");
    Promotion.SuppierRegionsError("");

    if (Promotion.Name().length == 0) {
        Promotion.NameError("Заполните описание промоакции");
        Promotion.NameError.valueHasMutated();
        ValidReturn = false;
    }

    if (Promotion.Annotation().length == 0) {
        Promotion.AnnotationError("Заполните аннотацию промоакции");
        Promotion.AnnotationError.valueHasMutated();
        ValidReturn = false;
    }

    if (Promotion.DrugList() == null) {
        Promotion.DrugListError("Выберите лекарства участвующие в акции");
        Promotion.DrugListError.valueHasMutated();
        ValidReturn = false;
    }
    else {
        if (Promotion.DrugList().length == 0) {
            Promotion.DrugListError("Выберите лекарства участвующие в акции");
            Promotion.DrugListError.valueHasMutated();
            ValidReturn = false;
        }
    }

    if (Promotion.RegionList() == null) {
        Promotion.RegionListError("Выберите регионы");
        Promotion.RegionListError.valueHasMutated();
        ValidReturn = false;
    }
    else {
        if (Promotion.RegionList().length == 0) {
            Promotion.RegionListError("Выберите регионы");
            Promotion.RegionListError.valueHasMutated();
            ValidReturn = false;
        }
    }

    if (Promotion.SuppierRegions() == null) {
        Promotion.SuppierRegionsError("Выберите поставщиков");
        Promotion.SuppierRegionsError.valueHasMutated();
        ValidReturn = false;
    }
    else {
        if (Promotion.SuppierRegions().length == 0) {
            Promotion.SuppierRegionsError("Выберите поставщиков");
            Promotion.SuppierRegionsError.valueHasMutated();
            ValidReturn = false;
        }
    }

    var DateBegin = Date(Promotion.Begin);
    var DateEnd = Date(Promotion.End);

    if (DateBegin > DateEnd) {
        ValidReturn = false;
    }

    return ValidReturn;
}

function DropdownReinit() {
    $('.drop').each(function () {
        $(this).trigger("chosen:updated");
    });
}

GetIndex = function (value, strict) {
    for (var i = 0; i < strict.length; i++) {
        if (strict[i] === value) return value;
    }
    return -1;
};

function Event_AllSupplier_Change() {
    var X = Promotion.AllSupplier();
    if (X) {
        if (Promotion.SuppierRegionsList() != null) {
            Promotion.SuppierRegions(["0"]);
            Promotion.SuppierRegions.valueHasMutated();
        }
        else {
            Promotion.AllSupplier(false);
            Promotion.AllSupplier.valueHasMutated();
        }
    }
    else {
        var X = [];
        Promotion.SuppierRegions(X);
        Promotion.SuppierRegions.valueHasMutated();
    }
    DropdownReinit();
    SubmitValidationForm();
}

