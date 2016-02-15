    function AddAppointment()
    {
        var AddName = document.getElementById("newAppountName").value;
        var SendString ='NewNameAppointment=' + AddName;

        $.ajax({
            type: "POST",
            url: '/ProducerInterface/Account/DolznostAddNew',
            data: SendString,
            success: function(result)
            {		
                var option = document.createElement("option");		
                option.value = result.split(';')[0];
                option.text =  result.split(';')[1];
                option.selected = 'selected';
                var select = document.getElementById("AppointmentId");
                select.appendChild(option);            
            }      
        });
    }
    $('#PhoneNumber').each(function () {
        $(this).mask("(999) 999-99-99");
    });


