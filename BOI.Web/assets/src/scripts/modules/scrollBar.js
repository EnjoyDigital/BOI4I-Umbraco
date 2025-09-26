export default function Scrollbar() {
  var current = -1,
    $slides = document.querySelectorAll(".scroll-bar__inner"),
    $slideWrapper = $('.scroll-bar'),
    $close = $slideWrapper.find('.js-close-notification-bar');
  var scollInterval;

  window.onresize = function () {
    clearInterval(scollInterval);
  }

  if ($slides.length > 1) {
    animateScrollBar()
    scollInterval = setInterval(function () {
      animateScrollBar()
    }, 5000);
  }
  else if ($slides.length == 1) {
    current = 0;
    $slides[current].classList.add('active');
  }

  function animateScrollBar() {
    for (var i = 0; i < $slides.length; i++) {
      $slides[i].classList.remove('active');
    }

    current = (current != $slides.length - 1) ? current + 1 : 0;
    $slides[current].classList.add('active');  
  }
  
  $close.on("click", function() {
    $slideWrapper.hide();
  })
}