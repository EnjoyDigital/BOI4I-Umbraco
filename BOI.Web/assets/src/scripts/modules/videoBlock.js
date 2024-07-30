export default function VideoBlock() {
  init()
}

function init() {
  var $playIcon = $('.play-icon')

  $playIcon.on('click', function() {
    $(this).parent().fadeOut(250)
    $(this).parent().siblings('video').get(0).play()
  })
}