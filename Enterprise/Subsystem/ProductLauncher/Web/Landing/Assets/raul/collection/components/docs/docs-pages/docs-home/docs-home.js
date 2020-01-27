import { h } from "@stencil/core";
import initPage from '../init-page';
export class DocsHome {
    componentDidLoad() {
        initPage('Home', false);
    }
    render() {
        return (h("div", { class: "docs-page-container docs-home" },
            h("div", { class: "docs-page-header flex flex-wrap lg:flex-row-reverse p-0 items-stretch lg:items-center min-h-screen" },
                h("div", { class: "home-illustration w-full lg:w-1/2 bg-center lg:bg-left lg:min-h-screen m-8 lg:m-0 lg:p-8" }),
                h("div", { class: "w-full lg:w-1/2 p-8 lg:pt-0 min-h-auto lg:min-h-full" },
                    h("h1", null, "RAUL 3"),
                    h("p", { class: "pb-12 pr-6" }, "The next generation of the Realpage Asset & UI Library is responsive, accessibile, and built with web components so it's also technology neutral."),
                    h("raul-button", { variant: "primary", href: "/elements/index" }, "Components")))));
    }
    static get is() { return "docs-home"; }
    static get originalStyleUrls() { return {
        "$": ["docs-home.scss"]
    }; }
    static get styleUrls() { return {
        "$": ["docs-home.css"]
    }; }
}
