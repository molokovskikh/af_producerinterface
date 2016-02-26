$(function () {

	$('#MailTo').chosen({ width: '100%' });

    // при клике кнопки Добавить почту - добавляем
	$('#addBtn').on('click', function () {
	    var newMail = $('#addMail');
	    var mailList = $('#MailTo');
	    if (!newMail)
	        return;
	    var op = $('<option selected></option>')
					.text(newMail.val())
					.val(newMail.val());
	    mailList.append(op);
	    newMail.val('');
	    mailList.trigger("chosen:updated");
	});

});