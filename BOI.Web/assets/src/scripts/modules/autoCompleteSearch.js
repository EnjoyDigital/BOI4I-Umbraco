import 'devbridge-autocomplete'

export default function AutoCompleteCriterea() {
    var selector = '#searchResultForm';

    jQuery.fn.extend({
        autocompleteSearch: function (options) {
            var form = $(this).closest('form');
            var elem = this;
            var params = {};

            this.autocomplete({
                serviceUrl: form.data('autocompleteurl'),
                paramName: "queryString",
                params: params,
                noCache: true,
                appendTo: selector,
                onSelect: function (suggestion) {
                    if (options.submitOnSelect) {
                        if (suggestion.data == 'true') {
                            elem.val(params.queryString);
                            form.submit();
                        }
                        else {
                            elem.val(suggestion.value);
                            form.find('.trigger').val('Autocomplete');
                            form.submit();
                        }
                    }
                },
                minChars: 3,
                onSearchComplete: function (query, suggestions) {
                    console.log('main search complete')
                }
            });
        }
    });

    $('form.search-bar input[name="searchTerm"]').autocompleteSearch({
        submitOnSelect: true,
        formType: 'header'
    });

    $(document).on('mousedown', '.autocomplete-suggestion', function(e) {
        $(e.target).click();
    });
}