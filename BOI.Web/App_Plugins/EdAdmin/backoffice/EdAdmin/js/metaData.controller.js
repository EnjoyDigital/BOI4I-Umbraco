(function () {
    "use strict";

    function metaDataController($scope, $routeParams, $timeout, navigationService, notificationsService, edAdminResources, Upload) {
        var mdc = this;
        mdc.name = "Upload meta data";
        mdc.description = "Add or update meta data";
        mdc.invalidFileFormat = false;
        mdc.rebuildInput = 1;
        mdc.file = null;

        mdc.back = function () {
            mdc.invalidFileFormat = false;
            mdc.file = null;
            mdc.fileName = null;
            mdc.success = false;
            mdc.error = false;
            mdc.processing = false;
            mdc.processed = false;
            mdc.rebuildInput += 1;
            $('#file').val(null);
        }

        mdc.upload = function () {
            if (mdc.file === null) {
                mdc.noFile = true;
                $timeout(function () {
                    mdc.noFile = false;
                }, 500);
                return;
            }
            mdc.processing = true;

            Upload.upload({
                url: "backoffice/EdAdmin/EdAdminApi/importMeta",
                file: mdc.file
            }).success(function () {
                console.log("success");
                mdc.success = "File has been successfully uploaded. Please check that the meta data has been applied as expected."
                mdc.error = false;
                mdc.processing = false;
                mdc.processed = true;
            }).error(function (data) {

                mdc.error = true;
                mdc.success = false;
                mdc.processing = false;
                mdc.processed = true;
            });
        }

        $scope.$on("filesSelected", function (event, args) {
            if (args.files.length > 0) {
                mdc.file = args.files[0];
                mdc.fileName = mdc.file.name;
            } else if (args.files.length <= 0 || mdc.processing) {
                mdc.file = null;
                return;
            }

            mdc.noFile = false;

            var extension = mdc.fileName.substring(mdc.fileName.lastIndexOf(".") + 1, mdc.fileName.length).toLowerCase();
            if (extension !== 'csv') {
                mdc.invalidFileFormat = true;
                $timeout(function () {
                    mdc.rebuildInput += 1;
                    mdc.file = null;
                    mdc.invalidFileFormat = false;
                }, 500);
                return;
            }
        });

        //Export start
        function exportSuccess(response) {
            console.log(response);
            var linkElement = document.createElement("a");
            try {
                //var today = new moment();
                //var filename = "metadata-" + today.format("d-MMM-YYYY") + ".csv";
                //var blob = new Blob([response.data], { type: "text/csv" });
                //var url = window.URL.createObjectURL(blob);

                //linkElement.setAttribute("href", url);
                //linkElement.setAttribute("download", filename);

                //var clickEvent = new MouseEvent("click", {
                //    "view": window,
                //    "bubbles": true,
                //    "cancelable": false
                //});
                //linkElement.dispatchEvent(clickEvent);
                //mdc.exportProcessing = false;
                //notificationsService.success("Success", "File should now be downloading");
            } catch (ex) {
                exportFailed(ex);
            }
        }

        function exportFailed(ex) {
            notificationsService.error("Error", "Failed to create Meta Data CSV export. Please contact Administrator for assistance.");
            mdc.exportProcessing = false;
        }

        mdc.export = function () {
            mdc.exportProcessing = true;
            edAdminResources.exportMeta().then(exportSuccess, exportFailed);
        }
        //Export end

        function syncTree() {
            navigationService.syncTree({ tree: "EdAdmin", path: [-1, $routeParams.id], forceReload: true });
        }

        syncTree();
    }
    angular.module("umbraco").controller("EdAdmin.Controllers.MetaDataController", metaDataController);
})()