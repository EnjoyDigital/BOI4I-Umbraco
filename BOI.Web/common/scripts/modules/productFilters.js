export default function ProductFilters() {
  init()
}

function init() {
  var $filtersToggle = $('.js-toggle-product-filters'),
      $productFilters = $('.js-product-filters')

  $filtersToggle.on('click', function() {
    $(this).toggleClass('-product-filters-open')
    $productFilters.slideToggle(250)
  })
}