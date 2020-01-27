//
// isElementInViewport
// Checks if the element completely fits within the current viewport
//
export function isElementInViewport(el) {
    el.style.display = 'block';
    const rect = el.getBoundingClientRect();
    el.style.display = '';
    const bounds = getViewportDimensions();
    return (rect.top >= 0 &&
        rect.left >= 0 &&
        rect.bottom <= bounds.height &&
        rect.right <= bounds.width);
}
//
// getViewportDimensions
// Returns the viewport's dimensions
//
export function getViewportDimensions() {
    return {
        height: window.innerHeight,
        width: window.innerWidth,
    };
}
//
// getScreenSize
// Returns the viewport's size label based on width
//
export function getScreenSize() {
    const { width } = getViewportDimensions();
    if (width >= 1024) {
        return "lg";
    }
    else if (width > 768) {
        return "md";
    }
    else {
        return "sm";
    }
}
/**
 * Keep this in sync with tailwind screens theme.
 */
export const screens = {
    sm: 640,
    md: 768,
    lg: 1024,
    xl: 1280,
};
export function isScreen(size) {
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
// elementBlockHeight
// Returns element's height
//
export function elementBlockHeight(el) {
    let height = el.offsetHeight;
    const originalDisplay = el.style.display;
    if (height === 0) {
        el.style.display = 'block';
        height = el.offsetHeight;
        el.style.display = originalDisplay;
    }
    return height;
}
//
// collapseHeight
// Collapse element height using transitions
//
export function collapseHeight(el, transition = true) {
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
export function expandHeight(el, transition = true) {
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
