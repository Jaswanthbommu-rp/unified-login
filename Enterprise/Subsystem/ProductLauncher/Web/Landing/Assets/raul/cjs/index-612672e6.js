'use strict';

function ripple(event, el) {
    const rippleAnimationDuration = 600;
    const rippleRemoveTimeout = rippleAnimationDuration + 50;
    const { left, top, width, height } = el.getBoundingClientRect();
    const rippleSize = width > height ? width : height;
    const rippleOffset = {
        left: left + window.scrollX,
        top: top + window.scrollY
    };
    const rippleX = event.pageX - rippleOffset.left - rippleSize / 2;
    const rippleY = event.pageY - rippleOffset.top - rippleSize / 2;
    const rippleSpan = document.createElement('span');
    rippleSpan.className = 'r-ripple';
    rippleSpan.style.borderRadius = '50%';
    rippleSpan.style.backgroundColor = 'currentColor';
    rippleSpan.style.transform = 'scale(0)';
    rippleSpan.style.position = 'absolute';
    rippleSpan.style.opacity = '0.3';
    rippleSpan.style.transition = `all 0.${rippleAnimationDuration}s linear`;
    rippleSpan.style.width = rippleSize + 'px';
    rippleSpan.style.height = rippleSize + 'px';
    rippleSpan.style.top = rippleY + 'px';
    rippleSpan.style.left = rippleX + 'px';
    el.appendChild(rippleSpan);
    setTimeout(() => {
        rippleSpan.style.transform = 'scale(2)';
        rippleSpan.style.opacity = '0';
    });
    setTimeout(() => {
        rippleSpan.remove();
    }, rippleRemoveTimeout);
}

exports.ripple = ripple;
