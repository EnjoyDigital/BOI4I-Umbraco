import Globals from '../modules/globals/globals'
import TabbedContent from '../modules/tabbedContent'
import ProductFilters from '../modules/productFilters'
import AutoCompleteFAQ from '../modules/autoCompleteFAQ'

Globals()

// Individually required scripts
TabbedContent()
ProductFilters()
AutoCompleteFAQ()

$(document).on('click', '#FAQSearch', function (e) {
    e.preventDefault();
    var formUrl = $(this).data('ajaxaddurl');
    var pageId = $(this).data('pageid');

    var faqCategory = document.getElementById('FAQCategory').value;
    var faqQuestion = document.getElementById('FAQName').value;

    $.ajax({
        type: 'POST',
        url: formUrl,
        data: {
            'faqQuestion': faqQuestion, 'faqCategory': faqCategory, 'pageId': pageId
        },
        success: function (data) {
            $('#faqResult').html(data);
        }
    });
});

$(document).on('click', '#FAQClear', function (e) {
    e.preventDefault();

    var formUrl = $(this).data('ajaxaddurl');
    var pageId = $(this).data('pageid');

    $('#FAQCategory').val('');
    document.getElementById('FAQName').value = '';

    $.ajax({
        type: 'POST',
        url: formUrl,
        data: {
            'faqQuestion': '', 'faqCategory': '', 'pageId': pageId
        },
        success: function (data) {
            $('#faqResult').html(data);
        }
    });
});

//$(document).on('click', ('#searchBespokeCriteriaOnly'), function (e) {
//    e.preventDefault();
//    var searchButton = $(this);
//    var formUrl = searchButton.data('ajaxaddurl');
//    var pageId = searchButton.data('pageid');
//    var criteriaName = searchButton.data('search-term');

//    $.ajax({
//        type: 'POST',
//        url: formUrl,
//        data: {
//            'criteriaName': criteriaName, 'pageId': pageId, 'searchCriteriaOnly': 'true'
//        },
//        success: function (data) {
//            if (searchButton.attr('id') === 'searchResidentialCriteriaOnly')
//                $('#criteriaResult').html(data);
//            else if (searchButton.attr('id') === 'searchBuyToLetCriteriaOnly')
//                $('#buyToLetResult').html(data);
//            else
//                $('#bespokeResult').html(data);
//            searchButton.closest('.tab-content').find('.accordion-heading').first().click();
//        }
//    });
//});

//$('body').on('click', '.alphabet-list a', function (e) {
//    e.preventDefault()

//    /* We're grabbing the parent so that we make sure we stay within
//     * the corresponding tab
//     */
//    var $href = $(this).attr('href'),
//        $letterToShow = $(this).data('letter'),
//        $parentTab = $(this).parents('.tab-content'),
//        $criteriaCategory = $parentTab.find('.criteria-category[data-letter="' + $letterToShow + '"]')

//    var url = new URL(window.location.href); // Returns full URL (https://example.com/path/example.html)
//    url.hash = $href;

//    history.replaceState(null, null, url);

//    $('html,body').animate({
//        scrollTop: $criteriaCategory.offset().top - 120
//    }, 500, function () {
//        //after animation set the focus
//        var $categoryHeading = $criteriaCategory.find('h3');
//        if ($categoryHeading.first()) {
//            $categoryHeading = $categoryHeading.first();
//            $categoryHeading.focus();
//            if ($categoryHeading.is(":focus")) { // Checking if the target was focused
//                return false;
//            } else {
//                $categoryHeading.attr('tabindex', '-1'); // Adding tabindex for elements not focusable
//                $categoryHeading.focus(); // Set focus again
//            };
//        }
//    })
//})