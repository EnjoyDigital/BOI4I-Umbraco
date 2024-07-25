(function () {
    "use strict";
    function EmbedController($scope, editorState, contentResource, elasticSearchPublishResources) {

        let vm = this;
        const currentNodeId = editorState.current.id;
      
        vm.processing = false;
        vm.processingComplete = false;
        vm.errorMessage = "";
        vm.indexingFailed = false;

        vm.indexItem = function () {
            vm.processingComplete = false;
            vm.processing = true;
            vm.indexingFailed = false;
            vm.errorMessage = "";
            elasticSearchPublishResources.indexItem(currentNodeId).then(indexingSucess, indexingFailed);
        }

        vm.indexItemAndDescendants = function () {
            elasticSearchPublishResources.indexWithDescendants(currentNodeId).then(
                indexingSucess, indexingFailed
            );

        }
        function indexingSucess() {
            vm.processing = false;
            vm.processingComplete = true;
        }
        function indexingFailed(ex) {
            vm.indexingFailed = true;
            vm.processing = false;
            vm.errorMessage = ex;

        }
       
    }
    angular.module("umbraco").controller("Enjoy.ElasticSearchPublishController", EmbedController);
})();