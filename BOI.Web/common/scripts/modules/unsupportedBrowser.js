import Cookies from 'js-cookie'

export default function UnsupportedBrowser () {
  init() // Call cookie handler functionality
}

function init () {
  msieversion()

  $('.js-unsupported-browser-close').on('click', function() {
    $('.unsupported-browser').slideUp(250)
    Cookies.set('unsupported-browser')
  })
}

function msieversion() {

  var ua = window.navigator.userAgent;
  var msie = ua.indexOf("MSIE ");
  var $browserMessage = $('.unsupported-browser')

  if (msie > 0 || !!navigator.userAgent.match(/Trident.*rv\:11\./))  // If Internet Explorer, return version number
  {
    if (!Cookies.get('unsupported-browser')) {
      $browserMessage.slideDown(250)
    }
  }

  return false;
}