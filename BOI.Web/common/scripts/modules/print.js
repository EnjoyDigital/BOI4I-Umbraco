export default function Print() {
  init()
}

function init() {
  $('body').on('click', '.js-print-products', function(e) {
    e.preventDefault()

    window.print()
  })
}