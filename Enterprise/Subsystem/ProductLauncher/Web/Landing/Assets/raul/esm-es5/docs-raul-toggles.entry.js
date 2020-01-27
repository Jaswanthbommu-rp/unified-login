import { r as registerInstance, h } from './core-9263a98c.js';
var DocsRaulToggles = /** @class */ (function () {
    function DocsRaulToggles(hostRef) {
        registerInstance(this, hostRef);
        this.basicToggles = [
            { label: 'Day', name: 'one' },
            { label: 'Work Week', name: 'two' },
            { label: 'Week', name: 'three' },
            { label: 'Month', name: 'four' },
        ];
        this.fullWidthToggles = [
            { label: 'Day', name: 'one' },
            { label: 'Work Week', name: 'two' },
            { label: 'Week', name: 'three' },
            { label: 'Month', name: 'four' },
        ];
        this.disabledToggles = [
            { label: 'Day', name: 'one' },
            { label: 'Work Week', name: 'two' },
            { label: 'Week', name: 'three' },
            { label: 'Month', name: 'four', disabled: true },
        ];
        this.selectOnMobileToggles = [
            { label: 'Day', name: 'one' },
            { label: 'Work Week', name: 'two' },
            { label: 'Week', name: 'three' },
            { label: 'Month', name: 'four' },
        ];
        this.basicTogglesActiveToggle = null;
        this.fullWidthTogglesActiveToggle = null;
        this.disabledTogglesActiveToggle = 'one';
        this.selectOnMobileActiveToggle = null;
    }
    DocsRaulToggles.prototype.handleRaulToggleChange = function (e, activeToggleName) {
        this[activeToggleName] = e.detail;
    };
    DocsRaulToggles.prototype.render = function () {
        var _this = this;
        return (h("docs-element", { title: "Toggles" }, h("div", { slot: "overview" }, h("div", null, h("div", { class: "heading-lg" }, "Basic"), h("raul-toggles", { toggles: this.basicToggles, activeToggle: this.basicTogglesActiveToggle, onRaulToggleChange: function (e) { return _this.handleRaulToggleChange(e, 'basicTogglesActiveToggle'); } })), h("div", { class: "mt-10" }, h("div", { class: "heading-lg" }, "Full Width"), h("raul-toggles", { toggles: this.fullWidthToggles, fullWidth: true, activeToggle: this.fullWidthTogglesActiveToggle, onRaulToggleChange: function (e) { return _this.handleRaulToggleChange(e, 'fullWidthTogglesActiveToggle'); } })), h("div", { class: "mt-10" }, h("div", { class: "heading-lg" }, "Select on Small Devices"), h("raul-toggles", { toggles: this.selectOnMobileToggles, selectOnMobile: true, activeToggle: this.selectOnMobileActiveToggle, onRaulToggleChange: function (e) { return _this.handleRaulToggleChange(e, 'selectOnMobileActiveToggle'); } })), h("div", { class: "mt-10" }, h("div", { class: "heading-lg" }, "Active, Disabled and Panes"), h("raul-toggles", { toggles: this.disabledToggles, activeToggle: this.disabledTogglesActiveToggle, onRaulToggleChange: function (e) { return _this.handleRaulToggleChange(e, 'disabledTogglesActiveToggle'); } }), h("div", { class: "bg-gray-lightest p-4 mt-4" }, this.disabledToggles.map(function (item) { return h("raul-toggle-pane", { name: item.name, style: { display: item.name === _this.disabledTogglesActiveToggle ? 'block' : 'none' } }, item.name === _this.disabledTogglesActiveToggle && item.label + " content"); })))), h("div", { slot: "design" }), h("div", { slot: "api" }, h("docs-interface", { component: "raul-toggles" }))));
    };
    return DocsRaulToggles;
}());
export { DocsRaulToggles as docs_raul_toggles };
