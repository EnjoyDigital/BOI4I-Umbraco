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