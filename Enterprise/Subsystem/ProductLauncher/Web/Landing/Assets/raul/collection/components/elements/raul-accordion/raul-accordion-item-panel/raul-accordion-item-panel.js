import { Host, h } from "@stencil/core";
export class RaulAccordionItemPanel {
    expandedChanged() {
        this.expanded ? this.expand(this.el) : this.collapse(this.el);
    }
    height(el) {
        let height = el.offsetHeight;
        if (!height) {
            const initialDisplay = el.style.display;
            el.style.display = 'block';
            height = el.offsetHeight;
            el.style.display = initialDisplay ? initialDisplay : null;
        }
        return height;
    }
    expand(el) {
        const height = this.height(el);
        const transitionDuration = parseFloat(getComputedStyle(el).transitionDuration) * 1000;
        el.classList.add('collapsing');
        el.style.display = 'block';
        el.style.overflow = 'hidden';
        el.style.height = '0';
        setTimeout(() => {
            el.style.height = `${height}px`;
            setTimeout(() => {
                el.style.height = el.style.overflow = null;
                el.classList.remove('collapsing');
            }, transitionDuration);
        }, 25);
    }
    collapse(el) {
        const height = this.height(el);
        const transitionDuration = parseFloat(getComputedStyle(el).transitionDuration) * 1000;
        el.classList.add('expanding');
        el.style.overflow = 'hidden';
        el.style.height = `${height}px`;
        setTimeout(() => {
            el.style.height = '0';
            setTimeout(() => {
                el.style.height = el.style.overflow = el.style.display = null;
                el.classList.remove('expanding');
            }, transitionDuration);
        }, 25);
    }
    render() {
        return (h(Host, { id: `${this.name}-panel`, "aria-labelledby": `${this.name}-header` },
            h("div", { class: "r-accordion__item__panel__content" },
                h("slot", null))));
    }
    static get is() { return "raul-accordion-item-panel"; }
    static get properties() { return {
        "name": {
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
            "attribute": "name",
            "reflect": true
        },
        "expanded": {
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
            "attribute": "expanded",
            "reflect": true
        }
    }; }
    static get elementRef() { return "el"; }
    static get watchers() { return [{
            "propName": "expanded",
            "methodName": "expandedChanged"
        }]; }
}
