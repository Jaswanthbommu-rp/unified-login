import { Host, h } from "@stencil/core";
export class DocsMenu {
    constructor() {
        this.open = false;
    }
    toggleMenu() {
        this.open = !this.open;
    }
    render() {
        return (h(Host, { class: this.open ? 'open' : '' },
            h("div", { class: "r-docs-menu" },
                h("button", { class: "docs-menu-toggle", onClick: () => this.toggleMenu() },
                    h("span", null),
                    h("span", null),
                    h("span", null),
                    h("span", null)),
                h("docs-menu-section", { name: "Home", url: "/" }),
                h("docs-menu-section", { name: "Overview" },
                    h("docs-menu-page", { name: "Introduction", url: "/overview/introduction" }),
                    h("docs-menu-page", { name: "Upgrading", url: "/overview/upgrading" }),
                    h("docs-menu-page", { name: "Compliance", url: "/overview/compliance" }),
                    h("docs-menu-page", { name: "Accessibility", url: "/overview/accessibility" })),
                h("docs-menu-section", { name: "Theme" },
                    h("docs-menu-page", { name: "Typography", url: "/theme/typography" }),
                    h("docs-menu-page", { name: "Colors", url: "/theme/colors" }),
                    h("docs-menu-page", { name: "Spacing", url: "/theme/spacing" })),
                h("docs-menu-section", { name: "Components" },
                    h("docs-menu-page", { name: "Overview", url: "/elements/index" }),
                    h("docs-menu-elem-page", { name: "Accordion", url: "/elements/accordion" }),
                    h("docs-menu-elem-page", { name: "Action menu", url: "/elements/action-menu" }),
                    h("docs-menu-elem-page", { name: "Alert", url: "/elements/alert" }),
                    h("docs-menu-elem-page", { name: "Aside", url: "/elements/aside" }),
                    h("docs-menu-elem-page", { name: "Avatar", url: "/elements/avatar" }),
                    h("docs-menu-elem-page", { name: "Badge", url: "/elements/badge" }),
                    h("docs-menu-elem-page", { name: "Breadcrumbs", url: "/elements/breadcrumbs" }),
                    h("docs-menu-elem-page", { name: "Bulk Action Bar", url: "/elements/bulk-action-bar" }),
                    h("docs-menu-elem-page", { name: "Button", url: "/elements/button" }),
                    h("docs-menu-elem-page", { name: "Card", url: "/elements/card" }),
                    h("docs-menu-elem-page", { name: "Checkbox", url: "/elements/checkbox" }),
                    h("docs-menu-elem-page", { name: "Chips", url: "/elements/chips" }),
                    h("docs-menu-elem-page", { name: "Date picker", url: "/elements/date-picker" }),
                    h("docs-menu-elem-page", { name: "Filter Bar", url: "/elements/filter-bar" }),
                    h("docs-menu-elem-page", { name: "Filter menu", url: "/elements/filter-menu" }),
                    h("docs-menu-elem-page", { name: "Footer", url: "/elements/footer" }),
                    h("docs-menu-elem-page", { name: "Grid", url: "/elements/grid" }),
                    h("docs-menu-elem-page", { name: "Input", url: "/elements/input" }),
                    h("docs-menu-elem-page", { name: "List", url: "/elements/list" }),
                    h("docs-menu-elem-page", { name: "Loaders", url: "/elements/loaders" }),
                    h("docs-menu-elem-page", { name: "Modal", url: "/elements/modal" }),
                    h("docs-menu-elem-page", { name: "Paging Bar", url: "/elements/paging-bar" }),
                    h("docs-menu-elem-page", { name: "Progress", url: "/elements/progress" }),
                    h("docs-menu-elem-page", { name: "Radio Button", url: "/elements/radio" }),
                    h("docs-menu-elem-page", { name: "Select", url: "/elements/select" }),
                    h("docs-menu-elem-page", { name: "Simple Select", url: "/elements/simple-select" }),
                    h("docs-menu-elem-page", { name: "Simple Table", url: "/elements/simple-table" }),
                    h("docs-menu-elem-page", { name: "Snackbar", url: "/elements/snackbar" }),
                    h("docs-menu-elem-page", { name: "Sortables", url: "/elements/sortables" }),
                    h("docs-menu-elem-page", { name: "Status", url: "/elements/status" }),
                    h("docs-menu-elem-page", { name: "Status Indicator", url: "/elements/status-indicator" }),
                    h("docs-menu-elem-page", { name: "Switch", url: "/elements/switch" }),
                    h("docs-menu-elem-page", { name: "Tabs", url: "/elements/tabs" }),
                    h("docs-menu-elem-page", { name: "Textarea", url: "/elements/textarea" }),
                    h("docs-menu-elem-page", { name: "Toggles", url: "/elements/toggles" }),
                    h("docs-menu-elem-page", { name: "Tooltip", url: "/elements/tooltip" })))));
    }
    static get is() { return "docs-menu"; }
    static get originalStyleUrls() { return {
        "$": ["docs-menu.scss"]
    }; }
    static get styleUrls() { return {
        "$": ["docs-menu.css"]
    }; }
    static get states() { return {
        "open": {}
    }; }
    static get listeners() { return [{
            "name": "menuPageActivated",
            "method": "toggleMenu",
            "target": "window",
            "capture": false,
            "passive": false
        }]; }
}
