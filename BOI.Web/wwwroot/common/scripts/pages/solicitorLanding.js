import Globals from '../modules/globals/globals'

Globals()

let $form = $('#solicitor-lookup')
let $listingSelector = "#solicitor-results"

var $postcodeField = $("#Postcode")
let $postcodeValidation = $('#postcode-message')


$form.on('submit', function (e) {
    let postcode = $postcodeField.val()
    let solicitorName = $('#SolicitorName').val()
    e.preventDefault()
    if (solicitorName.length == 0 && (postcode.length < 3 || !checkPostcode(postcode))) {
        $postcodeValidation.show()
        $postcodeValidation.attr('role', 'alert');
        $postcodeField.attr('aria-invalid', true);
        $postcodeField.attr('aria-describedby', $postcodeValidation.attr('id'));
        $postcodeField.focus();
    }
    else {
        $postcodeValidation.hide()
        $postcodeValidation.removeAttr('role');
        $postcodeField.removeAttr('aria-invalid');
        $postcodeField.removeAttr('aria-describedby');
        getContent($form.prop("action"), $form.serialize());
    }

})


$('.main').on('click', '.pagination a', function (e) {

    e.preventDefault();
    getContent($(this).attr('href'));
})

function getContent(url, data) {

    $.ajax({
        data: data,
        url: url,
        async: true,
        type: "GET",
        dataType: "HTML",
        success: function (data) {
            $postcodeValidation.hide()
            $postcodeValidation.removeAttr('role');
            $postcodeField.removeAttr('aria-invalid');
            $postcodeField.removeAttr('aria-describedby');
            $($listingSelector).replaceWith(data);
        },
        error: function (jqXHR, textStatus, errorThrown) {
            console.log(errorThrown);
        },
        complete: function () {

            $([document.documentElement, document.body]).animate({
                scrollTop: $("#solicitor-lookup").offset().top
            }, 1000);
        }
    });
}


function checkPostcode(postcode) {
    if (postcode != "") {
        const regex = /^[A-Za-z]{1,2}[0-9]{1,2}[A-Za-z]{0,1} ?[0-9][A-Za-z]{2}$/i

        if (regex.test(postcode)) {
            return true
        } else {
            return false
        }
    }
    else {
        return true
    }
}