export default function persistentNav() {
    persistentNavHandler()
}

var globals = {
    navScrollTop: 0,
    resizeTimer: 0
}

$(window).scroll(function() {
    persistentNavHandler()
})

function persistentNavHandler() {
    var $header = $('.site-header')
    var start = $(window).scrollTop()

    /*
    ** If header is active, we don't care about making nav persistent
    ** It should always be shown
    */
    if (!$header.children().hasClass('nav-active')) {
        if (start > 125) {
            $('.upper-nav').slideUp();
            $header.addClass('compact');

            if (!(start > globals.navScrollTop)) {
                $header.removeClass('top');
            }
        } else {
            $header.addClass('top');
            $('.upper-nav').slideDown();
            $header.removeClass('compact');
        }

        /* Fix for iOS */
        if ($('html').hasClass('isios')) {
            if ($header.hasClass('top')) {
                $('.upper-nav').slideDown();
                $header.removeClass('compact');
            }
        }

        globals.navScrollTop = start
    }
}
