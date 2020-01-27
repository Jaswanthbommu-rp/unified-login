import { h } from "@stencil/core";
const kindDirectory = {
    family: 'families',
    icon: 'icons',
    product: 'products',
    resource: 'resources'
};
const iconCache = {};
export class RaulIcon {
    constructor() {
        this.icon = null;
        this.kind = 'icon';
    }
    iconChanged() {
        this.loadSVG();
    }
    componentWillLoad() {
        this.loadSVG();
    }
    async fetchSVG() {
        try {
            const resp = await fetch(`//cdn.realpage.com/images/${kindDirectory[this.kind]}/${this.icon}.svg`, { cache: 'no-store' });
            if (resp.ok) {
                return resp.text();
            }
            else {
                throw new Error(`[RAUL] Failed to fetch icon ${this.kind}/${this.icon}`);
            }
        }
        catch (e) {
            console.error(e);
        }
    }
    async loadSVG() {
        if (this.icon) {
            const key = `${this.kind}-${this.icon}`;
            if (!iconCache[key]) {
                const svg = await this.fetchSVG();
                if (svg) {
                    //this is because svg's can be focused in IE with the tab key and this breaks keyboard/tab navigation
                    iconCache[key] = svg.replace('<svg', '<svg aria-hidden="true" focusable="false" ');
                }
            }
            this.svgContent = iconCache[key];
        }
    }
    render() {
        return (h("i", { class: "r-icon", innerHTML: this.svgContent }));
    }
    static get is() { return "raul-icon"; }
    static get originalStyleUrls() { return {
        "$": ["raul-icon.scss"]
    }; }
    static get styleUrls() { return {
        "$": ["raul-icon.css"]
    }; }
    static get properties() { return {
        "icon": {
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
            "attribute": "icon",
            "reflect": true,
            "defaultValue": "null"
        },
        "kind": {
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
            "attribute": "kind",
            "reflect": true,
            "defaultValue": "'icon'"
        }
    }; }
    static get states() { return {
        "svgContent": {}
    }; }
    static get watchers() { return [{
            "propName": "icon",
            "methodName": "iconChanged"
        }]; }
}
