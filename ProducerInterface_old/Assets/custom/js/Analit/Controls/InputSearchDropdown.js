function InputSearchDropdown(input, route, callback) {
	console.log("Запускаем выпадающие подсказки для элемента ", input);
	var lastRequest;
	var dropdown;

	var initialize = function() {
		dropdown = $("<div class='analit-dropdown'></div>").get(0);
		$(input).after(dropdown);
		$(dropdown).hide();
		$(input).on("keydown", onkeydown);
	}

	var onkeydown = function() {
		if (lastRequest)
			lastRequest.abort();
		var name = $(input).val();
		if (!name)
			return;

		var url = cli.getParam("baseurl") + route + "?id=" + encodeURIComponent(name);
		lastRequest = $.ajax({
			url: url,
			type: 'POST',
			dataType: "json",
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
					callback(event);
				});
				$(dropdown).show();
				if (obj.length <= 0)
					$(dropdown).hide();
			},
			error: function(event) {
				if (event.statusText == "abort")
					return;
				$(dropdown).hide();
			}
		});
	};

	this.disable = function() {
		$(input).off("keydown", null, onkeydown);
		$(dropdown).remove();
	}
	
	this.getElement = function() {
		return dropdown;
	}

	initialize();
}

