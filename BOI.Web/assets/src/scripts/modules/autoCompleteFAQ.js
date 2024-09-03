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
                            'faqQuestion': suggestion.value, 'pageId': searchInput.closest('form').data('pageid'), 'searchFaqOnly': 'true'
                        },
                        success: function (data) {
                            $('#faqResult').html(data);
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

    $('#FAQName').each(function () {
        $(this).autocompleteSearch();
    });
}