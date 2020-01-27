import { h } from "@stencil/core";
import initPage from '../init-page';
export class DocsTypography {
    componentDidLoad() {
        initPage('Typography', false);
    }
    render() {
        return (h("docs-element", { title: "Typography" },
            h("div", { slot: "overview" },
                h("h2", { class: "heading-xl" }, "Introduction"),
                "By default RAUL resets the styles and spacing on basic typographical elements like h1, h2, etc. and provides utility classes to control font size and weight. For longform HTML content (possibly user generated), RAUL provides the \\`raul-content\\` custom element that style that content appropriately.",
                h("h2", { class: "heading-xl mt-8" }, "Headings"),
                h("h1", { class: "heading-hero mt-3" }, "H1.heading-hero"),
                h("p", null,
                    "The ",
                    h("code", null, ".heading-hero"),
                    " class is reserved for page headers. Typically this is an H1."),
                h("h2", { class: "heading-xl mt-3" }, "H2.heading-xl"),
                h("p", null,
                    "The ",
                    h("code", null, ".heading-xl"),
                    " class is reserved for section headers. Typically this is an H2."),
                h("h3", { class: "heading-lg mt-3" }, "H3.heading-lg"),
                h("p", null,
                    "The ",
                    h("code", null, ".heading-lg"),
                    " class is reserved for subsection headers. Typically this is an H3."),
                h("h4", { class: "heading-md mt-3" }, "H4.heading-md"),
                h("p", null,
                    "The ",
                    h("code", null, ".heading-md"),
                    " class is reserved for page headers. Typically this is an H4."),
                h("h2", { class: "heading-xl mt-8" }, "Utility Classes"),
                h("p", null,
                    h("div", { class: "text-hero mt-3" }, ".text-hero"),
                    h("code", null, ".text-hero"),
                    " - Font size like h1"),
                h("p", null,
                    h("div", { class: "text-xl mt-3" }, ".text-xl"),
                    h("code", null, ".text-xl"),
                    " - Font size like h2"),
                h("p", null,
                    h("div", { class: "text-lg mt-3" }, ".text-lg"),
                    h("code", null, ".text-lg"),
                    " - Font size like h3"),
                h("p", null,
                    h("div", { class: "text-md mt-3" }, ".text-md"),
                    h("code", null, ".text-md"),
                    " - Font size normal"),
                h("p", null,
                    h("div", { class: "text-sm mt-3" }, ".text-sm"),
                    h("code", null, ".text-sm"),
                    " - Font size smaller"),
                h("p", null,
                    h("div", { class: "font-bold mt-3" }, ".font-bold"),
                    h("code", null, ".font-bold"),
                    " - Font weight 700"),
                h("p", null,
                    h("div", { class: "font-semibold mt-3" }, ".font-semibold"),
                    h("code", null, ".font-semibold"),
                    " - Font weight 600"),
                h("p", null,
                    h("div", { class: "font-normal mt-3" }, ".font-normal"),
                    h("code", null, ".font-normal"),
                    " - Font weight 400"),
                h("h2", { class: "heading-xl mt-8" }, "Freeform Content"),
                h("docs-markdown", null, `
                For markup over which you do not have control (and thus cannot add classes to), RAUL provides the \`raul-content\` element. Simply wrap your HTML content and it will be styled appropriately e.g.

                \`\`\`
                <raul-content>
                  <h1>Main St Appts<h1>
                  <p>Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nunc mollis purus sit amet.</p>
                </raul-content>
                \`\`\`
            `)),
            h("div", { slot: "design" }, "Design Guidelines Stuff"),
            h("div", { slot: "api" }, "API Stuff")));
    }
    static get is() { return "docs-typography"; }
}
