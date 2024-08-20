(function () {
    "use strict";

    function hrefLangController($scope, $routeParams, $timeout, navigationService, notificationsService, edAdminResources, Upload) {
        var hlc = this;
        hlc.name = "Upload hreflang data";
        hlc.description = "Add or update hreflang data";
        hlc.invalidFileFormat = false;
        hlc.rebuildInput = 1;
        hlc.file = null;

        hlc.back = function () {
            hlc.invalidFileFormat = false;
            hlc.file = null;
            hlc.fileName = null;
            hlc.success = false;
            hlc.error = false;
            hlc.processing = false;
            hlc.processed = false;
            hlc.rebuildInput += 1;
            $('#file').val(null);
        }

        hlc.upload = function () {
            if (hlc.file === null) {
                hlc.noFile = true;
                $timeout(function () {
                    hlc.noFile = false;
                }, 500);
                return;
            }
            hlc.processing = true;

            Upload.upload({
                url: "backoffice/EdAdmin/EdAdminApi/ImportHrefLang",
                file: hlc.file
            }).success(function () {
                hlc.success = "File has been successfully uploaded. Please check that the meta data has been applied as expected."
                hlc.error = false;
                hlc.processing = false;
                hlc.processed = true;
            }).error(function (data) {
                if (data.Errors.length > 0) {
                    hlc.error = data.Errors;
                } else {
                    hlc.error = ["An error has occured, please check your XML."];
                }
                hlc.success = false;
                hlc.processing = false;
                hlc.processed = true;
            });
        }

        $scope.$on("filesSelected", function (event, args) {
            if (args.files.length > 0) {
                hlc.file = args.files[0];
                hlc.fileName = hlc.file.name;
            } else if (args.files.length <= 0 || hlc.processing) {
                hlc.file = null;
                return;
            }

            hlc.noFile = false;

            var extension = hlc.fileName.substring(hlc.fileName.lastIndexOf(".") + 1, hlc.fileName.length).toLowerCase();
            if (extension !== 'xml') {
                hlc.invalidFileFormat = true;
                $timeout(function () {
                    hlc.rebuildInput += 1;
                    hlc.file = null;
                    hlc.invalidFileFormat = false;
                }, 500);
                return;
            }
        });

        function syncTree() {
            navigationService.syncTree({ tree: "EdAdmin", path: [-1, $routeParams.id], forceReload: true });
        }

        syncTree();
    }
    angular.module("umbraco").controller("EdAdmin.Controllers.HrefLangController", hrefLangController);
})()