angular.module("umbraco").controller("ProductImportController",
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
            
            if (vm.file === null) {
                vm.noFile = true;
                $timeout(function () {
                    vm.noFile = false;
                }, 500);
                return;
            }

            if (!vm.processing) {

                vm.processing = true;

                Upload.upload({
                    url: "backoffice/Product/ProductApi/ProcessFile",
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
            if (extension !== 'csv' && extension !== 'json') {
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