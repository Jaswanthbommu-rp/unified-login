'use strict';

Object.defineProperty(exports, '__esModule', { value: true });

const core = require('./core-40bc5f02.js');
const initPage = require('./init-page-fbbb8a1a.js');

const DocsTypography = class {
    constructor(hostRef) {
        core.registerInstance(this, hostRef);
    }
    componentDidLoad() {
        initPage.initPage('Typography', false);
    }
    render() {
        return (core.h("docs-element", { title: "Typography" }, core.h("div", { slot: "overview" }, core.h("h2", { class: "heading-xl" }, "Introduction"), "By default RAUL resets the styles and spacing on basic typographical elements like h1, h2, etc. and provides utility classes to control font size and weight. For longform HTML content (possibly user generated), RAUL provides the \\`raul-content\\` custom element that style that content appropriately.", core.h("h2", { class: "heading-xl mt-8" }, "Headings"), core.h("h1", { class: "heading-hero mt-3" }, "H1.heading-hero"), core.h("p", null, "The ", core.h("code", null, ".heading-hero"), " class is reserved for page headers. Typically this is an H1."), core.h("h2", { class: "heading-xl mt-3" }, "H2.heading-xl"), core.h("p", null, "The ", core.h("code", null, ".heading-xl"), " class is reserved for section headers. Typically this is an H2."), core.h("h3", { class: "heading-lg mt-3" }, "H3.heading-lg"), core.h("p", null, "The ", core.h("code", null, ".heading-lg"), " class is reserved for subsection headers. Typically this is an H3."), core.h("h4", { class: "heading-md mt-3" }, "H4.heading-md"), core.h("p", null, "The ", core.h("code", null, ".heading-md"), " class is reserved for page headers. Typically this is an H4."), core.h("h2", { class: "heading-xl mt-8" }, "Utility Classes"), core.h("p", null, core.h("div", { class: "text-hero mt-3" }, ".text-hero"), core.h("code", null, ".text-hero"), " - Font size like h1"), core.h("p", null, core.h("div", { class: "text-xl mt-3" }, ".text-xl"), core.h("code", null, ".text-xl"), " - Font size like h2"), core.h("p", null, core.h("div", { class: "text-lg mt-3" }, ".text-lg"), core.h("code", null, ".text-lg"), " - Font size like h3"), core.h("p", null, core.h("div", { class: "text-md mt-3" }, ".text-md"), core.h("code", null, ".text-md"), " - Font size normal"), core.h("p", null, core.h("div", { class: "text-sm mt-3" }, ".text-sm"), core.h("code", null, ".text-sm"), " - Font size smaller"), core.h("p", null, core.h("div", { class: "font-bold mt-3" }, ".font-bold"), core.h("code", null, ".font-bold"), " - Font weight 700"), core.h("p", null, core.h("div", { class: "font-semibold mt-3" }, ".font-semibold"), core.h("code", null, ".font-semibold"), " - Font weight 600"), core.h("p", null, core.h("div", { class: "font-normal mt-3" }, ".font-normal"), core.h("code", null, ".font-normal"), " - Font weight 400"), core.h("h2", { class: "heading-xl mt-8" }, "Freeform Content"), core.h("docs-markdown", null, `
                For markup over which you do not have control (and thus cannot add classes to), RAUL provides the \`raul-content\` element. Simply wrap your HTML content and it will be styled appropriately e.g.

                \`\`\`
                <raul-content>
                  <h1>Main St Appts<h1>
                  <p>Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nunc mollis purus sit amet.</p>
                </raul-content>
                \`\`\`
            `)), core.h("div", { slot: "design" }, "Design Guidelines Stuff"), core.h("div", { slot: "api" }, "API Stuff")));
    }
};

exports.docs_typography = DocsTypography;
