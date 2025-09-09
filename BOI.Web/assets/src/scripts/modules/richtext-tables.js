export default function RichTextTables() {
  $('.rich-text table').each(function() {
    const table = this;
    
    function isTableOverflowing() {
      return table.scrollWidth > table.clientWidth;
    }
    
    function handleTableFocus() {
      const isOverflowing = isTableOverflowing();
      const hasManualTabIndex = table.getAttribute('data-manual-tabindex') === 'true';
      
      if (isOverflowing && !hasManualTabIndex) {
        table.setAttribute('tabindex', '0');
        table.setAttribute('data-manual-tabindex', 'true');
      }
      else if (!isOverflowing && hasManualTabIndex) {
        table.removeAttribute('tabindex');
        table.removeAttribute('data-manual-tabindex');
      }
    }
    
    handleTableFocus();
    
    if (typeof ResizeObserver !== 'undefined') {
      let debounceTimeout;
      
      const resizeObserver = new ResizeObserver(() => {
        clearTimeout(debounceTimeout);
        debounceTimeout = setTimeout(() => {
          requestAnimationFrame(() => handleTableFocus());
        }, 100);
      });
      
      resizeObserver.observe(table);
    }
  });
}