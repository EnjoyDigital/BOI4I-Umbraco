export default function externalLinks() {
	init()
}

function init() {
	var links = document.querySelectorAll('main a, footer a');
	for (var i = 0; i < links.length; i++) {
		if (links[i].target == "_blank") {
			if (!links[i].querySelector("svg")) {
				links[i].classList.add("external-link");

				var svgElem = document.createElementNS('http://www.w3.org/2000/svg', 'svg'),
					useElem = document.createElementNS('http://www.w3.org/2000/svg', 'use'),
					srTextElem = document.createElement('span');

				svgElem.setAttribute('aria-hidden', 'true');
				useElem.setAttributeNS('http://www.w3.org/1999/xlink', 'xlink:href', '#sprite-icon-external-link');
				srTextElem.classList.add('sr-only');
				srTextElem.innerHTML = 'Opens in a new tab';

				svgElem.appendChild(useElem);
				links[i].appendChild(srTextElem);
				links[i].appendChild(svgElem);
			}
		}
	}
}