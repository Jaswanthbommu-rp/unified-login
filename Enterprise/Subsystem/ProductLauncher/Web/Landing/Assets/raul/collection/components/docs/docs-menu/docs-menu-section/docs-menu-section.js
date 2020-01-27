import { h } from "@stencil/core";
import { collapseHeight, expandHeight } from '../../../../utils/dom';
export class DocsMenuSection {
    constructor() {
        this.expanded = false;
    }
    componentDidLoad() {
        this.setInitialToggleState();
    }
    setInitialToggleState() {
        if (this.el.querySelector('.link-active')) {
            this.expanded = true;
            expandHeight(this.contentEl, false);
        }
    }
    handleClick(e) {
        if (this.el.contains(e.target)) {
            if (this.expanded) {
                this.expanded = false;
            }
            else {
                this.expanded = true;
                expandHeight(this.contentEl);
                this.expandSection.emit(this.el);
                this.el.classList.add("open");
            }
        }
    }
    maybeCollapse(e) {
        if (e.detail !== this.el) {
            this.expanded = false;
            collapseHeight(this.contentEl);
            this.el.classList.remove("open");
        }
    }
    renderSectionTitle() {
        if (this.url) {
            return (h("div", { class: "r-docs-menu__section" },
                h("h4", { onClick: (e) => this.handleClick(e), class: "r-docs-menu__section__name" },
                    h("stencil-route-link", { "anchor-class": "r-docs-menu__header__link", url: this.url }, this.name)),
                h("div", { class: "r-docs-menu__section__content", ref: (el) => this.contentEl = el },
                    h("slot", null))));
        }
        else {
            return (h("div", { class: "r-docs-menu__section" },
                h("h4", { onClick: (e) => this.handleClick(e), class: "r-docs-menu__section__name" },
                    this.name,
                    h("raul-icon", { icon: "arrow-down-v", class: "text-gray-dark" })),
                h("div", { class: "r-docs-menu__section__content", ref: (el) => this.contentEl = el },
                    h("slot", null))));
        }
    }
    render() {
        return (h("span", null, this.renderSectionTitle()));
    }
    static get is() { return "docs-menu-section"; }
    static get properties() { return {
        "name": {
            "type": "unknown",
            "mutable": false,
            "complexType": {
                "original": "String",
                "resolved": "String",
                "references": {
                    "String": {
                        "location": "global"
                    }
                }
            },
            "required": false,
            "optional": false,
            "docs": {
                "tags": [],
                "text": ""
            }
        },
        "url": {
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
            "attribute": "url",
            "reflect": false
        }
    }; }
    static get states() { return {
        "expanded": {}
    }; }
    static get events() { return [{
            "method": "expandSection",
            "name": "expandSection",
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
    static get elementRef() { return "el"; }
    static get listeners() { return [{
            "name": "expandSection",
            "method": "maybeCollapse",
            "target": "window",
            "capture": false,
            "passive": false
        }]; }
}
