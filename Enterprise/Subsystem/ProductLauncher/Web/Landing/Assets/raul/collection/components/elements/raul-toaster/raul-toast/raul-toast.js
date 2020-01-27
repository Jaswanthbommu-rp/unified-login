import { h } from "@stencil/core";
import TimeAgo from 'javascript-time-ago';
import en from 'javascript-time-ago/locale/en';
// Polyfill for javascript-time-ago / IE11
if (!Math.sign) {
    Math.sign = (n) => {
        return ((n > 0) - (n < 0)) || +n;
    };
}
TimeAgo.addLocale(en);
const timeAgo = new TimeAgo('en-US');
export class RaulToast {
    constructor() {
        this.severity = null;
        this.actions = null;
        this.refreshKey = 0;
        this.hidden = false;
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
        return (h("div", { class: {
                'r-toast': true,
                'r-toast--has-avatar': !!this.avatar,
                'r-toast--hidden': this.hidden
            }, ref: el => this.toastEl = el },
            h("div", { class: {
                    'r-toast__header': true,
                    'r-toast__header--read': this.read,
                } },
                h("div", { class: "r-toast__origin" }, this.origin),
                h("div", { class: "r-toast__timestamp" }, this.createdAtTimeAgo()),
                h("button", { type: "button", class: "r-toast__dismiss", onClick: (e) => this.emitToastAction(e, 'dismiss') },
                    h("raul-icon", { icon: "arrow-right-1" }))),
            h("div", { class: "r-toast__body" },
                this.avatar &&
                    h("div", { class: "r-toast__avatar" },
                        h("img", { src: this.avatar })),
                this.read &&
                    h("div", { class: "r-toast__status-wrapper" },
                        h("div", { class: {
                                'r-toast__status': true,
                                'r-toast__status--unread': true,
                            } })),
                h("div", { class: "r-toast__content" },
                    h("div", { class: "r-toast__title" }, this.heading),
                    h("div", { class: {
                            'r-toast__text': true,
                            'truncate': this.read
                        } }, this.body),
                    h("div", { class: "r-toast__meta" }, this.meta),
                    this.severity && this.severity === "High" &&
                        h("div", { class: "r-toast__priority" }, this.severity))),
            h("div", { class: "r-toast__footer" }, this.actions && this.actions.map((action) => h("button", { type: "button", class: "r-toast__action", onClick: (e) => this.emitToastAction(e, action.label) }, action.text)))));
    }
    static get is() { return "raul-toast"; }
    static get originalStyleUrls() { return {
        "$": ["raul-toast.scss"]
    }; }
    static get styleUrls() { return {
        "$": ["raul-toast.css"]
    }; }
    static get properties() { return {
        "timeout": {
            "type": "number",
            "mutable": false,
            "complexType": {
                "original": "number",
                "resolved": "number",
                "references": {}
            },
            "required": false,
            "optional": false,
            "docs": {
                "tags": [],
                "text": ""
            },
            "attribute": "timeout",
            "reflect": false
        },
        "avatar": {
            "type": "string",
            "mutable": false,
            "complexType": {
                "original": "string",
                "resolved": "string",
                "references": {}
            },
            "required": false,
            "optional": false,
            "docs": {
                "tags": [],
                "text": ""
            },
            "attribute": "avatar",
            "reflect": false
        },
        "origin": {
            "type": "string",
            "mutable": false,
            "complexType": {
                "original": "string",
                "resolved": "string",
                "references": {}
            },
            "required": false,
            "optional": false,
            "docs": {
                "tags": [],
                "text": ""
            },
            "attribute": "origin",
            "reflect": false
        },
        "heading": {
            "type": "string",
            "mutable": false,
            "complexType": {
                "original": "string",
                "resolved": "string",
                "references": {}
            },
            "required": false,
            "optional": false,
            "docs": {
                "tags": [],
                "text": ""
            },
            "attribute": "heading",
            "reflect": false
        },
        "body": {
            "type": "string",
            "mutable": false,
            "complexType": {
                "original": "string",
                "resolved": "string",
                "references": {}
            },
            "required": false,
            "optional": false,
            "docs": {
                "tags": [],
                "text": ""
            },
            "attribute": "body",
            "reflect": false
        },
        "meta": {
            "type": "string",
            "mutable": false,
            "complexType": {
                "original": "string",
                "resolved": "string",
                "references": {}
            },
            "required": false,
            "optional": false,
            "docs": {
                "tags": [],
                "text": ""
            },
            "attribute": "meta",
            "reflect": false
        },
        "read": {
            "type": "boolean",
            "mutable": false,
            "complexType": {
                "original": "boolean",
                "resolved": "boolean",
                "references": {}
            },
            "required": false,
            "optional": false,
            "docs": {
                "tags": [],
                "text": ""
            },
            "attribute": "read",
            "reflect": false
        },
        "severity": {
            "type": "string",
            "mutable": false,
            "complexType": {
                "original": "string",
                "resolved": "string",
                "references": {}
            },
            "required": false,
            "optional": false,
            "docs": {
                "tags": [],
                "text": ""
            },
            "attribute": "severity",
            "reflect": false,
            "defaultValue": "null"
        },
        "actions": {
            "type": "unknown",
            "mutable": false,
            "complexType": {
                "original": "Array<any>",
                "resolved": "any[]",
                "references": {
                    "Array": {
                        "location": "global"
                    }
                }
            },
            "required": false,
            "optional": false,
            "docs": {
                "tags": [],
                "text": ""
            },
            "defaultValue": "null"
        }
    }; }
    static get states() { return {
        "createdAt": {},
        "refreshKey": {},
        "hidden": {}
    }; }
    static get events() { return [{
            "method": "timedOut",
            "name": "timedOut",
            "bubbles": true,
            "cancelable": true,
            "composed": true,
            "docs": {
                "tags": [],
                "text": ""
            },
            "complexType": {
                "original": "any",
                "resolved": "any",
                "references": {}
            }
        }, {
            "method": "toastAction",
            "name": "toastAction",
            "bubbles": true,
            "cancelable": true,
            "composed": true,
            "docs": {
                "tags": [],
                "text": ""
            },
            "complexType": {
                "original": "any",
                "resolved": "any",
                "references": {}
            }
        }]; }
    static get methods() { return {
        "dismiss": {
            "complexType": {
                "signature": "() => Promise<void>",
                "parameters": [],
                "references": {
                    "Promise": {
                        "location": "global"
                    }
                },
                "return": "Promise<void>"
            },
            "docs": {
                "text": "",
                "tags": []
            }
        }
    }; }
    static get elementRef() { return "el"; }
    static get listeners() { return [{
            "name": "mouseenter",
            "method": "handleMouseenter",
            "target": undefined,
            "capture": false,
            "passive": true
        }, {
            "name": "mouseleave",
            "method": "handleMouseleave",
            "target": undefined,
            "capture": false,
            "passive": true
        }]; }
}
