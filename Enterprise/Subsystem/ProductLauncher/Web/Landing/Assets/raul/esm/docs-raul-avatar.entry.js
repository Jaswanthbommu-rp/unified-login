import { r as registerInstance, h } from './core-9263a98c.js';

const DocsRaulAvatar = class {
    constructor(hostRef) {
        registerInstance(this, hostRef);
    }
    render() {
        return (h("docs-element", { title: "Avatar" }, h("div", { slot: "overview" }, h("docs-showcase", null, h("div", null, h("raul-avatar", { class: "mr-3" }), h("raul-avatar", { variant: "property" })), h("div", { class: "mt-3" }, h("raul-avatar", { primary: true, class: "mr-3" }), h("raul-avatar", { primary: true, variant: "property" })), h("div", { class: "mt-3" }, h("raul-avatar", { small: true, class: "mr-3" }), h("raul-avatar", { small: true, variant: "property" })), h("div", { class: "mt-3" }, h("raul-avatar", { primary: true, small: true, class: "mr-3" }), h("raul-avatar", { primary: true, small: true, variant: "property" })))), h("div", { slot: "design" }, "Design Guidelines"), h("div", { slot: "api" }, h("docs-preview", { component: "raul-avatar" }), h("docs-interface", { component: "raul-avatar" }))));
    }
};

export { DocsRaulAvatar as docs_raul_avatar };
