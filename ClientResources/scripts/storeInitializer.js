define([
    "dojo",
    "dojo/_base/declare",
    "epi/_Module",
    "epi/dependency",
    "epi/routes",
    "epi/shell/store/JsonRest"
], function (
    dojo,
    declare,
    _Module,
    dependency,
    routes,
    JsonRest
) {
        return declare([_Module], {
            initialize: function () {
                this.inherited(arguments);

                var registry = this.resolveDependency("epi.storeregistry");

                //Register store
                registry.add("singleContentItemTrasher.extendedWastebasket",
                    new JsonRest({
                        target: this._getRestPath("extendedWastebasket"),
                        idProperty: "contentLink"
                    })
                );
            },

            _getRestPath: function (name) {
                return routes.getRestPath({ moduleArea: "SingleContentItemTrasher", storeName: name });
            }
        });
    });