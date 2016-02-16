tinyMCEPopup.requireLangPack();

var ImagelightDialog = {
    init: function () {
        var options = {
            dataType: "json",
            beforeSubmit: function (data) {
                $('#preview').attr('src', '/Content/images/spinning.gif');
            },
            success: function (data) {
                $('#preview').attr('src', '/Image/ById/' + data.Id);
            }
        };

        $('#upload').ajaxForm(options);
        $('#preview').click(function () {
            $('#file').click();
        });
    },

    insert: function () {
        // Insert the contents from the input into the document
        tinyMCEPopup.editor.execCommand('mceInsertContent', false, $('#image').html());
        tinyMCEPopup.close();
    }
};
tinyMCEPopup.onInit.add(ImagelightDialog.init, ImagelightDialog);

