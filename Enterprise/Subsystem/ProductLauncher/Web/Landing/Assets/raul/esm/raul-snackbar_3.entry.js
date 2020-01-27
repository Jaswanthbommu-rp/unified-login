import { h, r as registerInstance, g as getElement } from './core-9263a98c.js';
import { J as JavascriptTimeAgo, e as en } from './index-58cb3f49.js';
import './_commonjsHelpers-f34b4464.js';

const Snackbar = props => {
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

// Polyfill for javascript-time-ago / IE11
if (!Math.sign) {
    Math.sign = (n) => {
        return ((n > 0) - (n < 0)) || +n;
    };
}
JavascriptTimeAgo.addLocale(en);
const timeAgo = new JavascriptTimeAgo('en-US');
const RaulSnackbar = class {
    constructor(hostRef) {
        registerInstance(this, hostRef);
        /**
      * Determines the color of the left bar
      */
        this.variant = 'information';
        /**
      * Determines wether the snackbar has a close button. Defaults to `false`
      */
        this.dismissable = false;
        this.refreshKey = 0;
    }
    // @Event() timedOut: EventEmitter
    // @Event() toastAction: EventEmitter
    componentWillLoad() {
        // Initialize self-destruct mechanism
        if (this.timeout) {
            this.timeoutTimer = window.setTimeout(() => {
                // this.timedOut.emit()
                this.dismiss();
            }, this.timeout);
            this.timeoutStartedAt = new Date().getTime();
        }
        // Save timestamp at moment of creation
        this.createdAt = new Date();
        // Re-render component every 1 minute (to update timestamp)
        this.refreshTimer = window.setInterval(() => {
            this.refreshKey = this.refreshKey + 1;
        }, 60000);
    }
    disconnectedCallback() {
        clearTimeout(this.timeoutTimer);
        clearInterval(this.refreshTimer);
    }
    handleMouseenter() {
        if (this.timeout) {
            const timeout = this.timeoutLeft ? this.timeoutLeft : this.timeout;
            const timeoutPausedAt = new Date().getTime();
            this.timeoutLeft = timeout - (timeoutPausedAt - this.timeoutStartedAt);
            clearTimeout(this.timeoutTimer);
        }
    }
    handleMouseleave() {
        if (this.timeout) {
            this.timeoutTimer = window.setTimeout(() => {
                // this.timedOut.emit()
                this.dismiss();
            }, this.timeoutLeft);
            this.timeoutStartedAt = new Date().getTime();
        }
    }
    dismiss() {
        this.el.classList.add('r-snackbar--fadeout');
        setTimeout(() => this.el.remove(), 1000);
    }
    createdAtTimeAgo() {
        return timeAgo.format(this.createdAt);
    }
    // emitToastAction(e, label) {
    //   e.stopPropagation()
    //   this.toastAction.emit(label)
    // }
    render() {
        return (h(Snackbar, { dismissable: this.dismissable, variant: this.variant, heading: this.heading, content: this.content, ctaMessage: this.ctaMessage, ctaUrl: this.ctaUrl, ctaCallback: this.ctaCallback, onCloseCallback: () => this.dismiss() }));
    }
    get el() { return getElement(this); }
    static get style() { return "raul-snackbar{-webkit-animation:popIn .3s ease-in 0s 1;animation:popIn .3s ease-in 0s 1;-webkit-transition:max-height .3s ease-in;transition:max-height .3s ease-in;height:auto;max-height:300px}raul-snackbar.r-snackbar--fadeout{opacity:0;max-height:0;margin:0;-webkit-transition:opacity .5s ease,max-height .5s ease .5s,margin .5s ease .5s;transition:opacity .5s ease,max-height .5s ease .5s,margin .5s ease .5s}raul-snackbar .r-snackbar__container{display:-ms-flexbox;display:flex;-ms-flex-direction:row;flex-direction:row;-ms-flex-pack:start;justify-content:flex-start;-ms-flex-align:center;align-items:center;z-index:50;-webkit-box-shadow:0 16px 27px 0 rgba(0,0,0,.24);box-shadow:0 16px 27px 0 rgba(0,0,0,.24);width:360px;min-height:56px}\@media (max-width:640px){raul-snackbar .r-snackbar__container{width:100%}}raul-snackbar .r-snackbar__container .r-snackbar__left-bar{min-width:12px;-ms-flex-item-align:stretch;align-self:stretch;border-top-left-radius:4px;border-bottom-left-radius:4px;-ms-flex:0;flex:0}raul-snackbar .r-snackbar__container .r-snackbar__left-bar--information{background-color:#0076cc}raul-snackbar .r-snackbar__container .r-snackbar__left-bar--success{background-color:#139c3e}raul-snackbar .r-snackbar__container .r-snackbar__left-bar--warning{background-color:#fec12d}raul-snackbar .r-snackbar__container .r-snackbar__left-bar--danger{background-color:#d01a1f}raul-snackbar .r-snackbar__container .r-snackbar__content-container{background-color:#37474f;color:#fff;-ms-flex:1 1 0%;flex:1 1 0%;padding:1rem;-ms-flex-item-align:stretch;align-self:stretch;border-top-right-radius:4px;border-bottom-right-radius:4px}raul-snackbar .r-snackbar__container .r-snackbar__content-container .r-snackbar__heading{color:#fff;display:-ms-flexbox;display:flex;-ms-flex-align:center;align-items:center;-ms-flex-pack:justify;justify-content:space-between;font-size:16px;font-weight:700}raul-snackbar .r-snackbar__container .r-snackbar__content-container .r-snackbar__heading .r-snackbar__heading__close-button{display:-ms-flexbox;display:flex;-ms-flex-pack:center;justify-content:center;-ms-flex-item-align:stretch;align-self:stretch;-ms-flex-align:center;align-items:center;height:2rem;width:2rem;min-height:32px;min-width:32px;margin-top:-4px;margin-right:-8px;cursor:pointer}raul-snackbar .r-snackbar__container .r-snackbar__content-container .r-snackbar__heading .r-snackbar__heading__close-button:focus{outline-color:#fff}raul-snackbar .r-snackbar__container .r-snackbar__content-container .r-snackbar__content{padding-top:.25rem;font-size:14px;font-weight:400;color:#fff}raul-snackbar .r-snackbar__container .r-snackbar__content-container a{padding:.25rem;display:-ms-inline-flexbox;display:inline-flex;-ms-flex-pack:center;justify-content:center;-ms-flex-align:center;align-items:center;margin-bottom:-4px;margin-left:-4px;font-size:12px;font-size:14px;font-weight:400;color:#fff;text-decoration:underline}raul-snackbar .r-snackbar__container .r-snackbar__content-container a:focus{outline-color:#fff}raul-snackbar .r-snackbar__container .r-snackbar__content-container .r-snackbar__cta__message{padding:.25rem;display:-ms-inline-flexbox;display:inline-flex;-ms-flex-pack:center;justify-content:center;-ms-flex-align:center;align-items:center;margin-bottom:-4px;margin-left:-4px;font-size:12px;font-size:14px;font-weight:400;color:#fff;text-decoration:underline}raul-snackbar .r-snackbar__container .r-snackbar__content-container .r-snackbar__cta__message:focus{outline-color:#fff}\@-webkit-keyframes popIn{0%{-webkit-transform:translateY(100%);transform:translateY(100%);max-height:0}to{-webkit-transform:translateY 0;transform:translateY 0;max-height:500px}}\@keyframes popIn{0%{-webkit-transform:translateY(100%);transform:translateY(100%);max-height:0}to{-webkit-transform:translateY 0;transform:translateY 0;max-height:500px}}"; }
};

const RaulSnackbarGroup = class {
    constructor(hostRef) {
        registerInstance(this, hostRef);
    }
    render() {
        return null;
    }
    get el() { return getElement(this); }
    static get style() { return "raul-snackbar-group{display:-ms-flexbox;z-index:1030;position:fixed!important;display:flex;-ms-flex-direction:column-reverse;flex-direction:column-reverse;right:40px;bottom:40px}\@media (max-width:640px){raul-snackbar-group{left:40px}}raul-snackbar-group raul-snackbar .r-snackbar__container{margin-bottom:1rem}"; }
};

const RaulToaster = class {
    constructor(hostRef) {
        registerInstance(this, hostRef);
    }
    render() {
        return null;
    }
    get el() { return getElement(this); }
    static get style() { return "raul-toaster{display:block;position:fixed;margin-right:1rem;right:0;top:56px;z-index:1030}\@media (max-width:640px){raul-toaster{right:0;left:0;margin-left:1rem;margin-right:1rem}}"; }
};

export { RaulSnackbar as raul_snackbar, RaulSnackbarGroup as raul_snackbar_group, RaulToaster as raul_toaster };
