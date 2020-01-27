import { r as registerInstance, h, c as createEvent, g as getElement, H as Host } from './core-9263a98c.js';
var RaulAccordion = /** @class */ (function () {
    function RaulAccordion(hostRef) {
        registerInstance(this, hostRef);
    }
    RaulAccordion.prototype.render = function () {
        return (h("slot", null));
    };
    Object.defineProperty(RaulAccordion, "style", {
        get: function () { return "raul-accordion,raul-accordion-item{display:-ms-flexbox;display:flex;-ms-flex-direction:column;flex-direction:column}raul-accordion-item{border-bottom-width:1px;border-color:#ebedee}raul-accordion-item-header{display:-ms-flexbox;display:flex}raul-accordion-item-header .r-accordion__item__header{display:-ms-flexbox;display:flex;-ms-flex:1 1 0%;flex:1 1 0%;-ms-flex-align:center;align-items:center;text-align:left;padding-top:1rem;padding-bottom:1rem;padding-left:.25rem;padding-right:.25rem}raul-accordion-item-header .r-accordion__item__header__content{-ms-flex:1 1 0%;flex:1 1 0%}raul-accordion-item-header .r-accordion__item__header__content a{color:#0076cc}raul-accordion-item-header .r-accordion__item__header__content a:hover{text-decoration:underline}raul-accordion-item-header .r-accordion__item__header__arrow-icon{color:#9ba3a7;-webkit-transition:all .2s ease-in-out;transition:all .2s ease-in-out}body[modality=keyboard] raul-accordion-item-header .r-accordion__item__header:focus{outline:0;-webkit-box-shadow:0 0 0 1px #0076cc;box-shadow:0 0 0 1px #0076cc}raul-accordion-item-header[expanded] .r-accordion__item__header__arrow-icon{-webkit-transform:rotate(180deg);transform:rotate(180deg)}raul-accordion-item-panel{display:none;-webkit-transition:height .3s ease;transition:height .3s ease}raul-accordion-item-panel .r-accordion__item__panel__content{padding-bottom:1rem;padding-left:.25rem;padding-right:.25rem}raul-accordion-item-title{display:block;font-size:1rem;font-weight:600;line-height:1.25;margin-bottom:.5rem;margin-bottom:0}raul-accordion-item:first-of-type{border-top-width:1px}"; },
        enumerable: true,
        configurable: true
    });
    return RaulAccordion;
}());
var RaulAccordionItem = /** @class */ (function () {
    function RaulAccordionItem(hostRef) {
        registerInstance(this, hostRef);
        this.accordionId = accordionIds++;
        this.expandedState = false;
        this.name = "accordion-" + this.accordionId;
        this.expanded = null;
        this.disabled = false;
        this.raulChange = createEvent(this, "raulChange", 7);
    }
    RaulAccordionItem.prototype.nameChanged = function () {
        this.updateHeaderElName();
        this.updatePanelElName();
    };
    RaulAccordionItem.prototype.expandedChanged = function () {
        this.updateHeaderElExpanded();
        this.updatePanelElExpanded();
    };
    RaulAccordionItem.prototype.disabledChanged = function () {
        this.updateHeaderElDisabled();
    };
    RaulAccordionItem.prototype.componentDidLoad = function () {
        this.headerEl = this.el.querySelector('raul-accordion-item-header');
        this.panelEl = this.el.querySelector('raul-accordion-item-panel');
        this.updateHeaderElName();
        this.updateHeaderElExpanded();
        this.updateHeaderElDisabled();
        this.updatePanelElName();
        this.updatePanelElExpanded();
    };
    RaulAccordionItem.prototype.handleAccordionItemHeaderClick = function () {
        if (!this.isControlled()) {
            this.expandedState = !this.expandedState;
            this.updateHeaderElExpanded();
            this.updatePanelElExpanded();
        }
        this.raulChange.emit({ name: this.name, expanded: this.isControlled() ? !this.expanded : this.expandedState });
    };
    RaulAccordionItem.prototype.isControlled = function () {
        return this.expanded !== null;
    };
    RaulAccordionItem.prototype.updateHeaderElName = function () {
        if (this.headerEl)
            this.headerEl.name = this.name;
    };
    RaulAccordionItem.prototype.updateHeaderElExpanded = function () {
        if (this.headerEl)
            this.headerEl.expanded = this.isControlled() ? this.expanded : this.expandedState;
    };
    RaulAccordionItem.prototype.updateHeaderElDisabled = function () {
        if (this.headerEl)
            this.headerEl.disabled = this.disabled;
    };
    RaulAccordionItem.prototype.updatePanelElName = function () {
        if (this.panelEl)
            this.panelEl.name = this.name;
    };
    RaulAccordionItem.prototype.updatePanelElExpanded = function () {
        if (this.panelEl)
            this.panelEl.expanded = this.isControlled() ? this.expanded : this.expandedState;
    };
    RaulAccordionItem.prototype.render = function () {
        return (h("slot", null));
    };
    Object.defineProperty(RaulAccordionItem.prototype, "el", {
        get: function () { return getElement(this); },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(RaulAccordionItem, "watchers", {
        get: function () {
            return {
                "name": ["nameChanged"],
                "expanded": ["expandedChanged"],
                "disabled": ["disabledChanged"]
            };
        },
        enumerable: true,
        configurable: true
    });
    return RaulAccordionItem;
}());
var accordionIds = 0;
var RaulAccordionItemHeader = /** @class */ (function () {
    function RaulAccordionItemHeader(hostRef) {
        registerInstance(this, hostRef);
        this.accordionItemHeaderClick = createEvent(this, "accordionItemHeaderClick", 7);
    }
    RaulAccordionItemHeader.prototype.handleClick = function () {
        var panelEl = this.el.parentElement.querySelector('raul-accordion-item-panel');
        if (panelEl) {
            if (!panelEl.classList.contains('expanding') && !panelEl.classList.contains('collapsing')) {
                this.accordionItemHeaderClick.emit();
            }
        }
        else {
            this.accordionItemHeaderClick.emit();
        }
    };
    RaulAccordionItemHeader.prototype.render = function () {
        var _this = this;
        return (h("button", { type: "button", class: "r-accordion__item__header", id: this.name + "-header", "aria-controls": this.name + "-panel", "aria-expanded": this.expanded, "aria-disabled": this.disabled, onClick: function () { return _this.handleClick(); } }, h("div", { class: "r-accordion__item__header__content" }, h("slot", null)), h("raul-icon", { icon: "arrow-down-v", class: "r-accordion__item__header__arrow-icon" })));
    };
    Object.defineProperty(RaulAccordionItemHeader.prototype, "el", {
        get: function () { return getElement(this); },
        enumerable: true,
        configurable: true
    });
    return RaulAccordionItemHeader;
}());
var RaulAccordionItemPanel = /** @class */ (function () {
    function RaulAccordionItemPanel(hostRef) {
        registerInstance(this, hostRef);
    }
    RaulAccordionItemPanel.prototype.expandedChanged = function () {
        this.expanded ? this.expand(this.el) : this.collapse(this.el);
    };
    RaulAccordionItemPanel.prototype.height = function (el) {
        var height = el.offsetHeight;
        if (!height) {
            var initialDisplay = el.style.display;
            el.style.display = 'block';
            height = el.offsetHeight;
            el.style.display = initialDisplay ? initialDisplay : null;
        }
        return height;
    };
    RaulAccordionItemPanel.prototype.expand = function (el) {
        var height = this.height(el);
        var transitionDuration = parseFloat(getComputedStyle(el).transitionDuration) * 1000;
        el.classList.add('collapsing');
        el.style.display = 'block';
        el.style.overflow = 'hidden';
        el.style.height = '0';
        setTimeout(function () {
            el.style.height = height + "px";
            setTimeout(function () {
                el.style.height = el.style.overflow = null;
                el.classList.remove('collapsing');
            }, transitionDuration);
        }, 25);
    };
    RaulAccordionItemPanel.prototype.collapse = function (el) {
        var height = this.height(el);
        var transitionDuration = parseFloat(getComputedStyle(el).transitionDuration) * 1000;
        el.classList.add('expanding');
        el.style.overflow = 'hidden';
        el.style.height = height + "px";
        setTimeout(function () {
            el.style.height = '0';
            setTimeout(function () {
                el.style.height = el.style.overflow = el.style.display = null;
                el.classList.remove('expanding');
            }, transitionDuration);
        }, 25);
    };
    RaulAccordionItemPanel.prototype.render = function () {
        return (h(Host, { id: this.name + "-panel", "aria-labelledby": this.name + "-header" }, h("div", { class: "r-accordion__item__panel__content" }, h("slot", null))));
    };
    Object.defineProperty(RaulAccordionItemPanel.prototype, "el", {
        get: function () { return getElement(this); },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(RaulAccordionItemPanel, "watchers", {
        get: function () {
            return {
                "expanded": ["expandedChanged"]
            };
        },
        enumerable: true,
        configurable: true
    });
    return RaulAccordionItemPanel;
}());
var RaulAccordionItemTitle = /** @class */ (function () {
    function RaulAccordionItemTitle(hostRef) {
        registerInstance(this, hostRef);
    }
    RaulAccordionItemTitle.prototype.render = function () {
        return (h("slot", null));
    };
    return RaulAccordionItemTitle;
}());
export { RaulAccordion as raul_accordion, RaulAccordionItem as raul_accordion_item, RaulAccordionItemHeader as raul_accordion_item_header, RaulAccordionItemPanel as raul_accordion_item_panel, RaulAccordionItemTitle as raul_accordion_item_title };
