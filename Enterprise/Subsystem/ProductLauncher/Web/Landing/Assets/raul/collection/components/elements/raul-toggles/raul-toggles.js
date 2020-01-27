import { h } from "@stencil/core";
export class RaulToggles {
    constructor() {
        /**
         * An array of objects representing the navigation.
         */
        this.toggles = [];
    }
    connectedCallback() {
        this.xsDevice = window.innerWidth < 640;
    }
    handleResize() {
        this.xsDevice = window.innerWidth < 640;
    }
    selectOptions() {
        return this.toggles.reduce((acc, cv) => {
            acc = [...acc, ...[{ value: cv.name, text: cv.label }]];
            return acc;
        }, []);
    }
    handleClick(toggleName) {
        this.raulToggleChange.emit(toggleName);
    }
    handleRaulChange(e) {
        this.raulToggleChange.emit(e.detail.value);
    }
    render() {
        if (!(this.selectOnMobile && this.xsDevice)) {
            return (h("div", { class: "r-toggles", role: "tablist" },
                h("div", { class: "r-toggles__list" }, this.toggles.map(item => {
                    return (h("button", { role: "tab", class: {
                            'r-toggles__item': true,
                            'r-toggles__item--active': item.name === this.activeToggle,
                            'r-toggles__item--disabled': item.disabled
                        }, id: `${item.name}-toggle`, disabled: item.disabled, "aria-controls": item.id || item.name, "aria-selected": item.name === this.activeToggle, onClick: () => this.handleClick(item.name) }, item.label));
                }))));
        }
        else {
            return (h("raul-select", { options: this.selectOptions(), value: this.activeToggle, onRaulChange: (e) => this.handleRaulChange(e) }));
        }
    }
    static get is() { return "raul-toggles"; }
    static get originalStyleUrls() { return {
        "$": ["raul-toggles.scss"]
    }; }
    static get styleUrls() { return {
        "$": ["raul-toggles.css"]
    }; }
    static get properties() { return {
        "toggles": {
            "type": "unknown",
            "mutable": false,
            "complexType": {
                "original": "Array<ToggleInterface>",
                "resolved": "ToggleInterface[]",
                "references": {
                    "Array": {
                        "location": "global"
                    },
                    "ToggleInterface": {
                        "location": "import",
                        "path": "../../../utils/interfaces"
                    }
                }
            },
            "required": false,
            "optional": false,
            "docs": {
                "tags": [],
                "text": "An array of objects representing the navigation."
            },
            "defaultValue": "[]"
        },
        "activeToggle": {
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
                "text": "The name key of the active toggle."
            },
            "attribute": "active-toggle",
            "reflect": true
        },
        "selectOnMobile": {
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
                "text": "If `true`, toggles will render as a select on mobile devices."
            },
            "attribute": "select-on-mobile",
            "reflect": true
        },
        "fullWidth": {
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
                "text": "If `true`, the width of the parent will be distributed equally to the toggles."
            },
            "attribute": "full-width",
            "reflect": true
        }
    }; }
    static get states() { return {
        "xsDevice": {}
    }; }
    static get events() { return [{
            "method": "raulToggleChange",
            "name": "raulToggleChange",
            "bubbles": true,
            "cancelable": true,
            "composed": true,
            "docs": {
                "tags": [],
                "text": "Emitted when a toggle is clicked."
            },
            "complexType": {
                "original": "any",
                "resolved": "any",
                "references": {}
            }
        }]; }
    static get listeners() { return [{
            "name": "resize",
            "method": "handleResize",
            "target": "window",
            "capture": false,
            "passive": true
        }]; }
}
