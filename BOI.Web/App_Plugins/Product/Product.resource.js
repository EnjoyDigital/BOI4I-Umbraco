angular.module("umbraco.resources").factory("productResources", function ($http) {
    "use strict";
    return {
        uploadFile: function (file) {
            var request = {
                file: file
            };
            return $http({
                method: 'POST',
                url: "backoffice/Product/ProductApi/ProcessFile",
                headers: { 'Content-Type': undefined },
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
            return true;
        },
        exportProducts: function () {
            return $http({
                method: "GET",
                url: "backoffice/Product/ProductApi/ExportProducts",
                cache: false,
                responseType: "arraybuffer"
            });
        },
    };
});