$(document).ready(function () {
    CKEDITOR.replace('Description',
        {
            'filebrowserBrowseUrl': '/MediaFiles/Index/',
            'filebrowserImageBrowseUrl': '/MediaFiles/Index/',
            'filebrowserUploadUrl': '/MediaFiles/SaveFile/',
            'filebrowserImageUploadUrl': '/MediaFiles/SaveFile/',
            'height': '35em'
        });
});
function TargetBlackPreviewNews() {
    var Param1 = $('#Name').val();
    var ParamDescription = CKEDITOR.instances.Description.getData();
    //  var Param2 = $('#Description').val();
    window.open('http://localhost:53367/News/Preview' + "?Name=" + Param1 + "&Description=" + ParamDescription, '_blank');
}