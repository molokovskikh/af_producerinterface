var SliderStuff = function() {

	var _this = this;
	this.selectorSlider = ".SlideItem";
	this.selectorSliderId = ".SlideItem span[name='Id']";
	this.selectorSliderButtonUp = ".SlideItem span[name='SlideButtonUp']";
	this.selectorSliderButtonDown = ".SlideItem span[name='SlideButtonDown']";
	this.selectorSliderLink = ".SlideItem span[name='SlideLink']";
	this.selectorSliderAjaxUpdateUrl = "#SliderAjaxUpdateUrl";
	if ($("#SliderAjaxUpdateUrl").length == 0) {
		throw "На странице отсутствует эдемент с адресом обновления элементов.";
	}
	this.UpdateSlidePositions = function(urlForAjaxUpdate) {
		if ($(_this.selectorSlider).hasClass("ajaxRun") === false) {
			var ajaxArray = new Array();
			$(_this.selectorSliderId).each(function() {
				ajaxArray.unshift(parseInt($(this).html()));
			});
			$(_this.selectorSlider).addClass("ajaxRun");
			try {
				$.ajax({
					url: urlForAjaxUpdate,
					data: { "idList": ajaxArray },
					type: "POST",
					dataType: "json",
					success: function(data) {
						$(_this.selectorSlider).removeClass("ajaxRun");
						if (data !== "") {
							if (window.confirm(data + " Необходимо обновить страницу.")) {
								location.href = location.href;
							}
						}
					},
					error: function() {
						$(_this.selectorSlider).removeClass("ajaxRun");
						if (window.confirm("При изменении порядка элементов списка произошла ошибка! Необходимо обновить страницу.")) {
							location.href = location.href;
						}
					}
				});
			} catch (e) {
				$(_this.selectorSlider).removeClass("ajaxRun");
			}
		}
	};
	$.fn.moveUp = function() {
		$.each(this, function() {
			$(this).after($(this).prev());
		});
	};

	$.fn.moveDown = function() {
		$.each(this, function() {
			$(this).before($(this).next());
		});
	};

	this.UpdateEventForButtonUp = function() {
		$(_this.selectorSliderButtonUp).each(function() {
			$(this).parent().unbind("click").click(function() {
				$(this).parents(_this.selectorSlider).moveUp();
				_this.UpdateSlidePositions($(_this.selectorSliderAjaxUpdateUrl).val());
			});
		});
	};
	this.UpdateEventForButtonDown = function() {
		$(_this.selectorSliderButtonDown).each(function() {
			$(this).parent().unbind("click").click(function() {
				$(this).parents(_this.selectorSlider).moveDown();
				_this.UpdateSlidePositions($(_this.selectorSliderAjaxUpdateUrl).val());
			});
		});
	};
	this.UpdateTitleForLinks = function() {
		$(_this.selectorSliderLink).each(function() {
			var linkParent = $(this).parent();
			var link = $(linkParent).attr("href");
			link = "Перейти к '" + (link.length <= 100 ? link : link.substr(0, 100)) + "'";
			$(linkParent).attr("title", link);
		});
	};
	this.OnLoad = function() {
		_this.UpdateEventForButtonUp();
		_this.UpdateEventForButtonDown();
		_this.UpdateTitleForLinks();
	};
};