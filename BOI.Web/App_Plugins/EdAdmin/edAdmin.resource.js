angular.module("umbraco.resources").factory("edAdminResources", function ($http) {
    "use strict";
    return {
        importHrefLang: function (file) {
            var request = {
                file: file
            };
            return $http({
                method: "POST",
                url: "backoffice/EdAdmin/EdAdminApi/ImportHrefLang",
                headers: { "Content-Type": undefined },
                transformRequest: function (data) {
                    var formData = new FormData();
                    formData.append("file", data.file);
                    return formData;
                },
                data: request
            }).then(function (response) {
                if (response) {
                    var fileName = response.data;
                    return fileName;
                } else {
                    return false;

                }
            });
        },
        importMeta: function (file) {
            var request = {
                file: file
            };
            return $http({
                method: "POST",
                url: "backoffice/EdAdmin/EdAdminApi/ImportMeta",
                headers: { "Content-Type": undefined },
                transformRequest: function (data) {
                    var formData = new FormData();
                    formData.append("file", data.file);
                    return formData;
                },
                data: request
            }).then(function (response) {
                if (response) {
                    var fileName = response.data;
                    return fileName;
                } else {
                    return false;
                }
            });
        },
        exportMeta: function () {
            return $http({
                method: "GET",
                url: "backoffice/EdAdmin/EdAdminApi/ExportMeta",
                cache: false,
                responseType: "arraybuffer"
            });
        },
        exportMembers: function () {
            return $http({
                method: "GET",
                url: "backoffice/EdAdmin/EdAdminApi/ExportMembers",
                cache: false,
                responseType: "arraybuffer"
            });
        }
    };
});