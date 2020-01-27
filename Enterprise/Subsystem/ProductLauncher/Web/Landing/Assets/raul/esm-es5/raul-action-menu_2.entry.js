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
import { r as registerInstance, c as createEvent, h } from './core-9263a98c.js';
import { M as MenuItem } from './MenuItem-2544c288.js';
var RaulActionMenu = /** @class */ (function () {
    function class_1(hostRef) {
        registerInstance(this, hostRef);
        /**
         * If set to true, the last action will be separated with a divider
         */
        this.emphasizeFinal = false;
        /**
         * Disables actions
         */
        this.disabled = false;
        this.optionSelected = createEvent(this, "optionSelected", 7);
    }
    /**
     * Method to programatically close the menu
     */
    class_1.prototype.closeMenu = function () {
        return __awaiter(this, void 0, void 0, function () {
            return __generator(this, function (_a) {
                this.dropdownMenu.closeMenu();
                return [2 /*return*/];
            });
        });
    };
    class_1.prototype.render = function () {
        var _this = this;
        return (h("raul-dropdown-menu", { items: this.items, color: 'active', dividers: false, emphasizeFinal: this.emphasizeFinal, disabled: this.disabled, ref: function (el) { return _this.dropdownMenu = el; } }, h("raul-icon", { icon: 'navigation-show-more-vertical', slot: "toggle" }), h("slot", null)));
    };
    Object.defineProperty(class_1, "style", {
        get: function () { return "raul-action-menu raul-icon{font-size:1.25rem}raul-action-menu .r-dropdown-menu--show raul-icon{color:#0076cc}raul-action-menu .r-dropdown-menu__menu-item :not(.r-dropdown-menu__menu-item--disabled) .r-dropdown-menu__menu-item__label{color:#0076cc!important}raul-action-menu .r-dropdown-menu__menu-item :not(.r-dropdown-menu__menu-item--disabled) .r-dropdown-menu__menu-item__label:hover{text-decoration:underline!important}raul-dropdown-menu .r-dropdown-menu{opacity:1;-webkit-user-select:none;-moz-user-select:none;-ms-user-select:none;user-select:none;position:relative;display:inline-block}raul-dropdown-menu .r-dropdown-menu .r-dropdown-menu__toggle{position:relative;height:100%;width:100%;display:-ms-flexbox;display:flex;-ms-flex-pack:center;justify-content:center;-ms-flex-align:center;align-items:center;cursor:pointer}raul-dropdown-menu .r-dropdown-menu .r-dropdown-menu__toggle:focus{outline:none}raul-dropdown-menu .r-dropdown-menu .r-dropdown-menu__toggle:focus .r-dropdown-menu__toggle__focus-utility{outline:1px solid #0076cc}raul-dropdown-menu .r-dropdown-menu .r-dropdown-menu__toggle .r-dropdown-menu__toggle__focus-utility{width:100%;height:100%;display:-ms-flexbox;display:flex;-ms-flex-pack:center;justify-content:center;-ms-flex-align:center;align-items:center}raul-dropdown-menu .r-dropdown-menu .r-dropdown-menu__toggle .r-dropdown-menu__toggle__focus-utility:focus{outline:none}raul-dropdown-menu .r-dropdown-menu .r-dropdown-menu__dropdown{position:absolute;display:none;border-radius:.125rem;-webkit-box-shadow:0 1px 3px 0 rgba(0,0,0,.1),0 1px 2px 0 rgba(0,0,0,.06);box-shadow:0 1px 3px 0 rgba(0,0,0,.1),0 1px 2px 0 rgba(0,0,0,.06);padding-top:.5rem;padding-bottom:.5rem;background-color:#fff;max-width:180px;z-index:2000}raul-dropdown-menu .r-dropdown-menu .r-dropdown-menu__dropdown--show{display:-ms-flexbox;display:flex;-ms-flex-direction:column;flex-direction:column}raul-dropdown-menu .r-dropdown-menu .r-dropdown-menu__dropdown--default{top:32px;right:0;left:auto}raul-dropdown-menu .r-dropdown-menu .r-dropdown-menu__dropdown--top{bottom:32px;right:0;left:auto}raul-dropdown-menu .r-dropdown-menu .r-dropdown-menu__dropdown--right{left:0;right:auto}raul-dropdown-menu .r-dropdown-menu .r-dropdown-menu__dropdown raul-action-menu-item .r-dropdown-menu-item{color:#37474f;cursor:pointer;-webkit-user-select:none;-moz-user-select:none;-ms-user-select:none;user-select:none;text-decoration:none;min-height:32px;display:-ms-flexbox;display:flex;-ms-flex-direction:column;flex-direction:column;-ms-flex-pack:space-evenly;justify-content:space-evenly;-ms-flex-align:start;align-items:flex-start;opacity:1!important}raul-dropdown-menu .r-dropdown-menu .r-dropdown-menu__dropdown raul-action-menu-item .r-dropdown-menu-item .r-dropdown-menu-item__focus-utility{border-width:1px;border-color:transparent;width:100%;min-height:32px;display:-ms-flexbox;display:flex;-ms-flex-direction:column;flex-direction:column;-ms-flex-pack:distribute;justify-content:space-around}raul-dropdown-menu .r-dropdown-menu .r-dropdown-menu__dropdown raul-action-menu-item .r-dropdown-menu-item .r-dropdown-menu-item__focus-utility:focus{outline:none}raul-dropdown-menu .r-dropdown-menu .r-dropdown-menu__dropdown raul-action-menu-item .r-dropdown-menu-item .r-dropdown-menu-item__focus-utility .r-dropdown-menu__menu-item__label{color:#37474f;opacity:1!important}raul-dropdown-menu .r-dropdown-menu .r-dropdown-menu__dropdown raul-action-menu-item .r-dropdown-menu-item .r-dropdown-menu-item__focus-utility .r-dropdown-menu__menu-item__label:hover{text-decoration:underline}raul-dropdown-menu .r-dropdown-menu .r-dropdown-menu__dropdown raul-action-menu-item .r-dropdown-menu-item:focus{outline:none}raul-dropdown-menu .r-dropdown-menu .r-dropdown-menu__dropdown raul-action-menu-item .r-dropdown-menu-item:focus .r-dropdown-menu-item__focus-utility{border-width:1px;border-color:#0076cc}raul-dropdown-menu .r-dropdown-menu .r-dropdown-menu__dropdown raul-action-menu-item .r-dropdown-menu-item .r-dropdown-menu__menu-item__container{color:#37474f;width:100%;padding:0 8px;display:-ms-flexbox;display:flex;-ms-flex-direction:row;flex-direction:row;-ms-flex-pack:start;justify-content:flex-start;-ms-flex-align:center;align-items:center}raul-dropdown-menu .r-dropdown-menu .r-dropdown-menu__dropdown raul-action-menu-item .r-dropdown-menu-item .r-dropdown-menu__menu-item__container .r-dropdown-menu__menu-item__label{padding:0 8px;line-height:18px!important;font-size:12px!important;text-transform:capitalize;white-space:nowrap;overflow:hidden;text-overflow:ellipsis}raul-dropdown-menu .r-dropdown-menu .r-dropdown-menu__dropdown raul-action-menu-item .r-dropdown-menu-item--disabled{pointer-events:none}raul-dropdown-menu .r-dropdown-menu .r-dropdown-menu__dropdown raul-action-menu-item .r-dropdown-menu-item--disabled .r-dropdown-menu__menu-item__label{color:#9ba3a7!important}raul-dropdown-menu .r-dropdown-menu .r-dropdown-menu__dropdown raul-action-menu-item .r-dropdown-menu-item--disabled:focus{outline:none}raul-dropdown-menu .r-dropdown-menu--color-active .r-dropdown-menu__menu-item__container{color:#0076cc}raul-dropdown-menu .r-dropdown-menu--emphasize-final raul-action-menu-item:last-child .r-dropdown-menu__menu-item:before{border-top-width:1px;border-color:#f7f8f9;content:\"\";display:block;width:100%;margin-top:8px;margin-bottom:8px}raul-dropdown-menu .r-dropdown-menu--dividers .r-dropdown-menu__dropdown{padding:0}raul-dropdown-menu .r-dropdown-menu--dividers .r-dropdown-menu__dropdown .r-dropdown-menu__menu-item{border-bottom-width:1px;border-color:#f7f8f9}raul-dropdown-menu .r-dropdown-menu--dividers .r-dropdown-menu__dropdown .r-dropdown-menu__menu-item:last-child{border-bottom:none}"; },
        enumerable: true,
        configurable: true
    });
    return class_1;
}());
var RaulDropdownMenu = /** @class */ (function () {
    function RaulDropdownMenu(hostRef) {
        registerInstance(this, hostRef);
        this.clickCallback = createEvent(this, "clickCallback", 7);
        this.blurCallback = createEvent(this, "blurCallback", 7);
        this.optionSelected = createEvent(this, "optionSelected", 7);
    }
    RaulDropdownMenu.prototype.render = function () {
        return (h(MenuItem, { url: this.url, disabled: this.disabled, payload: this.payload, onClickCallback: this.clickCallback, onBlurCallback: this.blurCallback, event: this.optionSelected }, h("slot", null)));
    };
    Object.defineProperty(RaulDropdownMenu, "style", {
        get: function () { return "raul-action-menu raul-icon{font-size:1.25rem}raul-action-menu .r-dropdown-menu--show raul-icon{color:#0076cc}raul-action-menu .r-dropdown-menu__menu-item :not(.r-dropdown-menu__menu-item--disabled) .r-dropdown-menu__menu-item__label{color:#0076cc!important}raul-action-menu .r-dropdown-menu__menu-item :not(.r-dropdown-menu__menu-item--disabled) .r-dropdown-menu__menu-item__label:hover{text-decoration:underline!important}raul-dropdown-menu .r-dropdown-menu{opacity:1;-webkit-user-select:none;-moz-user-select:none;-ms-user-select:none;user-select:none;position:relative;display:inline-block}raul-dropdown-menu .r-dropdown-menu .r-dropdown-menu__toggle{position:relative;height:100%;width:100%;display:-ms-flexbox;display:flex;-ms-flex-pack:center;justify-content:center;-ms-flex-align:center;align-items:center;cursor:pointer}raul-dropdown-menu .r-dropdown-menu .r-dropdown-menu__toggle:focus{outline:none}raul-dropdown-menu .r-dropdown-menu .r-dropdown-menu__toggle:focus .r-dropdown-menu__toggle__focus-utility{outline:1px solid #0076cc}raul-dropdown-menu .r-dropdown-menu .r-dropdown-menu__toggle .r-dropdown-menu__toggle__focus-utility{width:100%;height:100%;display:-ms-flexbox;display:flex;-ms-flex-pack:center;justify-content:center;-ms-flex-align:center;align-items:center}raul-dropdown-menu .r-dropdown-menu .r-dropdown-menu__toggle .r-dropdown-menu__toggle__focus-utility:focus{outline:none}raul-dropdown-menu .r-dropdown-menu .r-dropdown-menu__dropdown{position:absolute;display:none;border-radius:.125rem;-webkit-box-shadow:0 1px 3px 0 rgba(0,0,0,.1),0 1px 2px 0 rgba(0,0,0,.06);box-shadow:0 1px 3px 0 rgba(0,0,0,.1),0 1px 2px 0 rgba(0,0,0,.06);padding-top:.5rem;padding-bottom:.5rem;background-color:#fff;max-width:180px;z-index:2000}raul-dropdown-menu .r-dropdown-menu .r-dropdown-menu__dropdown--show{display:-ms-flexbox;display:flex;-ms-flex-direction:column;flex-direction:column}raul-dropdown-menu .r-dropdown-menu .r-dropdown-menu__dropdown--default{top:32px;right:0;left:auto}raul-dropdown-menu .r-dropdown-menu .r-dropdown-menu__dropdown--top{bottom:32px;right:0;left:auto}raul-dropdown-menu .r-dropdown-menu .r-dropdown-menu__dropdown--right{left:0;right:auto}raul-dropdown-menu .r-dropdown-menu .r-dropdown-menu__dropdown raul-action-menu-item .r-dropdown-menu-item{color:#37474f;cursor:pointer;-webkit-user-select:none;-moz-user-select:none;-ms-user-select:none;user-select:none;text-decoration:none;min-height:32px;display:-ms-flexbox;display:flex;-ms-flex-direction:column;flex-direction:column;-ms-flex-pack:space-evenly;justify-content:space-evenly;-ms-flex-align:start;align-items:flex-start;opacity:1!important}raul-dropdown-menu .r-dropdown-menu .r-dropdown-menu__dropdown raul-action-menu-item .r-dropdown-menu-item .r-dropdown-menu-item__focus-utility{border-width:1px;border-color:transparent;width:100%;min-height:32px;display:-ms-flexbox;display:flex;-ms-flex-direction:column;flex-direction:column;-ms-flex-pack:distribute;justify-content:space-around}raul-dropdown-menu .r-dropdown-menu .r-dropdown-menu__dropdown raul-action-menu-item .r-dropdown-menu-item .r-dropdown-menu-item__focus-utility:focus{outline:none}raul-dropdown-menu .r-dropdown-menu .r-dropdown-menu__dropdown raul-action-menu-item .r-dropdown-menu-item .r-dropdown-menu-item__focus-utility .r-dropdown-menu__menu-item__label{color:#37474f;opacity:1!important}raul-dropdown-menu .r-dropdown-menu .r-dropdown-menu__dropdown raul-action-menu-item .r-dropdown-menu-item .r-dropdown-menu-item__focus-utility .r-dropdown-menu__menu-item__label:hover{text-decoration:underline}raul-dropdown-menu .r-dropdown-menu .r-dropdown-menu__dropdown raul-action-menu-item .r-dropdown-menu-item:focus{outline:none}raul-dropdown-menu .r-dropdown-menu .r-dropdown-menu__dropdown raul-action-menu-item .r-dropdown-menu-item:focus .r-dropdown-menu-item__focus-utility{border-width:1px;border-color:#0076cc}raul-dropdown-menu .r-dropdown-menu .r-dropdown-menu__dropdown raul-action-menu-item .r-dropdown-menu-item .r-dropdown-menu__menu-item__container{color:#37474f;width:100%;padding:0 8px;display:-ms-flexbox;display:flex;-ms-flex-direction:row;flex-direction:row;-ms-flex-pack:start;justify-content:flex-start;-ms-flex-align:center;align-items:center}raul-dropdown-menu .r-dropdown-menu .r-dropdown-menu__dropdown raul-action-menu-item .r-dropdown-menu-item .r-dropdown-menu__menu-item__container .r-dropdown-menu__menu-item__label{padding:0 8px;line-height:18px!important;font-size:12px!important;text-transform:capitalize;white-space:nowrap;overflow:hidden;text-overflow:ellipsis}raul-dropdown-menu .r-dropdown-menu .r-dropdown-menu__dropdown raul-action-menu-item .r-dropdown-menu-item--disabled{pointer-events:none}raul-dropdown-menu .r-dropdown-menu .r-dropdown-menu__dropdown raul-action-menu-item .r-dropdown-menu-item--disabled .r-dropdown-menu__menu-item__label{color:#9ba3a7!important}raul-dropdown-menu .r-dropdown-menu .r-dropdown-menu__dropdown raul-action-menu-item .r-dropdown-menu-item--disabled:focus{outline:none}raul-dropdown-menu .r-dropdown-menu--color-active .r-dropdown-menu__menu-item__container{color:#0076cc}raul-dropdown-menu .r-dropdown-menu--emphasize-final raul-action-menu-item:last-child .r-dropdown-menu__menu-item:before{border-top-width:1px;border-color:#f7f8f9;content:\"\";display:block;width:100%;margin-top:8px;margin-bottom:8px}raul-dropdown-menu .r-dropdown-menu--dividers .r-dropdown-menu__dropdown{padding:0}raul-dropdown-menu .r-dropdown-menu--dividers .r-dropdown-menu__dropdown .r-dropdown-menu__menu-item{border-bottom-width:1px;border-color:#f7f8f9}raul-dropdown-menu .r-dropdown-menu--dividers .r-dropdown-menu__dropdown .r-dropdown-menu__menu-item:last-child{border-bottom:none}"; },
        enumerable: true,
        configurable: true
    });
    return RaulDropdownMenu;
}());
export { RaulActionMenu as raul_action_menu, RaulDropdownMenu as raul_action_menu_item };
