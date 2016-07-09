    function AddAppointment()
    {
        $.ajax({
            type: "POST",
            url: $('#addAppointmentUrl').val(),
            data: 'name=' + $("#appointmentName").val(),
			dataType: "json",
            success: function (result, status, jqXHR)
            {		
                var option = document.createElement("option");		
                option.value = result.id;
                option.text =  result.name;
                option.selected = 'selected';
                var select = document.getElementById("AppointmentId");
                select.appendChild(option);
                $('.in').collapse('hide');
            },
            error: function ()
            {
            	var msg = "Не удалось выполнить запрос, попробуйте повторить операцию позднее";
            	$('.server-message').html('<div class="col-md-12 alert alert-danger">' +  msg + '</div>');
            }
        });
    }
