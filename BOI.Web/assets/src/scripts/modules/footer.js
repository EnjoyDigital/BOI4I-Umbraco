export default function Footer() {
    // addActiveStates()
    // clickHandlers()

    scrollToTop();
}

function scrollToTop() {
    $(".scroll-top").click(function () {
        $("html, body").animate({ scrollTop: 0 }, "slow");
        return false;
    });
}

// help text
$(document).on('mouseenter', '.common-print', function (e) {
    $(this).addClass('active');
});
$(document).on('mouseleave', '.common-print', function (e) {
    $(this).removeClass('active');
});