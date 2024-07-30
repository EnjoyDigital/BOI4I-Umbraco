import Globals from '../modules/globals/globals'
import TabbedContent from '../modules/tabbedContent'
import ProductFilters from '../modules/productFilters'
import AutoCompleteCriterea from '../modules/autoCompleteCriterea'

Globals()

// Individually required scripts
TabbedContent()
ProductFilters()
AutoCompleteCriterea()

console.log('criteria');

$(document).on('click', '#ResidentialSearch', function (e) {
    e.preventDefault();
    var formUrl = $(this).data('ajaxaddurl');
    var pageId = $(this).data('pageid');

    var criteriaCategory = document.getElementById('CriteriaCategory').value;
    var criteriaName = document.getElementById('CriteriaName').value;

    $.ajax({
        type: 'POST',
        url: formUrl,
        data: {
            'criteriaName': criteriaName, 'criteriaCategory': criteriaCategory, 'pageId': pageId
        },
        success: function (data) {
            $('#criteriaResult').html(data);
        }
    });
});

$(document).on('click', '#ResidentialClear', function (e) {
    e.preventDefault();

    var formUrl = $(this).data('ajaxaddurl');
    var pageId = $(this).data('pageid');

    $('#CriteriaCategory').val('');
    document.getElementById('CriteriaName').value = '';

    $.ajax({
        type: 'POST',
        url: formUrl,
        data: {
            'criteriaName': '', 'criteriaCategory': '', 'pageId': pageId
        },
        success: function (data) {
            $('#criteriaResult').html(data);
        }
    });
});

$(document).on('click', '#BuyToLetSearch', function (e) {
    e.preventDefault();

    var formUrl = $(this).data('ajaxurl');
    var pageId = $(this).data('pageid');

    var criteriaCategory = document.getElementById('BuyToLetCriteriaCategory').value;
    var criteriaName = document.getElementById('BuyToLetCriteriaName').value;

    $.ajax({
        type: 'POST',
        url: formUrl,
        data: {
            'criteriaName': criteriaName, 'criteriaCategory': criteriaCategory, 'pageId': pageId
        },
        success: function (data) {
            $('#buyToLetResult').html(data);
        }
    });
});

$(document).on('click', '#BuyToLetClear', function (e) {
    e.preventDefault();

    var formUrl = $(this).data('ajaxurl');
    var pageId = $(this).data('pageid');

    $('#BuyToLetCriteriaCategory').val('');
    document.getElementById('BuyToLetCriteriaName').value = '';

    $.ajax({
        type: 'POST',
        url: formUrl,
        data: {
            'criteriaName': '', 'criteriaCategory': '', 'pageId': pageId
        },
        success: function (data) {
            $('#buyToLetResult').html(data);
        }
    });
});

$(document).on('click', '#BespokeSearch', function (e) {
    e.preventDefault();

    var formUrl = $(this).data('ajaxurl');
    var pageId = $(this).data('pageid');

    var criteriaCategory = document.getElementById('BespokeCriteriaCategory').value;
    var criteriaName = document.getElementById('BespokeCriteriaName').value;

    $.ajax({
        type: 'POST',
        url: formUrl,
        data: {
            'criteriaName': criteriaName, 'criteriaCategory': criteriaCategory, 'pageId': pageId
        },
        success: function (data) {
            $('#bespokeResult').html(data);
        }
    });
});

$(document).on('click', '#BespokeClear', function (e) {
    e.preventDefault();

    var formUrl = $(this).data('ajaxurl');
    var pageId = $(this).data('pageid');

    $('#BespokeCriteriaCategory').val('');
    document.getElementById('BespokeCriteriaName').value = '';

    $.ajax({
        type: 'POST',
        url: formUrl,
        data: {
            'criteriaName': '', 'criteriaCategory': '', 'pageId': pageId
        },
        success: function (data) {
            $('#bespokeResult').html(data);
        }
    });
});

$(document).on('click', ('#searchBespokeCriteriaOnly, #searchBuyToLetCriteriaOnly, #searchResidentialCriteriaOnly'), function (e) {
    e.preventDefault();
    var searchButton = $(this);
    var formUrl = searchButton.data('ajaxaddurl');
    var pageId = searchButton.data('pageid');
    var criteriaName = searchButton.data('search-term');

    $.ajax({
        type: 'POST',
        url: formUrl,
        data: {
            'criteriaName': criteriaName, 'pageId': pageId, 'searchCriteriaOnly': 'true'
        },
        success: function (data) {
            if (searchButton.attr('id') === 'searchResidentialCriteriaOnly')
                $('#criteriaResult').html(data);
            else if (searchButton.attr('id') === 'searchBuyToLetCriteriaOnly')
                $('#buyToLetResult').html(data);
            else
                $('#bespokeResult').html(data);
            searchButton.closest('.tab-content').find('.accordion-heading').first().click();
        }
    });
});

$('body').on('click', '.alphabet-list a', function (e) {
    e.preventDefault()

    /* We're grabbing the parent so that we make sure we stay within
     * the corresponding tab
     */
    var $href = $(this).attr('href'),
        $letterToShow = $(this).data('letter'),
        $parentTab = $(this).parents('.tab-content'),
        $criteriaCategory = $parentTab.find('.criteria-category[data-letter="' + $letterToShow + '"]')

    var url = new URL(window.location.href); // Returns full URL (https://example.com/path/example.html)
    url.hash = $href;

    history.replaceState(null, null, url);

    $('html,body').animate({
        scrollTop: $criteriaCategory.offset().top - 120
    }, 500, function () {
        //after animation set the focus
        var $categoryHeading = $criteriaCategory.find('h3');
        if ($categoryHeading.first()) {
            $categoryHeading = $categoryHeading.first();
            $categoryHeading.focus();
            if ($categoryHeading.is(":focus")) { // Checking if the target was focused
                return false;
            } else {
                $categoryHeading.attr('tabindex', '-1'); // Adding tabindex for elements not focusable
                $categoryHeading.focus(); // Set focus again
            };
        }
    })
})