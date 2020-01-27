'use strict';

Object.defineProperty(exports, '__esModule', { value: true });

const core = require('./core-40bc5f02.js');
const initPage = require('./init-page-fbbb8a1a.js');

const DocsRaulSortableList = class {
    constructor(hostRef) {
        core.registerInstance(this, hostRef);
    }
    componentDidLoad() {
        initPage.initPage('Sortable List');
    }
    render() {
        return (core.h("docs-element", { title: "Sortable" }, core.h("div", { slot: "overview" }, "Overview", core.h("docs-showcase", null, core.h("raul-heading", null, "Sortable card groups"), core.h("div", { style: { display: 'flex', flexDirection: 'row', justifyContent: 'flex-start', maxWidth: '1024px' } }, core.h("raul-sortable-list", { group: "example", style: { flex: '0' } }, core.h("raul-card", { class: "m-2" }, core.h("raul-card-header", null, core.h("raul-card-title", null, "Aardvark")), core.h("raul-card-body", null, "Vivamus quis sapien in nisi ullamcorper.")), core.h("raul-card", { class: "m-2" }, core.h("raul-card-header", null, core.h("raul-card-title", null, "Baboon")), core.h("raul-card-body", null, "Ut auctor enim ligula, pretium.")), core.h("raul-card", { class: "m-2" }, core.h("raul-card-header", null, core.h("raul-card-title", null, "Dingo")), core.h("raul-card-body", null, "Morbi consequat, velit vel.")), core.h("raul-card", { class: "m-2" }, core.h("raul-card-header", null, core.h("raul-card-title", null, "Aardvark")), core.h("raul-card-body", null, "Vivamus quis sapien in nisi ullamcorper."))), core.h("raul-sortable-list", { group: "example", style: { flex: '0' } }, core.h("raul-card", { class: "m-2" }, core.h("raul-card-header", null, core.h("raul-card-title", null, "Elephant")), core.h("raul-card-body", null, "Vivamus quis sapien in nisi ullamcorper.")), core.h("raul-card", { class: "m-2" }, core.h("raul-card-header", null, core.h("raul-card-title", null, "Flamingo")), core.h("raul-card-body", null, "Ut auctor enim ligula, pretium.")), core.h("raul-card", { class: "m-2" }, core.h("raul-card-header", null, core.h("raul-card-title", null, "Gorilla")), core.h("raul-card-body", null, "Morbi consequat, velit vel.")), core.h("raul-card", { class: "m-2" }, core.h("raul-card-header", null, core.h("raul-card-title", null, "Hawk")), core.h("raul-card-body", null, "Vivamus quis sapien in nisi ullamcorper."))))), core.h("docs-readme", { component: "raul-sortable-list" })), core.h("div", { slot: "design" }, "Design Guidelines Stuff"), core.h("div", { slot: "api" }, core.h("docs-interface", { component: "raul-sortable-list" }))));
    }
};

exports.docs_raul_sortable_list = DocsRaulSortableList;
