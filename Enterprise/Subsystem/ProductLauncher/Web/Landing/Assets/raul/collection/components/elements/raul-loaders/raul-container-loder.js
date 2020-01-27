import { h } from "@stencil/core";
const Dot = (props) => h("div", { class: "r-container-loader__dot", style: { animationDelay: props.animationDelay } });
export class RaulcontainerLoader {
    render() {
        return (h("div", { class: 'r-container-loader__container' },
            h("div", { class: 'r-container-loader__dots-container' }, Array.from({ length: 3 }).map((_, index) => h(Dot, { animationDelay: `${index * 333}ms` })))));
    }
    static get is() { return "raul-container-loader"; }
    static get originalStyleUrls() { return {
        "$": ["raul-container-loader.scss"]
    }; }
    static get styleUrls() { return {
        "$": ["raul-container-loader.css"]
    }; }
}
