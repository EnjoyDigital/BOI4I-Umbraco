import 'slick-carousel'

export default function ImageGallery() {
  init()
}

function init() {
  $('.image-gallery-block').each(function() {
    var $imageCaptionBlock = $(this).find('.js-image-gallery-caption'),
        $imageGallerySlider = $(this).find('.image-gallery-slider')

    $imageGallerySlider.slick({
      dots: false,
      appendArrows: $(this).find('.image-gallery-arrows-wrapper'),
      arrows: true,
      autoplay: false, // false is debug mode
      autoplaySpeed: 5000
    })

    $imageGallerySlider.on('beforeChange', function(event, slick, currentSlide, nextSlide){
      $imageCaptionBlock.fadeOut(300, function() {
        var $nextSlideObject = $imageGallerySlider.find('.slick-slide[data-slick-index="' + nextSlide + '"]'),
            imageCaption = $nextSlideObject.find('img').data('caption')
  
        $(this).html(imageCaption)
        $(this).fadeIn(300)
      })
    })
  })
}