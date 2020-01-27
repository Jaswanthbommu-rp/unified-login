import { r as registerInstance, h } from './core-9263a98c.js';

const DocsRaulFilterBar = class {
    constructor(hostRef) {
        registerInstance(this, hostRef);
        this.products = [
            {
                text: 'All Products',
                value: ''
            },
            {
                text: 'Asset Optimization',
                value: 'AO'
            },
            {
                text: 'OneSite',
                value: 'OS'
            },
            {
                text: 'Unified Platform',
                value: 'UP'
            }
        ];
        this.productsTwo = [
            {
                text: 'All Products',
                value: ''
            },
            {
                text: 'Asset Optimization',
                value: 'AO'
            },
            {
                text: 'OneSite',
                value: 'OS'
            },
            {
                text: 'Unified Platform',
                value: 'UP'
            }
        ];
        this.priorities = [
            {
                text: 'All Priorities',
                value: ''
            },
            {
                text: 'High',
                value: 'high'
            },
            {
                text: 'Normal',
                value: 'normal'
            },
        ];
        this.prioritiesTwo = [
            {
                text: 'All Priorities',
                value: ''
            },
            {
                text: 'High',
                value: 'high'
            },
            {
                text: 'Normal',
                value: 'normal'
            },
        ];
        this.properties = [
            {
                text: 'All Properites',
                value: ''
            },
            {
                text: 'Arbor Hills',
                value: 'ah'
            },
            {
                text: 'Jackson Park',
                value: 'jp'
            },
            {
                text: 'Meadow Creek',
                value: 'mc'
            },
            {
                text: 'Willow Court',
                value: 'wc'
            },
        ];
        this.productSelectValue = '';
        this.prioritiesSelectValue = '';
        this.productTwoSelectValue = '';
        this.prioritiesTwoSelectValue = '';
        this.propertiesSelectValue = 'jp';
    }
    open(value) {
        value.split(', ').forEach(id => document.getElementById(id).open());
    }
    close(value) {
        value.split(', ').forEach(id => document.getElementById(id).close());
    }
    handleRaulChange(e, value) {
        this[value] = e.detail.value;
    }
    render() {
        return (h("docs-element", { title: "Filter Bar" }, h("div", { slot: "overview" }, h("raul-content", null, h("div", { class: "text-lg font-semibold mt-8 mb-3" }, "Simple Filter Layout"), h("raul-filter-bar", null, h("div", { class: "flex-grow md:w-1/3" }, h("raul-input", { type: "search" }, h("input", { type: "text", placeholder: "Search" }))), h("div", { class: "w-2/3 hidden md:flex" }, h("div", { class: "w-1/2 ml-6" }, h("raul-select", { options: this.products, value: this.productSelectValue, onRaulChange: e => this.handleRaulChange(e, 'productSelectValue') })), h("div", { class: "w-1/2 ml-6" }, h("raul-select", { options: this.priorities, value: this.prioritiesSelectValue, onRaulChange: e => this.handleRaulChange(e, 'prioritiesSelectValue') }))), h("raul-filter-bar-button", { class: "md:hidden", onClick: () => this.open('simpleOverflow') })), h("div", { class: "text-lg font-semibold mt-8 mb-0" }, "Responsive Overflow"), h("p", null, "Filters should overflow into an aside and only search and the filters overflow button should display on small screens."), h("docs-device", null, h("raul-filter-bar", null, h("div", { class: "flex-grow" }, h("raul-input", { type: "search" }, h("input", { type: "text", placeholder: "Search" }))), h("raul-filter-bar-button", null))), h("div", { class: "text-lg font-semibold mt-8 mb-3" }, "Complex Filter Layout"), h("raul-filter-bar", null, h("div", { class: "flex-grow md:w-1/2" }, h("raul-input", { type: "search" }, h("input", { type: "text", placeholder: "Search" }))), h("div", { class: "w-1/2 lg:w-2/3 hidden md:flex" }, h("div", { class: "w-full lg:w-1/2 ml-6" }, h("raul-select", { options: this.productsTwo, value: this.productTwoSelectValue, onRaulChange: e => this.handleRaulChange(e, 'productTwoSelectValue') })), h("div", { class: "hidden lg:block w-1/2 ml-6" }, h("raul-select", { options: this.prioritiesTwo, value: this.prioritiesTwoSelectValue, onRaulChange: e => this.handleRaulChange(e, 'prioritiesTwoSelectValue') }))), h("raul-filter-bar-button", { "filter-count": 1, onClick: () => this.open('advancedOverflow') }), h("raul-filter-menu", null, h("raul-filter-menu-item", { icon: 'list-bullets-3' }, "Table"), h("raul-filter-menu-item", { icon: 'content-view-module-2' }, "Cards"), h("raul-filter-menu-item", { icon: 'content-view-module-1' }, "Workflow")))), h("raul-aside", { id: "simpleOverflow", size: "small" }, h("raul-aside-header", null, h("div", { class: "flex items-end" }, h("div", { class: "flex-grow" }, h("h2", { class: "text-xl font-medium" }, "Filters")), h("div", { class: "flex-grow text-right" }, h("button", { class: "text-primary pb-1", type: "button" }, "Clear Filters")))), h("raul-aside-body", null, h("raul-select", { label: "Products", options: this.products, value: this.productSelectValue, class: "mb-4", onRaulChange: e => this.handleRaulChange(e, 'productSelectValue') }), h("raul-select", { label: "Priority", options: this.priorities, value: this.prioritiesSelectValue, class: "mb-4", onRaulChange: e => this.handleRaulChange(e, 'prioritiesSelectValue') })), h("raul-aside-footer", null, h("div", { class: "flex justify-end" }, h("raul-button", { class: "mr-2 w-1/2", variant: "secondary", onClick: () => this.close('simpleOverflow') }, "Cancel"), h("raul-button", { class: "ml-2 w-1/2", variant: "primary", onClick: () => this.close('simpleOverflow') }, "Apply")))), h("raul-aside", { id: "advancedOverflow", size: "small" }, h("raul-aside-header", null, h("div", { class: "flex items-end" }, h("div", { class: "flex-grow" }, h("h2", { class: "text-xl font-medium" }, "Filters")), h("div", { class: "flex-grow text-right" }, h("button", { class: "text-primary pb-1", type: "button" }, "Clear Filters")))), h("raul-aside-body", null, h("raul-select", { label: "Products", options: this.productsTwo, value: this.productTwoSelectValue, class: "md:hidden mb-4", onRaulChange: e => this.handleRaulChange(e, 'productTwoSelectValue') }), h("raul-select", { label: "Priority", options: this.prioritiesTwo, value: this.prioritiesTwoSelectValue, class: "lg:hidden mb-4", onRaulChange: e => this.handleRaulChange(e, 'prioritiesTwoSelectValue') }), h("raul-select", { label: "Property", options: this.properties, value: this.propertiesSelectValue, onRaulChange: e => this.handleRaulChange(e, 'propertiesSelectValue') })), h("raul-aside-footer", null, h("div", { class: "flex justify-end" }, h("raul-button", { class: "mr-2 w-1/2", variant: "secondary", onClick: () => this.close('advancedOverflow') }, "Cancel"), h("raul-button", { class: "ml-2 w-1/2", variant: "primary", onClick: () => this.close('advancedOverflow') }, "Apply"))))), h("div", { slot: "design" }, h("docs-readme", { component: "raul-filter-bar" })), h("div", { slot: "api" }, h("docs-preview", { component: "raul-filter-bar", content: "Filter Bar" }), h("docs-interface", { component: "raul-filter-bar" }))));
    }
};

export { DocsRaulFilterBar as docs_raul_filter_bar };
