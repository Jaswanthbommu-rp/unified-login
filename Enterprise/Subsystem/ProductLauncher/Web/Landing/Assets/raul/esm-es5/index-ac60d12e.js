function ripple(event, el) {
    var rippleAnimationDuration = 600;
    var rippleRemoveTimeout = rippleAnimationDuration + 50;
    var _a = el.getBoundingClientRect(), left = _a.left, top = _a.top, width = _a.width, height = _a.height;
    var rippleSize = width > height ? width : height;
    var rippleOffset = {
        left: left + window.scrollX,
        top: top + window.scrollY
    };
    var rippleX = event.pageX - rippleOffset.left - rippleSize / 2;
    var rippleY = event.pageY - rippleOffset.top - rippleSize / 2;
    var rippleSpan = document.createElement('span');
    rippleSpan.className = 'r-ripple';
    rippleSpan.style.borderRadius = '50%';
    rippleSpan.style.backgroundColor = 'currentColor';
    rippleSpan.style.transform = 'scale(0)';
    rippleSpan.style.position = 'absolute';
    rippleSpan.style.opacity = '0.3';
    rippleSpan.style.transition = "all 0." + rippleAnimationDuration + "s linear";
    rippleSpan.style.width = rippleSize + 'px';
    rippleSpan.style.height = rippleSize + 'px';
    rippleSpan.style.top = rippleY + 'px';
    rippleSpan.style.left = rippleX + 'px';
    el.appendChild(rippleSpan);
    setTimeout(function () {
        rippleSpan.style.transform = 'scale(2)';
        rippleSpan.style.opacity = '0';
    });
    setTimeout(function () {
        rippleSpan.remove();
    }, rippleRemoveTimeout);
}
export { ripple as r };
