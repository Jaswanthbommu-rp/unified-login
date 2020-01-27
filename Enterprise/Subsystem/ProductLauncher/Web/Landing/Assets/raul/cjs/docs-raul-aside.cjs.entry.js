'use strict';

Object.defineProperty(exports, '__esModule', { value: true });

const core = require('./core-40bc5f02.js');

const DocsRaulAside = class {
    constructor(hostRef) {
        core.registerInstance(this, hostRef);
    }
    open(value) {
        value.split(', ').forEach(id => document.getElementById(id).open());
    }
    close(value) {
        value.split(', ').forEach(id => document.getElementById(id).close());
    }
    render() {
        const asideHeaderContent = (name) => {
            return [
                core.h("raul-aside-title", null, name, " Aside Title"),
                core.h("raul-aside-subtitle", null, name, " Aside Subtitle")
            ];
        };
        const asideBodyText = () => {
            return (core.h("div", null, "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum."));
        };
        const asideActions = (id) => {
            return (core.h("raul-aside-actions", null, core.h("raul-button", { variant: "secondary", onClick: () => this.close(id) }, "Close"), core.h("raul-button", { class: "ml-3", variant: "primary", onClick: () => this.close(id) }, "Save")));
        };
        const renderAside = (id, size, name) => {
            return (core.h("raul-aside", { id: id, size: size }, core.h("raul-aside-header", null, asideHeaderContent(name)), core.h("raul-aside-body", null, asideBodyText()), core.h("raul-aside-footer", null, core.h("raul-aside-actions", null, asideActions(id)))));
        };
        const renderNestedAsides = (id, primarySize, secondarySize) => {
            return (core.h("raul-aside", { id: `primary-${id}`, size: primarySize }, core.h("raul-aside-header", null, asideHeaderContent('Primary')), core.h("raul-aside-body", null, asideBodyText(), core.h("raul-button", { class: "mt-3", variant: "secondary", onClick: () => this.open(`secondary-${id}`) }, "Open Secondary Aside")), core.h("raul-aside-footer", null, asideActions(`primary-${id}`)), core.h("raul-aside", { id: `secondary-${id}`, slot: "secondary-aside", size: secondarySize }, core.h("raul-aside-header", null, asideHeaderContent('Secondary')), core.h("raul-aside-body", null, asideBodyText()), core.h("raul-aside-footer", null, core.h("raul-aside-actions", null, core.h("raul-button", { variant: "secondary", onClick: () => this.close(`secondary-${id}`) }, "Close"), core.h("raul-button", { class: "ml-3", variant: "primary", onClick: () => this.close(`${`primary-${id}`}, ${`secondary-${id}`}`) }, "Save"))))));
        };
        return (core.h("docs-element", { title: "Asides" }, core.h("div", { slot: "overview" }, core.h("docs-showcase", null, core.h("div", { class: "mt-10" }, core.h("div", { class: "heading-lg" }, "Basic"), core.h("div", null, core.h("raul-button", { class: "mr-3", onClick: () => this.open('small-aside') }, "Small"), core.h("raul-button", { class: "mr-3", onClick: () => this.open('medium-aside') }, "Medium"), core.h("raul-button", { class: "mr-3", onClick: () => this.open('large-aside') }, "Large"))), core.h("div", { class: "mt-10" }, core.h("div", { class: "heading-lg" }, "Nested"), core.h("div", null, core.h("raul-button", { class: "mr-3", onClick: () => this.open('primary-small-to-small-aside') }, "Small to Small"), core.h("raul-button", { class: "mr-3", onClick: () => this.open('primary-small-to-medium-aside') }, "Small to Medium"), core.h("raul-button", { class: "mr-3", onClick: () => this.open('primary-small-to-large-aside') }, "Small to Large")), core.h("div", { class: "mt-4" }, core.h("raul-button", { class: "mr-3", onClick: () => this.open('primary-medium-to-small-aside') }, "Medium to Small"), core.h("raul-button", { class: "mr-3", onClick: () => this.open('primary-medium-to-medium-aside') }, "Medium to Medium"), core.h("raul-button", { class: "mr-3", onClick: () => this.open('primary-medium-to-large-aside') }, "Medium to Large")), core.h("div", { class: "mt-4" }, core.h("raul-button", { class: "mr-3", onClick: () => this.open('primary-large-to-small-aside') }, "Large to Small"), core.h("raul-button", { class: "mr-3", onClick: () => this.open('primary-large-to-medium-aside') }, "Large to Medium"), core.h("raul-button", { class: "mr-3", onClick: () => this.open('primary-large-to-large-aside') }, "Large to Large"))), renderAside('small-aside', 'small', 'Small'), renderAside('medium-aside', 'medium', 'Medium'), renderAside('large-aside', 'large', 'Large'), renderNestedAsides('small-to-small-aside', 'small', 'small'), renderNestedAsides('small-to-medium-aside', 'small', 'medium'), renderNestedAsides('small-to-large-aside', 'small', 'large'), renderNestedAsides('medium-to-small-aside', 'medium', 'small'), renderNestedAsides('medium-to-medium-aside', 'medium', 'medium'), renderNestedAsides('medium-to-large-aside', 'medium', 'large'), renderNestedAsides('large-to-small-aside', 'large', 'small'), renderNestedAsides('large-to-medium-aside', 'large', 'medium'), renderNestedAsides('large-to-large-aside', 'large', 'large'))), core.h("div", { slot: "design" }, core.h("docs-readme", { component: "raul-aside" })), core.h("div", { slot: "api" }, core.h("docs-interface", { component: "raul-aside" }))));
    }
};

exports.docs_raul_aside = DocsRaulAside;
