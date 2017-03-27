define([
    "dojo/_base/declare",
    "customTrash/TrashWidget",
    "epi-cms/component/Trash"
],
    function (
        declare,
        TrashWidget,
        Trash
    ) {
        return declare([Trash], {
            templateString: "<div class=\"epi-trashComponent\"><div data-dojo-type='customTrash/TrashWidget' data-dojo-attach-point='trash'></div></div>"
        });
    });