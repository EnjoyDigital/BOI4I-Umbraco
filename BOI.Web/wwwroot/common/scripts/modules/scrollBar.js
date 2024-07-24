export default function Scrollbar() {
  var current = -1,
    $slides = document.querySelectorAll(".scroll-bar__inner"),
    $slideWrapper = $('.scroll-bar');
  var scrollBar = document.querySelector(".scroll-bar");
  var scollInterval;

  if ($slideWrapper.length > 0) {
    setScollBarHeight()
  }

  window.onresize = function () {
    clearInterval(scollInterval);
    setScollBarHeight()
  }

  function setScollBarHeight() {
    if ($slides.length > 1) {
      animateScrollBar()
      scollInterval = setInterval(function () {
        animateScrollBar()
      }, 3000);
    }
    else if ($slides.length == 1) {
      current = 0;
      $slides[current].classList.add('active');
      scrollBar.style.height = getSlideHeight($slides[current]) + 'px';
    }
  }

  function animateScrollBar() {
    for (var i = 0; i < $slides.length; i++) {
      $slides[i].classList.remove('active');
    }

    current = (current != $slides.length - 1) ? current + 1 : 0;
    $slides[current].classList.add('active');

    // Change height of bar
    scrollBar.style.height = getSlideHeight($slides[current]) + 'px';
  
  }

  function getSlideHeight(slide) {
    var $height = slide.offsetHeight;

    if (window.innerWidth > 768) {
      $height += 24;
    } else {
      $height += 72;
    }

    return $height;
  }
}