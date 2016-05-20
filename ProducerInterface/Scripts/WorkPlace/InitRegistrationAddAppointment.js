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


    function FeedBackAddDomainName()
    {
        //http://producerinterface/feedBack/Index_Type?Id=5&IdProducer=10
        var ProducerId = document.getElementById("Producers").value;      
        var Url = "/FeedBack/Index_Type";
        var SendString =Url + "?Id=5" + "&" + "IdProducer=" + ProducerId;
        
        location.href = Url;
      
    }


