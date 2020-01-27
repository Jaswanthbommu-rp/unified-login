import { r as registerInstance, h } from './core-9263a98c.js';
var DocsRaulAside = /** @class */ (function () {
    function DocsRaulAside(hostRef) {
        registerInstance(this, hostRef);
    }
    DocsRaulAside.prototype.open = function (value) {
        value.split(', ').forEach(function (id) { return document.getElementById(id).open(); });
    };
    DocsRaulAside.prototype.close = function (value) {
        value.split(', ').forEach(function (id) { return document.getElementById(id).close(); });
    };
    DocsRaulAside.prototype.render = function () {
        var _this = this;
        var asideHeaderContent = function (name) {
            return [
                h("raul-aside-title", null, name, " Aside Title"),
                h("raul-aside-subtitle", null, name, " Aside Subtitle")
            ];
        };
        var asideBodyText = function () {
            return (h("div", null, "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum."));
        };
        var asideActions = function (id) {
            return (h("raul-aside-actions", null, h("raul-button", { variant: "secondary", onClick: function () { return _this.close(id); } }, "Close"), h("raul-button", { class: "ml-3", variant: "primary", onClick: function () { return _this.close(id); } }, "Save")));
        };
        var renderAside = function (id, size, name) {
            return (h("raul-aside", { id: id, size: size }, h("raul-aside-header", null, asideHeaderContent(name)), h("raul-aside-body", null, asideBodyText()), h("raul-aside-footer", null, h("raul-aside-actions", null, asideActions(id)))));
        };
        var renderNestedAsides = function (id, primarySize, secondarySize) {
            return (h("raul-aside", { id: "primary-" + id, size: primarySize }, h("raul-aside-header", null, asideHeaderContent('Primary')), h("raul-aside-body", null, asideBodyText(), h("raul-button", { class: "mt-3", variant: "secondary", onClick: function () { return _this.open("secondary-" + id); } }, "Open Secondary Aside")), h("raul-aside-footer", null, asideActions("primary-" + id)), h("raul-aside", { id: "secondary-" + id, slot: "secondary-aside", size: secondarySize }, h("raul-aside-header", null, asideHeaderContent('Secondary')), h("raul-aside-body", null, asideBodyText()), h("raul-aside-footer", null, h("raul-aside-actions", null, h("raul-button", { variant: "secondary", onClick: function () { return _this.close("secondary-" + id); } }, "Close"), h("raul-button", { class: "ml-3", variant: "primary", onClick: function () { return _this.close("primary-" + id + ", " + ("secondary-" + id)); } }, "Save"))))));
        };
        return (h("docs-element", { title: "Asides" }, h("div", { slot: "overview" }, h("docs-showcase", null, h("div", { class: "mt-10" }, h("div", { class: "heading-lg" }, "Basic"), h("div", null, h("raul-button", { class: "mr-3", onClick: function () { return _this.open('small-aside'); } }, "Small"), h("raul-button", { class: "mr-3", onClick: function () { return _this.open('medium-aside'); } }, "Medium"), h("raul-button", { class: "mr-3", onClick: function () { return _this.open('large-aside'); } }, "Large"))), h("div", { class: "mt-10" }, h("div", { class: "heading-lg" }, "Nested"), h("div", null, h("raul-button", { class: "mr-3", onClick: function () { return _this.open('primary-small-to-small-aside'); } }, "Small to Small"), h("raul-button", { class: "mr-3", onClick: function () { return _this.open('primary-small-to-medium-aside'); } }, "Small to Medium"), h("raul-button", { class: "mr-3", onClick: function () { return _this.open('primary-small-to-large-aside'); } }, "Small to Large")), h("div", { class: "mt-4" }, h("raul-button", { class: "mr-3", onClick: function () { return _this.open('primary-medium-to-small-aside'); } }, "Medium to Small"), h("raul-button", { class: "mr-3", onClick: function () { return _this.open('primary-medium-to-medium-aside'); } }, "Medium to Medium"), h("raul-button", { class: "mr-3", onClick: function () { return _this.open('primary-medium-to-large-aside'); } }, "Medium to Large")), h("div", { class: "mt-4" }, h("raul-button", { class: "mr-3", onClick: function () { return _this.open('primary-large-to-small-aside'); } }, "Large to Small"), h("raul-button", { class: "mr-3", onClick: function () { return _this.open('primary-large-to-medium-aside'); } }, "Large to Medium"), h("raul-button", { class: "mr-3", onClick: function () { return _this.open('primary-large-to-large-aside'); } }, "Large to Large"))), renderAside('small-aside', 'small', 'Small'), renderAside('medium-aside', 'medium', 'Medium'), renderAside('large-aside', 'large', 'Large'), renderNestedAsides('small-to-small-aside', 'small', 'small'), renderNestedAsides('small-to-medium-aside', 'small', 'medium'), renderNestedAsides('small-to-large-aside', 'small', 'large'), renderNestedAsides('medium-to-small-aside', 'medium', 'small'), renderNestedAsides('medium-to-medium-aside', 'medium', 'medium'), renderNestedAsides('medium-to-large-aside', 'medium', 'large'), renderNestedAsides('large-to-small-aside', 'large', 'small'), renderNestedAsides('large-to-medium-aside', 'large', 'medium'), renderNestedAsides('large-to-large-aside', 'large', 'large'))), h("div", { slot: "design" }, h("docs-readme", { component: "raul-aside" })), h("div", { slot: "api" }, h("docs-interface", { component: "raul-aside" }))));
    };
    return DocsRaulAside;
}());
export { DocsRaulAside as docs_raul_aside };
