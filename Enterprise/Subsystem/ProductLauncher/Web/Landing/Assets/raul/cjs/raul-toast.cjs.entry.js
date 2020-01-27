'use strict';

Object.defineProperty(exports, '__esModule', { value: true });

const core = require('./core-40bc5f02.js');
const index = require('./index-10dc7d89.js');
require('./_commonjsHelpers-559125c6.js');

// Polyfill for javascript-time-ago / IE11
if (!Math.sign) {
    Math.sign = (n) => {
        return ((n > 0) - (n < 0)) || +n;
    };
}
index.JavascriptTimeAgo.addLocale(index.en);
const timeAgo = new index.JavascriptTimeAgo('en-US');
const RaulToast = class {
    constructor(hostRef) {
        core.registerInstance(this, hostRef);
        this.severity = null;
        this.actions = null;
        this.refreshKey = 0;
        this.hidden = false;
        this.timedOut = core.createEvent(this, "timedOut", 7);
        this.toastAction = core.createEvent(this, "toastAction", 7);
    }
    componentWillLoad() {
        // Initialize self-destruct mechanism
        if (this.timeout) {
            this.timeoutTimer = window.setTimeout(() => {
                this.timedOut.emit();
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
                this.timedOut.emit();
                this.dismiss();
            }, this.timeoutLeft);
            this.timeoutStartedAt = new Date().getTime();
        }
    }
    async dismiss() {
        this.hidden = true;
        const animationDuration = parseFloat(window.getComputedStyle(this.toastEl).animationDuration) * 1000;
        setTimeout(() => this.el.remove(), animationDuration);
    }
    createdAtTimeAgo() {
        return timeAgo.format(this.createdAt);
    }
    emitToastAction(e, label) {
        e.stopPropagation();
        this.toastAction.emit(label);
    }
    render() {
        return (core.h("div", { class: {
                'r-toast': true,
                'r-toast--has-avatar': !!this.avatar,
                'r-toast--hidden': this.hidden
            }, ref: el => this.toastEl = el }, core.h("div", { class: {
                'r-toast__header': true,
                'r-toast__header--read': this.read,
            } }, core.h("div", { class: "r-toast__origin" }, this.origin), core.h("div", { class: "r-toast__timestamp" }, this.createdAtTimeAgo()), core.h("button", { type: "button", class: "r-toast__dismiss", onClick: (e) => this.emitToastAction(e, 'dismiss') }, core.h("raul-icon", { icon: "arrow-right-1" }))), core.h("div", { class: "r-toast__body" }, this.avatar &&
            core.h("div", { class: "r-toast__avatar" }, core.h("img", { src: this.avatar })), this.read &&
            core.h("div", { class: "r-toast__status-wrapper" }, core.h("div", { class: {
                    'r-toast__status': true,
                    'r-toast__status--unread': true,
                } })), core.h("div", { class: "r-toast__content" }, core.h("div", { class: "r-toast__title" }, this.heading), core.h("div", { class: {
                'r-toast__text': true,
                'truncate': this.read
            } }, this.body), core.h("div", { class: "r-toast__meta" }, this.meta), this.severity && this.severity === "High" &&
            core.h("div", { class: "r-toast__priority" }, this.severity))), core.h("div", { class: "r-toast__footer" }, this.actions && this.actions.map((action) => core.h("button", { type: "button", class: "r-toast__action", onClick: (e) => this.emitToastAction(e, action.label) }, action.text)))));
    }
    get el() { return core.getElement(this); }
    static get style() { return "raul-toast{display:block}raul-toast .r-toast{background-color:#fff;width:22.5rem;padding-left:.75rem;padding-right:.75rem;padding-top:1rem;padding-bottom:1rem;margin-top:1rem;-webkit-box-shadow:0 8px 16px 0 rgba(82,97,115,.18);box-shadow:0 8px 16px 0 rgba(82,97,115,.18);position:relative;overflow:hidden;opacity:1;-webkit-animation:slideIn 1s;animation:slideIn 1s}raul-toast .r-toast__header{display:-ms-flexbox;display:flex;-ms-flex-pack:justify;justify-content:space-between;margin-bottom:.25rem;font-size:.75rem;position:relative}raul-toast .r-toast__header--read{padding-left:1rem}raul-toast .r-toast__origin,raul-toast .r-toast__timestamp{font-size:.75rem}raul-toast .r-toast__timestamp{color:#9ba3a7;-webkit-transition:opacity .35s linear;transition:opacity .35s linear}raul-toast .r-toast__timestamp:first-letter{text-transform:capitalize}raul-toast .r-toast__body{display:-ms-flexbox;display:flex;margin-top:.25rem}raul-toast .r-toast__avatar{width:3rem;height:3rem;margin-right:1rem}raul-toast .r-toast__avatar img{width:100%;height:auto}raul-toast .r-toast__dismiss{color:#c6ccd0;font-size:1.375rem;line-height:1;display:block;position:absolute;right:0;top:-4px;opacity:0;cursor:pointer;-webkit-transition:opacity .35s linear;transition:opacity .35s linear}raul-toast .r-toast__content{-ms-flex:1 1 0%;flex:1 1 0%;min-width:0}raul-toast .r-toast__title{font-size:.875rem;font-weight:600;line-height:1.25;margin-bottom:.5rem;margin-bottom:0}raul-toast .r-toast__priority{font-size:.75rem;display:inline-block;margin-top:.25rem;font-weight:500;padding:3px 6px;color:#d01a1f;background-color:#fae8e9}raul-toast .r-toast__meta{font-size:.75rem;color:#9ba3a7;font-style:italic}raul-toast .r-toast__footer{display:-ms-flexbox;display:flex;-ms-flex-wrap:wrap;flex-wrap:wrap;-ms-flex-pack:end;justify-content:flex-end}raul-toast .r-toast__action{color:#0076cc;text-align:right;font-weight:500;text-transform:uppercase;margin-top:.25rem;margin-left:.75rem}raul-toast .r-toast__action:hover{text-decoration:underline}raul-toast .r-toast__status-wrapper{padding-top:.25rem;padding-bottom:.25rem;margin-right:.5rem}raul-toast .r-toast__status{width:9px;height:9px;border-radius:50%}raul-toast .r-toast__status--unread{background:#f65216}raul-toast .r-toast--has-avatar .r-toast__header{margin-left:4rem}raul-toast .r-toast--hidden{-webkit-animation:slideOut 1s;animation:slideOut 1s}\@media (max-width:640px){raul-toast .r-toast{width:100%}}raul-toast .r-toast:hover .r-toast__timestamp{opacity:0}raul-toast .r-toast:hover .r-toast__dismiss{opacity:1}\@-webkit-keyframes slideIn{0%{opacity:0;-webkit-transform:translate(100%,-10px);transform:translate(100%,-10px)}to{opacity:1;-webkit-transform:translate(0);transform:translate(0)}}\@keyframes slideIn{0%{opacity:0;-webkit-transform:translate(100%,-10px);transform:translate(100%,-10px)}to{opacity:1;-webkit-transform:translate(0);transform:translate(0)}}\@-webkit-keyframes slideOut{0%{opacity:1;-webkit-transform:translate(0);transform:translate(0)}to{opacity:0;-webkit-transform:translate(100%,-10px);transform:translate(100%,-10px)}}\@keyframes slideOut{0%{opacity:1;-webkit-transform:translate(0);transform:translate(0)}to{opacity:0;-webkit-transform:translate(100%,-10px);transform:translate(100%,-10px)}}"; }
};

exports.raul_toast = RaulToast;
