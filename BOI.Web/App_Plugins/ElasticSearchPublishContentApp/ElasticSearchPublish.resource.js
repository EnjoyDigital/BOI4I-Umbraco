angular.module("umbraco.resources").factory("elasticSearchPublishResources", function ($http) {
    "use strict";
    return {
        indexItem: function (id) {
            return $http({
                method: "POST",
                url: "backoffice/ElasticSearchPublish/ElasticSearchPublishApi/IndexContentItem?id="+id,
                cache: false,                
            });
        },
        indexWithDescendants: function (id) {

            return $http({
                method: "POST",
                url: "backoffice/ElasticSearchPublish/ElasticSearchPublishApi/IndexContentItemWithDescendants?id=" + id,
                cache: false,
            });
            
        }

      
    };
});