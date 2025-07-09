export default function TabbedContent(tabsOnly) {
    init(tabsOnly)
}

function init(tabsOnly) {
    var mobileNavBreakpoint = 767;

   
    function TabBindings() {
        $('.tab-list .tab > button').on('click', Tab);
        $('.tab-list').on('keydown', TabByKey);
    };

    function AccordionBindings() {
        $('.tab-accordion').on('click', Accordion);
    };

    function TabByKey(e) {
        if (e.key === "ArrowRight" || e.key === "ArrowLeft") {
            var tabParent = $(this).parent();
            var tab = $(this).find('.tab > button[aria-selected="true"]').parent();

            if (e.key === "ArrowRight") {
                var nextTab = tab.next();
                if (nextTab.length > 0) {
                    swapTabs(tabParent, nextTab.find('> button').attr('aria-controls'));
                }
            }
            if (e.key === "ArrowLeft") {
                var prevTab = tab.prev();
                if (prevTab.length > 0) {
                    swapTabs(tabParent, prevTab.find('> button').attr('aria-controls'));
                }
            }
        }
    };

    function Tab() {
        var tabParent = $(this).parents('.tabbed-content');
        var tab = $(this);

        swapTabs(tabParent, tab.attr('aria-controls'));
    };

    function Accordion() {
        var tabParent = $(this).parents('.tabbed-content');
        var tab = $(this);
        var tabContent = tabParent.find('#' + tab.attr('aria-controls'));

        if (tab.attr('aria-expanded') == 'true') {
            tab.attr('aria-expanded', 'false');
            tabContent.removeAttr('data-tab-active');
            tabContent.slideUp(250, function () {
                tabContent.attr('hidden', true);
            });
            setUrl();
        }
        else {
            tab.attr('aria-expanded', 'true');
            tabContent.attr('hidden', false);
            tabContent.attr('data-tab-active', 'true');
            tabContent.slideDown(250);
            setUrl(tabContent);
        }        
    };

    function swapTabs(tabsContainer, activeTabId) {
        var tab = tabsContainer.find('.tab > button[aria-controls="' + activeTabId + '"]');
        var tabContent = tabsContainer.find('#' + activeTabId);

        tabsContainer.find('.tab-list .tab > button').attr('aria-selected', false);
        tabsContainer.find('.tab-list .tab > button').attr('tabindex', -1);
        tabsContainer.find('.tab-content').attr('hidden', true);
        tabsContainer.find('.tab-content').removeAttr('data-tab-active');
        tab.attr('aria-selected', true);
        tab.attr('tabindex', 0);
        tab.focus();
        tabContent.attr('hidden', false);
        tabContent.attr('data-tab-active', 'true');

        setUrl(tabContent);
    }

    function setUrl(tabContent) {
        var url = new URL(window.location.href); // Returns full URL (https://example.com/path/example.html)
        url.hash = ''; //remove hash incase page was linked to via URL with hash

        if (typeof tabContent != 'undefined') {
            if (tabContent.data('isbuytolet')) {
                if ((window.location.href.indexOf('?isBuyToLet=') != -1) || (window.location.href.indexOf('&isBuyToLet=') != -1)) {
                    //do nothing, you're already on this tab
                } else {
                    url.searchParams.delete('isBespoke');
                    url.searchParams.set('isBuyToLet', "true");
                }
            }
            else if (tabContent.data('isbespoke')) {
                if ((window.location.href.indexOf('?isBespoke=') != -1) || (window.location.href.indexOf('&isBespoke=') != -1)) {
                    //do nothing, you're already on this tab
                } else {
                    url.searchParams.delete('isBuyToLet');
                    url.searchParams.set('isBespoke', "true");
                }
            }
            else {
                url.searchParams.delete('isBuyToLet');
                url.searchParams.delete('isBespoke');
            }

            url.hash = "tabs";
        }
        else {
            url.searchParams.delete('isBuyToLet');
            url.searchParams.delete('isBespoke');
            url.hash = '';
        }

        history.replaceState(null, null, url);
    }

    function GenerateTabs() {
        $('.tabbed-content').each(function () {
            var tabActive = false;
            var tabList = '<ul class="tab-list">';
            var count = '';
            var tabbedContent = $(this);
            var customActiveTab = 0;

            tabbedContent.children('.tab-content').each(function (index) {
                if ($(this).is('[data-tab-active]')) {
                    customActiveTab = index;
                }
            });

            tabbedContent.children('.tab-content').each(function (index) {
                count++;
                if (index == customActiveTab) {
                    tabActive = true;
                } else {
                    tabActive = false;
                }
                tabList += '<li class="tab"><button role="tab" id="Tab_' + $(this).attr('id') + '" aria-selected="' + tabActive + '" aria-controls="' + $(this).attr('id') + '" tabindex="' + (tabActive ? '0' : '-1') + '">' + $(this).data('tab-title') + '</button></li>';

                $(this).attr('role', 'tabpanel');
                $(this).attr('aria-labelledby', 'Tab_' + $(this).attr('id'));
                $(this).attr('hidden', !tabActive);
            });

            tabList += '</ul>';

            tabbedContent.prepend(tabList);
        });

        TabBindings();
    };

    function GenerateAccordions() {
        $('.tabbed-content').each(function () {
            var tabActive = false;
            var tabAccordion = '';
            var count = '';
            var tabbedContent = $(this);
            var customActiveTab = 0;

            tabbedContent.children('.tab-content').each(function (index) {
                if ($(this).is('[data-tab-active]')) {
                    customActiveTab = index;
                }
            });

            tabbedContent.children('.tab-content').each(function (index) {
                count++;
                if (index == customActiveTab) {
                    tabActive = true;
                } else {
                    tabActive = false;
                }

                tabAccordion = '<button class="tab-accordion" id ="Accordion_' + $(this).attr('id') + '" aria-expanded="' + tabActive + '" aria-controls="' + $(this).attr('id') + '"><span class="accordion-cross"></span>' + $(this).data('tab-title') + '</button>';
                tabbedContent.prepend(tabAccordion);
                tabbedContent.children('.tab-accordion[aria-controls="' + $(this).attr('id') + '"]').insertBefore($(this));

                $(this).attr('aria-labelledby', 'Accordion_' + $(this).attr('id'));
                $(this).attr('hidden', !tabActive);
                if (!tabActive) {
                    $(this).css('dispaly', 'none');
                }
            });
        });

        AccordionBindings();
    };

    function ClearTabMarkup() {
        $('.tabbed-content').each(function () {
            $(this).find('.tab-list').remove();
            $(this).children('.tab-content').each(function () {
                $(this).removeAttr('aria-labelledby')
                $(this).removeAttr('hidden');
                
            });
        });
    }

    function ClearAccordionMarkup() {
        $('.tabbed-content').each(function () {
            $(this).find('.tab-accordion').remove();
            $(this).children('.tab-content').each(function () {
                $(this).removeAttr('aria-labelledby')
                $(this).removeAttr('hidden')
                $(this).css('display', '');
            });
        });
    }

    function initializeTabsOrAccordion() {
        if (tabsOnly) {
            $('.tabbed-content').each(function () {
                if ($(this).find('.tab-list').length == 0) {
                    ClearAccordionMarkup();
                    GenerateTabs();
                }
            });
        } else {
            if ($(window).width() > mobileNavBreakpoint) {
                $('.tabbed-content').each(function () {
                    if ($(this).find('.tab-list').length == 0) {
                        ClearAccordionMarkup();
                        GenerateTabs();
                    }
                });
            }
            else {
                $('.tabbed-content').each(function () {
                    if ($(this).find('.tab-accordion').length == 0) {
                        ClearTabMarkup();
                        GenerateAccordions();
                    }
                });
            }
        }
    }

    initializeTabsOrAccordion(tabsOnly);

    $(window).on('resize', function () {
        initializeTabsOrAccordion(tabsOnly);
    });
}