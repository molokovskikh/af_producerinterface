function InputTextEditor(element, textElement, callback) {
	console.log('Создаем текстовый редактор');
	var surrogate;

	var initialize = function() {
		surrogate = $("<input type='text'></div>").get(0);
		$(element).after(surrogate);
		$(element).detach();
		$(surrogate).focus();
		//Это необходимо для того, чтобы саобытие по которому создается объект не отменило редактирование
		//Мы как бы на передаем управление псевдопотоку собтия и он доходит до окна, а мы только потом вешаем свое событие
		setTimeout(function() {
			$(window).on("keydown", checkKeyPress);
			$(window).on("click", checkClick);
			$(surrogate).on("click", function(e) {
				e.stopPropagation();
			});
		},1);

	}

	var disable = function (success) {
		$(window).off("click",null,checkClick);
		$(window).off("keydown",null,checkKeyPress);
		var value = $(surrogate).val();
		var event = { newValue : value, success : success};
		$(surrogate).after($(element));
		$(surrogate).remove();
		if (success) {
			if(textElement)
				$(textElement).html(value);
		}
		callback(event);
	}
	this.disable = disable;

	var checkKeyPress = function(e) {
		if (e.keyCode == 27)
			disable(false);
		else if (e.keyCode == 13)
			disable(true);
	};
	var checkClick = function(e) {
		disable(false);
	}

	this.getElement = function() {
		return element;
	}

	this.getInput = function() {
		return surrogate;
	}

	initialize();
}

