export default function BackToTop() {
    handlers()
}

function handlers() {
    window.onscroll = function () {
        scrollfunction()
    };

    $(".back-to-top").click(function (e) { 
        e.preventDefault();
        e.stopPropagation();

        var critereaLookup = $(".criteria-lookup-container");

        if (critereaLookup && critereaLookup.length > 0) {
            $('html, body').animate({
                scrollTop: critereaLookup.first().offset().top - 140
            });

            critereaLookup.find('button, a, input, select, textarea, [tabindex]:not([tabindex="-1"])').first()[0].focus({
                preventScroll: true,
            });
        }
        else {
            $('html, body').animate({
                scrollTop: 0
            });

            // document.querySelector(':is(button, a, input, select, textarea, [tabindex]:not([tabindex="-1"])):not(.skip-to-content-link)').focus({
            //     preventScroll: true,
            // });
        }
    });
}

function scrollfunction() {
    var scrollTop = document.getElementById("backToTop");

    if (document.body.scrollTop > 20 || document.documentElement.scrollTop > 20) {
        scrollTop.classList.add('active');
    } else {
        scrollTop.classList.remove('active');
    }
}
