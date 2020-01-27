var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    function adopt(value) { return value instanceof P ? value : new P(function (resolve) { resolve(value); }); }
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : adopt(result.value).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
var __generator = (this && this.__generator) || function (thisArg, body) {
    var _ = { label: 0, sent: function() { if (t[0] & 1) throw t[1]; return t[1]; }, trys: [], ops: [] }, f, y, t, g;
    return g = { next: verb(0), "throw": verb(1), "return": verb(2) }, typeof Symbol === "function" && (g[Symbol.iterator] = function() { return this; }), g;
    function verb(n) { return function (v) { return step([n, v]); }; }
    function step(op) {
        if (f) throw new TypeError("Generator is already executing.");
        while (_) try {
            if (f = 1, y && (t = op[0] & 2 ? y["return"] : op[0] ? y["throw"] || ((t = y["return"]) && t.call(y), 0) : y.next) && !(t = t.call(y, op[1])).done) return t;
            if (y = 0, t) op = [op[0] & 2, t.value];
            switch (op[0]) {
                case 0: case 1: t = op; break;
                case 4: _.label++; return { value: op[1], done: false };
                case 5: _.label++; y = op[1]; op = [0]; continue;
                case 7: op = _.ops.pop(); _.trys.pop(); continue;
                default:
                    if (!(t = _.trys, t = t.length > 0 && t[t.length - 1]) && (op[0] === 6 || op[0] === 2)) { _ = 0; continue; }
                    if (op[0] === 3 && (!t || (op[1] > t[0] && op[1] < t[3]))) { _.label = op[1]; break; }
                    if (op[0] === 6 && _.label < t[1]) { _.label = t[1]; t = op; break; }
                    if (t && _.label < t[2]) { _.label = t[2]; _.ops.push(op); break; }
                    if (t[2]) _.ops.pop();
                    _.trys.pop(); continue;
            }
            op = body.call(thisArg, _);
        } catch (e) { op = [6, e]; y = 0; } finally { f = t = 0; }
        if (op[0] & 5) throw op[1]; return { value: op[0] ? op[1] : void 0, done: true };
    }
};
import { r as registerInstance, c as createEvent, h, H as Host, g as getElement } from './core-9263a98c.js';
var RaulAside = /** @class */ (function () {
    function class_1(hostRef) {
        registerInstance(this, hostRef);
        this.visible = false;
        this.expanded = false;
        this.focused = false;
        this.size = 'medium';
        this.raulAsideOpen = createEvent(this, "raulAsideOpen", 7);
        this.raulAsideClose = createEvent(this, "raulAsideClose", 7);
    }
    class_1.prototype.componentDidLoad = function () {
        this.dialogTransitionDuration = parseFloat(window.getComputedStyle(this.dialogEl).transitionDuration) * 1000;
    };
    class_1.prototype.handleRaulAsideOpen = function (e) {
        var _this = this;
        if (this.el !== e.target) {
            requestAnimationFrame(function () {
                _this.dialogWidth = _this.dialogEl.offsetWidth;
                _this.secondaryDialogWidth = e.target.querySelector('.r-aside__dialog').offsetWidth;
            });
            this.blur();
        }
    };
    class_1.prototype.handleRaulAsideClose = function (e) {
        if (this.el !== e.target) {
            this.secondaryDialogWidth = 0;
            this.focus();
        }
    };
    /**
     * Opens the aside.
     * @returns {Promise<void>}
     */
    class_1.prototype.open = function () {
        return __awaiter(this, void 0, void 0, function () {
            var _this = this;
            return __generator(this, function (_a) {
                this.asideTrigger = document.activeElement;
                this.visible = true;
                requestAnimationFrame(function () { return _this.expanded = true; });
                this.focus();
                this.raulAsideOpen.emit();
                document.body.classList.add('no-scroll');
                return [2 /*return*/];
            });
        });
    };
    /**
     * Closes the aside.
     * @returns {Promise<void>}
     */
    class_1.prototype.close = function () {
        return __awaiter(this, void 0, void 0, function () {
            var _this = this;
            return __generator(this, function (_a) {
                this.expanded = false;
                setTimeout(function () { return _this.visible = false; }, this.dialogTransitionDuration);
                this.blur();
                if (this.asideTrigger) {
                    this.asideTrigger.focus();
                }
                this.raulAsideClose.emit();
                document.body.classList.remove('no-scroll');
                return [2 /*return*/];
            });
        });
    };
    class_1.prototype.dialogOffsetX = function () {
        return -(this.secondaryDialogWidth - this.dialogWidth + 40);
    };
    class_1.prototype.focus = function () {
        var _this = this;
        requestAnimationFrame(function () {
            _this.focused = true;
            _this.asideEl.focus();
        });
    };
    class_1.prototype.blur = function () {
        var _this = this;
        requestAnimationFrame(function () {
            _this.focused = false;
            _this.asideEl.blur();
        });
    };
    class_1.prototype.render = function () {
        var _this = this;
        return (h(Host, { visible: this.visible, expanded: this.expanded }, h("div", { class: "r-aside", role: "dialog", tabindex: "-1", "aria-hidden": !this.visible, onKeyDown: function (e) { return e.key === 'Escape' && _this.focused ? _this.close() : null; }, ref: function (el) { return _this.asideEl = el; } }, h("slot", { name: "secondary-aside" }), h("div", { class: "r-aside__backdrop", onClick: function () { return _this.close(); } }), h("div", { class: "r-aside__dialog", role: "document", style: { transform: this.secondaryDialogWidth ? "translateX(" + this.dialogOffsetX() + "px)" : null }, ref: function (el) { return _this.dialogEl = el; } }, h("slot", null)))));
    };
    Object.defineProperty(class_1.prototype, "el", {
        get: function () { return getElement(this); },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(class_1, "style", {
        get: function () { return "raul-aside{display:block}raul-aside .r-aside{display:none;position:fixed;left:0;top:0;width:100%;height:100%;overflow:hidden;outline:0;z-index:10000}raul-aside .r-aside__backdrop{position:absolute;left:0;top:0;width:100%;height:100%;background-color:#37474f;cursor:pointer;opacity:0;-webkit-transition:opacity .15s linear;transition:opacity .15s linear}raul-aside .r-aside__dialog{position:absolute;right:0;top:0;height:100%;display:-ms-flexbox;display:flex;-ms-flex-direction:column;flex-direction:column;background-color:#fff;border-left-width:1px;border-color:#ebedee;-webkit-transition:background-color .35s ease,-webkit-transform .35s ease;transition:background-color .35s ease,-webkit-transform .35s ease;transition:transform .35s ease,background-color .35s ease;transition:transform .35s ease,background-color .35s ease,-webkit-transform .35s ease;-webkit-transform:translateX(100%);transform:translateX(100%)}raul-aside .r-aside--visible{display:block}raul-aside .r-aside--visible>.r-aside__backdrop{opacity:.5}raul-aside .r-aside--visible>.r-aside__dialog{-webkit-transform:translateX(0);transform:translateX(0)}raul-aside[visible]>.r-aside{display:block}raul-aside[expanded]>.r-aside>.r-aside__backdrop{opacity:.5}raul-aside[expanded]>.r-aside>.r-aside__dialog{-webkit-transform:translateX(0);transform:translateX(0)}raul-aside[expanded]>.r-aside>raul-aside[expanded]~.r-aside__dialog{background-color:#ebedee}raul-aside[expanded]>.r-aside>raul-aside[expanded] .r-aside__backdrop{opacity:0}raul-aside[size=small]>.r-aside>.r-aside__dialog{width:100%}\@media (min-width:640px){raul-aside[size=small]>.r-aside>.r-aside__dialog{width:22.5rem}}raul-aside[size=medium]>.r-aside>.r-aside__dialog{width:100%}\@media (min-width:768px){raul-aside[size=medium]>.r-aside>.r-aside__dialog{width:37.5rem}}raul-aside[size=large]>.r-aside>.r-aside__dialog{width:100%}\@media (min-width:1024px){raul-aside[size=large]>.r-aside>.r-aside__dialog{width:64rem}}"; },
        enumerable: true,
        configurable: true
    });
    return class_1;
}());
var RaulAsideBody = /** @class */ (function () {
    function RaulAsideBody(hostRef) {
        registerInstance(this, hostRef);
    }
    RaulAsideBody.prototype.render = function () {
        return (h("slot", null));
    };
    Object.defineProperty(RaulAsideBody, "style", {
        get: function () { return "raul-aside-body{display:block;-ms-flex:1 1 0%;flex:1 1 0%;height:100%;padding-left:2.5rem;padding-right:2.5rem;padding-top:1.25rem;padding-bottom:1.25rem;overflow-y:auto}"; },
        enumerable: true,
        configurable: true
    });
    return RaulAsideBody;
}());
var RaulAsideFooter = /** @class */ (function () {
    function RaulAsideFooter(hostRef) {
        registerInstance(this, hostRef);
    }
    RaulAsideFooter.prototype.render = function () {
        return (h("slot", null));
    };
    Object.defineProperty(RaulAsideFooter, "style", {
        get: function () { return "raul-aside-footer{display:block;padding:1rem;border-top-width:1px;border-color:#ebedee}"; },
        enumerable: true,
        configurable: true
    });
    return RaulAsideFooter;
}());
var RaulAsideHeader = /** @class */ (function () {
    function RaulAsideHeader(hostRef) {
        registerInstance(this, hostRef);
    }
    RaulAsideHeader.prototype.render = function () {
        return (h("slot", null));
    };
    Object.defineProperty(RaulAsideHeader, "style", {
        get: function () { return "raul-aside-header{display:block;padding-left:2.5rem;padding-right:2.5rem;padding-top:2.5rem}"; },
        enumerable: true,
        configurable: true
    });
    return RaulAsideHeader;
}());
export { RaulAside as raul_aside, RaulAsideBody as raul_aside_body, RaulAsideFooter as raul_aside_footer, RaulAsideHeader as raul_aside_header };
