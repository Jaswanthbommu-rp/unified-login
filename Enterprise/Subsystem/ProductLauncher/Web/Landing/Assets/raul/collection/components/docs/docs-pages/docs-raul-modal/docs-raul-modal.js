import { h } from "@stencil/core";
import { loremIpsum } from './lorem-ipsum';
export class DocsRaulModal {
    constructor() {
        this.showBasicModal = false;
        this.showLongContentModal = false;
        this.showMediaModal = false;
    }
    handleCloseModal() {
        this.showBasicModal = false;
        this.showLongContentModal = false;
        this.showMediaModal = false;
    }
    render() {
        return (h("docs-element", { title: "Modal" },
            h("div", { slot: "overview" },
                h("docs-readme", { component: "raul-modal" }),
                h("raul-heading", { variant: 'content' }, "Examples"),
                h("raul-button", { variant: "text", size: "small", class: 'mb-4 inline-block', onClick: () => this.showBasicModal = !this.showBasicModal }, "Show a very simple modal"),
                h("div", null, this.showBasicModal &&
                    h("raul-modal", { variant: 'normal' },
                        h("raul-modal-header", { "modal-title": 'Lorem ipsum', description: 'Dolor sit amet' }),
                        h("raul-modal-footer", null,
                            h("raul-button", { size: 'small', onClick: () => this.showBasicModal = false }, "Dismiss"),
                            h("raul-button", { size: 'small', variant: 'primary', onClick: () => this.showBasicModal = false }, "Submit")))),
                h("raul-button", { variant: "text", size: "small", class: 'mb-4 inline-block', onClick: () => this.showLongContentModal = !this.showLongContentModal }, "Show a modal with a lot of content"),
                h("div", null, this.showLongContentModal &&
                    h("raul-modal", { variant: 'normal' },
                        h("raul-modal-header", { "modal-title": 'Lorem ipsum', description: 'Dolor sit amet' }),
                        h("raul-modal-body", null,
                            h("raul-content", null,
                                h("p", null, loremIpsum))),
                        h("raul-modal-footer", null,
                            h("raul-button", { size: 'small', onClick: () => this.showLongContentModal = false }, "Dismiss"),
                            h("raul-button", { size: 'small', variant: 'primary', onClick: () => this.showLongContentModal = false }, "Submit")))),
                h("raul-button", { variant: "text", size: "small", class: 'mb-4 inline-block', onClick: () => this.showMediaModal = !this.showMediaModal }, "Show a media modal"),
                h("div", null, this.showMediaModal &&
                    h("raul-modal", { variant: 'media' },
                        h("raul-modal-header", { "modal-title": 'Lorem ipsum', description: 'Dolor sit amet' }),
                        h("raul-modal-body", null,
                            h("img", { src: 'https://images.unsplash.com/photo-1569315618680-3d673b5e1514?ixlib=rb-1.2.1&ixid=eyJhcHBfaWQiOjEyMDd9&auto=format&fit=crop&w=1950&q=80' }))))),
            h("div", { slot: "design" }, "Design Guidelines Stuff"),
            h("div", { slot: "api" },
                h("docs-interface", { component: "raul-modal" }))));
    }
    static get is() { return "docs-raul-modal"; }
    static get states() { return {
        "showBasicModal": {},
        "showLongContentModal": {},
        "showMediaModal": {}
    }; }
    static get listeners() { return [{
            "name": "modalClose",
            "method": "handleCloseModal",
            "target": undefined,
            "capture": false,
            "passive": false
        }]; }
}
