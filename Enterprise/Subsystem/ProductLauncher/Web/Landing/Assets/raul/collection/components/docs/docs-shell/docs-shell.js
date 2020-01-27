import { h } from "@stencil/core";
export class DocsShell {
    render() {
        return (h("div", { class: "docs-container" },
            h("docs-menu", { slot: "navigation" }),
            h("main", { slot: "page" },
                h("stencil-router", null,
                    h("stencil-route-switch", { scrollTopOffset: 0 },
                        h("stencil-route", { url: "/", component: "docs-home", exact: true }),
                        h("stencil-route", { url: "/overview/introduction", component: "docs-introduction" }),
                        h("stencil-route", { url: "/overview/upgrading", component: "docs-upgrading" }),
                        h("stencil-route", { url: "/overview/compliance", component: "docs-compliance" }),
                        h("stencil-route", { url: "/overview/accessibility", component: "docs-accessibility" }),
                        h("stencil-route", { url: "/theme/typography", component: "docs-typography" }),
                        h("stencil-route", { url: "/theme/colors", component: "docs-colors" }),
                        h("stencil-route", { url: "/theme/spacing", component: "docs-spacing" }),
                        h("stencil-route", { url: "/elements/index", component: "docs-index" }),
                        h("stencil-route", { url: "/elements/heading", component: "docs-raul-heading" }),
                        h("stencil-route", { url: "/elements/text", component: "docs-raul-text" }),
                        h("stencil-route", { url: "/elements/alert", component: "docs-raul-alert" }),
                        h("stencil-route", { url: "/elements/button", component: "docs-raul-button" }),
                        h("stencil-route", { url: "/elements/checkbox", component: "docs-raul-checkbox" }),
                        h("stencil-route", { url: "/elements/radio", component: "docs-raul-radio-button" }),
                        h("stencil-route", { url: "/elements/badge", component: "docs-raul-badge" }),
                        h("stencil-route", { url: "/elements/modal", component: "docs-raul-modal" }),
                        h("stencil-route", { url: "/elements/simple-select", component: "docs-raul-simple-select" }),
                        h("stencil-route", { url: "/elements/select", component: "docs-raul-select" }),
                        h("stencil-route", { url: "/elements/action-menu", component: "docs-raul-action-menu" }),
                        h("stencil-route", { url: "/elements/filter-menu", component: "docs-raul-filter-menu" }),
                        h("stencil-route", { url: "/elements/sortables", component: "docs-raul-sortable-list" }),
                        h("stencil-route", { url: "/elements/loaders", component: "docs-raul-loaders" }),
                        h("stencil-route", { url: "/elements/input", component: "docs-raul-input" }),
                        h("stencil-route", { url: "/elements/textarea", component: "docs-raul-textarea" }),
                        h("stencil-route", { url: "/elements/tabs", component: "docs-raul-tabs" }),
                        h("stencil-route", { url: "/elements/date-picker", component: "docs-raul-date-picker" }),
                        h("stencil-route", { url: "/elements/paging-bar", component: "docs-raul-paging-bar" }),
                        h("stencil-route", { url: "/elements/bulk-action-bar", component: "docs-raul-bulk-action-bar" }),
                        h("stencil-route", { url: "/elements/snackbar", component: "docs-raul-snackbar" }),
                        h("stencil-route", { url: "/elements/switch", component: "docs-raul-switch" }),
                        h("stencil-route", { url: "/elements/chips", component: "docs-raul-chips" }),
                        h("stencil-route", { url: "/elements/status", component: "docs-raul-status" }),
                        h("stencil-route", { url: "/elements/status-indicator", component: "docs-raul-status-indicator" }),
                        h("stencil-route", { url: "/elements/aside", component: "docs-raul-aside" }),
                        h("stencil-route", { url: "/elements/list", component: "docs-raul-list" }),
                        h("stencil-route", { url: "/elements/simple-table", component: "docs-raul-simple-table" }),
                        h("stencil-route", { url: "/elements/grid", component: "docs-raul-grid" }),
                        h("stencil-route", { url: "/elements/filter-bar", component: "docs-raul-filter-bar" }),
                        h("stencil-route", { url: "/elements/accordion", component: "docs-raul-accordion" }),
                        h("stencil-route", { url: "/elements/avatar", component: "docs-raul-avatar" }),
                        h("stencil-route", { url: "/elements/breadcrumbs", component: "docs-raul-breadcrumbs" }),
                        h("stencil-route", { url: "/elements/toggles", component: "docs-raul-toggles" }),
                        h("stencil-route", { url: "/elements/card", component: "docs-raul-card" }),
                        h("stencil-route", { url: "/elements/progress", component: "docs-raul-progress" }),
                        h("stencil-route", { url: "/elements/footer", component: "docs-raul-footer" }),
                        h("stencil-route", { url: "/elements/tooltip", component: "docs-raul-tooltip" }),
                        h("stencil-route", { component: "docs-404" }))))));
    }
    static get is() { return "docs-shell"; }
    static get originalStyleUrls() { return {
        "$": ["docs-shell.scss"]
    }; }
    static get styleUrls() { return {
        "$": ["docs-shell.css"]
    }; }
}
