import { h } from "@stencil/core";
import TimeAgo from 'javascript-time-ago';
import en from 'javascript-time-ago/locale/en';
import { Snackbar } from './Snackbar';
// Polyfill for javascript-time-ago / IE11
if (!Math.sign) {
    Math.sign = (n) => {
        return ((n > 0) - (n < 0)) || +n;
    };
}
TimeAgo.addLocale(en);
const timeAgo = new TimeAgo('en-US');
export class RaulSnackbar {
    constructor() {
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
    static get is() { return "raul-snackbar"; }
    static get originalStyleUrls() { return {
        "$": ["raul-snackbar.scss"]
    }; }
    static get styleUrls() { return {
        "$": ["raul-snackbar.css"]
    }; }
    static get properties() { return {
        "variant": {
            "type": "string",
            "mutable": false,
            "complexType": {
                "original": "'information' | 'success' | 'warning' | 'danger'",
                "resolved": "\"danger\" | \"information\" | \"success\" | \"warning\"",
                "references": {}
            },
            "required": false,
            "optional": false,
            "docs": {
                "tags": [],
                "text": "Determines the color of the left bar"
            },
            "attribute": "variant",
            "reflect": false,
            "defaultValue": "'information'"
        },
        "dismissable": {
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
                "text": "Determines wether the snackbar has a close button. Defaults to `false`"
            },
            "attribute": "dismissable",
            "reflect": false,
            "defaultValue": "false"
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
                "text": "The first line of text"
            },
            "attribute": "heading",
            "reflect": false
        },
        "content": {
            "type": "string",
            "mutable": false,
            "complexType": {
                "original": "string",
                "resolved": "string",
                "references": {}
            },
            "required": false,
            "optional": true,
            "docs": {
                "tags": [],
                "text": "The second line of text"
            },
            "attribute": "content",
            "reflect": false
        },
        "ctaMessage": {
            "type": "string",
            "mutable": false,
            "complexType": {
                "original": "string",
                "resolved": "string",
                "references": {}
            },
            "required": false,
            "optional": true,
            "docs": {
                "tags": [],
                "text": "The call-to-action text"
            },
            "attribute": "cta-message",
            "reflect": false
        },
        "ctaUrl": {
            "type": "string",
            "mutable": false,
            "complexType": {
                "original": "string",
                "resolved": "string",
                "references": {}
            },
            "required": false,
            "optional": true,
            "docs": {
                "tags": [],
                "text": "A call-to-action url"
            },
            "attribute": "cta-url",
            "reflect": false
        },
        "ctaCallback": {
            "type": "unknown",
            "mutable": false,
            "complexType": {
                "original": "Function",
                "resolved": "Function",
                "references": {
                    "Function": {
                        "location": "global"
                    }
                }
            },
            "required": false,
            "optional": true,
            "docs": {
                "tags": [],
                "text": "A callback function hat will be called on CTA click if an url was not provided"
            }
        },
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
                "text": "The number of ms after which the snackbar self-destructs"
            },
            "attribute": "timeout",
            "reflect": false
        }
    }; }
    static get states() { return {
        "createdAt": {},
        "refreshKey": {}
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
