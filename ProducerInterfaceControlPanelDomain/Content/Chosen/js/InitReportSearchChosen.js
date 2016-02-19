$(function () {
    $('#Producer').chosen({ width: '100%' });
});


function getPage(id) {
    $('#CurrentPageIndex').val(id);
    $('#sform').submit();
}

function getSearch() {
    $('#CurrentPageIndex').val('');
    $('#sform').submit();
}