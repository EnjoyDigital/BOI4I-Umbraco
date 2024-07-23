export default function Navigation() {
    var $button = document.querySelector('.header-top__mobile--hamburger');
    var $mobileNav = document.querySelector('.header-bottom');
    var $mobileTop = document.querySelector('.header-top__content');
    var $main = document.querySelector('body');
    var $nav = document.querySelector('header');
    var $logo = document.querySelector('.header-logo');
    var $searchButton = document.querySelector('.header-bottom__menu--search');
    var $siteSearch = document.querySelector('.site-search');
    var $searchBox = document.querySelector('.search-bar');
    var $searchClose = document.querySelector('.search-bar__icons--grey');
    var $headerBottom = document.querySelector('.header-bottom__menu');
    var $mobileSubmenu = document.querySelectorAll('.show-submenu');
    var $mobileMenu = document.querySelectorAll('.header-bottom__menu--menu');
    if ($button) {
        $button.addEventListener('click', function() {
            if ($mobileNav.classList.contains('nav-active')) {
                $mobileNav.classList.remove('nav-active');
                $mobileTop.classList.remove('nav-active');
                $searchBox.classList.remove('search-active');
                $main.style.position = "static";
                $button.setAttribute('aria-label', 'Open main menu');
                $button.setAttribute('aria-expanded', false);
                $button.innerHTML = '<img src="/common/images/icons/icon-hamburger.svg">';
            } else {
                $mobileNav.classList.add('nav-active');
                $mobileTop.classList.add('nav-active');
                $searchBox.classList.add('search-active');
                $main.style.position = "fixed";
                $button.setAttribute('aria-label', 'Close main menu');
                $button.setAttribute('aria-expanded', true);
                $button.innerHTML = '<img src="/common/images/icons/icon-mobile-menu-close.svg">';
                while ($mobileTop.childNodes.length > 0) { 
                    $headerBottom.appendChild($mobileTop.childNodes[0]);
                }
                while ($searchBox.childNodes.length > 0) {
                    $headerBottom.appendChild($siteSearch.childNodes[0]);
                }
            }
        });
    }


    if ($searchButton != null) {
        $searchButton.addEventListener('click', function () {
            $searchButton.setAttribute('aria-expanded', true);
            $searchBox.classList.add('search-active');
            $siteSearch.classList.add('search-active');
            $mobileNav.classList.remove('bottom-nav-active');
            $("#searchTerm").focus();
        });
    }

    $searchClose.addEventListener('click', function () {
        $searchButton.setAttribute('aria-expanded', false);
        $searchBox.classList.remove('search-active');
        $mobileNav.classList.add('bottom-nav-active');
        $siteSearch.classList.remove('search-active');
    });

    mobileSubmenu();

    function mobileSubmenu() {
        for (let i = 0; i < $mobileSubmenu.length; i++) {
            $mobileSubmenu[i].parentElement.addEventListener('focusout', function (e) {
                // If focus is still in the parent, do nothing
                if ($mobileSubmenu[i].parentElement.contains(e.relatedTarget)) return;

                // If on mobile do nothing
                if ($(window).width() < 768) return;

                var $parent = $mobileSubmenu[i].parentElement;
                var $sibling = $mobileSubmenu[i].nextElementSibling;
                var $topLevelLink = $mobileSubmenu[i].previousElementSibling;

                $sibling.style.display = "";
                $parent.classList.remove('submenu-open');
                $headerBottom.style.height = "auto";
                $topLevelLink.setAttribute('aria-expanded', false)
            });

            $mobileSubmenu[i].parentElement.addEventListener('click', function (e) {
                if (e.detail == 1) { //is actually a click event, not a keyboard event in disguise
                    if (typeof (e.target.href) != 'undefined') {
                        if (e.target.href == '' || e.target.href.slice(-1) == '#') {
                            e.preventDefault();
                        }
                    }
                }
                else {
                    if (e.target.getAttribute('aria-haspopup') == 'true') {
                        e.preventDefault();
                    }
                }
                var $parent = $mobileSubmenu[i].parentElement;
                var $sibling = $mobileSubmenu[i].nextElementSibling;
                var $topLevelLink = $mobileSubmenu[i].previousElementSibling;

                if ($parent.classList.contains('submenu-open')) {
                    $sibling.style.display = "";
                    $parent.classList.remove('submenu-open');
                    $headerBottom.style.height = "auto";
                    $topLevelLink.setAttribute('aria-expanded', false)
                } else {
                    $sibling.style.display = "flex";
                    $parent.classList.add('submenu-open');
                    $headerBottom.style.height = "calc(100% + 140px)";
                    $topLevelLink.setAttribute('aria-expanded', true)
                }
            });
        }
    }

    function stickyNavigation() { 
        if (window.scrollY > 200) {
            $nav.classList.add('nav-scroll');
        } else {
            $nav.classList.remove('nav-scroll');
        }
    }

    window.addEventListener('scroll', stickyNavigation);
}