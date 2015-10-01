//Объект для перемещение значений из одного селекта в другой
function MultipleSelectValueMover(fromContainer, toContainer, addButton, removeButton) {
    var callback = function () { };
    var moveSelected = function (from, to, select) {
        var selected = $(from).find("option:selected").get(0);
        if (!selected)
            return false;
        console.log("Moving ", selected, "to", to);
        $(selected).remove();
        $(to).append(selected);
        this.sort(to);
        if (select) {
            console.log("setting focus on hidden select", selected);
            $(selected).prop("selected", true);
            $(selected).attr("selected", "selected");
        }

        if (callback)
            callback();
        return false;
    }
    $(addButton).on("click", moveSelected.bind(this, fromContainer, toContainer, true));
    $(removeButton).on("click", moveSelected.bind(this, toContainer, fromContainer, false));
    this.sort = function (selectElement) {
        var options = $(selectElement).find("option").toArray();
        var sortedArray = options.sort(function (el1, el2) {
            return $(el1).html() > $(el2).html();
        });
        $(selectElement).html();
        $(selectElement).append(sortedArray);
    }
    this.onChange = function (callbackFunc) {
        callback = callbackFunc.bind(this);
    }
    this.getFromContainer = function() {
        return fromContainer;
    }
    this.getToContainer = function () {
        return toContainer;
    }
    this.getAddButton = function () {
        return addButton;
    }
    this.getRemoveButton = function () {
        return removeButton;
    }
}