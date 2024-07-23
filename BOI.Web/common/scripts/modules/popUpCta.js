export default function PopUpCta() {
    init()
}

// var $popUpClose = document.querySelectorAll('.popup-close');
// var $popUps = document.querySelectorAll('.popup-block.cta-popup');
// var $popUpInner = $popUp.querySelector('.popup-block__inner');
var $openPopup = document.querySelectorAll(".popup-open");

function init() {
    // If falsy, user didn't visited your page yet.
    if ($openPopup.length > 0) {
        // $popUp.style.display = "flex";

        for (let i = 0; i < $openPopup.length; i++) {

            $openPopup[i].addEventListener('click', function (e) {

                e.preventDefault();

                var popupId = this.dataset.popupid;
                if (popupId) {
                    var popup = document.getElementById(popupId);

                    if (popup) {
                        popup.style.display = "flex";
                    }

                    popup.querySelector(".popup-close").addEventListener('click', function (e) {
                        popup.style.display = "none";
                    });

                    popup.querySelector(".continue").addEventListener('click', function (e) {
                        var checkId = popupId + "-check";
                        var checkbox = document.getElementById(checkId);

                        if (checkbox.checked == false) {
                            e.preventDefault();
                            var checkError = popupId + "-error";
                            var checkErrorElem = document.getElementById(checkError);

                            checkErrorElem.style.display = "block";
                            checkErrorElem.setAttribute('role', 'alert');
                            checkbox.setAttribute('aria-invalid', true);
                            checkbox.setAttribute('aria-describedby', checkErrorElem.getAttribute('id'));
                            checkbox.focus();

                        }
                        else {
                            popup.style.display = "none";

                            checkErrorElem.style.display = "none";
                            checkErrorElem.removeAttribute('role');
                            checkbox.removeAttribute('aria-invalid');
                            checkbox.removeAttribute('aria-describedby');
                        }
                    });
                }
            });
        }
    }
}

// function showPopup() {
//     for (let i = 0; i < $popUpClose.length; i++) {
//         $popUpClose[i].addEventListener('click', function() {
//             $popUp.style.display = "none";
//         });
//     }

//     document.addEventListener('click', function(event) {
//       var isClickInside = $popUpInner.contains(event.target);
//       if (!isClickInside) {
//         $popUp.style.display = "none";
//       }
//     });
// }
