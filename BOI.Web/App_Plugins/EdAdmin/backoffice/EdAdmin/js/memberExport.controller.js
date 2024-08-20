(function () {
    "use strict";

    function memberExportController($scope, $routeParams, $timeout, navigationService, notificationsService, edAdminResources, Upload) {
        var mec = this;
        mec.name = "Export member data";
        mec.description = "Export all members to CSV";

        //Export start

        function exportSuccess(response) {
            var linkElement = document.createElement("a");
            try {
                var today = new moment();
                var filename = "MembersExport-" + today.format("DD-MMMM-YYYY") + ".csv";
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
            notificationsService.error("Error", "Failed to create Member CSV export. Please contact Administrator for assistance.");
            mec.exportProcessing = false;
        }

        mec.export = function () {
            mec.exportProcessing = true;
            edAdminResources.exportMembers().then(exportSuccess, exportFailed);
        }

        //Export end

        function syncTree() {
            navigationService.syncTree({ tree: "EdAdmin", path: [-1, $routeParams.id], forceReload: true });
        }

        syncTree();
    }
    angular.module("umbraco").controller("EdAdmin.Controllers.MemberExportController", memberExportController);
})()