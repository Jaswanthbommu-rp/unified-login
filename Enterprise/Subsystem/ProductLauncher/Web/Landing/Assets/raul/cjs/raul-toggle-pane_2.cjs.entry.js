'use strict';

Object.defineProperty(exports, '__esModule', { value: true });

const core = require('./core-40bc5f02.js');

const RaulTogglePane = class {
    constructor(hostRef) {
        core.registerInstance(this, hostRef);
    }
    render() {
        return (core.h("div", { class: "r-toggles", role: "tabpanel", id: this.name, "aria-labelledby": `${this.name}-toggle` }, core.h("slot", null)));
    }
};

const RaulToggles = class {
    constructor(hostRef) {
        core.registerInstance(this, hostRef);
        /**
         * An array of objects representing the navigation.
         */
        this.toggles = [];
        this.raulToggleChange = core.createEvent(this, "raulToggleChange", 7);
    }
    connectedCallback() {
        this.xsDevice = window.innerWidth < 640;
    }
    handleResize() {
        this.xsDevice = window.innerWidth < 640;
    }
    selectOptions() {
        return this.toggles.reduce((acc, cv) => {
            acc = [...acc, ...[{ value: cv.name, text: cv.label }]];
            return acc;
        }, []);
    }
    handleClick(toggleName) {
        this.raulToggleChange.emit(toggleName);
    }
    handleRaulChange(e) {
        this.raulToggleChange.emit(e.detail.value);
    }
    render() {
        if (!(this.selectOnMobile && this.xsDevice)) {
            return (core.h("div", { class: "r-toggles", role: "tablist" }, core.h("div", { class: "r-toggles__list" }, this.toggles.map(item => {
                return (core.h("button", { role: "tab", class: {
                        'r-toggles__item': true,
                        'r-toggles__item--active': item.name === this.activeToggle,
                        'r-toggles__item--disabled': item.disabled
                    }, id: `${item.name}-toggle`, disabled: item.disabled, "aria-controls": item.id || item.name, "aria-selected": item.name === this.activeToggle, onClick: () => this.handleClick(item.name) }, item.label));
            }))));
        }
        else {
            return (core.h("raul-select", { options: this.selectOptions(), value: this.activeToggle, onRaulChange: (e) => this.handleRaulChange(e) }));
        }
    }
    static get style() { return "raul-toggles{display:block}raul-toggles .r-toggles{height:2.5rem;overflow:hidden}raul-toggles .r-toggles__list{display:-ms-flexbox;display:flex;overflow-x:auto;height:4rem}raul-toggles .r-toggles__item{background-color:#fff;border-width:1px;border-color:#c6ccd0;font-size:.875rem;height:2.5rem;padding-left:1rem;padding-right:1rem;white-space:nowrap;border-right-color:transparent}raul-toggles .r-toggles__item:first-child{border-top-left-radius:.125rem;border-bottom-left-radius:.125rem}raul-toggles .r-toggles__item:last-child{border-top-right-radius:.125rem;border-bottom-right-radius:.125rem;border-right-color:#c6ccd0}raul-toggles .r-toggles__item:hover{color:#0076cc;text-shadow:0 0 1px #0076cc}body[modality=keyboard] raul-toggles .r-toggles__item:focus{outline:0;border-color:#0076cc}body[modality=keyboard] raul-toggles .r-toggles__item:focus+.r-toggles__item{border-left-color:transparent}raul-toggles .r-toggles__item.r-toggles__item--active:not(.r-toggles__item--disabled){background-color:#0076cc;border-color:#0076cc;color:#fff;text-shadow:0 0 1px #fff}raul-toggles .r-toggles__item.r-toggles__item--disabled{background-color:#f7f8f9;color:#9ba3a7;pointer-events:none}raul-toggles[full-width] .r-toggles__item{-ms-flex:1 1 0%;flex:1 1 0%}"; }
};

exports.raul_toggle_pane = RaulTogglePane;
exports.raul_toggles = RaulToggles;
