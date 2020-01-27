'use strict';

Object.defineProperty(exports, '__esModule', { value: true });

const core = require('./core-40bc5f02.js');

const DocsRaulFilterBar = class {
    constructor(hostRef) {
        core.registerInstance(this, hostRef);
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
        return (core.h("docs-element", { title: "Filter Bar" }, core.h("div", { slot: "overview" }, core.h("raul-content", null, core.h("div", { class: "text-lg font-semibold mt-8 mb-3" }, "Simple Filter Layout"), core.h("raul-filter-bar", null, core.h("div", { class: "flex-grow md:w-1/3" }, core.h("raul-input", { type: "search" }, core.h("input", { type: "text", placeholder: "Search" }))), core.h("div", { class: "w-2/3 hidden md:flex" }, core.h("div", { class: "w-1/2 ml-6" }, core.h("raul-select", { options: this.products, value: this.productSelectValue, onRaulChange: e => this.handleRaulChange(e, 'productSelectValue') })), core.h("div", { class: "w-1/2 ml-6" }, core.h("raul-select", { options: this.priorities, value: this.prioritiesSelectValue, onRaulChange: e => this.handleRaulChange(e, 'prioritiesSelectValue') }))), core.h("raul-filter-bar-button", { class: "md:hidden", onClick: () => this.open('simpleOverflow') })), core.h("div", { class: "text-lg font-semibold mt-8 mb-0" }, "Responsive Overflow"), core.h("p", null, "Filters should overflow into an aside and only search and the filters overflow button should display on small screens."), core.h("docs-device", null, core.h("raul-filter-bar", null, core.h("div", { class: "flex-grow" }, core.h("raul-input", { type: "search" }, core.h("input", { type: "text", placeholder: "Search" }))), core.h("raul-filter-bar-button", null))), core.h("div", { class: "text-lg font-semibold mt-8 mb-3" }, "Complex Filter Layout"), core.h("raul-filter-bar", null, core.h("div", { class: "flex-grow md:w-1/2" }, core.h("raul-input", { type: "search" }, core.h("input", { type: "text", placeholder: "Search" }))), core.h("div", { class: "w-1/2 lg:w-2/3 hidden md:flex" }, core.h("div", { class: "w-full lg:w-1/2 ml-6" }, core.h("raul-select", { options: this.productsTwo, value: this.productTwoSelectValue, onRaulChange: e => this.handleRaulChange(e, 'productTwoSelectValue') })), core.h("div", { class: "hidden lg:block w-1/2 ml-6" }, core.h("raul-select", { options: this.prioritiesTwo, value: this.prioritiesTwoSelectValue, onRaulChange: e => this.handleRaulChange(e, 'prioritiesTwoSelectValue') }))), core.h("raul-filter-bar-button", { "filter-count": 1, onClick: () => this.open('advancedOverflow') }), core.h("raul-filter-menu", null, core.h("raul-filter-menu-item", { icon: 'list-bullets-3' }, "Table"), core.h("raul-filter-menu-item", { icon: 'content-view-module-2' }, "Cards"), core.h("raul-filter-menu-item", { icon: 'content-view-module-1' }, "Workflow")))), core.h("raul-aside", { id: "simpleOverflow", size: "small" }, core.h("raul-aside-header", null, core.h("div", { class: "flex items-end" }, core.h("div", { class: "flex-grow" }, core.h("h2", { class: "text-xl font-medium" }, "Filters")), core.h("div", { class: "flex-grow text-right" }, core.h("button", { class: "text-primary pb-1", type: "button" }, "Clear Filters")))), core.h("raul-aside-body", null, core.h("raul-select", { label: "Products", options: this.products, value: this.productSelectValue, class: "mb-4", onRaulChange: e => this.handleRaulChange(e, 'productSelectValue') }), core.h("raul-select", { label: "Priority", options: this.priorities, value: this.prioritiesSelectValue, class: "mb-4", onRaulChange: e => this.handleRaulChange(e, 'prioritiesSelectValue') })), core.h("raul-aside-footer", null, core.h("div", { class: "flex justify-end" }, core.h("raul-button", { class: "mr-2 w-1/2", variant: "secondary", onClick: () => this.close('simpleOverflow') }, "Cancel"), core.h("raul-button", { class: "ml-2 w-1/2", variant: "primary", onClick: () => this.close('simpleOverflow') }, "Apply")))), core.h("raul-aside", { id: "advancedOverflow", size: "small" }, core.h("raul-aside-header", null, core.h("div", { class: "flex items-end" }, core.h("div", { class: "flex-grow" }, core.h("h2", { class: "text-xl font-medium" }, "Filters")), core.h("div", { class: "flex-grow text-right" }, core.h("button", { class: "text-primary pb-1", type: "button" }, "Clear Filters")))), core.h("raul-aside-body", null, core.h("raul-select", { label: "Products", options: this.productsTwo, value: this.productTwoSelectValue, class: "md:hidden mb-4", onRaulChange: e => this.handleRaulChange(e, 'productTwoSelectValue') }), core.h("raul-select", { label: "Priority", options: this.prioritiesTwo, value: this.prioritiesTwoSelectValue, class: "lg:hidden mb-4", onRaulChange: e => this.handleRaulChange(e, 'prioritiesTwoSelectValue') }), core.h("raul-select", { label: "Property", options: this.properties, value: this.propertiesSelectValue, onRaulChange: e => this.handleRaulChange(e, 'propertiesSelectValue') })), core.h("raul-aside-footer", null, core.h("div", { class: "flex justify-end" }, core.h("raul-button", { class: "mr-2 w-1/2", variant: "secondary", onClick: () => this.close('advancedOverflow') }, "Cancel"), core.h("raul-button", { class: "ml-2 w-1/2", variant: "primary", onClick: () => this.close('advancedOverflow') }, "Apply"))))), core.h("div", { slot: "design" }, core.h("docs-readme", { component: "raul-filter-bar" })), core.h("div", { slot: "api" }, core.h("docs-preview", { component: "raul-filter-bar", content: "Filter Bar" }), core.h("docs-interface", { component: "raul-filter-bar" }))));
    }
};

exports.docs_raul_filter_bar = DocsRaulFilterBar;
