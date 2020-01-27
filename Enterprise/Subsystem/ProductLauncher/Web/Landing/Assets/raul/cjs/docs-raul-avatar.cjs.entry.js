'use strict';

Object.defineProperty(exports, '__esModule', { value: true });

const core = require('./core-40bc5f02.js');

const DocsRaulAvatar = class {
    constructor(hostRef) {
        core.registerInstance(this, hostRef);
    }
    render() {
        return (core.h("docs-element", { title: "Avatar" }, core.h("div", { slot: "overview" }, core.h("docs-showcase", null, core.h("div", null, core.h("raul-avatar", { class: "mr-3" }), core.h("raul-avatar", { variant: "property" })), core.h("div", { class: "mt-3" }, core.h("raul-avatar", { primary: true, class: "mr-3" }), core.h("raul-avatar", { primary: true, variant: "property" })), core.h("div", { class: "mt-3" }, core.h("raul-avatar", { small: true, class: "mr-3" }), core.h("raul-avatar", { small: true, variant: "property" })), core.h("div", { class: "mt-3" }, core.h("raul-avatar", { primary: true, small: true, class: "mr-3" }), core.h("raul-avatar", { primary: true, small: true, variant: "property" })))), core.h("div", { slot: "design" }, "Design Guidelines"), core.h("div", { slot: "api" }, core.h("docs-preview", { component: "raul-avatar" }), core.h("docs-interface", { component: "raul-avatar" }))));
    }
};

exports.docs_raul_avatar = DocsRaulAvatar;
