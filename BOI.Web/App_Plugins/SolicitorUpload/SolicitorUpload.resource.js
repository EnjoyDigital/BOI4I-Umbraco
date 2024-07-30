angular.module("umbraco.resources").factory("solicitorUploadResources", function ($http) {
    "use strict";
    return {
        uploadFile: function (file) {
            console.log('resources');
            var request = {
                file: file
            };
            return $http({
                method: 'POST',
                url: "backoffice/SolicitorUpload/SolicitorUploadApi/ProcessFile",
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
        }
    };
});