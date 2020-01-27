import { h } from "@stencil/core";
import Popper from 'popper.js';
export class RaulTooltip {
    constructor() {
        this.tooltipId = `raul-tooltip-${tooltipIds++}`;
        this.popper = null;
        this.text = '';
        this.placement = 'top';
        this.disabledHoverListener = false;
        this.disabledFocusListener = false;
    }
    handleMouseEnter() {
        if (!this.disabledHoverListener) {
            this.show();
        }
    }
    handleMouseLeave() {
        if (!this.disabledHoverListener) {
            this.hide();
        }
    }
    handleFocusIn() {
        if (!this.disabledFocusListener) {
            this.show();
        }
    }
    handleFocusOut() {
        if (!this.disabledFocusListener) {
            this.hide();
        }
    }
    async show() {
        this.createTooltip();
    }
    async hide() {
        this.removeTooltip();
    }
    tooltipRef() {
        return document.getElementById(this.tooltipId);
    }
    tooltipArrowRef() {
        return this.tooltipRef().querySelector('.r-tooltip__arrow');
    }
    tooltipElement() {
        const tooltipTemplate = `
      <div class="r-tooltip" id="${this.tooltipId}" role="tooltip">
        <div class="r-tooltip__arrow"></div>
        
        <div class="r-tooltip__content">
          ${this.text}        
        </div>
      </div>
    `;
        return new DOMParser().parseFromString(tooltipTemplate, 'text/html').body.firstChild;
    }
    popperOptions() {
        return {
            placement: this.placement,
            modifiers: {
                arrow: {
                    element: this.tooltipArrowRef()
                }
            }
        };
    }
    createTooltip() {
        if (!this.popper) {
            document.body.appendChild(this.tooltipElement());
            this.tooltipRef().classList.add('r-tooltip--show');
            this.popper = new Popper(this.el, this.tooltipRef(), this.popperOptions());
        }
    }
    removeTooltip() {
        if (this.popper) {
            const transitionDuration = parseFloat(getComputedStyle(this.tooltipRef()).transitionDuration) * 1000;
            this.tooltipRef().classList.remove('r-tooltip--show');
            setTimeout(() => {
                this.tooltipRef().parentNode.removeChild(this.tooltipRef());
                this.popper.destroy();
                this.popper = null;
            }, transitionDuration);
        }
    }
    render() {
        return (h("slot", null));
    }
    static get is() { return "raul-tooltip"; }
    static get originalStyleUrls() { return {
        "$": ["raul-tooltip.scss"]
    }; }
    static get styleUrls() { return {
        "$": ["raul-tooltip.css"]
    }; }
    static get properties() { return {
        "text": {
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
            "attribute": "text",
            "reflect": true,
            "defaultValue": "''"
        },
        "placement": {
            "type": "string",
            "mutable": false,
            "complexType": {
                "original": "PopperPlacements",
                "resolved": "\"auto\" | \"auto-end\" | \"auto-start\" | \"bottom\" | \"bottom-end\" | \"bottom-start\" | \"left\" | \"left-end\" | \"left-start\" | \"right\" | \"right-end\" | \"right-start\" | \"top\" | \"top-end\" | \"top-start\"",
                "references": {
                    "PopperPlacements": {
                        "location": "import",
                        "path": "../../../utils/types"
                    }
                }
            },
            "required": false,
            "optional": false,
            "docs": {
                "tags": [],
                "text": ""
            },
            "attribute": "placement",
            "reflect": true,
            "defaultValue": "'top'"
        },
        "disabledHoverListener": {
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
            "attribute": "disabled-hover-listener",
            "reflect": true,
            "defaultValue": "false"
        },
        "disabledFocusListener": {
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
            "attribute": "disabled-focus-listener",
            "reflect": true,
            "defaultValue": "false"
        }
    }; }
    static get methods() { return {
        "show": {
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
        },
        "hide": {
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
            "method": "handleMouseEnter",
            "target": undefined,
            "capture": false,
            "passive": true
        }, {
            "name": "mouseleave",
            "method": "handleMouseLeave",
            "target": undefined,
            "capture": false,
            "passive": true
        }, {
            "name": "focusin",
            "method": "handleFocusIn",
            "target": undefined,
            "capture": false,
            "passive": false
        }, {
            "name": "focusout",
            "method": "handleFocusOut",
            "target": undefined,
            "capture": false,
            "passive": false
        }]; }
}
let tooltipIds = 0;
