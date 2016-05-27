    function AddAppointment()
    {
        $.ajax({
            type: "POST",
            url: $('#addAppointmentUrl').val(),
            data: 'name=' + $("#appointmentName").val(),
            success: function(result)
            {		
                var option = document.createElement("option");		
                option.value = result.split(';')[0];
                option.text =  result.split(';')[1];
                option.selected = 'selected';
                var select = document.getElementById("AppointmentId");
                select.appendChild(option);
                $('.in').collapse('hide');
            }      
        });
    }
