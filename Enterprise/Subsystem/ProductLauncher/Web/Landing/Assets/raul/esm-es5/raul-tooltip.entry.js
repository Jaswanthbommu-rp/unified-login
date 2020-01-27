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
import { r as registerInstance, h, g as getElement } from './core-9263a98c.js';
import { P as Popper } from './popper-26f59066.js';
var RaulTooltip = /** @class */ (function () {
    function class_1(hostRef) {
        registerInstance(this, hostRef);
        this.tooltipId = "raul-tooltip-" + tooltipIds++;
        this.popper = null;
        this.text = '';
        this.placement = 'top';
        this.disabledHoverListener = false;
        this.disabledFocusListener = false;
    }
    class_1.prototype.handleMouseEnter = function () {
        if (!this.disabledHoverListener) {
            this.show();
        }
    };
    class_1.prototype.handleMouseLeave = function () {
        if (!this.disabledHoverListener) {
            this.hide();
        }
    };
    class_1.prototype.handleFocusIn = function () {
        if (!this.disabledFocusListener) {
            this.show();
        }
    };
    class_1.prototype.handleFocusOut = function () {
        if (!this.disabledFocusListener) {
            this.hide();
        }
    };
    class_1.prototype.show = function () {
        return __awaiter(this, void 0, void 0, function () {
            return __generator(this, function (_a) {
                this.createTooltip();
                return [2 /*return*/];
            });
        });
    };
    class_1.prototype.hide = function () {
        return __awaiter(this, void 0, void 0, function () {
            return __generator(this, function (_a) {
                this.removeTooltip();
                return [2 /*return*/];
            });
        });
    };
    class_1.prototype.tooltipRef = function () {
        return document.getElementById(this.tooltipId);
    };
    class_1.prototype.tooltipArrowRef = function () {
        return this.tooltipRef().querySelector('.r-tooltip__arrow');
    };
    class_1.prototype.tooltipElement = function () {
        var tooltipTemplate = "\n      <div class=\"r-tooltip\" id=\"" + this.tooltipId + "\" role=\"tooltip\">\n        <div class=\"r-tooltip__arrow\"></div>\n        \n        <div class=\"r-tooltip__content\">\n          " + this.text + "        \n        </div>\n      </div>\n    ";
        return new DOMParser().parseFromString(tooltipTemplate, 'text/html').body.firstChild;
    };
    class_1.prototype.popperOptions = function () {
        return {
            placement: this.placement,
            modifiers: {
                arrow: {
                    element: this.tooltipArrowRef()
                }
            }
        };
    };
    class_1.prototype.createTooltip = function () {
        if (!this.popper) {
            document.body.appendChild(this.tooltipElement());
            this.tooltipRef().classList.add('r-tooltip--show');
            this.popper = new Popper(this.el, this.tooltipRef(), this.popperOptions());
        }
    };
    class_1.prototype.removeTooltip = function () {
        var _this = this;
        if (this.popper) {
            var transitionDuration = parseFloat(getComputedStyle(this.tooltipRef()).transitionDuration) * 1000;
            this.tooltipRef().classList.remove('r-tooltip--show');
            setTimeout(function () {
                _this.tooltipRef().parentNode.removeChild(_this.tooltipRef());
                _this.popper.destroy();
                _this.popper = null;
            }, transitionDuration);
        }
    };
    class_1.prototype.render = function () {
        return (h("slot", null));
    };
    Object.defineProperty(class_1.prototype, "el", {
        get: function () { return getElement(this); },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(class_1, "style", {
        get: function () { return "raul-tooltip{display:inline-block}.r-tooltip{background-color:#37474f;color:#fff;font-size:.75rem;padding:.75rem;border-radius:.125rem;-webkit-box-shadow:0 16px 27px 0 rgba(82,97,115,.22);box-shadow:0 16px 27px 0 rgba(82,97,115,.22);opacity:0;-webkit-transition:opacity .15s linear;transition:opacity .15s linear;z-index:50;max-width:16rem}.r-tooltip__arrow{height:0;width:0;border-color:transparent;position:absolute;border-width:.5rem}.r-tooltip--show{opacity:1}.r-tooltip[x-placement^=top]{margin-bottom:.75rem}.r-tooltip[x-placement^=top] .r-tooltip__arrow{border-bottom-width:0;margin-bottom:-.5rem;bottom:0;border-top-color:#37474f}.r-tooltip[x-placement^=bottom]{margin-top:.75rem}.r-tooltip[x-placement^=bottom] .r-tooltip__arrow{border-top-width:0;margin-top:-.5rem;top:0;border-bottom-color:#37474f}.r-tooltip[x-placement^=right]{margin-left:.75rem}.r-tooltip[x-placement^=right] .r-tooltip__arrow{border-left-width:0;margin-left:-.5rem;left:0;border-right-color:#37474f}.r-tooltip[x-placement^=left]{margin-right:.75rem}.r-tooltip[x-placement^=left] .r-tooltip__arrow{border-right-width:0;margin-right:-.5rem;right:0;border-left-color:#37474f}.r-tooltip[x-placement^=bottom-end] .r-tooltip__arrow,.r-tooltip[x-placement^=top-end] .r-tooltip__arrow{left:auto!important;right:.5rem!important}.r-tooltip[x-placement^=bottom-start] .r-tooltip__arrow,.r-tooltip[x-placement^=top-start] .r-tooltip__arrow{left:.5rem!important;right:auto!important}.r-tooltip[x-placement^=left-start] .r-tooltip__arrow,.r-tooltip[x-placement^=right-start] .r-tooltip__arrow{top:.5rem!important;bottom:auto!important}.r-tooltip[x-placement^=left-end] .r-tooltip__arrow,.r-tooltip[x-placement^=right-end] .r-tooltip__arrow{top:auto!important;bottom:.5rem!important}"; },
        enumerable: true,
        configurable: true
    });
    return class_1;
}());
var tooltipIds = 0;
export { RaulTooltip as raul_tooltip };
