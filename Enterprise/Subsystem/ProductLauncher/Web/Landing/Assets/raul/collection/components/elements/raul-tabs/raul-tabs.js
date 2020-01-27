import { h } from "@stencil/core";
export class RaulTabs {
    constructor() {
        /**
         * An array of objects representing the navigation.
         */
        this.tabs = [];
    }
    connectedCallback() {
        this.xsDevice = window.innerWidth < 640;
    }
    handleResize() {
        this.xsDevice = window.innerWidth < 640;
    }
    selectOptions() {
        return this.tabs.reduce((acc, cv) => {
            acc = [...acc, ...[{ value: cv.name, text: cv.label }]];
            return acc;
        }, []);
    }
    handleClick(tabName) {
        this.raulTabChange.emit(tabName);
    }
    handleRaulChange(e) {
        this.raulTabChange.emit(e.detail.value);
    }
    render() {
        if (!(this.selectOnMobile && this.xsDevice)) {
            return (h("div", { class: "tabs", role: "tablist" },
                h("div", { class: "tabs__list" }, this.tabs.map(item => {
                    return (h("button", { role: "tab", class: {
                            'tabs__item': true,
                            'tabs__item--active': item.name === this.activeTab,
                            'tabs__item--disabled': item.disabled
                        }, id: `${item.name}-tab`, disabled: item.disabled, "aria-controls": item.id || item.name, "aria-selected": item.name === this.activeTab, onClick: () => this.handleClick(item.name) }, item.label));
                }))));
        }
        else {
            return (h("raul-select", { options: this.selectOptions(), value: this.activeTab, onRaulChange: (e) => this.handleRaulChange(e) }));
        }
    }
    static get is() { return "raul-tabs"; }
    static get originalStyleUrls() { return {
        "$": ["raul-tabs.scss"]
    }; }
    static get styleUrls() { return {
        "$": ["raul-tabs.css"]
    }; }
    static get properties() { return {
        "tabs": {
            "type": "unknown",
            "mutable": false,
            "complexType": {
                "original": "Array<TabInterface>",
                "resolved": "TabInterface[]",
                "references": {
                    "Array": {
                        "location": "global"
                    },
                    "TabInterface": {
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
        "activeTab": {
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
                "text": "The name key of the active tab."
            },
            "attribute": "active-tab",
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
                "text": "if `true`, tabs will render as a select on mobile devices."
            },
            "attribute": "select-on-mobile",
            "reflect": true
        }
    }; }
    static get states() { return {
        "xsDevice": {}
    }; }
    static get events() { return [{
            "method": "raulTabChange",
            "name": "raulTabChange",
            "bubbles": true,
            "cancelable": true,
            "composed": true,
            "docs": {
                "tags": [],
                "text": "Emitted when a tab is clicked."
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
