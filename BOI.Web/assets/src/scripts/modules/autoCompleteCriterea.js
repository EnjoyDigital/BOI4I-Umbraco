import 'devbridge-autocomplete'

export default function AutoCompleteSearch() {
    jQuery.fn.extend({
        autocompleteSearch: function (options) {
            var searchInput = $(this);
            this.autocomplete({
                serviceUrl: searchInput.closest('form').data('autocompleteurl'),
                
                paramName: "queryString",
                noCache: true,
                appendTo: searchInput.closest('.product-filters-input-wrap'),
                onSelect: function (suggestion) {

                    $.ajax({
                        type: 'POST',
                        url: searchInput.closest('form').data('ajaxaddurl'),
                        data: {
                            'criteriaName': suggestion.value, 'pageId': searchInput.closest('form').data('pageid'), 'searchCriteriaOnly': 'true'
                        },
                        success: function (data) {

                            if (searchInput.attr('id') === 'CriteriaName') {
                                $('#criteriaResult').html(data);
                            }
                            else if (searchInput.attr('id') === 'BuyToLetCriteriaName')
                                $('#buyToLetResult').html(data);
                            else
                                $('#bespokeResult').html(data);
                            searchInput.closest('.tab-content').find('.accordion-heading').first().click();
                        }
                    });


                },
                minChars: 3,
                onSearchComplete: function (query, suggestions) {
                }
            });
        }
    });

    $('#CriteriaName, #BuyToLetCriteriaName, #BespokeCriteriaName').each(function () {
        $(this).autocompleteSearch();
    });
}