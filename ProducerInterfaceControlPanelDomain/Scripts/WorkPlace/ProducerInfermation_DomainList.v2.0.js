function LoadProducerDomainList() {
    var sup = $('#idproducer');
    var prm = "idproducer=" + $('#idproducer').val();

    var stringPathName = document.location.pathname;

    stringPathName = (stringPathName.substring(0, stringPathName.length - 10));

    stringPathName = stringPathName + '/GetDomain';

    $.ajax(
        {
            url: stringPathName, data: prm, type: "POST", success: function (data) {
                $('#replacecontent').html(data);
            }
        });
}
    