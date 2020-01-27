export function debounce(func, wait, immediate = false) {
    let timeout;
    return () => {
        const later = function () {
            timeout = null;
            if (!immediate)
                func.apply(this, arguments);
        };
        var callNow = immediate && !timeout;
        clearTimeout(timeout);
        timeout = setTimeout(later, wait);
        if (callNow)
            func.apply(this, arguments);
    };
}
