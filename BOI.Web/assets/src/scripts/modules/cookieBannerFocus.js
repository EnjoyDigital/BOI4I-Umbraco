export default function CookieBannerFocus() {
  const bannerSelector = '#cookiebanner';
  let observer;
  let timeout;
  
  function handleBannerFocus(banner) {
    if (banner && banner.length) {
      banner.attr('tabindex', '-1');
      banner.focus();
      
      if (observer) {
        observer.disconnect();
        observer = null;
      }
    }
  }
  
  function checkForBanner() {
    const banner = $(bannerSelector);
    if (banner.length > 0) {
      handleBannerFocus(banner);
      return true;
    }
    return false;
  }
  
  // Check if banner already exists- skip mutation observer if so
  if (checkForBanner()) {
    return;
  }
  
  if (typeof MutationObserver !== 'undefined') {
    observer = new MutationObserver((mutations) => {
      for (const mutation of mutations) {
        if (mutation.type === 'childList' && mutation.target === document.body) {
          for (const node of mutation.addedNodes) {
            if (node.nodeType === 1) { // Element node
              const $node = $(node);
              if ($node.is(bannerSelector)) {
                handleBannerFocus($node);
                return;
              }
            }
          }
        }
      }
    });
    
    observer.observe(document.body, {
      childList: true,
      subtree: false // Only direct children of body
    });
  }
}