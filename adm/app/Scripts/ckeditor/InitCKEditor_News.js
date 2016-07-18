$(document).ready(function () {
   // var UrlString = window.location.href.replace("News/Create", "MediaFiles");
    var FullUrlString = $('#FullUrlStringFile').val();
    CKEDITOR.replace('Description',
        {
            'filebrowserBrowseUrl': FullUrlString + 'Index/',
            'filebrowserImageBrowseUrl': FullUrlString + 'Index/',
            'filebrowserUploadUrl': FullUrlString + 'SaveNewsFile/',
            'filebrowserImageUploadUrl': FullUrlString + 'SaveNewsFile/',
            'height': '35em'
        });
});
function TargetBlackPreviewNews() {
    var Param1 = $('#Name').val();
    var ParamDescription = CKEDITOR.instances.Description.getData();    
    var UrlString = window.location.href.replace("Create", "Preview");
    window.open(UrlString + "?Name=" + Param1 + "&Description=" + ParamDescription, '_blank');
}

