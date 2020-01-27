import { r as registerInstance, c as createEvent, h, H as Host, g as getElement } from './core-9263a98c.js';

const RaulAside = class {
    constructor(hostRef) {
        registerInstance(this, hostRef);
        this.visible = false;
        this.expanded = false;
        this.focused = false;
        this.size = 'medium';
        this.raulAsideOpen = createEvent(this, "raulAsideOpen", 7);
        this.raulAsideClose = createEvent(this, "raulAsideClose", 7);
    }
    componentDidLoad() {
        this.dialogTransitionDuration = parseFloat(window.getComputedStyle(this.dialogEl).transitionDuration) * 1000;
    }
    handleRaulAsideOpen(e) {
        if (this.el !== e.target) {
            requestAnimationFrame(() => {
                this.dialogWidth = this.dialogEl.offsetWidth;
                this.secondaryDialogWidth = e.target.querySelector('.r-aside__dialog').offsetWidth;
            });
            this.blur();
        }
    }
    handleRaulAsideClose(e) {
        if (this.el !== e.target) {
            this.secondaryDialogWidth = 0;
            this.focus();
        }
    }
    /**
     * Opens the aside.
     * @returns {Promise<void>}
     */
    async open() {
        this.asideTrigger = document.activeElement;
        this.visible = true;
        requestAnimationFrame(() => this.expanded = true);
        this.focus();
        this.raulAsideOpen.emit();
        document.body.classList.add('no-scroll');
    }
    /**
     * Closes the aside.
     * @returns {Promise<void>}
     */
    async close() {
        this.expanded = false;
        setTimeout(() => this.visible = false, this.dialogTransitionDuration);
        this.blur();
        if (this.asideTrigger) {
            this.asideTrigger.focus();
        }
        this.raulAsideClose.emit();
        document.body.classList.remove('no-scroll');
    }
    dialogOffsetX() {
        return -(this.secondaryDialogWidth - this.dialogWidth + 40);
    }
    focus() {
        requestAnimationFrame(() => {
            this.focused = true;
            this.asideEl.focus();
        });
    }
    blur() {
        requestAnimationFrame(() => {
            this.focused = false;
            this.asideEl.blur();
        });
    }
    render() {
        return (h(Host, { visible: this.visible, expanded: this.expanded }, h("div", { class: "r-aside", role: "dialog", tabindex: "-1", "aria-hidden": !this.visible, onKeyDown: e => e.key === 'Escape' && this.focused ? this.close() : null, ref: (el) => this.asideEl = el }, h("slot", { name: "secondary-aside" }), h("div", { class: "r-aside__backdrop", onClick: () => this.close() }), h("div", { class: "r-aside__dialog", role: "document", style: { transform: this.secondaryDialogWidth ? `translateX(${this.dialogOffsetX()}px)` : null }, ref: el => this.dialogEl = el }, h("slot", null)))));
    }
    get el() { return getElement(this); }
    static get style() { return "raul-aside{display:block}raul-aside .r-aside{display:none;position:fixed;left:0;top:0;width:100%;height:100%;overflow:hidden;outline:0;z-index:10000}raul-aside .r-aside__backdrop{position:absolute;left:0;top:0;width:100%;height:100%;background-color:#37474f;cursor:pointer;opacity:0;-webkit-transition:opacity .15s linear;transition:opacity .15s linear}raul-aside .r-aside__dialog{position:absolute;right:0;top:0;height:100%;display:-ms-flexbox;display:flex;-ms-flex-direction:column;flex-direction:column;background-color:#fff;border-left-width:1px;border-color:#ebedee;-webkit-transition:background-color .35s ease,-webkit-transform .35s ease;transition:background-color .35s ease,-webkit-transform .35s ease;transition:transform .35s ease,background-color .35s ease;transition:transform .35s ease,background-color .35s ease,-webkit-transform .35s ease;-webkit-transform:translateX(100%);transform:translateX(100%)}raul-aside .r-aside--visible{display:block}raul-aside .r-aside--visible>.r-aside__backdrop{opacity:.5}raul-aside .r-aside--visible>.r-aside__dialog{-webkit-transform:translateX(0);transform:translateX(0)}raul-aside[visible]>.r-aside{display:block}raul-aside[expanded]>.r-aside>.r-aside__backdrop{opacity:.5}raul-aside[expanded]>.r-aside>.r-aside__dialog{-webkit-transform:translateX(0);transform:translateX(0)}raul-aside[expanded]>.r-aside>raul-aside[expanded]~.r-aside__dialog{background-color:#ebedee}raul-aside[expanded]>.r-aside>raul-aside[expanded] .r-aside__backdrop{opacity:0}raul-aside[size=small]>.r-aside>.r-aside__dialog{width:100%}\@media (min-width:640px){raul-aside[size=small]>.r-aside>.r-aside__dialog{width:22.5rem}}raul-aside[size=medium]>.r-aside>.r-aside__dialog{width:100%}\@media (min-width:768px){raul-aside[size=medium]>.r-aside>.r-aside__dialog{width:37.5rem}}raul-aside[size=large]>.r-aside>.r-aside__dialog{width:100%}\@media (min-width:1024px){raul-aside[size=large]>.r-aside>.r-aside__dialog{width:64rem}}"; }
};

const RaulAsideBody = class {
    constructor(hostRef) {
        registerInstance(this, hostRef);
    }
    render() {
        return (h("slot", null));
    }
    static get style() { return "raul-aside-body{display:block;-ms-flex:1 1 0%;flex:1 1 0%;height:100%;padding-left:2.5rem;padding-right:2.5rem;padding-top:1.25rem;padding-bottom:1.25rem;overflow-y:auto}"; }
};

const RaulAsideFooter = class {
    constructor(hostRef) {
        registerInstance(this, hostRef);
    }
    render() {
        return (h("slot", null));
    }
    static get style() { return "raul-aside-footer{display:block;padding:1rem;border-top-width:1px;border-color:#ebedee}"; }
};

const RaulAsideHeader = class {
    constructor(hostRef) {
        registerInstance(this, hostRef);
    }
    render() {
        return (h("slot", null));
    }
    static get style() { return "raul-aside-header{display:block;padding-left:2.5rem;padding-right:2.5rem;padding-top:2.5rem}"; }
};

export { RaulAside as raul_aside, RaulAsideBody as raul_aside_body, RaulAsideFooter as raul_aside_footer, RaulAsideHeader as raul_aside_header };
