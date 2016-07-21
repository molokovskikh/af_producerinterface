$(function () {
    var FullUrlString = $('#FullUrlStringFile').val();
    CKEDITOR.replace('Body',
        {
            'filebrowserBrowseUrl': FullUrlString + 'Index/',
            'filebrowserImageBrowseUrl': FullUrlString + 'Index/',
            'filebrowserUploadUrl': FullUrlString + 'SaveNewsFile/',
            'filebrowserImageUploadUrl': FullUrlString + 'SaveNewsFile/',
            'height': '23em'
        });
});
function TargetBlackPreviewNews() {
    var Param1 = $('#Subject').val();
    var ParamDescription = CKEDITOR.instances.Body.getData();
    var UrlString = window.location.href.replace("Create", "Preview");
    window.open(UrlString + "?Name=" + Param1 + "&Description=" + ParamDescription, '_blank');
}

