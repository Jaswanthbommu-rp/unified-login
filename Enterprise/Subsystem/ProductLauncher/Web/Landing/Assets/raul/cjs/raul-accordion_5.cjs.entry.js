'use strict';

Object.defineProperty(exports, '__esModule', { value: true });

const core = require('./core-40bc5f02.js');

const RaulAccordion = class {
    constructor(hostRef) {
        core.registerInstance(this, hostRef);
    }
    render() {
        return (core.h("slot", null));
    }
    static get style() { return "raul-accordion,raul-accordion-item{display:-ms-flexbox;display:flex;-ms-flex-direction:column;flex-direction:column}raul-accordion-item{border-bottom-width:1px;border-color:#ebedee}raul-accordion-item-header{display:-ms-flexbox;display:flex}raul-accordion-item-header .r-accordion__item__header{display:-ms-flexbox;display:flex;-ms-flex:1 1 0%;flex:1 1 0%;-ms-flex-align:center;align-items:center;text-align:left;padding-top:1rem;padding-bottom:1rem;padding-left:.25rem;padding-right:.25rem}raul-accordion-item-header .r-accordion__item__header__content{-ms-flex:1 1 0%;flex:1 1 0%}raul-accordion-item-header .r-accordion__item__header__content a{color:#0076cc}raul-accordion-item-header .r-accordion__item__header__content a:hover{text-decoration:underline}raul-accordion-item-header .r-accordion__item__header__arrow-icon{color:#9ba3a7;-webkit-transition:all .2s ease-in-out;transition:all .2s ease-in-out}body[modality=keyboard] raul-accordion-item-header .r-accordion__item__header:focus{outline:0;-webkit-box-shadow:0 0 0 1px #0076cc;box-shadow:0 0 0 1px #0076cc}raul-accordion-item-header[expanded] .r-accordion__item__header__arrow-icon{-webkit-transform:rotate(180deg);transform:rotate(180deg)}raul-accordion-item-panel{display:none;-webkit-transition:height .3s ease;transition:height .3s ease}raul-accordion-item-panel .r-accordion__item__panel__content{padding-bottom:1rem;padding-left:.25rem;padding-right:.25rem}raul-accordion-item-title{display:block;font-size:1rem;font-weight:600;line-height:1.25;margin-bottom:.5rem;margin-bottom:0}raul-accordion-item:first-of-type{border-top-width:1px}"; }
};

const RaulAccordionItem = class {
    constructor(hostRef) {
        core.registerInstance(this, hostRef);
        this.accordionId = accordionIds++;
        this.expandedState = false;
        this.name = `accordion-${this.accordionId}`;
        this.expanded = null;
        this.disabled = false;
        this.raulChange = core.createEvent(this, "raulChange", 7);
    }
    nameChanged() {
        this.updateHeaderElName();
        this.updatePanelElName();
    }
    expandedChanged() {
        this.updateHeaderElExpanded();
        this.updatePanelElExpanded();
    }
    disabledChanged() {
        this.updateHeaderElDisabled();
    }
    componentDidLoad() {
        this.headerEl = this.el.querySelector('raul-accordion-item-header');
        this.panelEl = this.el.querySelector('raul-accordion-item-panel');
        this.updateHeaderElName();
        this.updateHeaderElExpanded();
        this.updateHeaderElDisabled();
        this.updatePanelElName();
        this.updatePanelElExpanded();
    }
    handleAccordionItemHeaderClick() {
        if (!this.isControlled()) {
            this.expandedState = !this.expandedState;
            this.updateHeaderElExpanded();
            this.updatePanelElExpanded();
        }
        this.raulChange.emit({ name: this.name, expanded: this.isControlled() ? !this.expanded : this.expandedState });
    }
    isControlled() {
        return this.expanded !== null;
    }
    updateHeaderElName() {
        if (this.headerEl)
            this.headerEl.name = this.name;
    }
    updateHeaderElExpanded() {
        if (this.headerEl)
            this.headerEl.expanded = this.isControlled() ? this.expanded : this.expandedState;
    }
    updateHeaderElDisabled() {
        if (this.headerEl)
            this.headerEl.disabled = this.disabled;
    }
    updatePanelElName() {
        if (this.panelEl)
            this.panelEl.name = this.name;
    }
    updatePanelElExpanded() {
        if (this.panelEl)
            this.panelEl.expanded = this.isControlled() ? this.expanded : this.expandedState;
    }
    render() {
        return (core.h("slot", null));
    }
    get el() { return core.getElement(this); }
    static get watchers() { return {
        "name": ["nameChanged"],
        "expanded": ["expandedChanged"],
        "disabled": ["disabledChanged"]
    }; }
};
let accordionIds = 0;

