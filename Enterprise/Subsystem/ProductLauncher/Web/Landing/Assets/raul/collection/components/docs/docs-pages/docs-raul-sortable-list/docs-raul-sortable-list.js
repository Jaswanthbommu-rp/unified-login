import { h } from "@stencil/core";
import initPage from '../init-page';
export class DocsRaulSortableList {
    componentDidLoad() {
        initPage('Sortable List');
    }
    render() {
        return (h("docs-element", { title: "Sortable" },
            h("div", { slot: "overview" },
                "Overview",
                h("docs-showcase", null,
                    h("raul-heading", null, "Sortable card groups"),
                    h("div", { style: { display: 'flex', flexDirection: 'row', justifyContent: 'flex-start', maxWidth: '1024px' } },
                        h("raul-sortable-list", { group: "example", style: { flex: '0' } },
                            h("raul-card", { class: "m-2" },
                                h("raul-card-header", null,
                                    h("raul-card-title", null, "Aardvark")),
                                h("raul-card-body", null, "Vivamus quis sapien in nisi ullamcorper.")),
                            h("raul-card", { class: "m-2" },
                                h("raul-card-header", null,
                                    h("raul-card-title", null, "Baboon")),
                                h("raul-card-body", null, "Ut auctor enim ligula, pretium.")),
                            h("raul-card", { class: "m-2" },
                                h("raul-card-header", null,
                                    h("raul-card-title", null, "Dingo")),
                                h("raul-card-body", null, "Morbi consequat, velit vel.")),
                            h("raul-card", { class: "m-2" },
                                h("raul-card-header", null,
                                    h("raul-card-title", null, "Aardvark")),
                                h("raul-card-body", null, "Vivamus quis sapien in nisi ullamcorper."))),
                        h("raul-sortable-list", { group: "example", style: { flex: '0' } },
                            h("raul-card", { class: "m-2" },
                                h("raul-card-header", null,
                                    h("raul-card-title", null, "Elephant")),
                                h("raul-card-body", null, "Vivamus quis sapien in nisi ullamcorper.")),
                            h("raul-card", { class: "m-2" },
                                h("raul-card-header", null,
                                    h("raul-card-title", null, "Flamingo")),
                                h("raul-card-body", null, "Ut auctor enim ligula, pretium.")),
                            h("raul-card", { class: "m-2" },
                                h("raul-card-header", null,
                                    h("raul-card-title", null, "Gorilla")),
                                h("raul-card-body", null, "Morbi consequat, velit vel.")),
                            h("raul-card", { class: "m-2" },
                                h("raul-card-header", null,
                                    h("raul-card-title", null, "Hawk")),
                                h("raul-card-body", null, "Vivamus quis sapien in nisi ullamcorper."))))),
                h("docs-readme", { component: "raul-sortable-list" })),
            h("div", { slot: "design" }, "Design Guidelines Stuff"),
            h("div", { slot: "api" },
                h("docs-interface", { component: "raul-sortable-list" }))));
    }
    static get is() { return "docs-raul-sortable-list"; }
}
