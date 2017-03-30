define([
    "dojo/_base/declare",
    "dojo/_base/lang",
    "dojo/_base/array",
    "dojo/_base/Deferred",
    "dojo/topic",
    "dijit/registry",
    "epi",
    "epi/dependency",
    "epi-cms/widget/Trash",
    "epi/shell/widget/dialog/Confirmation",

    // Resources
    "epi/i18n!epi/cms/nls/episerver.cms.components.trash"
],
    function (
        declare,
        lang,
        array,
        Deferred,
        topic,
        registry,
        epi,
        dependency,
        Trash,
        Confirmation,
        resources
    ) {
        return declare([Trash], {
            _setQueryOptionsAttr: function () {
                this.inherited(arguments);

                var selectedTab = this.tabContainer.selectedChildWidget;
                if (!selectedTab || !selectedTab.itemListId) {
                    return;
                }

                var trashItemListWidget = registry.byId(selectedTab.itemListId);
                if (trashItemListWidget._isDeleteActionModified) {
                    return;
                }
                this._modifyActionColumn(trashItemListWidget);

                var storeRegistry = dependency.resolve("epi.storeregistry");
                this._extendedWastebasketStore = storeRegistry.get("singleContentItemTrasher.extendedWastebasket");

                this._initDeleteOnClick(trashItemListWidget);

                trashItemListWidget._isDeleteActionModified = true;
            },

            _createConfirmationDialog: function (row) {
                return new Confirmation({
                    title: "Delete",
                    description: lang.replace("Are you sure you want to permanently delete '{name}'?", row.data),
                    onShow: lang.hitch(this, function () {
                        var trashItemList = this.tabContainer.selectedChildWidget.getChildren()[0];
                        trashItemList.clearSelection();
                    })
                });
            },

            _modifyActionColumn: function (trashItemListWidget) {
                var actionFormatterFunc = trashItemListWidget._grid.columns.action.formatter;

                function renderActionMenu(value) {
                    return actionFormatterFunc() + "<a class=\"epi-gridDeleteAction epi-visibleLink\">Delete permanently</a>";
                }
                trashItemListWidget._grid.columns.action.formatter = renderActionMenu;
            },

            _initDeleteOnClick: function (trashItemListWidget) {
                trashItemListWidget._grid.on(".epi-gridDeleteAction:click", lang.hitch(this, function (evt) {
                    var row = trashItemListWidget._grid.row(evt);
                    var currentTrash = this.model.get("currentTrash");
                    if (!row.data || !currentTrash) {
                        return;
                    }

                    var dialog = this._createConfirmationDialog(row);
                    dialog.connect(dialog, "onAction", lang.hitch(this, function (confirm) {
                        if (!confirm) {
                            return;
                        }

                        var contentId = row.data.contentLink;
                        Deferred.when(this._extendedWastebasketStore.executeMethod("PermanentDelete", contentId), lang.hitch(this, function (response) {

                            var trashes = this.model.get("trashes");

                            array.forEach(trashes, lang.hitch(this, function (trash) {
                                if (response.extraInformation === trash.wasteBasketLink) {
                                    trash.deletedByUsers = [];
                                    trash.isRequireLoad = true;
                                    trash.deletedByUsers = [];
                                    this.model.set("currentTrash", trash);
                                }
                            }));
                        }), lang.hitch(this, function (response) {
                            if (response.status === 403) {
                                this.model.set("actionResponse", resources.emptytrash.accessdenied);
                            }
                        }
                        ));
                    }));

                    dialog.show();
                }
                ));
            }
        });
    });