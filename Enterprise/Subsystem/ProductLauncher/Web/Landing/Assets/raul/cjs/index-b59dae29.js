'use strict';

//
//
// getViewportDimensions
// Returns the viewport's dimensions
//
function getViewportDimensions() {
    return {
        height: window.innerHeight,
        width: window.innerWidth,
    };
}
/**
 * Keep this in sync with tailwind screens theme.
 */
const screens = {
    sm: 640,
    md: 768,
    lg: 1024,
    xl: 1280,
};
function isScreen(size) {
    const { width } = getViewportDimensions();
    if (size === 'sm') {
        return width > screens.sm;
    }
    else if (size === 'md') {
        return width > screens.md;
    }
    else if (size === 'lg') {
        return width > screens.lg;
    }
    else if (size === 'xl') {
        return width >= screens.xl;
    }
}
//
// collapseHeight
// Collapse element height using transitions
//
function collapseHeight(el, transition = true) {
    const sectionHeight = el.scrollHeight;
    const elementTransition = el.style.transition;
    if (!transition)
        el.style.transition = 'none';
    requestAnimationFrame(function () {
        el.style.height = sectionHeight + 'px';
        el.style.height = 0 + 'px';
        requestAnimationFrame(function () {
            el.style.transition = elementTransition;
        });
    });
}
//
// expandHeight
// Expand element height using transitions
//
function expandHeight(el, transition = true) {
    const sectionHeight = el.scrollHeight;
    const elementTransition = el.style.transition;
    if (!transition)
        el.style.transition = 'none';
    requestAnimationFrame(function () {
        el.style.height = 0 + 'px';
        el.style.height = sectionHeight + 'px';
        requestAnimationFrame(function () {
            el.style.transition = elementTransition;
        });
    });
}

exports.collapseHeight = collapseHeight;
exports.expandHeight = expandHeight;
exports.isScreen = isScreen;
