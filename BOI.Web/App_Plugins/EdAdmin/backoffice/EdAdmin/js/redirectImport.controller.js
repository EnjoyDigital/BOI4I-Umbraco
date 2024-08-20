(function () {
    "use strict";

    function redirectImportController($scope, $routeParams, $timeout, navigationService, notificationsService, edAdminResources, Upload) {
        var ric = this;
        ric.name = "Import redirects";
        ric.description = "Import redirects will appear on the dashboard in the 'Redirects' tab"
        ric.invalidFileFormat = false;
        ric.rebuildInput = 1;
        ric.file = null;

        ric.back = function () {
            ric.invalidFileFormat = false;
            ric.file = null;
            ric.fileName = null;
            ric.success = false;
            ric.error = false;
            ric.processing = false;
            ric.processed = false;
            ric.rebuildInput += 1;
            $('#file').val(null);
        }

        ric.upload = function () {
            if (ric.file === null) {
                ric.noFile = true;
                $timeout(function () {
                    ric.noFile = false;
                }, 500);
                return;
            }
            ric.processing = true;

            Upload.upload({
                url: "backoffice/EdAdmin/EdAdminApi/importRedirects",
                file: ric.file
            }).success(function (response) {

                if (response === "InternalServerError") {
                    ric.error = "An error has occured, please check your CSV and consult the Umbraco Logs.";
                    ric.success = false;
                } else {
                    ric.success = "File has been successfully uploaded. Please check that the redirect data has been applied as expected."
                    ric.error = false;
                }

                ric.processing = false;
                ric.processed = true;
            }).error(function (data) {

                ric.error = "An error has occured, please check your CSV and consult the Umbraco Logs.";

                //if (data.Errors.length > 0) {
                //    ric.error = data.Errors;
                //} else {
                //    pic.error = ["An error has occured, please check your CSV."];
                //}
                ric.success = false;
                ric.processing = false;
                ric.processed = true;
            });
        }

        $scope.$on("filesSelected", function (event, args) {
            if (args.files.length > 0) {
                ric.file = args.files[0];
                ric.fileName = ric.file.name;
            } else if (args.files.length <= 0 || ric.processing) {
                ric.file = null;
                return;
            }

            ric.noFile = false;

            var extension = ric.fileName.substring(ric.fileName.lastIndexOf(".") + 1, ric.fileName.length).toLowerCase();
            if (extension !== 'csv') {
                ric.invalidFileFormat = true;
                $timeout(function () {
                    ric.rebuildInput += 1;
                    ric.file = null;
                    ric.invalidFileFormat = false;
                }, 500);
                return;
            }
        });

        //Export start
        function exportSuccess(response) {
            var linkElement = document.createElement("a");
            try {
                var today = new moment();
                var filename = "redirects-" + today.format("d-MMM-YYYY") + ".csv";
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
                ric.exportProcessing = false;
                notificationsService.success("Success", "File should now be downloading");
            } catch (ex) {
                exportFailed(ex);
            }
        }

        function exportFailed(ex) {
            notificationsService.error("Error", "Failed to create Redirects CSV export. Please contact Administrator for assistance.");
            ric.exportProcessing = false;
        }

        ric.export = function () {
            ric.exportProcessing = true;
            edAdminResources.exportRedirects().then(exportSuccess, exportFailed);
        }
        //Export end

        function syncTree() {
            navigationService.syncTree({ tree: "EdAdmin", path: [-1, $routeParams.id], forceReload: true });
        }

        syncTree();
    }
    angular.module("umbraco").controller("EdAdmin.Controllers.RedirectImportController", redirectImportController);
})()