export default function Popup() {
    init()
}

var $popUp = document.querySelector('.popup-block.single-popup');

if ($popUp) {
    var $popUpClose = document.querySelectorAll('.popup-close');
    var $popUpInner = $popUp.querySelector('.popup-block__inner');
    var hasVisited = sessionStorage.getItem("hasVisited");

    function init() {
        // If falsy, user didn't visited your page yet.
        if (!hasVisited) {
            if ($popUp) {
                sessionStorage.setItem("hasVisited", true);
                $popUp.style.display = "flex";
                showPopup(); // Shows popup only once
            }
        }
    }

    function showPopup() {
        if ($popUpClose) {
            for (let i = 0; i < $popUpClose.length; i++) {
                $popUpClose[i].addEventListener('click', function () {
                    $popUp.style.display = "none";
                });
            }
        }

        document.addEventListener('click', function (event) {
            if ($popUpInner) {
                var isClickInside = $popUpInner.contains(event.target);
                if (!isClickInside) {
                    $popUp.style.display = "none";
                }
            }
        });
    }
}