const RaulAccordionItemHeader = class {
    constructor(hostRef) {
        core.registerInstance(this, hostRef);
        this.accordionItemHeaderClick = core.createEvent(this, "accordionItemHeaderClick", 7);
    }
    handleClick() {
        const panelEl = this.el.parentElement.querySelector('raul-accordion-item-panel');
        if (panelEl) {
            if (!panelEl.classList.contains('expanding') && !panelEl.classList.contains('collapsing')) {
                this.accordionItemHeaderClick.emit();
            }
        }
        else {
            this.accordionItemHeaderClick.emit();
        }
    }
    render() {
        return (core.h("button", { type: "button", class: "r-accordion__item__header", id: `${this.name}-header`, "aria-controls": `${this.name}-panel`, "aria-expanded": this.expanded, "aria-disabled": this.disabled, onClick: () => this.handleClick() }, core.h("div", { class: "r-accordion__item__header__content" }, core.h("slot", null)), core.h("raul-icon", { icon: "arrow-down-v", class: "r-accordion__item__header__arrow-icon" })));
    }
    get el() { return core.getElement(this); }
};

const RaulAccordionItemPanel = class {
    constructor(hostRef) {
        core.registerInstance(this, hostRef);
    }
    expandedChanged() {
        this.expanded ? this.expand(this.el) : this.collapse(this.el);
    }
    height(el) {
        let height = el.offsetHeight;
        if (!height) {
            const initialDisplay = el.style.display;
            el.style.display = 'block';
            height = el.offsetHeight;
            el.style.display = initialDisplay ? initialDisplay : null;
        }
        return height;
    }
    expand(el) {
        const height = this.height(el);
        const transitionDuration = parseFloat(getComputedStyle(el).transitionDuration) * 1000;
        el.classList.add('collapsing');
        el.style.display = 'block';
        el.style.overflow = 'hidden';
        el.style.height = '0';
        setTimeout(() => {
            el.style.height = `${height}px`;
            setTimeout(() => {
                el.style.height = el.style.overflow = null;
                el.classList.remove('collapsing');
            }, transitionDuration);
        }, 25);
    }
    collapse(el) {
        const height = this.height(el);
        const transitionDuration = parseFloat(getComputedStyle(el).transitionDuration) * 1000;
        el.classList.add('expanding');
        el.style.overflow = 'hidden';
        el.style.height = `${height}px`;
        setTimeout(() => {
            el.style.height = '0';
            setTimeout(() => {
                el.style.height = el.style.overflow = el.style.display = null;
                el.classList.remove('expanding');
            }, transitionDuration);
        }, 25);
    }
    render() {
        return (core.h(core.Host, { id: `${this.name}-panel`, "aria-labelledby": `${this.name}-header` }, core.h("div", { class: "r-accordion__item__panel__content" }, core.h("slot", null))));
    }
    get el() { return core.getElement(this); }
    static get watchers() { return {
        "expanded": ["expandedChanged"]
    }; }
};

const RaulAccordionItemTitle = class {
    constructor(hostRef) {
        core.registerInstance(this, hostRef);
    }
    render() {
        return (core.h("slot", null));
    }
};

exports.raul_accordion = RaulAccordion;
exports.raul_accordion_item = RaulAccordionItem;
exports.raul_accordion_item_header = RaulAccordionItemHeader;
exports.raul_accordion_item_panel = RaulAccordionItemPanel;
exports.raul_accordion_item_title = RaulAccordionItemTitle;
