function InputSearchDropdown(input, route, callback) {
    console.log("Запускаем выпадающие подсказки для элемента ", input);
    var self = this;
	var lastRequest;
	var dropdown;
	var onchange = function () { };
    var onRequestSend = function() {};

	var initialize = function() {
		dropdown = $("<div class='analit-dropdown'></div>").get(0);
		$(input).after(dropdown);
		$(dropdown).hide();
		$(input).on("keyup", self.search);
	}

	this.search = function () {
		if (lastRequest)
			lastRequest.abort();
		var name = $(input).val();

		var data = { id: name}
	    onRequestSend(data);
	    var url = cli.getParam("baseurl") + route;
		lastRequest = $.ajax({
			url: url,
			type: 'POST',
			dataType: "json",
            data : data,
			success: function(data) {
				var obj = data;
				var str = "";
				$(dropdown).html("");
				for (var i = 0; i < obj.length; i++) {
					var id = obj[i].Value;
					var name = obj[i].Name;
					str += "<div style='cursor: pointer' data='" + id + "'>" + name + "</div>";
				}
				$(dropdown).html(str);
				$(dropdown).find("div").on("click", function(e) {
					var name = $(this).html();
					var value = $(this).attr("data");
					$(input).val(name);
					e.stopPropagation();
					var event = { Value: value, Name: name }
					callback.bind(self,event)();
				});
				$(dropdown).show();
				if (obj.length <= 0)
				    $(dropdown).hide();
				onchange.bind(self, obj)();
			},
			error: function(event) {
				if (event.statusText == "abort")
					return;
				$(dropdown).hide();
			}
		});
	};
	this.disable = function() {
	    $(input).off("keyup", null, self.search);
		$(dropdown).remove();
	}
	
	this.getElement = function() {
		return dropdown;
	}
    this.getSelectElement = function() {
        return input;
    }
    this.onChange = function(callback) {
        onchange = callback;
    }
    this.onRequestSend = function(callback) {
        onRequestSend = callback;
    }
	initialize();
}

