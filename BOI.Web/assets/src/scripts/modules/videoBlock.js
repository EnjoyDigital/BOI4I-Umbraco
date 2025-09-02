import Plyr from 'plyr/dist/plyr.js';

export default function VideoBlock() {
  init()
}

function init() {
  $('.video-block').each(function() {
    var $playIcon = $(this).find('.play-icon'),
        $overlay = $(this).find('.video-block-video__overlay'),
        $video = $(this).find('video');

    var player = new Plyr($video[0], {
      controls: ['play', 'progress', 'current-time', 'mute', 'volume', 'fullscreen'],
      hideControls: false,
      keyboard: { focused: true, global: false },
      iconUrl: '/assets/dist/images/icons/plyr/plyr.svg'
    });

    $playIcon.on('click', function() {
      player.play();
      player.media.focus();
    });

    player.on('play', function() {
      $overlay.fadeOut(250);
    });
  });  
}