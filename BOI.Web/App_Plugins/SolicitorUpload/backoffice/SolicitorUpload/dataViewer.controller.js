//angular.module("umbraco").controller("SolicitorUploadImportController",
//    function ($rootScope, $scope, $timeout, solicitorUploadResource, Upload, notificationsService) {
//        'use strict';
//        $scope.exportUrl = solicitorUploadResource.ExportUrl;
//        $scope.filesUploading = [];
//        $scope.csvData = [];
//        $scope.fileUploaded = false;
//        $scope.uploadInProgress = false;
//        $scope.invalidFileFormat = false;
//        $scope.rebuildInput = 1;
//        $scope.file = null;

//        console.log('xyz');
//        //$scope.uploadFile = function (file) {
//        //    $scope.file = file;
//        //    $scope.fileUploaded = true;
//        //    Upload.upload({
//        //        url: solicitorUploadResource.postAddFileUrl,
//        //        file: $scope.file,
//        //        fileUploaded: true,
//        //        maxNumberOfFiles: 1,
//        //        acceptFileTypes: /(\.|\/)(csv)$/i
//        //    });
//        //};

//        $scope.acceptFile = function () {
//            console.log('abc');
//            $scope.file = file;
//            $scope.fileUploaded = true;
//            Upload.upload({
//                url: solicitorUploadResource.postAddFileUrl,
//                file: $scope.file,
//                fileUploaded: true,
//                maxNumberOfFiles: 1,
//                acceptFileTypes: /(\.|\/)(csv)$/i
//            });

//            if ($scope.uploadInProgress === false) {
//                $scope.uploadInProgress = true;
//                solicitorUploadResource.ProcessFile()
//                    .success(function () {
//                        notificationsService.success('The new file has successfully been added');
//                        $scope.uploadInProgress = false;
//                    }).error(function (d) {
//                        notificationsService.error("Error processing file " + d.Message);
//                        $scope.uploadInProgress = false;
//                    });
//            }

//        };

//        $scope.deleteFile = function () {
//            console.log('deleteFile');

//            solicitorUploadResource.DeleteFile().success(function () {
//                notificationsService.success('File removed');
//                $scope.fileUploaded = false;
//            }).error(function (d) {
//                notificationsService.error("Error deleting file " + d.Message);
//            });
//        };

//        $scope.$on('fileuploadstop', function () {
//            console.log('fileuploadstop');

//            $scope.queue = [];
//            $scope.filesUploading = [];
//        });

//        $scope.$on('fileuploadprocessalways', function (e, data) {
//            console.log('fileuploadprocessalways');

//            $scope.$apply(function () {
//                $scope.filesUploading.push(data.files[data.index]);
//            });
//        });

//        $scope.$on('fileuploaddragover', function () {
//            console.log('fileuploaddragover');

//            if (!$scope.dragClearTimeout) {
//                $scope.$apply(function () {
//                    $scope.dropping = true;
//                });
//            } else {
//                $timeout.cancel($scope.dragClearTimeout);
//            }
//            $scope.dragClearTimeout = $timeout(function () {
//                $scope.dropping = null;
//                $scope.dragClearTimeout = null;
//            }, 300);
//        });
//    });

//$scope.$on("filesSelected", function (event, args) {
//    console.log('filesSelected');
//    if (args.files.length > 0) {
//        $scope.file = args.files[0];
//        $scope.fileName = $scope.file.name;
//    } else if (args.files.length <= 0 || $scope.uploadInProgress) {
//        $scope.file = null;
//        return;
//    }

//    $scope.noFile = false;

//    var extension = $scope.fileName.substring($scope.fileName.lastIndexOf(".") + 1, $scope.fileName.length).toLowerCase();
//    if (extension !== 'csv') {
//        $scope.invalidFileFormat = true;
//        $timeout(function () {
//            $scope.rebuildInput += 1;
//            $scope.file = null;
//            $scope.invalidFileFormat = false;
//        }, 500);
//        return;
//    }
//});

//$scope.back = function () {
//    $scope.invalidFileFormat = false;
//    $scope.file = null;
//    $scope.fileName = null;
//    $scope.success = false;
//    $scope.error = false;
//    $scope.uploadInProgress = false;
//    $scope.fileUploaded = false;
//    $scope.rebuildInput += 1;
//    $('#file').val(null);
//}

angular.module("umbraco").controller("SolicitorUploadImportController",
    function ($scope, $timeout, Upload) {
        "use strict";

        var vm = this;
        vm.invalidFileFormat = false;
        vm.rebuildInput = 1;
        vm.file = null;

        vm.back = function () {
            vm.invalidFileFormat = false;
            vm.file = null;
            vm.fileName = null;
            vm.success = false;
            vm.error = false;
            vm.processing = false;
            vm.processed = false;
            vm.rebuildInput += 1;
            $('#file').val(null);

        }

        vm.upload = function () {
            console.log('dataviewer controller');

            if (vm.file === null) {
                vm.noFile = true;
                $timeout(function () {
                    vm.noFile = false;
                }, 500);
                return;
            }
            vm.processing = true;

            Upload.upload({
                url: "backoffice/SolicitorUpload/SolicitorUploadApi/ProcessFile",
                file: vm.file
            }).success(function () {
                vm.success = "File has been successfully uploaded. Please allow around 5 minutes for the file to be processed, depending on the amount of records to be processed."
                vm.error = false;
                vm.processing = false;
                vm.processed = true;
            }).error(function (data) {
                if (data.Errors) {
                    vm.error = '';
                    $.each(data.Errors, function () {
                        vm.error += this.Message;

                    });

                } else {
                    vm.error = "An error has occured";
                }
                vm.success = false;
                vm.processing = false;
                vm.processed = true;
            });
        }

        $scope.$on("filesSelected", function (event, args) {
            if (args.files.length > 0) {
                vm.file = args.files[0];
                vm.fileName = vm.file.name;
            } else if (args.files.length <= 0 || vm.processing) {
                vm.file = null;
                return;
            }

            vm.noFile = false;

            var extension = vm.fileName.substring(vm.fileName.lastIndexOf(".") + 1, vm.fileName.length).toLowerCase();
            if (extension !== 'csv') {
                vm.invalidFileFormat = true;
                $timeout(function () {
                    vm.rebuildInput += 1;
                    vm.file = null;
                    vm.invalidFileFormat = false;
                }, 500);
                return;
            }
        });

    });