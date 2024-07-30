export default function onResize () {
  $(window).resize((callback) => {
    clearTimeout(200)
    window.requestAnimationFrame(() => {
      setTimeout(callback, 200)
    })
  })
}
