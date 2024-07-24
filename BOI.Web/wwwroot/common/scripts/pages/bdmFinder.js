import Globals from '../modules/globals/globals'

Globals()

var $bdmText = $('.js-bdm-text');
var $form = $('#bdm-finder');
var $listing = $('#bdm-listing');
var $postcodeField = $("#postcode")
var $postcodeValidation = $('#postcode-message')
var $fcaField = $("#fcaNumber")
var $fcaValidation = $('#fca-message')
var $buttonWrap = $('.-button-wrap');

$(document).ready(function () {
    $listing.hide()
})

$(document).on('click', '.js-bdm-text', function () {
    var $shortText = $(this).data('short-text'),
        $fullText = $(this).data('full-text')

    if ($(this).hasClass('-revealed')) {
        $(this).html($shortText)
        $(this).removeClass('-revealed')
        $(this).attr('data-toggle', 'more')
    } else {
        $(this).html($fullText)
        $(this).addClass('-revealed')
        $(this).attr('data-toggle', 'less')
    }
})

$form.on('submit', function (e) {
    e.preventDefault()
    let postcode = $("#postcode").val().trim();
    let fcaNumber = $("#fcaNumber").val().trim();
    let valFail = false;
    if (!validateFCA(fcaNumber)) {
        valFail = true;

        $fcaValidation.show();
        $fcaValidation.attr('role', 'alert');
        $fcaField.attr('aria-invalid', true);
        $fcaField.attr('aria-describedby', $postcodeValidation.attr('id'));
        $fcaField.focus();
    }
    if (postcode === '' || !checkPostcode(postcode)) {
        valFail = true;
        $postcodeValidation.show();
        $postcodeValidation.attr('role', 'alert');
        $postcodeField.attr('aria-invalid', true);
        $postcodeField.attr('aria-describedby', $postcodeValidation.attr('id'));
        $postcodeField.focus();
    }
    if (valFail) {
        return;
    }
    $.ajax({
        data: $form.serialize(),
        async: true,
        type: "GET",
        dataType: "HTML",
        success: function (data) {
            $postcodeValidation.hide()
            $postcodeValidation.removeAttr('role');
            $postcodeField.removeAttr('aria-invalid');
            $postcodeField.removeAttr('aria-describedby');

            $fcaValidation.hide()
            $fcaValidation.removeAttr('role');
            $fcaField.removeAttr('aria-invalid');
            $fcaField.removeAttr('aria-describedby');

            $listing.html(data);
            $listing.show();
        },
        error: function (jqXHR, textStatus, errorThrown) {

        },
        complete: function () {
            //scroll to top
        }
    });
})

function validateFCA(fcaCodce) {

    if (fcaCodce.length < 6) {
        return false;
    }

    if (isNaN(fcaCodce)) {
        return false;
    }

    return true;
}

function checkPostcode(postcode) {
    const regex = /^[A-Za-z]{1,2}[0-9]{1,2}[A-Za-z]{0,1} ?[0-9][A-Za-z]{2}$/i

    if (regex.test(postcode)) {
        return true
    } else {
        return false
    }
}