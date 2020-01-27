import { h } from "@stencil/core";
export class RaulGridSorter {
    constructor() {
        this.direction = null;
    }
    handleSort() {
        if (this.direction === null) {
            this.direction = 'ascending';
        }
        else if (this.direction === 'ascending') {
            this.direction = 'descending';
        }
        else if (this.direction === 'descending') {
            this.direction = null;
        }
        this.raulSort.emit({ direction: this.direction, field: this.field });
    }
    render() {
        return (h("button", { type: "button", class: "r-grid__sorter", onClick: () => this.handleSort() },
            h("span", { class: {
                    'r-grid__sorter__icon': true,
                    'r-grid__sorter__icon--ascending': true,
                    'r-grid__sorter__icon--active': this.direction === 'ascending'
                } }),
            h("span", { class: {
                    'r-grid__sorter__icon': true,
                    'r-grid__sorter__icon--descending': true,
                    'r-grid__sorter__icon--active': this.direction === 'descending'
                } })));
    }
    static get is() { return "raul-grid-sorter"; }
    static get originalStyleUrls() { return {
        "$": ["raul-grid-sorter.scss"]
    }; }
    static get styleUrls() { return {
        "$": ["raul-grid-sorter.css"]
    }; }
    static get properties() { return {
        "direction": {
            "type": "string",
            "mutable": true,
            "complexType": {
                "original": "'ascending' | 'descending' | null",
                "resolved": "\"ascending\" | \"descending\"",
                "references": {}
            },
            "required": false,
            "optional": false,
            "docs": {
                "tags": [],
                "text": ""
            },
            "attribute": "direction",
            "reflect": true,
            "defaultValue": "null"
        },
        "field": {
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
            "attribute": "field",
            "reflect": true
        }
    }; }
    static get events() { return [{
            "method": "raulSort",
            "name": "raulSort",
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
}
