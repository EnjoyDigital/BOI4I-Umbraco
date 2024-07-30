export default function Pagination() {
    init()
}

function init() {
  
   
}

$('.currentpage').blur(function () {
    if ($(this).val() == "") {
        $(this).val($(this).attr('data-page'));

        $('.next').attr('href', replaceQueryParam($(this).attr("name"), $('.next').attr("data-page")));
    }
});

$('.currentpage').keypress(function (event) {
    var keycode = (event.keyCode ? event.keyCode : event.which);
    if (keycode == '13') {
        window.location = replaceQueryParam($(this).attr("name"), $(this).val());
    }
});

$('.currentpage').keyup(function () {
    $('.next').attr('href', replaceQueryParam($(this).attr("name"), $(this).val()));
});

function replaceQueryParam(param, newval, urlParameters = window.location.search) {

    var regex = new RegExp("([?;&])" + param + "[^&;]*[;&]?");
    var query = urlParameters.replace(regex, "$1").replace(/&$/, '');

    return (query.length > 2 ? query + "&" : "?") + (newval ? param + "=" + newval : '');
}