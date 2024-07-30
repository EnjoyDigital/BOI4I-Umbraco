export default function MegaNav() {
    handlers()
}

function handlers () {
    var $headerItem = $('.header-bottom__menu > li'),
        $windowWidth = 0

    $headerItem.on('mouseenter', function() {
        var $subMenu = $(this).find('.header-bottom__menu--submenu')

        if ($subMenu.length < 1) {
            return false
        }

        // get window width
        $windowWidth = $(window).width()

        // get this submenu width and x offset
        var $subMenuWidth = $subMenu.outerWidth(),
            $subMenuXOffset = $subMenu.offset().left,
            $overallOffset = $windowWidth - $subMenuWidth - $subMenuXOffset

        if ($overallOffset < 0) {
            $subMenu.addClass('-align-right') 
        }
    })
}