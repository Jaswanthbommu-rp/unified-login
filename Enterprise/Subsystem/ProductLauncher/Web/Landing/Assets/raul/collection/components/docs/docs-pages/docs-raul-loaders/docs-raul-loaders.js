import { h } from "@stencil/core";
import initPage from '../init-page';
export class DocsRaulLoaders {
    constructor() {
        this.showPageLoader = false;
        this.showContainerLoader = false;
    }
    componentDidLoad() {
        initPage('Loaders');
        window.addEventListener('keydown', e => {
            if (['Esc', 'Escape'].includes(e.key))
                this.showPageLoader = false;
        });
    }
    render() {
        return (h("docs-element", { title: "Loaders" },
            h("div", { slot: "overview" },
                h("raul-heading", { variant: 'page' }, "Loaders"),
                h("raul-heading", { variant: 'section' }, "Page loader"),
                h("raul-heading", { variant: 'content' }, "Usage"),
                h("raul-content", null,
                    h("p", null,
                        "You render a ",
                        h("code", null, `<raul-page-loader></raul-page-loader>`),
                        " wherever and whenever you need a page loader.")),
                h("raul-heading", { variant: 'content' }, "Examples"),
                h("raul-button", { variant: 'secondary', size: 'small', onClick: () => this.showPageLoader = true, class: "mb-12" }, "Show page loader"),
                h("raul-content", null,
                    h("p", null, "*Press Escape to hide the page loader")),
                this.showPageLoader &&
                    h("raul-page-loader", null),
                h("raul-heading", { variant: 'section' }, "Container loader"),
                h("raul-heading", { variant: 'content' }, "Usage"),
                h("raul-content", null,
                    h("p", null,
                        "You render a ",
                        h("code", null, `<raul-container-loader></raul-container-loader>`),
                        " as a child of the container that needs to be covered by the loader. The container needs to have ",
                        h("code", null, "position: relative"))),
                h("raul-button", { variant: 'secondary', size: 'small', onClick: () => this.showContainerLoader = !this.showContainerLoader, class: "mb-8" },
                    this.showContainerLoader ? 'Hide' : 'Show',
                    " container loader"),
                h("div", { class: "relative mb-12 p-6 w-full border border-gray-light" },
                    this.showContainerLoader && h("raul-container-loader", null),
                    h("raul-content", null,
                        h("p", null, "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Maecenas molestie bibendum felis at pulvinar. Duis porttitor nulla ligula, in volutpat ex iaculis nec. Mauris sollicitudin auctor turpis, a vehicula diam accumsan ultrices. Aenean lobortis dolor elit, id sodales felis volutpat sit amet. Donec elementum malesuada lorem non tincidunt. Morbi purus lectus, pharetra ac consequat vitae, interdum placerat tortor. Phasellus ac auctor dolor. Proin cursus, elit dignissim placerat porta, dolor metus tristique nunc, at fringilla mauris purus sed dolor. Suspendisse imperdiet leo ut nulla rutrum tristique nec in orci. Proin sit amet mauris fermentum, euismod metus nec, convallis dolor. Nulla et ornare nisi. Nunc at ligula rhoncus, venenatis purus vel, sodales dolor. Fusce finibus augue sem, ut volutpat arcu fermentum at.")),
                    h("docs-interface", { component: "raul-loaders" }))),
            h("div", { slot: "design" }, "Design Guidelines Stuff"),
            h("div", { slot: "api" }, "API Stuff")));
    }
    static get is() { return "docs-raul-loaders"; }
    static get states() { return {
        "showPageLoader": {},
        "showContainerLoader": {}
    }; }
}
