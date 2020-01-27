var __spreadArrays = (this && this.__spreadArrays) || function () {
    for (var s = 0, i = 0, il = arguments.length; i < il; i++) s += arguments[i].length;
    for (var r = Array(s), k = 0, i = 0; i < il; i++)
        for (var a = arguments[i], j = 0, jl = a.length; j < jl; j++, k++)
            r[k] = a[j];
    return r;
};
import { r as registerInstance, h, c as createEvent } from './core-9263a98c.js';
var RaulTogglePane = /** @class */ (function () {
    function RaulTogglePane(hostRef) {
        registerInstance(this, hostRef);
    }
    RaulTogglePane.prototype.render = function () {
        return (h("div", { class: "r-toggles", role: "tabpanel", id: this.name, "aria-labelledby": this.name + "-toggle" }, h("slot", null)));
    };
    return RaulTogglePane;
}());
var RaulToggles = /** @class */ (function () {
    function RaulToggles(hostRef) {
        registerInstance(this, hostRef);
        /**
         * An array of objects representing the navigation.
         */
        this.toggles = [];
        this.raulToggleChange = createEvent(this, "raulToggleChange", 7);
    }
    RaulToggles.prototype.connectedCallback = function () {
        this.xsDevice = window.innerWidth < 640;
    };
    RaulToggles.prototype.handleResize = function () {
        this.xsDevice = window.innerWidth < 640;
    };
    RaulToggles.prototype.selectOptions = function () {
        return this.toggles.reduce(function (acc, cv) {
            acc = __spreadArrays(acc, [{ value: cv.name, text: cv.label }]);
            return acc;
        }, []);
    };
    RaulToggles.prototype.handleClick = function (toggleName) {
        this.raulToggleChange.emit(toggleName);
    };
    RaulToggles.prototype.handleRaulChange = function (e) {
        this.raulToggleChange.emit(e.detail.value);
    };
    RaulToggles.prototype.render = function () {
        var _this = this;
        if (!(this.selectOnMobile && this.xsDevice)) {
            return (h("div", { class: "r-toggles", role: "tablist" }, h("div", { class: "r-toggles__list" }, this.toggles.map(function (item) {
                return (h("button", { role: "tab", class: {
                        'r-toggles__item': true,
                        'r-toggles__item--active': item.name === _this.activeToggle,
                        'r-toggles__item--disabled': item.disabled
                    }, id: item.name + "-toggle", disabled: item.disabled, "aria-controls": item.id || item.name, "aria-selected": item.name === _this.activeToggle, onClick: function () { return _this.handleClick(item.name); } }, item.label));
            }))));
        }
        else {
            return (h("raul-select", { options: this.selectOptions(), value: this.activeToggle, onRaulChange: function (e) { return _this.handleRaulChange(e); } }));
        }
    };
    Object.defineProperty(RaulToggles, "style", {
        get: function () { return "raul-toggles{display:block}raul-toggles .r-toggles{height:2.5rem;overflow:hidden}raul-toggles .r-toggles__list{display:-ms-flexbox;display:flex;overflow-x:auto;height:4rem}raul-toggles .r-toggles__item{background-color:#fff;border-width:1px;border-color:#c6ccd0;font-size:.875rem;height:2.5rem;padding-left:1rem;padding-right:1rem;white-space:nowrap;border-right-color:transparent}raul-toggles .r-toggles__item:first-child{border-top-left-radius:.125rem;border-bottom-left-radius:.125rem}raul-toggles .r-toggles__item:last-child{border-top-right-radius:.125rem;border-bottom-right-radius:.125rem;border-right-color:#c6ccd0}raul-toggles .r-toggles__item:hover{color:#0076cc;text-shadow:0 0 1px #0076cc}body[modality=keyboard] raul-toggles .r-toggles__item:focus{outline:0;border-color:#0076cc}body[modality=keyboard] raul-toggles .r-toggles__item:focus+.r-toggles__item{border-left-color:transparent}raul-toggles .r-toggles__item.r-toggles__item--active:not(.r-toggles__item--disabled){background-color:#0076cc;border-color:#0076cc;color:#fff;text-shadow:0 0 1px #fff}raul-toggles .r-toggles__item.r-toggles__item--disabled{background-color:#f7f8f9;color:#9ba3a7;pointer-events:none}raul-toggles[full-width] .r-toggles__item{-ms-flex:1 1 0%;flex:1 1 0%}"; },
        enumerable: true,
        configurable: true
    });
    return RaulToggles;
}());
export { RaulTogglePane as raul_toggle_pane, RaulToggles as raul_toggles };
