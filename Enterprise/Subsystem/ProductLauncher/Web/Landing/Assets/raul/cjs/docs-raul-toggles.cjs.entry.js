'use strict';

Object.defineProperty(exports, '__esModule', { value: true });

const core = require('./core-40bc5f02.js');

const DocsRaulToggles = class {
    constructor(hostRef) {
        core.registerInstance(this, hostRef);
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
    handleRaulToggleChange(e, activeToggleName) {
        this[activeToggleName] = e.detail;
    }
    render() {
        return (core.h("docs-element", { title: "Toggles" }, core.h("div", { slot: "overview" }, core.h("div", null, core.h("div", { class: "heading-lg" }, "Basic"), core.h("raul-toggles", { toggles: this.basicToggles, activeToggle: this.basicTogglesActiveToggle, onRaulToggleChange: (e) => this.handleRaulToggleChange(e, 'basicTogglesActiveToggle') })), core.h("div", { class: "mt-10" }, core.h("div", { class: "heading-lg" }, "Full Width"), core.h("raul-toggles", { toggles: this.fullWidthToggles, fullWidth: true, activeToggle: this.fullWidthTogglesActiveToggle, onRaulToggleChange: (e) => this.handleRaulToggleChange(e, 'fullWidthTogglesActiveToggle') })), core.h("div", { class: "mt-10" }, core.h("div", { class: "heading-lg" }, "Select on Small Devices"), core.h("raul-toggles", { toggles: this.selectOnMobileToggles, selectOnMobile: true, activeToggle: this.selectOnMobileActiveToggle, onRaulToggleChange: (e) => this.handleRaulToggleChange(e, 'selectOnMobileActiveToggle') })), core.h("div", { class: "mt-10" }, core.h("div", { class: "heading-lg" }, "Active, Disabled and Panes"), core.h("raul-toggles", { toggles: this.disabledToggles, activeToggle: this.disabledTogglesActiveToggle, onRaulToggleChange: (e) => this.handleRaulToggleChange(e, 'disabledTogglesActiveToggle') }), core.h("div", { class: "bg-gray-lightest p-4 mt-4" }, this.disabledToggles.map(item => core.h("raul-toggle-pane", { name: item.name, style: { display: item.name === this.disabledTogglesActiveToggle ? 'block' : 'none' } }, item.name === this.disabledTogglesActiveToggle && `${item.label} content`))))), core.h("div", { slot: "design" }), core.h("div", { slot: "api" }, core.h("docs-interface", { component: "raul-toggles" }))));
    }
};

exports.docs_raul_toggles = DocsRaulToggles;
