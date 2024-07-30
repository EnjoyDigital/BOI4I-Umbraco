import Globals from '../modules/globals/globals'

Globals()

function GetVideos() {
    var videoButton = document.getElementById("more-videos");

    if(videoButton) {
        videoButton.addEventListener("click", function () {
            LoadMedia(this, "video", this.dataset.listing, this.dataset.container);
        });
    }

}

GetVideos();

function GetArticles() {
    var articleButton = document.getElementById("more-articles");

    if(articleButton) {

        articleButton.addEventListener("click", function () {
            LoadMedia(this, "articles", this.dataset.listing, this.dataset.container);
        });
    }

}

GetArticles();

function GetPodcasts() {
    var podcastButton = document.getElementById("more-podcasts");

    if(podcastButton) {
        podcastButton.addEventListener("click", function () {
            LoadMedia(this, "podcast", this.dataset.listing, this.dataset.container);
        });
    }

}

GetPodcasts();

function GetDocuments() {
    var docButton = document.getElementById("more-documents");

    if(docButton) {
        docButton.addEventListener("click", function () {
            LoadMedia(this, "documents", this.dataset.listing, this.dataset.container);
        });
    }

}

GetDocuments();

function LoadMedia(button, mediaType, listingNumber, container) {
    var xhr = new XMLHttpRequest();
    xhr.open('GET', '/umbraco/surface/mediaLibrary/GetMedia?mediaType=' + mediaType + "&listingNumber=" + listingNumber);
    xhr.onload = function () {
        if (xhr.status === 200) {

            $(container).append(xhr.responseText)
            listingNumber++;
            button.dataset.listing = listingNumber;

            if(button.dataset.count <= document.querySelector(container).childElementCount) {
                button.classList.add("hidden");
            }

        }
    };
    xhr.send();
}