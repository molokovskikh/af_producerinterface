function OnDeleteSucces(id_email) {
	var elementID = "#email_item_" + id_email;
	$(elementID).css("display", "none");
	$("#listemailview").show();
}

function AddEmail() {
	var url_ = document.location.pathname;
	var url__ = url_.toLowerCase().replace("index", "addemail")
	var SendItem = $('#addemail').val();
	$.ajax({
		type: 'POST',
		url: url__,
		data: 'Mail=' + SendItem,
		success: function(data) {
			$('#accountemaillist').append(data);
		}
	});
}


var tempStuff = function() {

	this.AttrValueSet = "href";
	this.AttrValueSetClean = "url-clean";
	this.AttrValueGetName = "url-param";
	this.AttrValueGetValue = "url-val";
	this.AttrValueGetObj = "url-id";

	this.updateBySelector = function(obj, selectors) {
		var selectorList = selectors.split(',');
		var newUrl = "";
		for (var i = 0; i < selectorList.length; i++) {
			if (this.AttrValueGetValue != "") {
				newUrl += "?";
			} else {
				newUrl += "&";
			}
			newUrl += $(selectorList[i]).attr(this.AttrValueGetName) + '=' + $(selectorList[i]).attr(this.AttrValueGetValue);
		}
		$(obj).attr(this.AttrValueSet, $(obj).attr(this.AttrValueSetClean) + encodeURI(newUrl));
	}

	this.SetValueTo = function(obj, attr, value) {
		if (attr === 'html') {
			$(obj).attr(attr, value);
		} else {
			$(obj).html(value);
		}
	}

	this.ReplaceAndGo = function (selector) {
		this.updateBySelector($("#" + selector), '[url-id="' + selector + '"]');
	}

	this.UpdateModal = function(selector, value) {
		var item = $("input[url-attr=interval]");
		item.attr("url-target", selector);
		item.val(value);
	}
	this.UpdateInterval = function() {
		var itemInput = $("input[url-attr=interval]");
		var item = $("[url-id=" + $(itemInput).attr("url-target") + "]");
		$(item).attr("url-val", $(itemInput).val());
		$(item).html($(itemInput).val());
		this.ReplaceAndGo($(itemInput).attr("url-target"));
		$(itemInput).attr("url-target", "");
	}
}
var stuff = new tempStuff();
