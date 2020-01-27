import { h } from "@stencil/core";
export class RaulBulkActionBar {
    constructor() {
        this.buttonWidths = [];
        this.toggleOverflow = false;
        this.selectedCount = 0;
        this.totalRecords = 0;
        this.open = false;
        this.showTray = false;
    }
    openChanged() {
        if (this.open) {
            this.getButtonWidths();
            this.handelMoreButtons(this.bulkActionEl, this.buttonWidths, this.bulkActionEl.querySelectorAll('.r-bulk-actions__left raul-button, .r-bulk-actions__more-tray raul-button'));
            setTimeout(() => {
                this.toggleOverflow = true;
            }, 350);
        }
        else {
            this.toggleOverflow = false;
            // Reset buttons
            setTimeout(() => {
                this.buttonWidths = [];
                this.handelMoreButtons(this.bulkActionEl, this.buttonWidths, this.bulkActionEl.querySelectorAll('.r-bulk-actions__left raul-button, .r-bulk-actions__more-tray raul-button'));
            }, 350);
        }
    }
    handleResize() {
        if (this.buttonWidths.length > 0)
            this.handelMoreButtons(this.bulkActionEl, this.buttonWidths, this.bulkActionEl.querySelectorAll('.r-bulk-actions__left raul-button, .r-bulk-actions__more-tray raul-button'));
    }
    getButtonWidths() {
        let buttons = this.bulkActionEl.querySelectorAll('.r-bulk-actions__left raul-button, .r-bulk-actions__more-tray raul-button');
        this.buttonWidths = [];
        buttons.forEach((button) => {
            let thisButton = button;
            this.buttonWidths.push(thisButton.offsetWidth);
        });
    }
    closeBar() {
        this.open = this.showTray = false;
        this.bulkActionsClose.emit();
    }
    debounce(func, delay) {
        let inDebounce;
        return function () {
            const context = this;
            const args = arguments;
            clearTimeout(inDebounce);
            inDebounce = setTimeout(() => func.apply(context, args), delay);
        };
    }
    toggleTray() {
        this.showTray = !this.showTray;
    }
    handelMoreButtons(element, buttonWidths, buttons) {
        window.addEventListener('click', (e) => {
            let tray = this.moreTray;
            // @ts-ignore
            if (tray && (!tray.contains(e.target) || e.target.classList.contains('r-button__element'))) {
                this.showTray = false;
            }
        }, true);
        let leftSideWidth = element.querySelector('.r-bulk-actions__left').offsetWidth;
        this.buttonsInTray = [];
        let buttonsNotInTray = [];
        let visibleButtonTotalWidth = 0;
        let maxButtonWidth = Math.max(...buttonWidths);
        buttons.forEach((button, index) => {
            if (!button.classList.contains('r-bulk-actions__button-more')) {
                visibleButtonTotalWidth = visibleButtonTotalWidth + buttonWidths[index] + 8;
                if ((visibleButtonTotalWidth + maxButtonWidth) > leftSideWidth) {
                    this.buttonsInTray.push(button);
                }
                else {
                    buttonsNotInTray.push(button);
                }
            }
        });
        this.buttonsInTray.forEach((button) => {
            this.moreTray.appendChild(button);
        });
        buttonsNotInTray.forEach((button) => {
            element.querySelector('.r-bulk-actions__buttons-wrapper').appendChild(button);
        });
        if (this.buttonsInTray.length > 0) {
            element.querySelector('.r-bulk-actions__button-more').classList.add('r-bulk-actions__button-more--show');
        }
        else {
            element.querySelector('.r-bulk-actions__button-more').classList.remove('r-bulk-actions__button-more--show');
        }
    }
    render() {
        return (h("div", { class: {
                'r-bulk-actions': true,
                'r-bulk-actions--expanded': this.open,
                'overflow-visible': this.toggleOverflow,
                'overflow-hidden': !this.toggleOverflow,
            }, ref: (el) => this.bulkActionEl = el },
            h("div", { class: "r-bulk-actions__wrapper" },
                h("div", { class: "r-bulk-actions__left" },
                    h("div", { class: "r-bulk-actions__buttons-wrapper" },
                        h("slot", null)),
                    h("div", { class: "r-bulk-actions__more" },
                        h("raul-button", { class: "r-bulk-actions__button-more", variant: "reverse", size: "small", onClick: () => this.toggleTray() }, "More"),
                        h("div", { class: {
                                'r-bulk-actions__more-tray': true,
                                'r-bulk-actions__more-tray--show': this.showTray,
                            }, ref: (el) => this.moreTray = el }))),
                h("div", { class: "r-bulk-actions__right" },
                    h("button", { type: "button", class: "r-bulk-actions__select-all", onClick: () => this.bulkActionsSelectAll.emit() },
                        "Select all ",
                        this.totalRecords,
                        " records"),
                    h("span", { class: "r-bulk-actions__selected-count" },
                        this.selectedCount,
                        " Selected"),
                    h("button", { type: "button", class: "r-bulk-actions__close", onClick: () => this.closeBar() },
                        h("raul-icon", { icon: "close" }))))));
    }
    static get is() { return "raul-bulk-action-bar"; }
    static get originalStyleUrls() { return {
        "$": ["raul-bulk-action-bar.scss"]
    }; }
    static get styleUrls() { return {
        "$": ["raul-bulk-action-bar.css"]
    }; }
    static get properties() { return {
        "selectedCount": {
            "type": "number",
            "mutable": false,
            "complexType": {
                "original": "number",
                "resolved": "number",
                "references": {}
            },
            "required": false,
            "optional": true,
            "docs": {
                "tags": [],
                "text": ""
            },
            "attribute": "selected-count",
            "reflect": true,
            "defaultValue": "0"
        },
        "totalRecords": {
            "type": "number",
            "mutable": false,
            "complexType": {
                "original": "number",
                "resolved": "number",
                "references": {}
            },
            "required": false,
            "optional": true,
            "docs": {
                "tags": [],
                "text": ""
            },
            "attribute": "total-records",
            "reflect": true,
            "defaultValue": "0"
        },
        "open": {
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
            "attribute": "open",
            "reflect": true,
            "defaultValue": "false"
        }
    }; }
    static get states() { return {
        "buttonsInTray": {},
        "showTray": {}
    }; }
    static get events() { return [{
            "method": "bulkActionsClose",
            "name": "bulkActionsClose",
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
            "method": "bulkActionsSelectAll",
            "name": "bulkActionsSelectAll",
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
    static get watchers() { return [{
            "propName": "open",
            "methodName": "openChanged"
        }]; }
    static get listeners() { return [{
            "name": "resize",
            "method": "handleResize",
            "target": "window",
            "capture": false,
            "passive": true
        }]; }
}
