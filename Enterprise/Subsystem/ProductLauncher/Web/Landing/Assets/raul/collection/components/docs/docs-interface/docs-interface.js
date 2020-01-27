import { h } from "@stencil/core";
import getDocs from '../get-docs';
export class DocsInterface {
    componentWillLoad() {
        this.docs = getDocs(this.component);
    }
    render() {
        const renderDefault = (defaultValue) => {
            if (defaultValue === undefined) {
                return 'null';
            }
            else if (typeof defaultValue === 'string') {
                return defaultValue;
            }
            else {
                return JSON.stringify(defaultValue);
            }
        };
        const renderNotApplicable = (type) => {
            return (h("div", { class: "r-docs-interface__na" },
                "No ",
                type,
                " available."));
        };
        return (h("div", { class: "r-docs-interface page-section" },
            h("h2", { class: "text-lg font-bold mb-6" }, "Component API"),
            h("div", { class: "r-docs-interface__section page-section" },
                h("h4", { id: "props", class: "text-sm uppercase font-bold" }, "Props"),
                this.docs.props.length > 0 &&
                    h("raul-simple-table", null,
                        h("table", null,
                            h("thead", null,
                                h("tr", null,
                                    h("th", null, "Name"),
                                    h("th", null, "Type"),
                                    h("th", null, "Default"),
                                    h("th", null, "Description"))),
                            h("tbody", null, this.docs.props.map(prop => {
                                return (h("tr", null,
                                    h("td", null, prop.name),
                                    h("td", { style: { maxWidth: '300px' } }, prop.type),
                                    h("td", null,
                                        h("code", null, renderDefault(prop.default))),
                                    h("td", null, prop.docs || '[MISSING]')));
                            })))) || renderNotApplicable('properties')),
            h("div", { class: "r-docs-interface__section page-section" },
                h("h4", { id: "methods", class: "text-sm uppercase font-bold" }, "Methods"),
                this.docs.methods.length > 0 &&
                    h("raul-simple-table", null,
                        h("table", null,
                            h("thead", null,
                                h("tr", null,
                                    h("th", null, "Method"),
                                    h("th", null, "Description"))),
                            h("tbody", null, this.docs.methods.map(method => {
                                return (h("tr", null,
                                    h("td", null,
                                        h("code", null, method.signature)),
                                    h("td", null, method.docs || '[MISSING]')));
                            })))) || renderNotApplicable('methods')),
            h("div", { class: "r-docs-interface__section page-section" },
                h("h4", { id: "events", class: "text-sm uppercase font-bold" }, "Events"),
                this.docs.events.length > 0 &&
                    h("raul-simple-table", null,
                        h("table", null,
                            h("thead", null,
                                h("tr", null,
                                    h("th", null, "Name"),
                                    h("th", null, "Description"))),
                            h("tbody", null, this.docs.events.map(event => {
                                return (h("tr", null,
                                    h("td", null, event.event),
                                    h("td", null, event.docs || '[MISSING]')));
                            })))) || renderNotApplicable('events'))));
    }
    static get is() { return "docs-interface"; }
    static get originalStyleUrls() { return {
        "$": ["docs-interface.scss"]
    }; }
    static get styleUrls() { return {
        "$": ["docs-interface.css"]
    }; }
    static get properties() { return {
        "component": {
            "type": "string",
            "mutable": false,
            "complexType": {
                "original": "string",
                "resolved": "string",
                "references": {}
            },
            "required": true,
            "optional": false,
            "docs": {
                "tags": [],
                "text": ""
            },
            "attribute": "component",
            "reflect": true
        }
    }; }
}
