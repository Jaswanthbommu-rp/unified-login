'use strict';

Object.defineProperty(exports, '__esModule', { value: true });

const core = require('./core-40bc5f02.js');
const initPage = require('./init-page-fbbb8a1a.js');

const DocsHome = class {
    constructor(hostRef) {
        core.registerInstance(this, hostRef);
    }
    componentDidLoad() {
        initPage.initPage('Home', false);
    }
    render() {
        return (core.h("div", { class: "docs-page-container docs-home" }, core.h("div", { class: "docs-page-header flex flex-wrap lg:flex-row-reverse p-0 items-stretch lg:items-center min-h-screen" }, core.h("div", { class: "home-illustration w-full lg:w-1/2 bg-center lg:bg-left lg:min-h-screen m-8 lg:m-0 lg:p-8" }), core.h("div", { class: "w-full lg:w-1/2 p-8 lg:pt-0 min-h-auto lg:min-h-full" }, core.h("h1", null, "RAUL 3"), core.h("p", { class: "pb-12 pr-6" }, "The next generation of the Realpage Asset & UI Library is responsive, accessibile, and built with web components so it's also technology neutral."), core.h("raul-button", { variant: "primary", href: "/elements/index" }, "Components")))));
    }
    static get style() { return ".docs-home{background-color:#fff}.docs-home .docs-page-header{background:url(/assets/illustrations/bg-blob.svg) -20vw -20vw no-repeat;background-size:cover;min-height:calc(100vh - .75rem)}.docs-home .docs-page-header h1{white-space:nowrap;font-size:9vw}.docs-home .docs-page-header .home-illustration{background-image:url(/assets/illustrations/coding_pages.svg);background-repeat:no-repeat;background-size:contain;min-height:20vh}\@media (min-width:1024px){.docs-home .docs-page-header .home-illustration{min-height:100vh}}"; }
};

exports.docs_home = DocsHome;
