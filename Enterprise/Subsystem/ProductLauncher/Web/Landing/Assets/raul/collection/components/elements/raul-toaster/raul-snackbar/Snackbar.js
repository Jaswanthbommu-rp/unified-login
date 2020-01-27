import { h } from "@stencil/core";
export const Snackbar = props => {
    const { variant, dismissable, heading, content, ctaMessage, ctaUrl, ctaCallback, onCloseCallback } = props;
    return (h("div", { class: 'r-snackbar__container' },
        h("div", { class: {
                'r-snackbar__left-bar': true,
                [`r-snackbar__left-bar--${variant}`]: true
            } }),
        h("div", { class: 'r-snackbar__content-container' },
            h("div", { class: 'r-snackbar__heading' },
                h("span", null, heading),
                dismissable &&
                    h("div", { class: 'r-snackbar__heading__close-button', onClick: () => onCloseCallback(), role: "button", tabindex: 0, onKeyPress: e => { if (e.key === 'Enter') {
                            onCloseCallback();
                        } } },
                        h("raul-icon", { icon: 'close' }))),
            content &&
                h("div", { class: 'r-snackbar__content' }, content),
            ctaMessage
                ? ctaUrl ?
                    h("a", { href: ctaUrl },
                        h("div", { class: 'r-snackbar__cta' }, ctaMessage))
                    :
                        h("div", { class: 'r-snackbar__cta__message', role: 'button', tabindex: 0, onKeyPress: e => { if (e.key === 'Enter') {
                                ctaCallback && ctaCallback();
                            } } },
                            h("span", { role: 'button', onClick: ctaCallback && (() => ctaCallback()) }, ctaMessage))
                : null)));
};
