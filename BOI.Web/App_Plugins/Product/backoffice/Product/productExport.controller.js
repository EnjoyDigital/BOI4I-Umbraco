(function () {
    "use strict";

    function productExportController($scope, $routeParams, $timeout, navigationService, notificationsService, productResources, Upload) {
        var mec = this;

        //Export start

        function exportSuccess(response) {
            var linkElement = document.createElement("a");
            try {
                var today = new moment();
                var filename = "ProductsExport-" + today.format("DD-MMMM-YYYY") + ".csv";
                var blob = new Blob([response.data], { type: "text/csv" });
                var url = window.URL.createObjectURL(blob);

                linkElement.setAttribute("href", url);
                linkElement.setAttribute("download", filename);

                var clickEvent = new MouseEvent("click", {
                    "view": window,
                    "bubbles": true,
                    "cancelable": false
                });
                linkElement.dispatchEvent(clickEvent);
                mec.exportProcessing = false;
                notificationsService.success("Success", "File should now be downloading");
            } catch (ex) {
                exportFailed(ex);
            }
        }

        function exportFailed(ex) {
            notificationsService.error("Error", "Failed to create meta csv: " + ex);
            mec.exportError = "Failed to create meta csv";
            mec.exportProcessing = false;
            console.log(ex);
        }

        mec.export = function () {
            mec.exportProcessing = true;
            mec.exportError = false;
            productResources.exportProducts().then(exportSuccess, exportFailed);
        }

        //Export end

        function syncTree() {
            navigationService.syncTree({ tree: "Product", path: [-1, $routeParams.id], forceReload: true });
        }

        syncTree();
    }
    angular.module("umbraco").controller("Product.Controllers.ProductExportController", productExportController);
})()