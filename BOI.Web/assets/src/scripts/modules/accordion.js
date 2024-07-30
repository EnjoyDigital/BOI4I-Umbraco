export default function accordion() {
    $(document).on('click', '.accordion-heading', accToggle);
    $(document).on('click', '.accordion-copylink > button', copyHyperlink);
    $(document).ready(function () { checkAutoOpen(window.location.hash, 200, 250); });
    $(document).ready(function () { addAccordionOpenEvent(); });
}


function addAccordionOpenEvent() {
    $("a[href^='#']").click(function (e) {
        let hrefValue = $(this).prop('href');
        let hashIndex = hrefValue.indexOf('#');
        let idFromHash = hrefValue.substr(hashIndex, hrefValue.length - hashIndex);
        if (idFromHash != '#') {
            let element = $(idFromHash);
            if (element.hasClass('accordion')) {
                e.preventDefault();
                history.pushState(null, null, idFromHash);

                var divActiveAccordions = $('div.accordion.active');
                divActiveAccordions.each(function (index, element) {
                    closeAccordion($(element))
                });
                checkAutoOpen(idFromHash, 0, 250);
            }
        }
    });
}

function checkAutoOpen(idHash, additonalOffset, wait = 0) {
    var accordions = [];

    if (idHash.length > 0 && idHash !== '#tabs') {
        var targetedAccordion;

        if (window.location.href.indexOf('?isBuyToLet') < 1 && window.location.href.indexOf('?isBespoke') < 1) {
            targetedAccordion = $('#ResidentialCriteria .criteria-category ' + idHash + '.accordion').first();
        }
        else if (window.location.href.indexOf('?isBuyToLet') >= 1 && window.location.href.indexOf('?isBespoke') < 1) {
            targetedAccordion = $('#BuyToLetCriteria .criteria-category ' + idHash + '.accordion').first();
        }
        else if (window.location.href.indexOf('?isBuyToLet') < 1 && window.location.href.indexOf('?isBespoke') >= 1) {
            targetedAccordion = $('#BespokeCriteria .criteria-category ' + idHash + '.accordion').first();
        }

        if (targetedAccordion.length == 0 && $(idHash + '.accordion').length > 0) {
            targetedAccordion = $(idHash + '.accordion').first();
        }

        if (targetedAccordion) {
            accordions.push(...targetedAccordion.parents('.accordion'), targetedAccordion);

            if (accordions && accordions.length > 0) {
                accordions.forEach(accordion => {
                    var accordionHeader = $(accordion).find('> .accordion-heading');

                    if (accordionHeader && accordionHeader.length > 0) {
                        accordionHeader.click();
                    }
                });

                setTimeout(function () {
                    $('html, body').animate({
                        scrollTop: targetedAccordion.offset().top - (140 + additonalOffset)
                    });
                }, wait);
            }
        }
    }
}

function accToggle() {
    var accordion = $(this).parent('.accordion');
    var siblingAccordion = accordion.siblings('.accordion');

    if (accordion.hasClass('active')) {
        closeAccordion(accordion);
        return false
    }

    if (accordion.parent().hasClass("faq-block")) {
        //gross, but works to close multiple levels of accordions within faq-blocks
        closeAccordion(accordion.parent().parent().find(".faq-block .accordion"));
    }
    else if (accordion.parent().hasClass("criteria-category")) {
        //gross, but works to close multiple levels of accordions within criterea-category
        closeAccordion(accordion.parent().parent().find(".criteria-category .accordion"));
    }
    else if (accordion.parent().hasClass("accordion-content")) {
        //gross, but works to close multiple levels of accordions within criterea-category
        closeAccordion(accordion.parent().parent().find(".accordion-content .accordion"));
    }
    else {
        closeAccordion(siblingAccordion);
    }

    openAccordion(accordion);
}

function openAccordion(accordion) {
    accordion.addClass('active');
    accordion.find('.accordion-heading').attr('aria-expanded', true);
    accordion.find('> .accordion-content').slideDown(250);
}

function closeAccordion(accordion) {
    accordion.removeClass('active');
    accordion.find('.accordion-heading').attr('aria-expanded', false);
    accordion.find('> .accordion-content').slideUp(250);
}

function copyHyperlink(e) {
    e.preventDefault();

    var url = location.origin + location.pathname + location.search + '#' + $(this).parent().parent().parent().attr("id");

    navigator.clipboard.writeText(url).then(
        () => {
            /* clipboard successfully set */
            var alert = $('<div class="accordion-copylink-alert" role="alert"><svg class="[ icon -white ]"><use xlink:href="#sprite-icon-tick-white"></use></svg><span class="sr-only">Link </span>Copied to clipboard</div>');
            alert.insertAfter($(this));
            setTimeout(function () {
                alert.remove();
            }, 5000)
        },
        () => {
            /* clipboard write failed */
            var alert = $('<div class="accordion-copylink-alert" role="alert"><svg class="[ icon -white ]"><use xlink:href="#sprite-icon-alert"></use></svg>Failed to copy to clipboard. Please manually copy the url from the address bar.</div>');
            alert.insertAfter($(this));
            setTimeout(function () {
                alert.remove();
            }, 5000)
        }
    );
}