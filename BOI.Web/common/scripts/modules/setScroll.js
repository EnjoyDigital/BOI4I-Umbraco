export default function setScroll() {
    $(document).ready(function () { scrollTop(500); });
}


function scrollTop(margin) {
    $("a[href^='#']").click(function (e) {
        let hrefValue = $(e.currentTarget).prop('href');
        let hashIndex = hrefValue.indexOf('#');
        let idFromHash = hrefValue.substr(hashIndex, hrefValue.length - hashIndex);
        if (idFromHash != '#') {
            let element = $(idFromHash);

            if (element && !element.hasClass('accordion', 'back-to-top')) {
                $('html, body').animate({
                    scrollTop: element.offset().top - (margin)
                });
            }
        }
    });
}