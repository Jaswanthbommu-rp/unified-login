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
var __spreadArrays = (this && this.__spreadArrays) || function () {
    for (var s = 0, i = 0, il = arguments.length; i < il; i++) s += arguments[i].length;
    for (var r = Array(s), k = 0, i = 0; i < il; i++)
        for (var a = arguments[i], j = 0, jl = a.length; j < jl; j++, k++)
            r[k] = a[j];
    return r;
};
import { r as registerInstance, h, g as getElement, H as Host, c as createEvent } from './core-9263a98c.js';
import { i as isScreen } from './index-4f8d920e.js';
import { P as Popper } from './popper-26f59066.js';
import { a as commonjsGlobal, c as createCommonjsModule } from './_commonjsHelpers-f34b4464.js';
import { N as NAVIGATION_KEYS, E as ESCAPE_KEY, a as DOWN_KEY, U as UP_KEY } from './index-45f40cb4.js';
import { r as randomUID } from './index-b2673790.js';
import { g as getDocs } from './get-docs-ce58db96.js';
var DocsElement = /** @class */ (function () {
    function DocsElement(hostRef) {
        registerInstance(this, hostRef);
        this.activeTab = "overview";
    }
    DocsElement.prototype.render = function () {
        var _this = this;
        return (h("div", { class: "docs-element" }, h("div", { class: "docs-page-header tabbed" }, h("h1", null, this.title), h("raul-tabs", { tabs: [
                { label: "OVERVIEW", name: "overview" },
                { label: "DESIGN GUIDELINES", name: "design" },
                { label: "API", name: "api" },
            ], "active-tab": this.activeTab, onRaulTabChange: function (e) { return _this.activeTab = e.detail; } })), h("div", { class: "p-4 md:p-12" }, h("div", { style: { display: this.activeTab === 'overview' ? 'block' : 'none' } }, h("slot", { name: "overview" })), h("div", { style: { display: this.activeTab === 'design' ? 'block' : 'none' } }, h("slot", { name: "design" })), h("div", { style: { display: this.activeTab === 'api' ? 'block' : 'none' } }, h("slot", { name: "api" })))));
    };
    return DocsElement;
}());
var DocsInterface = /** @class */ (function () {
    function DocsInterface(hostRef) {
        registerInstance(this, hostRef);
    }
    DocsInterface.prototype.componentWillLoad = function () {
        this.docs = getDocs(this.component);
    };
    DocsInterface.prototype.render = function () {
        var renderDefault = function (defaultValue) {
            if (defaultValue === undefined) {
                return 'null';
            }
            else if (typeof defaultValue === 'string') {
                return defaultValue;
            }
            else {
                return JSON.stringify(defaultValue);
            }
        };
        var renderNotApplicable = function (type) {
            return (h("div", { class: "r-docs-interface__na" }, "No ", type, " available."));
        };
        return (h("div", { class: "r-docs-interface page-section" }, h("h2", { class: "text-lg font-bold mb-6" }, "Component API"), h("div", { class: "r-docs-interface__section page-section" }, h("h4", { id: "props", class: "text-sm uppercase font-bold" }, "Props"), this.docs.props.length > 0 &&
            h("raul-simple-table", null, h("table", null, h("thead", null, h("tr", null, h("th", null, "Name"), h("th", null, "Type"), h("th", null, "Default"), h("th", null, "Description"))), h("tbody", null, this.docs.props.map(function (prop) {
                return (h("tr", null, h("td", null, prop.name), h("td", { style: { maxWidth: '300px' } }, prop.type), h("td", null, h("code", null, renderDefault(prop.default))), h("td", null, prop.docs || '[MISSING]')));
            })))) || renderNotApplicable('properties')), h("div", { class: "r-docs-interface__section page-section" }, h("h4", { id: "methods", class: "text-sm uppercase font-bold" }, "Methods"), this.docs.methods.length > 0 &&
            h("raul-simple-table", null, h("table", null, h("thead", null, h("tr", null, h("th", null, "Method"), h("th", null, "Description"))), h("tbody", null, this.docs.methods.map(function (method) {
                return (h("tr", null, h("td", null, h("code", null, method.signature)), h("td", null, method.docs || '[MISSING]')));
            })))) || renderNotApplicable('methods')), h("div", { class: "r-docs-interface__section page-section" }, h("h4", { id: "events", class: "text-sm uppercase font-bold" }, "Events"), this.docs.events.length > 0 &&
            h("raul-simple-table", null, h("table", null, h("thead", null, h("tr", null, h("th", null, "Name"), h("th", null, "Description"))), h("tbody", null, this.docs.events.map(function (event) {
                return (h("tr", null, h("td", null, event.event), h("td", null, event.docs || '[MISSING]')));
            })))) || renderNotApplicable('events'))));
    };
    Object.defineProperty(DocsInterface, "style", {
        get: function () { return "docs-interface{display:block}.r-docs-interface__section{font-size:14px}.r-docs-interface__section raul-simple-table{width:100%}.r-docs-interface__na{opacity:.5;font-size:14px}"; },
        enumerable: true,
        configurable: true
    });
    return DocsInterface;
}());
var RaulCheckbox = /** @class */ (function () {
    function RaulCheckbox(hostRef) {
        registerInstance(this, hostRef);
        /**
         * If `true`, the checkbox border will become red. This can be useful for form validations.
         */
        this.invalid = false;
        /**
         * If `true`, the checkbox size will be small.
         */
        this.small = false;
    }
    RaulCheckbox.prototype.render = function () {
        return (h("div", { class: {
                'r-checkbox': true,
                'r-checkbox--invalid': this.invalid,
                'r-checkbox--small': this.small
            } }, h("label", { class: "r-checkbox__label" }, h("slot", null), h("div", { class: "r-checkbox__label__icon" }), this.labelText &&
            h("div", { class: "r-checkbox__label__text" }, this.labelText))));
    };
    Object.defineProperty(RaulCheckbox.prototype, "el", {
        get: function () { return getElement(this); },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(RaulCheckbox, "style", {
        get: function () { return "raul-checkbox{display:inline-block}raul-checkbox .r-checkbox__label{cursor:pointer;display:-ms-flexbox;display:flex;line-height:20px;position:relative}raul-checkbox .r-checkbox__label input[type=checkbox]{height:0;opacity:0;position:absolute;width:0;z-index:-1}raul-checkbox .r-checkbox__label input[type=checkbox]:checked~.r-checkbox__label__icon,raul-checkbox .r-checkbox__label input[type=checkbox]:indeterminate~.r-checkbox__label__icon{background-color:var(--raul-checkbox-checked-or-indeterminate-bg-color,#0076cc);border-color:#0076cc}raul-checkbox .r-checkbox__label input[type=checkbox]:checked~.r-checkbox__label__icon:after{content:\"\";position:absolute;border-top:2px solid #fff;border-left:2px solid #fff;height:.7em;left:.3em;top:.7em;-webkit-transform:rotate(-135deg);transform:rotate(-135deg);-webkit-transform-origin:0 0;transform-origin:0 0;width:.3em}raul-checkbox .r-checkbox__label input[type=checkbox]:indeterminate~.r-checkbox__label__icon:after{content:\"\";position:absolute;border-top:2px solid #fff;left:50%;top:50%;-webkit-transform:translate(-50%,-50%);transform:translate(-50%,-50%);width:.6em}raul-checkbox .r-checkbox__label input[type=checkbox]:disabled~.r-checkbox__label__icon{background-color:#ebedee;border-color:#ebedee}raul-checkbox .r-checkbox__label input[type=checkbox]:disabled:checked~.r-checkbox__label__icon:after,raul-checkbox .r-checkbox__label input[type=checkbox]:disabled:indeterminate~.r-checkbox__label__icon:after{border-color:#9ba3a7}body[modality] raul-checkbox .r-checkbox__label input[type=checkbox]:focus~.r-checkbox__label__icon:before{border-radius:var(--raul-checkbox-border-radius,2px);bottom:-4px;-webkit-box-shadow:0 0 0 1px #0076cc;box-shadow:0 0 0 1px #0076cc;content:\"\";left:-4px;right:-4px;position:absolute;top:-4px}raul-checkbox .r-checkbox__label__icon{background-color:#fff;border:1px solid #b8bec1;border-radius:var(--raul-checkbox-border-radius,2px);font-size:20px;-ms-flex:none;flex:none;height:20px;position:relative;width:20px;-webkit-transition:background-color .15s linear,border-color .15s linear;transition:background-color .15s linear,border-color .15s linear}raul-checkbox .r-checkbox__label__text{font-size:14px;margin-left:8px;word-break:break-all}raul-checkbox .r-checkbox--small .r-checkbox__label{line-height:14px}raul-checkbox .r-checkbox--small .r-checkbox__label__icon{font-size:14px;height:14px;width:14px}raul-checkbox .r-checkbox--invalid .r-checkbox__label__icon{border-color:#d01a1f}"; },
        enumerable: true,
        configurable: true
    });
    return RaulCheckbox;
}());
var RaulOption = /** @class */ (function () {
    function RaulOption(hostRef) {
        registerInstance(this, hostRef);
    }
    RaulOption.prototype.render = function () {
        var _this = this;
        return (h(Host, { class: {
                'r-option': true,
                'r-option--multiple': this.multiple,
                'r-option--active': this.focused,
                'r-option--disabled': this.disabled,
                'r-option--selected': this.selected,
                'r-option--group': this.variant === 'group',
                'r-option--heading': this.variant === 'heading',
                'r-option--sub-option': this.variant === 'sub-option',
                'r-option--with-description': !!this.description
            }, role: "option", tabindex: "-1", id: this.optionId, "aria-selected": this.selected ? 'true' : 'false', "aria-disabled": this.disabled ? 'true' : null }, this.icon && h("raul-icon", { class: "r-option__icon", icon: this.icon, kind: this.iconKind }), h("div", { class: "r-option__text" }, h("div", { class: "r-option__text__title", title: this.text }, this.text), this.description && h("div", { class: "r-option__text__description", title: this.description }, this.description)), this.multiple
            ? h("raul-checkbox", { small: true }, h("input", { type: "checkbox", tabindex: "-1", "aria-hidden": "true", checked: this.selected, disabled: this.disabled, value: this.value, ref: function (el) { return el && (el.indeterminate = _this.indeterminate); } }))
            : h("raul-icon", { icon: "check-mark-success", class: "r-option__check-mark" })));
    };
    return RaulOption;
}());
/**
 * Removes all key-value entries from the list cache.
 *
 * @private
 * @name clear
 * @memberOf ListCache
 */
function listCacheClear() {
    this.__data__ = [];
    this.size = 0;
}
var _listCacheClear = listCacheClear;
/**
 * Performs a
 * [`SameValueZero`](http://ecma-international.org/ecma-262/7.0/#sec-samevaluezero)
 * comparison between two values to determine if they are equivalent.
 *
 * @static
 * @memberOf _
 * @since 4.0.0
 * @category Lang
 * @param {*} value The value to compare.
 * @param {*} other The other value to compare.
 * @returns {boolean} Returns `true` if the values are equivalent, else `false`.
 * @example
 *
 * var object = { 'a': 1 };
 * var other = { 'a': 1 };
 *
 * _.eq(object, object);
 * // => true
 *
 * _.eq(object, other);
 * // => false
 *
 * _.eq('a', 'a');
 * // => true
 *
 * _.eq('a', Object('a'));
 * // => false
 *
 * _.eq(NaN, NaN);
 * // => true
 */
function eq(value, other) {
    return value === other || (value !== value && other !== other);
}
var eq_1 = eq;
/**
 * Gets the index at which the `key` is found in `array` of key-value pairs.
 *
 * @private
 * @param {Array} array The array to inspect.
 * @param {*} key The key to search for.
 * @returns {number} Returns the index of the matched value, else `-1`.
 */
function assocIndexOf(array, key) {
    var length = array.length;
    while (length--) {
        if (eq_1(array[length][0], key)) {
            return length;
        }
    }
    return -1;
}
var _assocIndexOf = assocIndexOf;
/** Used for built-in method references. */
var arrayProto = Array.prototype;
/** Built-in value references. */
var splice = arrayProto.splice;
/**
 * Removes `key` and its value from the list cache.
 *
 * @private
 * @name delete
 * @memberOf ListCache
 * @param {string} key The key of the value to remove.
 * @returns {boolean} Returns `true` if the entry was removed, else `false`.
 */
function listCacheDelete(key) {
    var data = this.__data__, index = _assocIndexOf(data, key);
    if (index < 0) {
        return false;
    }
    var lastIndex = data.length - 1;
    if (index == lastIndex) {
        data.pop();
    }
    else {
        splice.call(data, index, 1);
    }
    --this.size;
    return true;
}
var _listCacheDelete = listCacheDelete;
/**
 * Gets the list cache value for `key`.
 *
 * @private
 * @name get
 * @memberOf ListCache
 * @param {string} key The key of the value to get.
 * @returns {*} Returns the entry value.
 */
function listCacheGet(key) {
    var data = this.__data__, index = _assocIndexOf(data, key);
    return index < 0 ? undefined : data[index][1];
}
var _listCacheGet = listCacheGet;
/**
 * Checks if a list cache value for `key` exists.
 *
 * @private
 * @name has
 * @memberOf ListCache
 * @param {string} key The key of the entry to check.
 * @returns {boolean} Returns `true` if an entry for `key` exists, else `false`.
 */
function listCacheHas(key) {
    return _assocIndexOf(this.__data__, key) > -1;
}
var _listCacheHas = listCacheHas;
/**
 * Sets the list cache `key` to `value`.
 *
 * @private
 * @name set
 * @memberOf ListCache
 * @param {string} key The key of the value to set.
 * @param {*} value The value to set.
 * @returns {Object} Returns the list cache instance.
 */
function listCacheSet(key, value) {
    var data = this.__data__, index = _assocIndexOf(data, key);
    if (index < 0) {
        ++this.size;
        data.push([key, value]);
    }
    else {
        data[index][1] = value;
    }
    return this;
}
var _listCacheSet = listCacheSet;
/**
 * Creates an list cache object.
 *
 * @private
 * @constructor
 * @param {Array} [entries] The key-value pairs to cache.
 */
function ListCache(entries) {
    var index = -1, length = entries == null ? 0 : entries.length;
    this.clear();
    while (++index < length) {
        var entry = entries[index];
        this.set(entry[0], entry[1]);
    }
}
// Add methods to `ListCache`.
ListCache.prototype.clear = _listCacheClear;
ListCache.prototype['delete'] = _listCacheDelete;
ListCache.prototype.get = _listCacheGet;
ListCache.prototype.has = _listCacheHas;
ListCache.prototype.set = _listCacheSet;
var _ListCache = ListCache;
/**
 * Removes all key-value entries from the stack.
 *
 * @private
 * @name clear
 * @memberOf Stack
 */
function stackClear() {
    this.__data__ = new _ListCache;
    this.size = 0;
}
var _stackClear = stackClear;
/**
 * Removes `key` and its value from the stack.
 *
 * @private
 * @name delete
 * @memberOf Stack
 * @param {string} key The key of the value to remove.
 * @returns {boolean} Returns `true` if the entry was removed, else `false`.
 */
function stackDelete(key) {
    var data = this.__data__, result = data['delete'](key);
    this.size = data.size;
    return result;
}
var _stackDelete = stackDelete;
/**
 * Gets the stack value for `key`.
 *
 * @private
 * @name get
 * @memberOf Stack
 * @param {string} key The key of the value to get.
 * @returns {*} Returns the entry value.
 */
function stackGet(key) {
    return this.__data__.get(key);
}
var _stackGet = stackGet;
/**
 * Checks if a stack value for `key` exists.
 *
 * @private
 * @name has
 * @memberOf Stack
 * @param {string} key The key of the entry to check.
 * @returns {boolean} Returns `true` if an entry for `key` exists, else `false`.
 */
function stackHas(key) {
    return this.__data__.has(key);
}
var _stackHas = stackHas;
/** Detect free variable `global` from Node.js. */
var freeGlobal = typeof commonjsGlobal == 'object' && commonjsGlobal && commonjsGlobal.Object === Object && commonjsGlobal;
var _freeGlobal = freeGlobal;
/** Detect free variable `self`. */
var freeSelf = typeof self == 'object' && self && self.Object === Object && self;
/** Used as a reference to the global object. */
var root = _freeGlobal || freeSelf || Function('return this')();
var _root = root;
/** Built-in value references. */
var Symbol = _root.Symbol;
var _Symbol = Symbol;
/** Used for built-in method references. */
var objectProto = Object.prototype;
/** Used to check objects for own properties. */
var hasOwnProperty = objectProto.hasOwnProperty;
/**
 * Used to resolve the
 * [`toStringTag`](http://ecma-international.org/ecma-262/7.0/#sec-object.prototype.tostring)
 * of values.
 */
var nativeObjectToString = objectProto.toString;
/** Built-in value references. */
var symToStringTag = _Symbol ? _Symbol.toStringTag : undefined;
/**
 * A specialized version of `baseGetTag` which ignores `Symbol.toStringTag` values.
 *
 * @private
 * @param {*} value The value to query.
 * @returns {string} Returns the raw `toStringTag`.
 */
function getRawTag(value) {
    var isOwn = hasOwnProperty.call(value, symToStringTag), tag = value[symToStringTag];
    try {
        value[symToStringTag] = undefined;
        var unmasked = true;
    }
    catch (e) { }
    var result = nativeObjectToString.call(value);
    if (unmasked) {
        if (isOwn) {
            value[symToStringTag] = tag;
        }
        else {
            delete value[symToStringTag];
        }
    }
    return result;
}
var _getRawTag = getRawTag;
/** Used for built-in method references. */
var objectProto$1 = Object.prototype;
/**
 * Used to resolve the
 * [`toStringTag`](http://ecma-international.org/ecma-262/7.0/#sec-object.prototype.tostring)
 * of values.
 */
var nativeObjectToString$1 = objectProto$1.toString;
/**
 * Converts `value` to a string using `Object.prototype.toString`.
 *
 * @private
 * @param {*} value The value to convert.
 * @returns {string} Returns the converted string.
 */
function objectToString(value) {
    return nativeObjectToString$1.call(value);
}
var _objectToString = objectToString;
/** `Object#toString` result references. */
var nullTag = '[object Null]', undefinedTag = '[object Undefined]';
/** Built-in value references. */
var symToStringTag$1 = _Symbol ? _Symbol.toStringTag : undefined;
/**
 * The base implementation of `getTag` without fallbacks for buggy environments.
 *
 * @private
 * @param {*} value The value to query.
 * @returns {string} Returns the `toStringTag`.
 */
function baseGetTag(value) {
    if (value == null) {
        return value === undefined ? undefinedTag : nullTag;
    }
    return (symToStringTag$1 && symToStringTag$1 in Object(value))
        ? _getRawTag(value)
        : _objectToString(value);
}
var _baseGetTag = baseGetTag;
/**
 * Checks if `value` is the
 * [language type](http://www.ecma-international.org/ecma-262/7.0/#sec-ecmascript-language-types)
 * of `Object`. (e.g. arrays, functions, objects, regexes, `new Number(0)`, and `new String('')`)
 *
 * @static
 * @memberOf _
 * @since 0.1.0
 * @category Lang
 * @param {*} value The value to check.
 * @returns {boolean} Returns `true` if `value` is an object, else `false`.
 * @example
 *
 * _.isObject({});
 * // => true
 *
 * _.isObject([1, 2, 3]);
 * // => true
 *
 * _.isObject(_.noop);
 * // => true
 *
 * _.isObject(null);
 * // => false
 */
function isObject(value) {
    var type = typeof value;
    return value != null && (type == 'object' || type == 'function');
}
var isObject_1 = isObject;
/** `Object#toString` result references. */
var asyncTag = '[object AsyncFunction]', funcTag = '[object Function]', genTag = '[object GeneratorFunction]', proxyTag = '[object Proxy]';
/**
 * Checks if `value` is classified as a `Function` object.
 *
 * @static
 * @memberOf _
 * @since 0.1.0
 * @category Lang
 * @param {*} value The value to check.
 * @returns {boolean} Returns `true` if `value` is a function, else `false`.
 * @example
 *
 * _.isFunction(_);
 * // => true
 *
 * _.isFunction(/abc/);
 * // => false
 */
function isFunction(value) {
    if (!isObject_1(value)) {
        return false;
    }
    // The use of `Object#toString` avoids issues with the `typeof` operator
    // in Safari 9 which returns 'object' for typed arrays and other constructors.
    var tag = _baseGetTag(value);
    return tag == funcTag || tag == genTag || tag == asyncTag || tag == proxyTag;
}
var isFunction_1 = isFunction;
/** Used to detect overreaching core-js shims. */
var coreJsData = _root['__core-js_shared__'];
var _coreJsData = coreJsData;
/** Used to detect methods masquerading as native. */
var maskSrcKey = (function () {
    var uid = /[^.]+$/.exec(_coreJsData && _coreJsData.keys && _coreJsData.keys.IE_PROTO || '');
    return uid ? ('Symbol(src)_1.' + uid) : '';
}());
/**
 * Checks if `func` has its source masked.
 *
 * @private
 * @param {Function} func The function to check.
 * @returns {boolean} Returns `true` if `func` is masked, else `false`.
 */
function isMasked(func) {
    return !!maskSrcKey && (maskSrcKey in func);
}
var _isMasked = isMasked;
/** Used for built-in method references. */
var funcProto = Function.prototype;
/** Used to resolve the decompiled source of functions. */
var funcToString = funcProto.toString;
/**
 * Converts `func` to its source code.
 *
 * @private
 * @param {Function} func The function to convert.
 * @returns {string} Returns the source code.
 */
function toSource(func) {
    if (func != null) {
        try {
            return funcToString.call(func);
        }
        catch (e) { }
        try {
            return (func + '');
        }
        catch (e) { }
    }
    return '';
}
var _toSource = toSource;
/**
 * Used to match `RegExp`
 * [syntax characters](http://ecma-international.org/ecma-262/7.0/#sec-patterns).
 */
var reRegExpChar = /[\\^$.*+?()[\]{}|]/g;
/** Used to detect host constructors (Safari). */
var reIsHostCtor = /^\[object .+?Constructor\]$/;
/** Used for built-in method references. */
var funcProto$1 = Function.prototype, objectProto$2 = Object.prototype;
/** Used to resolve the decompiled source of functions. */
var funcToString$1 = funcProto$1.toString;
/** Used to check objects for own properties. */
var hasOwnProperty$1 = objectProto$2.hasOwnProperty;
/** Used to detect if a method is native. */
var reIsNative = RegExp('^' +
    funcToString$1.call(hasOwnProperty$1).replace(reRegExpChar, '\\$&')
        .replace(/hasOwnProperty|(function).*?(?=\\\()| for .+?(?=\\\])/g, '$1.*?') + '$');
/**
 * The base implementation of `_.isNative` without bad shim checks.
 *
 * @private
 * @param {*} value The value to check.
 * @returns {boolean} Returns `true` if `value` is a native function,
 *  else `false`.
 */
function baseIsNative(value) {
    if (!isObject_1(value) || _isMasked(value)) {
        return false;
    }
    var pattern = isFunction_1(value) ? reIsNative : reIsHostCtor;
    return pattern.test(_toSource(value));
}
var _baseIsNative = baseIsNative;
/**
 * Gets the value at `key` of `object`.
 *
 * @private
 * @param {Object} [object] The object to query.
 * @param {string} key The key of the property to get.
 * @returns {*} Returns the property value.
 */
function getValue(object, key) {
    return object == null ? undefined : object[key];
}
var _getValue = getValue;
/**
 * Gets the native function at `key` of `object`.
 *
 * @private
 * @param {Object} object The object to query.
 * @param {string} key The key of the method to get.
 * @returns {*} Returns the function if it's native, else `undefined`.
 */
function getNative(object, key) {
    var value = _getValue(object, key);
    return _baseIsNative(value) ? value : undefined;
}
var _getNative = getNative;
/* Built-in method references that are verified to be native. */
var Map = _getNative(_root, 'Map');
var _Map = Map;
/* Built-in method references that are verified to be native. */
var nativeCreate = _getNative(Object, 'create');
var _nativeCreate = nativeCreate;
/**
 * Removes all key-value entries from the hash.
 *
 * @private
 * @name clear
 * @memberOf Hash
 */
function hashClear() {
    this.__data__ = _nativeCreate ? _nativeCreate(null) : {};
    this.size = 0;
}
var _hashClear = hashClear;
/**
 * Removes `key` and its value from the hash.
 *
 * @private
 * @name delete
 * @memberOf Hash
 * @param {Object} hash The hash to modify.
 * @param {string} key The key of the value to remove.
 * @returns {boolean} Returns `true` if the entry was removed, else `false`.
 */
function hashDelete(key) {
    var result = this.has(key) && delete this.__data__[key];
    this.size -= result ? 1 : 0;
    return result;
}
var _hashDelete = hashDelete;
/** Used to stand-in for `undefined` hash values. */
var HASH_UNDEFINED = '__lodash_hash_undefined__';
/** Used for built-in method references. */
var objectProto$3 = Object.prototype;
/** Used to check objects for own properties. */
var hasOwnProperty$2 = objectProto$3.hasOwnProperty;
/**
 * Gets the hash value for `key`.
 *
 * @private
 * @name get
 * @memberOf Hash
 * @param {string} key The key of the value to get.
 * @returns {*} Returns the entry value.
 */
function hashGet(key) {
    var data = this.__data__;
    if (_nativeCreate) {
        var result = data[key];
        return result === HASH_UNDEFINED ? undefined : result;
    }
    return hasOwnProperty$2.call(data, key) ? data[key] : undefined;
}
var _hashGet = hashGet;
/** Used for built-in method references. */
var objectProto$4 = Object.prototype;
/** Used to check objects for own properties. */
var hasOwnProperty$3 = objectProto$4.hasOwnProperty;
/**
 * Checks if a hash value for `key` exists.
 *
 * @private
 * @name has
 * @memberOf Hash
 * @param {string} key The key of the entry to check.
 * @returns {boolean} Returns `true` if an entry for `key` exists, else `false`.
 */
function hashHas(key) {
    var data = this.__data__;
    return _nativeCreate ? (data[key] !== undefined) : hasOwnProperty$3.call(data, key);
}
var _hashHas = hashHas;
/** Used to stand-in for `undefined` hash values. */
var HASH_UNDEFINED$1 = '__lodash_hash_undefined__';
/**
 * Sets the hash `key` to `value`.
 *
 * @private
 * @name set
 * @memberOf Hash
 * @param {string} key The key of the value to set.
 * @param {*} value The value to set.
 * @returns {Object} Returns the hash instance.
 */
function hashSet(key, value) {
    var data = this.__data__;
    this.size += this.has(key) ? 0 : 1;
    data[key] = (_nativeCreate && value === undefined) ? HASH_UNDEFINED$1 : value;
    return this;
}
var _hashSet = hashSet;
/**
 * Creates a hash object.
 *
 * @private
 * @constructor
 * @param {Array} [entries] The key-value pairs to cache.
 */
function Hash(entries) {
    var index = -1, length = entries == null ? 0 : entries.length;
    this.clear();
    while (++index < length) {
        var entry = entries[index];
        this.set(entry[0], entry[1]);
    }
}
// Add methods to `Hash`.
Hash.prototype.clear = _hashClear;
Hash.prototype['delete'] = _hashDelete;
Hash.prototype.get = _hashGet;
Hash.prototype.has = _hashHas;
Hash.prototype.set = _hashSet;
var _Hash = Hash;
/**
 * Removes all key-value entries from the map.
 *
 * @private
 * @name clear
 * @memberOf MapCache
 */
function mapCacheClear() {
    this.size = 0;
    this.__data__ = {
        'hash': new _Hash,
        'map': new (_Map || _ListCache),
        'string': new _Hash
    };
}
var _mapCacheClear = mapCacheClear;
/**
 * Checks if `value` is suitable for use as unique object key.
 *
 * @private
 * @param {*} value The value to check.
 * @returns {boolean} Returns `true` if `value` is suitable, else `false`.
 */
function isKeyable(value) {
    var type = typeof value;
    return (type == 'string' || type == 'number' || type == 'symbol' || type == 'boolean')
        ? (value !== '__proto__')
        : (value === null);
}
var _isKeyable = isKeyable;
/**
 * Gets the data for `map`.
 *
 * @private
 * @param {Object} map The map to query.
 * @param {string} key The reference key.
 * @returns {*} Returns the map data.
 */
function getMapData(map, key) {
    var data = map.__data__;
    return _isKeyable(key)
        ? data[typeof key == 'string' ? 'string' : 'hash']
        : data.map;
}
var _getMapData = getMapData;
/**
 * Removes `key` and its value from the map.
 *
 * @private
 * @name delete
 * @memberOf MapCache
 * @param {string} key The key of the value to remove.
 * @returns {boolean} Returns `true` if the entry was removed, else `false`.
 */
function mapCacheDelete(key) {
    var result = _getMapData(this, key)['delete'](key);
    this.size -= result ? 1 : 0;
    return result;
}
var _mapCacheDelete = mapCacheDelete;
/**
 * Gets the map value for `key`.
 *
 * @private
 * @name get
 * @memberOf MapCache
 * @param {string} key The key of the value to get.
 * @returns {*} Returns the entry value.
 */
function mapCacheGet(key) {
    return _getMapData(this, key).get(key);
}
var _mapCacheGet = mapCacheGet;
/**
 * Checks if a map value for `key` exists.
 *
 * @private
 * @name has
 * @memberOf MapCache
 * @param {string} key The key of the entry to check.
 * @returns {boolean} Returns `true` if an entry for `key` exists, else `false`.
 */
function mapCacheHas(key) {
    return _getMapData(this, key).has(key);
}
var _mapCacheHas = mapCacheHas;
/**
 * Sets the map `key` to `value`.
 *
 * @private
 * @name set
 * @memberOf MapCache
 * @param {string} key The key of the value to set.
 * @param {*} value The value to set.
 * @returns {Object} Returns the map cache instance.
 */
function mapCacheSet(key, value) {
    var data = _getMapData(this, key), size = data.size;
    data.set(key, value);
    this.size += data.size == size ? 0 : 1;
    return this;
}
var _mapCacheSet = mapCacheSet;
/**
 * Creates a map cache object to store key-value pairs.
 *
 * @private
 * @constructor
 * @param {Array} [entries] The key-value pairs to cache.
 */
function MapCache(entries) {
    var index = -1, length = entries == null ? 0 : entries.length;
    this.clear();
    while (++index < length) {
        var entry = entries[index];
        this.set(entry[0], entry[1]);
    }
}
// Add methods to `MapCache`.
MapCache.prototype.clear = _mapCacheClear;
MapCache.prototype['delete'] = _mapCacheDelete;
MapCache.prototype.get = _mapCacheGet;
MapCache.prototype.has = _mapCacheHas;
MapCache.prototype.set = _mapCacheSet;
var _MapCache = MapCache;
/** Used as the size to enable large array optimizations. */
var LARGE_ARRAY_SIZE = 200;
/**
 * Sets the stack `key` to `value`.
 *
 * @private
 * @name set
 * @memberOf Stack
 * @param {string} key The key of the value to set.
 * @param {*} value The value to set.
 * @returns {Object} Returns the stack cache instance.
 */
function stackSet(key, value) {
    var data = this.__data__;
    if (data instanceof _ListCache) {
        var pairs = data.__data__;
        if (!_Map || (pairs.length < LARGE_ARRAY_SIZE - 1)) {
            pairs.push([key, value]);
            this.size = ++data.size;
            return this;
        }
        data = this.__data__ = new _MapCache(pairs);
    }
    data.set(key, value);
    this.size = data.size;
    return this;
}
var _stackSet = stackSet;
/**
 * Creates a stack cache object to store key-value pairs.
 *
 * @private
 * @constructor
 * @param {Array} [entries] The key-value pairs to cache.
 */
function Stack(entries) {
    var data = this.__data__ = new _ListCache(entries);
    this.size = data.size;
}
// Add methods to `Stack`.
Stack.prototype.clear = _stackClear;
Stack.prototype['delete'] = _stackDelete;
Stack.prototype.get = _stackGet;
Stack.prototype.has = _stackHas;
Stack.prototype.set = _stackSet;
var _Stack = Stack;
/**
 * A specialized version of `_.forEach` for arrays without support for
 * iteratee shorthands.
 *
 * @private
 * @param {Array} [array] The array to iterate over.
 * @param {Function} iteratee The function invoked per iteration.
 * @returns {Array} Returns `array`.
 */
function arrayEach(array, iteratee) {
    var index = -1, length = array == null ? 0 : array.length;
    while (++index < length) {
        if (iteratee(array[index], index, array) === false) {
            break;
        }
    }
    return array;
}
var _arrayEach = arrayEach;
var defineProperty = (function () {
    try {
        var func = _getNative(Object, 'defineProperty');
        func({}, '', {});
        return func;
    }
    catch (e) { }
}());
var _defineProperty = defineProperty;
/**
 * The base implementation of `assignValue` and `assignMergeValue` without
 * value checks.
 *
 * @private
 * @param {Object} object The object to modify.
 * @param {string} key The key of the property to assign.
 * @param {*} value The value to assign.
 */
function baseAssignValue(object, key, value) {
    if (key == '__proto__' && _defineProperty) {
        _defineProperty(object, key, {
            'configurable': true,
            'enumerable': true,
            'value': value,
            'writable': true
        });
    }
    else {
        object[key] = value;
    }
}
var _baseAssignValue = baseAssignValue;
/** Used for built-in method references. */
var objectProto$5 = Object.prototype;
/** Used to check objects for own properties. */
var hasOwnProperty$4 = objectProto$5.hasOwnProperty;
/**
 * Assigns `value` to `key` of `object` if the existing value is not equivalent
 * using [`SameValueZero`](http://ecma-international.org/ecma-262/7.0/#sec-samevaluezero)
 * for equality comparisons.
 *
 * @private
 * @param {Object} object The object to modify.
 * @param {string} key The key of the property to assign.
 * @param {*} value The value to assign.
 */
function assignValue(object, key, value) {
    var objValue = object[key];
    if (!(hasOwnProperty$4.call(object, key) && eq_1(objValue, value)) ||
        (value === undefined && !(key in object))) {
        _baseAssignValue(object, key, value);
    }
}
var _assignValue = assignValue;
/**
 * Copies properties of `source` to `object`.
 *
 * @private
 * @param {Object} source The object to copy properties from.
 * @param {Array} props The property identifiers to copy.
 * @param {Object} [object={}] The object to copy properties to.
 * @param {Function} [customizer] The function to customize copied values.
 * @returns {Object} Returns `object`.
 */
function copyObject(source, props, object, customizer) {
    var isNew = !object;
    object || (object = {});
    var index = -1, length = props.length;
    while (++index < length) {
        var key = props[index];
        var newValue = customizer
            ? customizer(object[key], source[key], key, object, source)
            : undefined;
        if (newValue === undefined) {
            newValue = source[key];
        }
        if (isNew) {
            _baseAssignValue(object, key, newValue);
        }
        else {
            _assignValue(object, key, newValue);
        }
    }
    return object;
}
var _copyObject = copyObject;
/**
 * The base implementation of `_.times` without support for iteratee shorthands
 * or max array length checks.
 *
 * @private
 * @param {number} n The number of times to invoke `iteratee`.
 * @param {Function} iteratee The function invoked per iteration.
 * @returns {Array} Returns the array of results.
 */
function baseTimes(n, iteratee) {
    var index = -1, result = Array(n);
    while (++index < n) {
        result[index] = iteratee(index);
    }
    return result;
}
var _baseTimes = baseTimes;
/**
 * Checks if `value` is object-like. A value is object-like if it's not `null`
 * and has a `typeof` result of "object".
 *
 * @static
 * @memberOf _
 * @since 4.0.0
 * @category Lang
 * @param {*} value The value to check.
 * @returns {boolean} Returns `true` if `value` is object-like, else `false`.
 * @example
 *
 * _.isObjectLike({});
 * // => true
 *
 * _.isObjectLike([1, 2, 3]);
 * // => true
 *
 * _.isObjectLike(_.noop);
 * // => false
 *
 * _.isObjectLike(null);
 * // => false
 */
function isObjectLike(value) {
    return value != null && typeof value == 'object';
}
var isObjectLike_1 = isObjectLike;
/** `Object#toString` result references. */
var argsTag = '[object Arguments]';
/**
 * The base implementation of `_.isArguments`.
 *
 * @private
 * @param {*} value The value to check.
 * @returns {boolean} Returns `true` if `value` is an `arguments` object,
 */
function baseIsArguments(value) {
    return isObjectLike_1(value) && _baseGetTag(value) == argsTag;
}
var _baseIsArguments = baseIsArguments;
/** Used for built-in method references. */
var objectProto$6 = Object.prototype;
/** Used to check objects for own properties. */
var hasOwnProperty$5 = objectProto$6.hasOwnProperty;
/** Built-in value references. */
var propertyIsEnumerable = objectProto$6.propertyIsEnumerable;
/**
 * Checks if `value` is likely an `arguments` object.
 *
 * @static
 * @memberOf _
 * @since 0.1.0
 * @category Lang
 * @param {*} value The value to check.
 * @returns {boolean} Returns `true` if `value` is an `arguments` object,
 *  else `false`.
 * @example
 *
 * _.isArguments(function() { return arguments; }());
 * // => true
 *
 * _.isArguments([1, 2, 3]);
 * // => false
 */
var isArguments = _baseIsArguments(function () { return arguments; }()) ? _baseIsArguments : function (value) {
    return isObjectLike_1(value) && hasOwnProperty$5.call(value, 'callee') &&
        !propertyIsEnumerable.call(value, 'callee');
};
var isArguments_1 = isArguments;
/**
 * Checks if `value` is classified as an `Array` object.
 *
 * @static
 * @memberOf _
 * @since 0.1.0
 * @category Lang
 * @param {*} value The value to check.
 * @returns {boolean} Returns `true` if `value` is an array, else `false`.
 * @example
 *
 * _.isArray([1, 2, 3]);
 * // => true
 *
 * _.isArray(document.body.children);
 * // => false
 *
 * _.isArray('abc');
 * // => false
 *
 * _.isArray(_.noop);
 * // => false
 */
var isArray = Array.isArray;
var isArray_1 = isArray;
/**
 * This method returns `false`.
 *
 * @static
 * @memberOf _
 * @since 4.13.0
 * @category Util
 * @returns {boolean} Returns `false`.
 * @example
 *
 * _.times(2, _.stubFalse);
 * // => [false, false]
 */
function stubFalse() {
    return false;
}
var stubFalse_1 = stubFalse;
var isBuffer_1 = createCommonjsModule(function (module, exports) {
    /** Detect free variable `exports`. */
    var freeExports = exports && !exports.nodeType && exports;
    /** Detect free variable `module`. */
    var freeModule = freeExports && 'object' == 'object' && module && !module.nodeType && module;
    /** Detect the popular CommonJS extension `module.exports`. */
    var moduleExports = freeModule && freeModule.exports === freeExports;
    /** Built-in value references. */
    var Buffer = moduleExports ? _root.Buffer : undefined;
    /* Built-in method references for those with the same name as other `lodash` methods. */
    var nativeIsBuffer = Buffer ? Buffer.isBuffer : undefined;
    /**
     * Checks if `value` is a buffer.
     *
     * @static
     * @memberOf _
     * @since 4.3.0
     * @category Lang
     * @param {*} value The value to check.
     * @returns {boolean} Returns `true` if `value` is a buffer, else `false`.
     * @example
     *
     * _.isBuffer(new Buffer(2));
     * // => true
     *
     * _.isBuffer(new Uint8Array(2));
     * // => false
     */
    var isBuffer = nativeIsBuffer || stubFalse_1;
    module.exports = isBuffer;
});
/** Used as references for various `Number` constants. */
var MAX_SAFE_INTEGER = 9007199254740991;
/** Used to detect unsigned integer values. */
var reIsUint = /^(?:0|[1-9]\d*)$/;
/**
 * Checks if `value` is a valid array-like index.
 *
 * @private
 * @param {*} value The value to check.
 * @param {number} [length=MAX_SAFE_INTEGER] The upper bounds of a valid index.
 * @returns {boolean} Returns `true` if `value` is a valid index, else `false`.
 */
function isIndex(value, length) {
    var type = typeof value;
    length = length == null ? MAX_SAFE_INTEGER : length;
    return !!length &&
        (type == 'number' ||
            (type != 'symbol' && reIsUint.test(value))) &&
        (value > -1 && value % 1 == 0 && value < length);
}
var _isIndex = isIndex;
/** Used as references for various `Number` constants. */
var MAX_SAFE_INTEGER$1 = 9007199254740991;
/**
 * Checks if `value` is a valid array-like length.
 *
 * **Note:** This method is loosely based on
 * [`ToLength`](http://ecma-international.org/ecma-262/7.0/#sec-tolength).
 *
 * @static
 * @memberOf _
 * @since 4.0.0
 * @category Lang
 * @param {*} value The value to check.
 * @returns {boolean} Returns `true` if `value` is a valid length, else `false`.
 * @example
 *
 * _.isLength(3);
 * // => true
 *
 * _.isLength(Number.MIN_VALUE);
 * // => false
 *
 * _.isLength(Infinity);
 * // => false
 *
 * _.isLength('3');
 * // => false
 */
function isLength(value) {
    return typeof value == 'number' &&
        value > -1 && value % 1 == 0 && value <= MAX_SAFE_INTEGER$1;
}
var isLength_1 = isLength;
/** `Object#toString` result references. */
var argsTag$1 = '[object Arguments]', arrayTag = '[object Array]', boolTag = '[object Boolean]', dateTag = '[object Date]', errorTag = '[object Error]', funcTag$1 = '[object Function]', mapTag = '[object Map]', numberTag = '[object Number]', objectTag = '[object Object]', regexpTag = '[object RegExp]', setTag = '[object Set]', stringTag = '[object String]', weakMapTag = '[object WeakMap]';
var arrayBufferTag = '[object ArrayBuffer]', dataViewTag = '[object DataView]', float32Tag = '[object Float32Array]', float64Tag = '[object Float64Array]', int8Tag = '[object Int8Array]', int16Tag = '[object Int16Array]', int32Tag = '[object Int32Array]', uint8Tag = '[object Uint8Array]', uint8ClampedTag = '[object Uint8ClampedArray]', uint16Tag = '[object Uint16Array]', uint32Tag = '[object Uint32Array]';
/** Used to identify `toStringTag` values of typed arrays. */
var typedArrayTags = {};
typedArrayTags[float32Tag] = typedArrayTags[float64Tag] =
    typedArrayTags[int8Tag] = typedArrayTags[int16Tag] =
        typedArrayTags[int32Tag] = typedArrayTags[uint8Tag] =
            typedArrayTags[uint8ClampedTag] = typedArrayTags[uint16Tag] =
                typedArrayTags[uint32Tag] = true;
typedArrayTags[argsTag$1] = typedArrayTags[arrayTag] =
    typedArrayTags[arrayBufferTag] = typedArrayTags[boolTag] =
        typedArrayTags[dataViewTag] = typedArrayTags[dateTag] =
            typedArrayTags[errorTag] = typedArrayTags[funcTag$1] =
                typedArrayTags[mapTag] = typedArrayTags[numberTag] =
                    typedArrayTags[objectTag] = typedArrayTags[regexpTag] =
                        typedArrayTags[setTag] = typedArrayTags[stringTag] =
                            typedArrayTags[weakMapTag] = false;
/**
 * The base implementation of `_.isTypedArray` without Node.js optimizations.
 *
 * @private
 * @param {*} value The value to check.
 * @returns {boolean} Returns `true` if `value` is a typed array, else `false`.
 */
function baseIsTypedArray(value) {
    return isObjectLike_1(value) &&
        isLength_1(value.length) && !!typedArrayTags[_baseGetTag(value)];
}
var _baseIsTypedArray = baseIsTypedArray;
/**
 * The base implementation of `_.unary` without support for storing metadata.
 *
 * @private
 * @param {Function} func The function to cap arguments for.
 * @returns {Function} Returns the new capped function.
 */
function baseUnary(func) {
    return function (value) {
        return func(value);
    };
}
var _baseUnary = baseUnary;
var _nodeUtil = createCommonjsModule(function (module, exports) {
    /** Detect free variable `exports`. */
    var freeExports = exports && !exports.nodeType && exports;
    /** Detect free variable `module`. */
    var freeModule = freeExports && 'object' == 'object' && module && !module.nodeType && module;
    /** Detect the popular CommonJS extension `module.exports`. */
    var moduleExports = freeModule && freeModule.exports === freeExports;
    /** Detect free variable `process` from Node.js. */
    var freeProcess = moduleExports && _freeGlobal.process;
    /** Used to access faster Node.js helpers. */
    var nodeUtil = (function () {
        try {
            // Use `util.types` for Node.js 10+.
            var types = freeModule && freeModule.require && freeModule.require('util').types;
            if (types) {
                return types;
            }
            // Legacy `process.binding('util')` for Node.js < 10.
            return freeProcess && freeProcess.binding && freeProcess.binding('util');
        }
        catch (e) { }
    }());
    module.exports = nodeUtil;
});
/* Node.js helper references. */
var nodeIsTypedArray = _nodeUtil && _nodeUtil.isTypedArray;
/**
 * Checks if `value` is classified as a typed array.
 *
 * @static
 * @memberOf _
 * @since 3.0.0
 * @category Lang
 * @param {*} value The value to check.
 * @returns {boolean} Returns `true` if `value` is a typed array, else `false`.
 * @example
 *
 * _.isTypedArray(new Uint8Array);
 * // => true
 *
 * _.isTypedArray([]);
 * // => false
 */
var isTypedArray = nodeIsTypedArray ? _baseUnary(nodeIsTypedArray) : _baseIsTypedArray;
var isTypedArray_1 = isTypedArray;
/** Used for built-in method references. */
var objectProto$7 = Object.prototype;
/** Used to check objects for own properties. */
var hasOwnProperty$6 = objectProto$7.hasOwnProperty;
/**
 * Creates an array of the enumerable property names of the array-like `value`.
 *
 * @private
 * @param {*} value The value to query.
 * @param {boolean} inherited Specify returning inherited property names.
 * @returns {Array} Returns the array of property names.
 */
function arrayLikeKeys(value, inherited) {
    var isArr = isArray_1(value), isArg = !isArr && isArguments_1(value), isBuff = !isArr && !isArg && isBuffer_1(value), isType = !isArr && !isArg && !isBuff && isTypedArray_1(value), skipIndexes = isArr || isArg || isBuff || isType, result = skipIndexes ? _baseTimes(value.length, String) : [], length = result.length;
    for (var key in value) {
        if ((inherited || hasOwnProperty$6.call(value, key)) &&
            !(skipIndexes && (
            // Safari 9 has enumerable `arguments.length` in strict mode.
            key == 'length' ||
                // Node.js 0.10 has enumerable non-index properties on buffers.
                (isBuff && (key == 'offset' || key == 'parent')) ||
                // PhantomJS 2 has enumerable non-index properties on typed arrays.
                (isType && (key == 'buffer' || key == 'byteLength' || key == 'byteOffset')) ||
                // Skip index properties.
                _isIndex(key, length)))) {
            result.push(key);
        }
    }
    return result;
}
var _arrayLikeKeys = arrayLikeKeys;
/** Used for built-in method references. */
var objectProto$8 = Object.prototype;
/**
 * Checks if `value` is likely a prototype object.
 *
 * @private
 * @param {*} value The value to check.
 * @returns {boolean} Returns `true` if `value` is a prototype, else `false`.
 */
function isPrototype(value) {
    var Ctor = value && value.constructor, proto = (typeof Ctor == 'function' && Ctor.prototype) || objectProto$8;
    return value === proto;
}
var _isPrototype = isPrototype;
/**
 * Creates a unary function that invokes `func` with its argument transformed.
 *
 * @private
 * @param {Function} func The function to wrap.
 * @param {Function} transform The argument transform.
 * @returns {Function} Returns the new function.
 */
function overArg(func, transform) {
    return function (arg) {
        return func(transform(arg));
    };
}
var _overArg = overArg;
/* Built-in method references for those with the same name as other `lodash` methods. */
var nativeKeys = _overArg(Object.keys, Object);
var _nativeKeys = nativeKeys;
/** Used for built-in method references. */
var objectProto$9 = Object.prototype;
/** Used to check objects for own properties. */
var hasOwnProperty$7 = objectProto$9.hasOwnProperty;
/**
 * The base implementation of `_.keys` which doesn't treat sparse arrays as dense.
 *
 * @private
 * @param {Object} object The object to query.
 * @returns {Array} Returns the array of property names.
 */
function baseKeys(object) {
    if (!_isPrototype(object)) {
        return _nativeKeys(object);
    }
    var result = [];
    for (var key in Object(object)) {
        if (hasOwnProperty$7.call(object, key) && key != 'constructor') {
            result.push(key);
        }
    }
    return result;
}
var _baseKeys = baseKeys;
/**
 * Checks if `value` is array-like. A value is considered array-like if it's
 * not a function and has a `value.length` that's an integer greater than or
 * equal to `0` and less than or equal to `Number.MAX_SAFE_INTEGER`.
 *
 * @static
 * @memberOf _
 * @since 4.0.0
 * @category Lang
 * @param {*} value The value to check.
 * @returns {boolean} Returns `true` if `value` is array-like, else `false`.
 * @example
 *
 * _.isArrayLike([1, 2, 3]);
 * // => true
 *
 * _.isArrayLike(document.body.children);
 * // => true
 *
 * _.isArrayLike('abc');
 * // => true
 *
 * _.isArrayLike(_.noop);
 * // => false
 */
function isArrayLike(value) {
    return value != null && isLength_1(value.length) && !isFunction_1(value);
}
var isArrayLike_1 = isArrayLike;
/**
 * Creates an array of the own enumerable property names of `object`.
 *
 * **Note:** Non-object values are coerced to objects. See the
 * [ES spec](http://ecma-international.org/ecma-262/7.0/#sec-object.keys)
 * for more details.
 *
 * @static
 * @since 0.1.0
 * @memberOf _
 * @category Object
 * @param {Object} object The object to query.
 * @returns {Array} Returns the array of property names.
 * @example
 *
 * function Foo() {
 *   this.a = 1;
 *   this.b = 2;
 * }
 *
 * Foo.prototype.c = 3;
 *
 * _.keys(new Foo);
 * // => ['a', 'b'] (iteration order is not guaranteed)
 *
 * _.keys('hi');
 * // => ['0', '1']
 */
function keys(object) {
    return isArrayLike_1(object) ? _arrayLikeKeys(object) : _baseKeys(object);
}
var keys_1 = keys;
/**
 * The base implementation of `_.assign` without support for multiple sources
 * or `customizer` functions.
 *
 * @private
 * @param {Object} object The destination object.
 * @param {Object} source The source object.
 * @returns {Object} Returns `object`.
 */
function baseAssign(object, source) {
    return object && _copyObject(source, keys_1(source), object);
}
var _baseAssign = baseAssign;
/**
 * This function is like
 * [`Object.keys`](http://ecma-international.org/ecma-262/7.0/#sec-object.keys)
 * except that it includes inherited enumerable properties.
 *
 * @private
 * @param {Object} object The object to query.
 * @returns {Array} Returns the array of property names.
 */
function nativeKeysIn(object) {
    var result = [];
    if (object != null) {
        for (var key in Object(object)) {
            result.push(key);
        }
    }
    return result;
}
var _nativeKeysIn = nativeKeysIn;
/** Used for built-in method references. */
var objectProto$a = Object.prototype;
/** Used to check objects for own properties. */
var hasOwnProperty$8 = objectProto$a.hasOwnProperty;
/**
 * The base implementation of `_.keysIn` which doesn't treat sparse arrays as dense.
 *
 * @private
 * @param {Object} object The object to query.
 * @returns {Array} Returns the array of property names.
 */
function baseKeysIn(object) {
    if (!isObject_1(object)) {
        return _nativeKeysIn(object);
    }
    var isProto = _isPrototype(object), result = [];
    for (var key in object) {
        if (!(key == 'constructor' && (isProto || !hasOwnProperty$8.call(object, key)))) {
            result.push(key);
        }
    }
    return result;
}
var _baseKeysIn = baseKeysIn;
/**
 * Creates an array of the own and inherited enumerable property names of `object`.
 *
 * **Note:** Non-object values are coerced to objects.
 *
 * @static
 * @memberOf _
 * @since 3.0.0
 * @category Object
 * @param {Object} object The object to query.
 * @returns {Array} Returns the array of property names.
 * @example
 *
 * function Foo() {
 *   this.a = 1;
 *   this.b = 2;
 * }
 *
 * Foo.prototype.c = 3;
 *
 * _.keysIn(new Foo);
 * // => ['a', 'b', 'c'] (iteration order is not guaranteed)
 */
function keysIn$1(object) {
    return isArrayLike_1(object) ? _arrayLikeKeys(object, true) : _baseKeysIn(object);
}
var keysIn_1 = keysIn$1;
/**
 * The base implementation of `_.assignIn` without support for multiple sources
 * or `customizer` functions.
 *
 * @private
 * @param {Object} object The destination object.
 * @param {Object} source The source object.
 * @returns {Object} Returns `object`.
 */
function baseAssignIn(object, source) {
    return object && _copyObject(source, keysIn_1(source), object);
}
var _baseAssignIn = baseAssignIn;
var _cloneBuffer = createCommonjsModule(function (module, exports) {
    /** Detect free variable `exports`. */
    var freeExports = exports && !exports.nodeType && exports;
    /** Detect free variable `module`. */
    var freeModule = freeExports && 'object' == 'object' && module && !module.nodeType && module;
    /** Detect the popular CommonJS extension `module.exports`. */
    var moduleExports = freeModule && freeModule.exports === freeExports;
    /** Built-in value references. */
    var Buffer = moduleExports ? _root.Buffer : undefined, allocUnsafe = Buffer ? Buffer.allocUnsafe : undefined;
    /**
     * Creates a clone of  `buffer`.
     *
     * @private
     * @param {Buffer} buffer The buffer to clone.
     * @param {boolean} [isDeep] Specify a deep clone.
     * @returns {Buffer} Returns the cloned buffer.
     */
    function cloneBuffer(buffer, isDeep) {
        if (isDeep) {
            return buffer.slice();
        }
        var length = buffer.length, result = allocUnsafe ? allocUnsafe(length) : new buffer.constructor(length);
        buffer.copy(result);
        return result;
    }
    module.exports = cloneBuffer;
});
/**
 * Copies the values of `source` to `array`.
 *
 * @private
 * @param {Array} source The array to copy values from.
 * @param {Array} [array=[]] The array to copy values to.
 * @returns {Array} Returns `array`.
 */
function copyArray(source, array) {
    var index = -1, length = source.length;
    array || (array = Array(length));
    while (++index < length) {
        array[index] = source[index];
    }
    return array;
}
var _copyArray = copyArray;
/**
 * A specialized version of `_.filter` for arrays without support for
 * iteratee shorthands.
 *
 * @private
 * @param {Array} [array] The array to iterate over.
 * @param {Function} predicate The function invoked per iteration.
 * @returns {Array} Returns the new filtered array.
 */
function arrayFilter(array, predicate) {
    var index = -1, length = array == null ? 0 : array.length, resIndex = 0, result = [];
    while (++index < length) {
        var value = array[index];
        if (predicate(value, index, array)) {
            result[resIndex++] = value;
        }
    }
    return result;
}
var _arrayFilter = arrayFilter;
/**
 * This method returns a new empty array.
 *
 * @static
 * @memberOf _
 * @since 4.13.0
 * @category Util
 * @returns {Array} Returns the new empty array.
 * @example
 *
 * var arrays = _.times(2, _.stubArray);
 *
 * console.log(arrays);
 * // => [[], []]
 *
 * console.log(arrays[0] === arrays[1]);
 * // => false
 */
function stubArray() {
    return [];
}
var stubArray_1 = stubArray;
/** Used for built-in method references. */
var objectProto$b = Object.prototype;
/** Built-in value references. */
var propertyIsEnumerable$1 = objectProto$b.propertyIsEnumerable;
/* Built-in method references for those with the same name as other `lodash` methods. */
var nativeGetSymbols = Object.getOwnPropertySymbols;
/**
 * Creates an array of the own enumerable symbols of `object`.
 *
 * @private
 * @param {Object} object The object to query.
 * @returns {Array} Returns the array of symbols.
 */
var getSymbols = !nativeGetSymbols ? stubArray_1 : function (object) {
    if (object == null) {
        return [];
    }
    object = Object(object);
    return _arrayFilter(nativeGetSymbols(object), function (symbol) {
        return propertyIsEnumerable$1.call(object, symbol);
    });
};
var _getSymbols = getSymbols;
/**
 * Copies own symbols of `source` to `object`.
 *
 * @private
 * @param {Object} source The object to copy symbols from.
 * @param {Object} [object={}] The object to copy symbols to.
 * @returns {Object} Returns `object`.
 */
function copySymbols(source, object) {
    return _copyObject(source, _getSymbols(source), object);
}
var _copySymbols = copySymbols;
/**
 * Appends the elements of `values` to `array`.
 *
 * @private
 * @param {Array} array The array to modify.
 * @param {Array} values The values to append.
 * @returns {Array} Returns `array`.
 */
function arrayPush(array, values) {
    var index = -1, length = values.length, offset = array.length;
    while (++index < length) {
        array[offset + index] = values[index];
    }
    return array;
}
var _arrayPush = arrayPush;
/** Built-in value references. */
var getPrototype = _overArg(Object.getPrototypeOf, Object);
var _getPrototype = getPrototype;
/* Built-in method references for those with the same name as other `lodash` methods. */
var nativeGetSymbols$1 = Object.getOwnPropertySymbols;
/**
 * Creates an array of the own and inherited enumerable symbols of `object`.
 *
 * @private
 * @param {Object} object The object to query.
 * @returns {Array} Returns the array of symbols.
 */
var getSymbolsIn = !nativeGetSymbols$1 ? stubArray_1 : function (object) {
    var result = [];
    while (object) {
        _arrayPush(result, _getSymbols(object));
        object = _getPrototype(object);
    }
    return result;
};
var _getSymbolsIn = getSymbolsIn;
/**
 * Copies own and inherited symbols of `source` to `object`.
 *
 * @private
 * @param {Object} source The object to copy symbols from.
 * @param {Object} [object={}] The object to copy symbols to.
 * @returns {Object} Returns `object`.
 */
function copySymbolsIn(source, object) {
    return _copyObject(source, _getSymbolsIn(source), object);
}
var _copySymbolsIn = copySymbolsIn;
/**
 * The base implementation of `getAllKeys` and `getAllKeysIn` which uses
 * `keysFunc` and `symbolsFunc` to get the enumerable property names and
 * symbols of `object`.
 *
 * @private
 * @param {Object} object The object to query.
 * @param {Function} keysFunc The function to get the keys of `object`.
 * @param {Function} symbolsFunc The function to get the symbols of `object`.
 * @returns {Array} Returns the array of property names and symbols.
 */
function baseGetAllKeys(object, keysFunc, symbolsFunc) {
    var result = keysFunc(object);
    return isArray_1(object) ? result : _arrayPush(result, symbolsFunc(object));
}
var _baseGetAllKeys = baseGetAllKeys;
/**
 * Creates an array of own enumerable property names and symbols of `object`.
 *
 * @private
 * @param {Object} object The object to query.
 * @returns {Array} Returns the array of property names and symbols.
 */
function getAllKeys(object) {
    return _baseGetAllKeys(object, keys_1, _getSymbols);
}
var _getAllKeys = getAllKeys;
/**
 * Creates an array of own and inherited enumerable property names and
 * symbols of `object`.
 *
 * @private
 * @param {Object} object The object to query.
 * @returns {Array} Returns the array of property names and symbols.
 */
function getAllKeysIn(object) {
    return _baseGetAllKeys(object, keysIn_1, _getSymbolsIn);
}
var _getAllKeysIn = getAllKeysIn;
/* Built-in method references that are verified to be native. */
var DataView = _getNative(_root, 'DataView');
var _DataView = DataView;
/* Built-in method references that are verified to be native. */
var Promise = _getNative(_root, 'Promise');
var _Promise = Promise;
/* Built-in method references that are verified to be native. */
var Set = _getNative(_root, 'Set');
var _Set = Set;
/* Built-in method references that are verified to be native. */
var WeakMap = _getNative(_root, 'WeakMap');
var _WeakMap = WeakMap;
/** `Object#toString` result references. */
var mapTag$1 = '[object Map]', objectTag$1 = '[object Object]', promiseTag = '[object Promise]', setTag$1 = '[object Set]', weakMapTag$1 = '[object WeakMap]';
var dataViewTag$1 = '[object DataView]';
/** Used to detect maps, sets, and weakmaps. */
var dataViewCtorString = _toSource(_DataView), mapCtorString = _toSource(_Map), promiseCtorString = _toSource(_Promise), setCtorString = _toSource(_Set), weakMapCtorString = _toSource(_WeakMap);
/**
 * Gets the `toStringTag` of `value`.
 *
 * @private
 * @param {*} value The value to query.
 * @returns {string} Returns the `toStringTag`.
 */
var getTag = _baseGetTag;
// Fallback for data views, maps, sets, and weak maps in IE 11 and promises in Node.js < 6.
if ((_DataView && getTag(new _DataView(new ArrayBuffer(1))) != dataViewTag$1) ||
    (_Map && getTag(new _Map) != mapTag$1) ||
    (_Promise && getTag(_Promise.resolve()) != promiseTag) ||
    (_Set && getTag(new _Set) != setTag$1) ||
    (_WeakMap && getTag(new _WeakMap) != weakMapTag$1)) {
    getTag = function (value) {
        var result = _baseGetTag(value), Ctor = result == objectTag$1 ? value.constructor : undefined, ctorString = Ctor ? _toSource(Ctor) : '';
        if (ctorString) {
            switch (ctorString) {
                case dataViewCtorString: return dataViewTag$1;
                case mapCtorString: return mapTag$1;
                case promiseCtorString: return promiseTag;
                case setCtorString: return setTag$1;
                case weakMapCtorString: return weakMapTag$1;
            }
        }
        return result;
    };
}
var _getTag = getTag;
/** Used for built-in method references. */
var objectProto$c = Object.prototype;
/** Used to check objects for own properties. */
var hasOwnProperty$9 = objectProto$c.hasOwnProperty;
/**
 * Initializes an array clone.
 *
 * @private
 * @param {Array} array The array to clone.
 * @returns {Array} Returns the initialized clone.
 */
function initCloneArray(array) {
    var length = array.length, result = new array.constructor(length);
    // Add properties assigned by `RegExp#exec`.
    if (length && typeof array[0] == 'string' && hasOwnProperty$9.call(array, 'index')) {
        result.index = array.index;
        result.input = array.input;
    }
    return result;
}
var _initCloneArray = initCloneArray;
/** Built-in value references. */
var Uint8Array = _root.Uint8Array;
var _Uint8Array = Uint8Array;
/**
 * Creates a clone of `arrayBuffer`.
 *
 * @private
 * @param {ArrayBuffer} arrayBuffer The array buffer to clone.
 * @returns {ArrayBuffer} Returns the cloned array buffer.
 */
function cloneArrayBuffer(arrayBuffer) {
    var result = new arrayBuffer.constructor(arrayBuffer.byteLength);
    new _Uint8Array(result).set(new _Uint8Array(arrayBuffer));
    return result;
}
var _cloneArrayBuffer = cloneArrayBuffer;
/**
 * Creates a clone of `dataView`.
 *
 * @private
 * @param {Object} dataView The data view to clone.
 * @param {boolean} [isDeep] Specify a deep clone.
 * @returns {Object} Returns the cloned data view.
 */
function cloneDataView(dataView, isDeep) {
    var buffer = isDeep ? _cloneArrayBuffer(dataView.buffer) : dataView.buffer;
    return new dataView.constructor(buffer, dataView.byteOffset, dataView.byteLength);
}
var _cloneDataView = cloneDataView;
/** Used to match `RegExp` flags from their coerced string values. */
var reFlags = /\w*$/;
/**
 * Creates a clone of `regexp`.
 *
 * @private
 * @param {Object} regexp The regexp to clone.
 * @returns {Object} Returns the cloned regexp.
 */
function cloneRegExp(regexp) {
    var result = new regexp.constructor(regexp.source, reFlags.exec(regexp));
    result.lastIndex = regexp.lastIndex;
    return result;
}
var _cloneRegExp = cloneRegExp;
/** Used to convert symbols to primitives and strings. */
var symbolProto = _Symbol ? _Symbol.prototype : undefined, symbolValueOf = symbolProto ? symbolProto.valueOf : undefined;
/**
 * Creates a clone of the `symbol` object.
 *
 * @private
 * @param {Object} symbol The symbol object to clone.
 * @returns {Object} Returns the cloned symbol object.
 */
function cloneSymbol(symbol) {
    return symbolValueOf ? Object(symbolValueOf.call(symbol)) : {};
}
var _cloneSymbol = cloneSymbol;
/**
 * Creates a clone of `typedArray`.
 *
 * @private
 * @param {Object} typedArray The typed array to clone.
 * @param {boolean} [isDeep] Specify a deep clone.
 * @returns {Object} Returns the cloned typed array.
 */
function cloneTypedArray(typedArray, isDeep) {
    var buffer = isDeep ? _cloneArrayBuffer(typedArray.buffer) : typedArray.buffer;
    return new typedArray.constructor(buffer, typedArray.byteOffset, typedArray.length);
}
var _cloneTypedArray = cloneTypedArray;
/** `Object#toString` result references. */
var boolTag$1 = '[object Boolean]', dateTag$1 = '[object Date]', mapTag$2 = '[object Map]', numberTag$1 = '[object Number]', regexpTag$1 = '[object RegExp]', setTag$2 = '[object Set]', stringTag$1 = '[object String]', symbolTag = '[object Symbol]';
var arrayBufferTag$1 = '[object ArrayBuffer]', dataViewTag$2 = '[object DataView]', float32Tag$1 = '[object Float32Array]', float64Tag$1 = '[object Float64Array]', int8Tag$1 = '[object Int8Array]', int16Tag$1 = '[object Int16Array]', int32Tag$1 = '[object Int32Array]', uint8Tag$1 = '[object Uint8Array]', uint8ClampedTag$1 = '[object Uint8ClampedArray]', uint16Tag$1 = '[object Uint16Array]', uint32Tag$1 = '[object Uint32Array]';
/**
 * Initializes an object clone based on its `toStringTag`.
 *
 * **Note:** This function only supports cloning values with tags of
 * `Boolean`, `Date`, `Error`, `Map`, `Number`, `RegExp`, `Set`, or `String`.
 *
 * @private
 * @param {Object} object The object to clone.
 * @param {string} tag The `toStringTag` of the object to clone.
 * @param {boolean} [isDeep] Specify a deep clone.
 * @returns {Object} Returns the initialized clone.
 */
function initCloneByTag(object, tag, isDeep) {
    var Ctor = object.constructor;
    switch (tag) {
        case arrayBufferTag$1:
            return _cloneArrayBuffer(object);
        case boolTag$1:
        case dateTag$1:
            return new Ctor(+object);
        case dataViewTag$2:
            return _cloneDataView(object, isDeep);
        case float32Tag$1:
        case float64Tag$1:
        case int8Tag$1:
        case int16Tag$1:
        case int32Tag$1:
        case uint8Tag$1:
        case uint8ClampedTag$1:
        case uint16Tag$1:
        case uint32Tag$1:
            return _cloneTypedArray(object, isDeep);
        case mapTag$2:
            return new Ctor;
        case numberTag$1:
        case stringTag$1:
            return new Ctor(object);
        case regexpTag$1:
            return _cloneRegExp(object);
        case setTag$2:
            return new Ctor;
        case symbolTag:
            return _cloneSymbol(object);
    }
}
var _initCloneByTag = initCloneByTag;
/** Built-in value references. */
var objectCreate = Object.create;
/**
 * The base implementation of `_.create` without support for assigning
 * properties to the created object.
 *
 * @private
 * @param {Object} proto The object to inherit from.
 * @returns {Object} Returns the new object.
 */
var baseCreate = (function () {
    function object() { }
    return function (proto) {
        if (!isObject_1(proto)) {
            return {};
        }
        if (objectCreate) {
            return objectCreate(proto);
        }
        object.prototype = proto;
        var result = new object;
        object.prototype = undefined;
        return result;
    };
}());
var _baseCreate = baseCreate;
/**
 * Initializes an object clone.
 *
 * @private
 * @param {Object} object The object to clone.
 * @returns {Object} Returns the initialized clone.
 */
function initCloneObject(object) {
    return (typeof object.constructor == 'function' && !_isPrototype(object))
        ? _baseCreate(_getPrototype(object))
        : {};
}
var _initCloneObject = initCloneObject;
/** `Object#toString` result references. */
var mapTag$3 = '[object Map]';
/**
 * The base implementation of `_.isMap` without Node.js optimizations.
 *
 * @private
 * @param {*} value The value to check.
 * @returns {boolean} Returns `true` if `value` is a map, else `false`.
 */
function baseIsMap(value) {
    return isObjectLike_1(value) && _getTag(value) == mapTag$3;
}
var _baseIsMap = baseIsMap;
/* Node.js helper references. */
var nodeIsMap = _nodeUtil && _nodeUtil.isMap;
/**
 * Checks if `value` is classified as a `Map` object.
 *
 * @static
 * @memberOf _
 * @since 4.3.0
 * @category Lang
 * @param {*} value The value to check.
 * @returns {boolean} Returns `true` if `value` is a map, else `false`.
 * @example
 *
 * _.isMap(new Map);
 * // => true
 *
 * _.isMap(new WeakMap);
 * // => false
 */
var isMap = nodeIsMap ? _baseUnary(nodeIsMap) : _baseIsMap;
var isMap_1 = isMap;
/** `Object#toString` result references. */
var setTag$3 = '[object Set]';
/**
 * The base implementation of `_.isSet` without Node.js optimizations.
 *
 * @private
 * @param {*} value The value to check.
 * @returns {boolean} Returns `true` if `value` is a set, else `false`.
 */
function baseIsSet(value) {
    return isObjectLike_1(value) && _getTag(value) == setTag$3;
}
var _baseIsSet = baseIsSet;
/* Node.js helper references. */
var nodeIsSet = _nodeUtil && _nodeUtil.isSet;
/**
 * Checks if `value` is classified as a `Set` object.
 *
 * @static
 * @memberOf _
 * @since 4.3.0
 * @category Lang
 * @param {*} value The value to check.
 * @returns {boolean} Returns `true` if `value` is a set, else `false`.
 * @example
 *
 * _.isSet(new Set);
 * // => true
 *
 * _.isSet(new WeakSet);
 * // => false
 */
var isSet = nodeIsSet ? _baseUnary(nodeIsSet) : _baseIsSet;
var isSet_1 = isSet;
/** Used to compose bitmasks for cloning. */
var CLONE_DEEP_FLAG = 1, CLONE_FLAT_FLAG = 2, CLONE_SYMBOLS_FLAG = 4;
/** `Object#toString` result references. */
var argsTag$2 = '[object Arguments]', arrayTag$1 = '[object Array]', boolTag$2 = '[object Boolean]', dateTag$2 = '[object Date]', errorTag$1 = '[object Error]', funcTag$2 = '[object Function]', genTag$1 = '[object GeneratorFunction]', mapTag$4 = '[object Map]', numberTag$2 = '[object Number]', objectTag$2 = '[object Object]', regexpTag$2 = '[object RegExp]', setTag$4 = '[object Set]', stringTag$2 = '[object String]', symbolTag$1 = '[object Symbol]', weakMapTag$2 = '[object WeakMap]';
var arrayBufferTag$2 = '[object ArrayBuffer]', dataViewTag$3 = '[object DataView]', float32Tag$2 = '[object Float32Array]', float64Tag$2 = '[object Float64Array]', int8Tag$2 = '[object Int8Array]', int16Tag$2 = '[object Int16Array]', int32Tag$2 = '[object Int32Array]', uint8Tag$2 = '[object Uint8Array]', uint8ClampedTag$2 = '[object Uint8ClampedArray]', uint16Tag$2 = '[object Uint16Array]', uint32Tag$2 = '[object Uint32Array]';
/** Used to identify `toStringTag` values supported by `_.clone`. */
var cloneableTags = {};
cloneableTags[argsTag$2] = cloneableTags[arrayTag$1] =
    cloneableTags[arrayBufferTag$2] = cloneableTags[dataViewTag$3] =
        cloneableTags[boolTag$2] = cloneableTags[dateTag$2] =
            cloneableTags[float32Tag$2] = cloneableTags[float64Tag$2] =
                cloneableTags[int8Tag$2] = cloneableTags[int16Tag$2] =
                    cloneableTags[int32Tag$2] = cloneableTags[mapTag$4] =
                        cloneableTags[numberTag$2] = cloneableTags[objectTag$2] =
                            cloneableTags[regexpTag$2] = cloneableTags[setTag$4] =
                                cloneableTags[stringTag$2] = cloneableTags[symbolTag$1] =
                                    cloneableTags[uint8Tag$2] = cloneableTags[uint8ClampedTag$2] =
                                        cloneableTags[uint16Tag$2] = cloneableTags[uint32Tag$2] = true;
cloneableTags[errorTag$1] = cloneableTags[funcTag$2] =
    cloneableTags[weakMapTag$2] = false;
/**
 * The base implementation of `_.clone` and `_.cloneDeep` which tracks
 * traversed objects.
 *
 * @private
 * @param {*} value The value to clone.
 * @param {boolean} bitmask The bitmask flags.
 *  1 - Deep clone
 *  2 - Flatten inherited properties
 *  4 - Clone symbols
 * @param {Function} [customizer] The function to customize cloning.
 * @param {string} [key] The key of `value`.
 * @param {Object} [object] The parent object of `value`.
 * @param {Object} [stack] Tracks traversed objects and their clone counterparts.
 * @returns {*} Returns the cloned value.
 */
function baseClone(value, bitmask, customizer, key, object, stack) {
    var result, isDeep = bitmask & CLONE_DEEP_FLAG, isFlat = bitmask & CLONE_FLAT_FLAG, isFull = bitmask & CLONE_SYMBOLS_FLAG;
    if (customizer) {
        result = object ? customizer(value, key, object, stack) : customizer(value);
    }
    if (result !== undefined) {
        return result;
    }
    if (!isObject_1(value)) {
        return value;
    }
    var isArr = isArray_1(value);
    if (isArr) {
        result = _initCloneArray(value);
        if (!isDeep) {
            return _copyArray(value, result);
        }
    }
    else {
        var tag = _getTag(value), isFunc = tag == funcTag$2 || tag == genTag$1;
        if (isBuffer_1(value)) {
            return _cloneBuffer(value, isDeep);
        }
        if (tag == objectTag$2 || tag == argsTag$2 || (isFunc && !object)) {
            result = (isFlat || isFunc) ? {} : _initCloneObject(value);
            if (!isDeep) {
                return isFlat
                    ? _copySymbolsIn(value, _baseAssignIn(result, value))
                    : _copySymbols(value, _baseAssign(result, value));
            }
        }
        else {
            if (!cloneableTags[tag]) {
                return object ? value : {};
            }
            result = _initCloneByTag(value, tag, isDeep);
        }
    }
    // Check for circular references and return its corresponding clone.
    stack || (stack = new _Stack);
    var stacked = stack.get(value);
    if (stacked) {
        return stacked;
    }
    stack.set(value, result);
    if (isSet_1(value)) {
        value.forEach(function (subValue) {
            result.add(baseClone(subValue, bitmask, customizer, subValue, value, stack));
        });
    }
    else if (isMap_1(value)) {
        value.forEach(function (subValue, key) {
            result.set(key, baseClone(subValue, bitmask, customizer, key, value, stack));
        });
    }
    var keysFunc = isFull
        ? (isFlat ? _getAllKeysIn : _getAllKeys)
        : (isFlat ? keysIn : keys_1);
    var props = isArr ? undefined : keysFunc(value);
    _arrayEach(props || value, function (subValue, key) {
        if (props) {
            key = subValue;
            subValue = value[key];
        }
        // Recursively populate clone (susceptible to call stack limits).
        _assignValue(result, key, baseClone(subValue, bitmask, customizer, key, value, stack));
    });
    return result;
}
var _baseClone = baseClone;
/** Used to compose bitmasks for cloning. */
var CLONE_DEEP_FLAG$1 = 1, CLONE_SYMBOLS_FLAG$1 = 4;
/**
 * This method is like `_.clone` except that it recursively clones `value`.
 *
 * @static
 * @memberOf _
 * @since 1.0.0
 * @category Lang
 * @param {*} value The value to recursively clone.
 * @returns {*} Returns the deep cloned value.
 * @see _.clone
 * @example
 *
 * var objects = [{ 'a': 1 }, { 'b': 2 }];
 *
 * var deep = _.cloneDeep(objects);
 * console.log(deep[0] === objects[0]);
 * // => false
 */
function cloneDeep(value) {
    return _baseClone(value, CLONE_DEEP_FLAG$1 | CLONE_SYMBOLS_FLAG$1);
}
var cloneDeep_1 = cloneDeep;
/**
 * Gets the timestamp of the number of milliseconds that have elapsed since
 * the Unix epoch (1 January 1970 00:00:00 UTC).
 *
 * @static
 * @memberOf _
 * @since 2.4.0
 * @category Date
 * @returns {number} Returns the timestamp.
 * @example
 *
 * _.defer(function(stamp) {
 *   console.log(_.now() - stamp);
 * }, _.now());
 * // => Logs the number of milliseconds it took for the deferred invocation.
 */
var now = function () {
    return _root.Date.now();
};
var now_1 = now;
/** `Object#toString` result references. */
var symbolTag$2 = '[object Symbol]';
/**
 * Checks if `value` is classified as a `Symbol` primitive or object.
 *
 * @static
 * @memberOf _
 * @since 4.0.0
 * @category Lang
 * @param {*} value The value to check.
 * @returns {boolean} Returns `true` if `value` is a symbol, else `false`.
 * @example
 *
 * _.isSymbol(Symbol.iterator);
 * // => true
 *
 * _.isSymbol('abc');
 * // => false
 */
function isSymbol(value) {
    return typeof value == 'symbol' ||
        (isObjectLike_1(value) && _baseGetTag(value) == symbolTag$2);
}
var isSymbol_1 = isSymbol;
/** Used as references for various `Number` constants. */
var NAN = 0 / 0;
/** Used to match leading and trailing whitespace. */
var reTrim = /^\s+|\s+$/g;
/** Used to detect bad signed hexadecimal string values. */
var reIsBadHex = /^[-+]0x[0-9a-f]+$/i;
/** Used to detect binary string values. */
var reIsBinary = /^0b[01]+$/i;
/** Used to detect octal string values. */
var reIsOctal = /^0o[0-7]+$/i;
/** Built-in method references without a dependency on `root`. */
var freeParseInt = parseInt;
/**
 * Converts `value` to a number.
 *
 * @static
 * @memberOf _
 * @since 4.0.0
 * @category Lang
 * @param {*} value The value to process.
 * @returns {number} Returns the number.
 * @example
 *
 * _.toNumber(3.2);
 * // => 3.2
 *
 * _.toNumber(Number.MIN_VALUE);
 * // => 5e-324
 *
 * _.toNumber(Infinity);
 * // => Infinity
 *
 * _.toNumber('3.2');
 * // => 3.2
 */
function toNumber(value) {
    if (typeof value == 'number') {
        return value;
    }
    if (isSymbol_1(value)) {
        return NAN;
    }
    if (isObject_1(value)) {
        var other = typeof value.valueOf == 'function' ? value.valueOf() : value;
        value = isObject_1(other) ? (other + '') : other;
    }
    if (typeof value != 'string') {
        return value === 0 ? value : +value;
    }
    value = value.replace(reTrim, '');
    var isBinary = reIsBinary.test(value);
    return (isBinary || reIsOctal.test(value))
        ? freeParseInt(value.slice(2), isBinary ? 2 : 8)
        : (reIsBadHex.test(value) ? NAN : +value);
}
var toNumber_1 = toNumber;
/** Error message constants. */
var FUNC_ERROR_TEXT = 'Expected a function';
/* Built-in method references for those with the same name as other `lodash` methods. */
var nativeMax = Math.max, nativeMin = Math.min;
/**
 * Creates a debounced function that delays invoking `func` until after `wait`
 * milliseconds have elapsed since the last time the debounced function was
 * invoked. The debounced function comes with a `cancel` method to cancel
 * delayed `func` invocations and a `flush` method to immediately invoke them.
 * Provide `options` to indicate whether `func` should be invoked on the
 * leading and/or trailing edge of the `wait` timeout. The `func` is invoked
 * with the last arguments provided to the debounced function. Subsequent
 * calls to the debounced function return the result of the last `func`
 * invocation.
 *
 * **Note:** If `leading` and `trailing` options are `true`, `func` is
 * invoked on the trailing edge of the timeout only if the debounced function
 * is invoked more than once during the `wait` timeout.
 *
 * If `wait` is `0` and `leading` is `false`, `func` invocation is deferred
 * until to the next tick, similar to `setTimeout` with a timeout of `0`.
 *
 * See [David Corbacho's article](https://css-tricks.com/debouncing-throttling-explained-examples/)
 * for details over the differences between `_.debounce` and `_.throttle`.
 *
 * @static
 * @memberOf _
 * @since 0.1.0
 * @category Function
 * @param {Function} func The function to debounce.
 * @param {number} [wait=0] The number of milliseconds to delay.
 * @param {Object} [options={}] The options object.
 * @param {boolean} [options.leading=false]
 *  Specify invoking on the leading edge of the timeout.
 * @param {number} [options.maxWait]
 *  The maximum time `func` is allowed to be delayed before it's invoked.
 * @param {boolean} [options.trailing=true]
 *  Specify invoking on the trailing edge of the timeout.
 * @returns {Function} Returns the new debounced function.
 * @example
 *
 * // Avoid costly calculations while the window size is in flux.
 * jQuery(window).on('resize', _.debounce(calculateLayout, 150));
 *
 * // Invoke `sendMail` when clicked, debouncing subsequent calls.
 * jQuery(element).on('click', _.debounce(sendMail, 300, {
 *   'leading': true,
 *   'trailing': false
 * }));
 *
 * // Ensure `batchLog` is invoked once after 1 second of debounced calls.
 * var debounced = _.debounce(batchLog, 250, { 'maxWait': 1000 });
 * var source = new EventSource('/stream');
 * jQuery(source).on('message', debounced);
 *
 * // Cancel the trailing debounced invocation.
 * jQuery(window).on('popstate', debounced.cancel);
 */
function debounce(func, wait, options) {
    var lastArgs, lastThis, maxWait, result, timerId, lastCallTime, lastInvokeTime = 0, leading = false, maxing = false, trailing = true;
    if (typeof func != 'function') {
        throw new TypeError(FUNC_ERROR_TEXT);
    }
    wait = toNumber_1(wait) || 0;
    if (isObject_1(options)) {
        leading = !!options.leading;
        maxing = 'maxWait' in options;
        maxWait = maxing ? nativeMax(toNumber_1(options.maxWait) || 0, wait) : maxWait;
        trailing = 'trailing' in options ? !!options.trailing : trailing;
    }
    function invokeFunc(time) {
        var args = lastArgs, thisArg = lastThis;
        lastArgs = lastThis = undefined;
        lastInvokeTime = time;
        result = func.apply(thisArg, args);
        return result;
    }
    function leadingEdge(time) {
        // Reset any `maxWait` timer.
        lastInvokeTime = time;
        // Start the timer for the trailing edge.
        timerId = setTimeout(timerExpired, wait);
        // Invoke the leading edge.
        return leading ? invokeFunc(time) : result;
    }
    function remainingWait(time) {
        var timeSinceLastCall = time - lastCallTime, timeSinceLastInvoke = time - lastInvokeTime, timeWaiting = wait - timeSinceLastCall;
        return maxing
            ? nativeMin(timeWaiting, maxWait - timeSinceLastInvoke)
            : timeWaiting;
    }
    function shouldInvoke(time) {
        var timeSinceLastCall = time - lastCallTime, timeSinceLastInvoke = time - lastInvokeTime;
        // Either this is the first call, activity has stopped and we're at the
        // trailing edge, the system time has gone backwards and we're treating
        // it as the trailing edge, or we've hit the `maxWait` limit.
        return (lastCallTime === undefined || (timeSinceLastCall >= wait) ||
            (timeSinceLastCall < 0) || (maxing && timeSinceLastInvoke >= maxWait));
    }
    function timerExpired() {
        var time = now_1();
        if (shouldInvoke(time)) {
            return trailingEdge(time);
        }
        // Restart the timer.
        timerId = setTimeout(timerExpired, remainingWait(time));
    }
    function trailingEdge(time) {
        timerId = undefined;
        // Only invoke if we have `lastArgs` which means `func` has been
        // debounced at least once.
        if (trailing && lastArgs) {
            return invokeFunc(time);
        }
        lastArgs = lastThis = undefined;
        return result;
    }
    function cancel() {
        if (timerId !== undefined) {
            clearTimeout(timerId);
        }
        lastInvokeTime = 0;
        lastArgs = lastCallTime = lastThis = timerId = undefined;
    }
    function flush() {
        return timerId === undefined ? result : trailingEdge(now_1());
    }
    function debounced() {
        var time = now_1(), isInvoking = shouldInvoke(time);
        lastArgs = arguments;
        lastThis = this;
        lastCallTime = time;
        if (isInvoking) {
            if (timerId === undefined) {
                return leadingEdge(lastCallTime);
            }
            if (maxing) {
                // Handle invocations in a tight loop.
                clearTimeout(timerId);
                timerId = setTimeout(timerExpired, wait);
                return invokeFunc(lastCallTime);
            }
        }
        if (timerId === undefined) {
            timerId = setTimeout(timerExpired, wait);
        }
        return result;
    }
    debounced.cancel = cancel;
    debounced.flush = flush;
    return debounced;
}
var debounce_1 = debounce;
var RaulSelect = /** @class */ (function () {
    function class_1(hostRef) {
        registerInstance(this, hostRef);
        this.processedOptions = [];
        this.popper = null;
        this.opened = false;
        this.expanded = false;
        this.searchQuery = '';
        /**
         * If `true`, the user cannot interact with the select.
         */
        this.disabled = false;
        /**
         * If `true`, allows multiple selections.
         */
        this.multiple = false;
        /**
         * The name of the control, which is submitted with the form data.
         */
        this.name = "select-" + randomUID();
        /**
         * The text label.
         */
        this.label = null;
        /**
         * The text to display when the select value is empty.
         */
        this.placeholder = 'Select an option';
        /**
         * If `true`, adds a search field at the top of the select menu.
         */
        this.searchable = false;
        /**
         * If `true`, removes select toggle border.
         */
        this.borderless = false;
        /**
         * The value of the select.
         */
        this.value = this.multiple ? [] : '';
        /**
         * Options and groups of the select.
         */
        this.options = [];
        /**
         * Options menu's placement.
         */
        this.menuPlacement = 'bottom-start';
        /**
         * If `true` the menu position will be 'fixed'.
         */
        this.menuPositionFixed = false;
        this.raulSelectOpen = createEvent(this, "raulSelectOpen", 7);
        this.raulSelectClose = createEvent(this, "raulSelectClose", 7);
        this.raulChange = createEvent(this, "raulChange", 7);
    }
    class_1.prototype.optionsChanged = function () {
        this.processOptions();
    };
    class_1.prototype.valueChanged = function () {
        this.validateValue();
    };
    class_1.prototype.componentWillLoad = function () {
        this.processOptions();
        this.validateValue();
        this.handleDebouncedResize = debounce_1(this.handleDebouncedResize, 200);
    };
    class_1.prototype.handleKeyDown = function (e) {
        if (NAVIGATION_KEYS.includes(e.key)) {
            e.preventDefault();
            this.opened ? this.focusOption(e.key) : this.open();
        }
        else if (ESCAPE_KEY.includes(e.key)) {
            this.close();
        }
    };
    class_1.prototype.handleClickAndFocusOutside = function (e) {
        if (this.opened && !this.selectEl.contains(e.target)) {
            this.close();
        }
    };
    class_1.prototype.handleDebouncedResize = function () {
        if (this.opened) {
            isScreen('sm') ? this.createPopper() : this.destroyPopper();
        }
    };
    /**
     * Open the select menu.
     */
    class_1.prototype.open = function () {
        return __awaiter(this, void 0, void 0, function () {
            var _this = this;
            return __generator(this, function (_a) {
                this.opened = true;
                setTimeout(function () {
                    _this.expanded = true;
                    if (isScreen('sm')) {
                        _this.createPopper();
                    }
                    if (_this.searchable) {
                        _this.clearSearch();
                    }
                    _this.raulSelectOpen.emit({ value: _this.value });
                    if (!isScreen('sm'))
                        document.body.classList.add('no-scroll');
                }, 100);
                return [2 /*return*/];
            });
        });
    };
    /**
     * Close the select menu.
     */
    class_1.prototype.close = function () {
        return __awaiter(this, void 0, void 0, function () {
            var transitionDuration;
            var _this = this;
            return __generator(this, function (_a) {
                transitionDuration = parseFloat(window.getComputedStyle(this.selectMenuEl).transitionDuration) * 1000;
                this.expanded = false;
                setTimeout(function () {
                    _this.destroyPopper();
                    _this.opened = false;
                    _this.raulSelectClose.emit({ value: _this.value });
                }, transitionDuration);
                document.body.classList.remove('no-scroll');
                return [2 /*return*/];
            });
        });
    };
    /**
     * Toggle the select menu.
     */
    class_1.prototype.toggle = function () {
        return __awaiter(this, void 0, void 0, function () {
            return __generator(this, function (_a) {
                this.opened ? this.close() : this.open();
                return [2 /*return*/];
            });
        });
    };
    class_1.prototype.hasSlot = function (slotName) {
        return !!this.el.querySelector("[slot=" + slotName + "]");
    };
    class_1.prototype.matchSearchQuery = function (text) {
        return text.toLowerCase().indexOf(this.searchQuery) !== -1;
    };
    class_1.prototype.visibleOptionsAndGroups = function () {
        var _this = this;
        return this.processedOptions.filter(function (option) {
            if (option.options && option.options.length) {
                var someOptionsVisible = option.options.some(function (option) { return _this.matchSearchQuery(option.text); });
                return _this.matchSearchQuery(option.text) || someOptionsVisible;
            }
            else {
                return _this.matchSearchQuery(option.text);
            }
        });
    };
    class_1.prototype.optionsWithoutGroups = function () {
        return this.processedOptions.reduce(function (acc, cv) {
            if (cv.options && cv.options.length) {
                cv.options.forEach(function (option) { return acc.push(option); });
            }
            else {
                acc.push(cv);
            }
            return acc;
        }, []);
    };
    class_1.prototype.focusableOptionsAndGroups = function () {
        var _this = this;
        var flattenedVisibleOptionsAndGroups = this.visibleOptionsAndGroups().reduce(function (acc, cv) {
            if (!cv.disabled) {
                acc.push(cv);
                if (cv.options && cv.options.length) {
                    cv.options.forEach(function (option) {
                        if (!option.disabled && (_this.matchSearchQuery(option.text) || _this.matchSearchQuery(cv.text))) {
                            acc.push(option);
                        }
                    });
                }
            }
            return acc;
        }, []);
        return this.multiple
            ? flattenedVisibleOptionsAndGroups
            : flattenedVisibleOptionsAndGroups.filter(function (option) { return !(option.options && option.options.length); });
    };
    class_1.prototype.selectedOptions = function () {
        var _this = this;
        var isSelected = function (optionValue) {
            return _this.multiple ? _this.value.includes(optionValue) : _this.value === optionValue;
        };
        return this.optionsWithoutGroups().filter(function (option) { return isSelected(option.value); });
    };
    class_1.prototype.selectedOptionsText = function () {
        return this.selectedOptions().length > 1
            ? this.selectedOptions().length + " options selected"
            : this.selectedOptions().map(function (option) { return option.text; }).join();
    };
    class_1.prototype.processOptions = function () {
        this.processedOptions = cloneDeep_1(this.options).map(function (option) {
            option.id = "option-" + randomUID();
            if (option.options && option.options.length) {
                option.options.forEach(function (subOption) {
                    subOption.id = "option-" + randomUID();
                });
            }
            return option;
        });
    };
    class_1.prototype.validateValue = function () {
        var type = this.multiple ? 'array' : 'string';
        var isStringOrArray = this.multiple ? Array.isArray(this.value) : typeof this.value === 'string';
        if (!isStringOrArray) {
            throw new Error("value has to be " + type);
        }
    };
    class_1.prototype.focusOption = function (key) {
        var _this = this;
        var focusedOptionIndex;
        if (document.activeElement === this.searchInputEl || document.activeElement === this.toggleButtonEl) {
            focusedOptionIndex = -1;
        }
        else {
            focusedOptionIndex = this.focusedOptionId
                ? this.focusableOptionsAndGroups().findIndex(function (option) { return option.id === _this.focusedOptionId; })
                : -1;
        }
        var focus = function (key) {
            _this.focusedOptionId = key === 'down'
                ? _this.focusableOptionsAndGroups()[focusedOptionIndex + 1].id
                : _this.focusableOptionsAndGroups()[focusedOptionIndex - 1].id;
            setTimeout(function () { return document.getElementById(_this.focusedOptionId).focus(); });
        };
        if (DOWN_KEY.includes(key) && !(focusedOptionIndex >= this.focusableOptionsAndGroups().length - 1)) {
            focus('down');
        }
        else if (UP_KEY.includes(key) && !(focusedOptionIndex <= 0)) {
            focus('up');
        }
    };
    class_1.prototype.handleOptionChange = function (e, _a) {
        var id = _a.id, selected = _a.selected, value = _a.value;
        e.preventDefault();
        this.focusedOptionId = id;
        if (this.multiple) {
            this.value = selected ? __spreadArrays(this.value, [value]) : this.value.filter(function (optionValue) { return optionValue !== value; });
        }
        else {
            this.value = value;
            this.close();
            this.toggleButtonEl.focus();
        }
        this.raulChange.emit({ value: this.value });
    };
    class_1.prototype.handleOptionsGroupChange = function (e, _a) {
        var id = _a.id, selected = _a.selected, enabledOptionsValues = _a.enabledOptionsValues;
        e.preventDefault();
        this.focusedOptionId = id;
        this.value = selected
            ? __spreadArrays(this.value, enabledOptionsValues) : this.value.filter(function (option) { return !enabledOptionsValues.includes(option); });
        this.raulChange.emit({ value: this.value });
    };
    class_1.prototype.handleSearch = function (e) {
        this.searchQuery = e.target.value.toLowerCase();
    };
    class_1.prototype.createPopper = function () {
        if (!this.popper) {
            this.popper = new Popper(this.toggleButtonEl, this.selectMenuEl, {
                placement: this.menuPlacement,
                positionFixed: this.menuPositionFixed
            });
        }
    };
    class_1.prototype.destroyPopper = function () {
        if (this.popper) {
            this.popper.destroy();
            this.popper = null;
        }
    };
    class_1.prototype.clearSearch = function () {
        var _this = this;
        this.searchQuery = '';
        setTimeout(function () { return _this.searchInputEl.focus(); }, 0);
    };
    class_1.prototype.render = function () {
        var _this = this;
        var renderHiddenInput = function () {
            if (_this.multiple) {
                return _this.value && _this.value.length
                    ? _this.value.map(function (value) { return h("input", { type: "hidden", name: _this.name, value: value }); })
                    : h("input", { type: "hidden", name: _this.name, value: _this.value });
            }
            else {
                return h("input", { type: "hidden", name: _this.name, value: _this.value });
            }
        };
        var renderSelectToggleContent = function () {
            if (!_this.hasSlot('select-toggle')) {
                return (h("div", { class: {
                        'r-select__toggle__content': true,
                        'r-select__toggle__content--borderless': _this.borderless,
                    } }, h("div", { class: "r-select__toggle__content__text" }, _this.selectedOptionsText() || _this.placeholder), h("raul-icon", { icon: "arrow-down-v", class: "r-select__toggle__content__arrow" })));
            }
        };
        var renderSelectMenuFooter = function () {
            if (_this.hasSlot('select-menu-footer')) {
                return (h("div", { class: "r-select__menu__footer" }, h("slot", { name: "select-menu-footer" })));
            }
        };
        var renderSelectMenuMobileLabelContent = function () {
            if (!_this.hasSlot('select-mobile-label')) {
                return (h("div", { class: "r-select__mobile-label__content" }, h("div", { class: "r-select__mobile-label__content__button" }, h("span", { onClick: function () { return _this.close(); } }, "Cancel")), _this.label &&
                    h("div", { class: "r-select__mobile-label__content__text" }, _this.label), _this.multiple &&
                    h("div", { class: {
                            'r-select__mobile-label__content__button': true,
                            'r-select__mobile-label__content__button--right': true,
                            'r-select__mobile-label__content__button--disabled': false
                        } }, h("span", { onClick: function () { return _this.close(); } }, "Done"))));
            }
        };
        var renderSelectMenuSearch = function () {
            if (_this.searchable) {
                return (h("div", { class: "r-select__search" }, h("raul-icon", { icon: "search", class: "r-select__search__icon" }), h("input", { type: "text", class: "r-select__search__input", placeholder: "Search", value: _this.searchQuery, onInput: function (e) { return _this.handleSearch(e); }, ref: function (el) { return _this.searchInputEl = el; } }), _this.searchQuery &&
                    h("raul-icon", { icon: "remove-2", class: "r-select__search__icon-reset", onClick: function () { return _this.clearSearch(); } })));
            }
        };
        var renderNoSearchResults = function () {
            if (_this.processedOptions.length && !_this.visibleOptionsAndGroups().length) {
                return (h("div", { class: "r-select__no-search-results" }, "No results found. Try changing your search criteria."));
            }
        };
        var renderSelectMenuHeader = function () {
            return (h("div", { class: "r-select__menu__header" }, h("div", { class: "r-select__mobile-label" }, renderSelectMenuMobileLabelContent(), h("slot", { name: "select-mobile-label" })), renderSelectMenuSearch(), renderNoSearchResults()));
        };
        var renderOption = function (option, groupVisible, groupDisabled) {
            if (groupVisible === void 0) { groupVisible = false; }
            if (groupDisabled === void 0) { groupDisabled = false; }
            var id = option.id, text = option.text, description = option.description, disabled = option.disabled, icon = option.icon, iconKind = option.iconKind, variant = option.variant, value = option.value;
            var visible = _this.matchSearchQuery(text) || groupVisible;
            var focused = id === _this.focusedOptionId;
            var selected = _this.multiple ? _this.value.includes(value) : _this.value === value;
            var isDisabled = disabled || groupDisabled;
            if (visible) {
                return (h("raul-option", { multiple: _this.multiple, selected: selected, focused: focused, disabled: isDisabled, value: value, text: text, description: description, icon: icon, iconKind: iconKind, optionId: id, variant: variant, onClick: function (e) { return _this.handleOptionChange(e, { id: id, selected: _this.multiple ? !selected : true, value: value }); }, onKeyDown: function (e) { return e.key === 'Enter'
                        ? _this.handleOptionChange(e, { id: id, selected: _this.multiple ? !selected : true, value: value })
                        : null; } }));
            }
        };
        var renderOptionsGroup = function (group) {
            var id = group.id, text = group.text, description = group.description, disabled = group.disabled, value = group.value, options = group.options;
            var optionsValues = options.map(function (option) { return option.value; });
            var enabledOptionsValues = options.filter(function (option) { return !option.disabled; }).map(function (option) { return option.value; });
            var groupVisible = _this.matchSearchQuery(text);
            var focused = id === _this.focusedOptionId;
            var selected = _this.multiple ? optionsValues.every(function (option) { return _this.value.includes(option); }) : false;
            var indeterminate = _this.multiple
                ? !selected && optionsValues.some(function (option) { return _this.value.includes(option); })
                : false;
            var enabledOptionsSelected = _this.multiple
                ? enabledOptionsValues.every(function (option) { return _this.value.includes(option); })
                : false;
            return (h("div", { class: "r-options-group", role: "group" }, h("raul-option", { variant: "group", multiple: _this.multiple, selected: selected, focused: focused, disabled: disabled, value: value, text: text, description: description, indeterminate: indeterminate, optionId: id, onClick: _this.multiple
                    ? function (e) { return _this.handleOptionsGroupChange(e, { id: id, selected: !enabledOptionsSelected, enabledOptionsValues: enabledOptionsValues }); }
                    : null, onKeyDown: function (e) { return _this.multiple && e.key === 'Enter'
                    ? _this.handleOptionsGroupChange(e, { id: id, selected: !enabledOptionsSelected, enabledOptionsValues: enabledOptionsValues })
                    : null; } }), options.map(function (option) { return renderOption(option, groupVisible, disabled); })));
        };
        return [
            renderHiddenInput(),
            this.label && h("label", { class: "r-select__label" }, this.label),
            h("div", { class: {
                    'r-select': true,
                    'r-select--opened': this.opened,
                    'r-select--expanded': this.expanded,
                    'r-select--invalid': !!this.error,
                }, ref: function (el) { return _this.selectEl = el; } }, h("button", { type: "button", class: "r-select__toggle", disabled: this.disabled, "aria-haspopup": "true", "aria-expanded": this.opened ? 'true' : 'false', "aria-activedescendant": this.focusedOptionId, onClick: function () { return _this.toggle(); }, ref: function (el) { return _this.toggleButtonEl = el; } }, renderSelectToggleContent(), h("slot", { name: "select-toggle" })), this.hint && h("small", { class: "r-select__hint" }, this.hint), this.error &&
                h("div", { class: "r-select__error" }, h("raul-icon", { icon: "interface-alert-diamond", class: "r-select__error__icon" }), " ", this.error), h("div", { class: "r-select__menu", ref: function (el) { return _this.selectMenuEl = el; } }, h("div", { class: "r-select__menu__content" }, renderSelectMenuHeader(), h("div", { class: "r-select__menu__body", role: "listbox", tabindex: "-1", "aria-multiselectable": this.multiple ? 'true' : null, style: { maxHeight: this.optionsMaxHeight } }, this.visibleOptionsAndGroups().map(function (option) {
                return option.options && option.options.length
                    ? renderOptionsGroup(option)
                    : renderOption(option);
            })), renderSelectMenuFooter())))
        ];
    };
    Object.defineProperty(class_1.prototype, "el", {
        get: function () { return getElement(this); },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(class_1, "watchers", {
        get: function () {
            return {
                "options": ["optionsChanged"],
                "value": ["valueChanged"]
            };
        },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(class_1, "style", {
        get: function () { return "raul-select{display:block}raul-select .r-select{position:relative}raul-select .r-select__label{color:#37474f}raul-select .r-select__toggle{display:block;background-color:transparent;border-width:0;cursor:pointer;padding:0;text-align:left;outline:0;width:100%}raul-select .r-select__toggle__content{border-width:1px;border-color:#c6ccd0;display:-ms-flexbox;display:flex;-ms-flex-align:center;align-items:center;font-size:.875rem;height:2.5rem;padding-left:.75rem;padding-right:.75rem;line-height:1.25;background-color:var(--raul-select-bg,#fff);border-radius:var(--raul-select-border-radius,.125rem);color:var(--raul-select-color,#37474f)}raul-select .r-select__toggle__content__text{overflow:hidden;text-overflow:ellipsis;white-space:nowrap;-ms-flex-positive:1;flex-grow:1}raul-select .r-select__toggle__content__arrow{font-size:1rem;color:#c6ccd0;margin-left:.5rem}raul-select .r-select__toggle__content--borderless{border-color:transparent}raul-select .r-select__toggle:focus{outline:0}raul-select .r-select__toggle:focus .r-select__toggle__content{border-color:#0076cc}raul-select .r-select__toggle:disabled .r-select__toggle__content{background-color:#ebedee;border-color:#ebedee;color:#9ba3a7;cursor:not-allowed}raul-select .r-select__label{display:inline-block;font-weight:500;font-size:.75rem;margin-bottom:.25rem}raul-select .r-select__hint{display:block;font-size:.75rem;margin-top:.5rem}raul-select .r-select__error{display:-ms-flexbox;display:flex;font-size:.75rem;-ms-flex-align:center;align-items:center;color:#d01a1f;margin-top:.5rem}raul-select .r-select__error__icon{font-size:1rem;margin-right:.25rem}raul-select .r-select__menu{-ms-flex-direction:row;flex-direction:row;border-top-width:1px;border-bottom-width:1px;border-color:#ebedee;-webkit-box-shadow:0 8px 16px 0 rgba(82,97,115,.18);box-shadow:0 8px 16px 0 rgba(82,97,115,.18);position:absolute;left:0;top:100%;min-width:10rem;width:100%;margin-top:.5rem;margin-bottom:.5rem;display:none;opacity:0;background-color:var(--raul-select-menu-bg,#fff);border-radius:var(--raul-select-menu-border-radius,.125rem);-webkit-transition:opacity .2s ease,scale .2s linear;transition:opacity .2s ease,scale .2s linear;z-index:100000}raul-select .r-select__menu__content{display:-ms-flexbox;display:flex;-ms-flex-direction:column;flex-direction:column;max-height:20rem;width:100%}\@media (max-width:640px){raul-select .r-select__menu__content{max-height:none}}raul-select .r-select__menu__body{-ms-flex-positive:1;flex-grow:1;overflow:auto}\@media (max-width:640px){raul-select .r-select__menu{top:0;right:0;bottom:0;left:0;-webkit-box-shadow:none;box-shadow:none;margin:0;min-width:0;position:fixed;border-width:0;border-radius:0;-webkit-transform:scale(0);transform:scale(0)}}raul-select .r-select__mobile-label{display:none}\@media (max-width:640px){raul-select .r-select__mobile-label{display:block}raul-select .r-select__mobile-label__content{-ms-flex-align:center;align-items:center;border-bottom-width:1px;border-color:#ebedee;background-color:#f7f8f9;display:-ms-flexbox;display:flex;font-size:.875rem;height:2.5rem}raul-select .r-select__mobile-label__content__button,raul-select .r-select__mobile-label__content__text{width:33.333333%}raul-select .r-select__mobile-label__content__text{overflow:hidden;text-overflow:ellipsis;white-space:nowrap;color:#37474f;font-weight:500;text-align:center}raul-select .r-select__mobile-label__content__button{color:#0076cc;padding-left:.75rem;padding-right:.75rem}raul-select .r-select__mobile-label__content__button--disabled{color:#9ba3a7}raul-select .r-select__mobile-label__content__button--right{margin-left:auto;text-align:right}}raul-select .r-select__search{color:#37474f;font-size:.75rem;position:relative}raul-select .r-select__search__input{border-width:1px;border-top-width:0;border-color:#ebedee;height:2.5rem;padding-left:2rem;padding-right:.75rem;width:100%}raul-select .r-select__search__input:focus{outline:1px solid #0076cc;outline-offset:-1px}raul-select .r-select__search__input::-webkit-input-placeholder{color:#9ba3a7}raul-select .r-select__search__input::-moz-placeholder{color:#9ba3a7}raul-select .r-select__search__input:-ms-input-placeholder{color:#9ba3a7}raul-select .r-select__search__input::-ms-input-placeholder{color:#9ba3a7}raul-select .r-select__search__input::placeholder{color:#9ba3a7}\@media (max-width:640px){raul-select .r-select__search__input{border-left-width:0;border-right-width:0}}raul-select .r-select__search__icon{color:#9ba3a7;left:.75rem}raul-select .r-select__search__icon,raul-select .r-select__search__icon-reset{position:absolute;top:50%;-webkit-transform:translateY(-50%);transform:translateY(-50%)}raul-select .r-select__search__icon-reset{cursor:pointer;right:.75rem}raul-select .r-select__no-search-results{display:-ms-flexbox;display:flex;-ms-flex-align:center;align-items:center;font-size:.75rem;color:#37474f;-ms-flex-pack:center;justify-content:center;padding-top:.5rem;padding-bottom:.5rem;padding-left:1rem;padding-right:1rem;min-height:.75rem}raul-select .r-select--opened .r-select__menu{display:-ms-flexbox;display:flex}raul-select .r-select--expanded .r-select__menu{opacity:1}\@media (max-width:640px){raul-select .r-select--expanded .r-select__menu{-webkit-transform:scale(1);transform:scale(1)}}raul-select .r-select--invalid .r-select__toggle__content{border-color:#d01a1f}raul-select .r-option{border-width:1px;border-color:#ebedee;border-top-width:0;font-size:.75rem;cursor:pointer;-ms-flex-wrap:wrap;flex-wrap:wrap;padding-left:1rem;padding-right:1rem;padding-top:.5rem;padding-bottom:.5rem;white-space:nowrap;line-height:1.25;min-height:2.5rem}raul-select .r-option,raul-select .r-option__icon{display:-ms-flexbox;display:flex;-ms-flex-align:center;align-items:center;-ms-flex-pack:center;justify-content:center}raul-select .r-option__icon{background-color:#ebedee;border-radius:9999px;color:#9ba3a7;font-size:1rem;height:2rem;width:2rem;margin-right:1rem}raul-select .r-option__icon raul-icon{display:-ms-inline-flexbox;display:inline-flex}raul-select .r-option__text{color:#37474f;-ms-flex:1 1 0%;flex:1 1 0%;padding-right:.75rem}raul-select .r-option__text,raul-select .r-option__text__description,raul-select .r-option__text__title{overflow:hidden;text-overflow:ellipsis;white-space:nowrap}raul-select .r-option__text__description{margin-top:.25rem}raul-select .r-option__check-mark{color:#0076cc;font-size:.875rem;opacity:0}raul-select .r-option:not(.r-option--group):hover{background-color:#f7f8f9}body[modality=keyboard] raul-select .r-option:focus{outline:1px solid #0076cc;outline-offset:-1px}raul-select .r-option--selected .r-option__check-mark{opacity:1}raul-select .r-option--disabled{pointer-events:none}raul-select .r-option--disabled .r-option__check-mark,raul-select .r-option--disabled .r-option__text{color:#9ba3a7}raul-select .r-option--group,raul-select .r-option--heading{background-color:transparent;padding-top:1.5rem;padding-bottom:1rem}raul-select .r-option--group .r-option__text__title,raul-select .r-option--heading .r-option__text__title{font-weight:600}raul-select .r-option--group:not(.r-option--multiple){cursor:auto}raul-select .r-option--sub-option{padding-left:1.5rem}raul-select .r-option--with-description .r-option__text__title{font-weight:600}\@media (max-width:640px){raul-select .r-option{border-left-width:0;border-right-width:0}}"; },
        enumerable: true,
        configurable: true
    });
    return class_1;
}());
var RaulSimpleTable = /** @class */ (function () {
    function RaulSimpleTable(hostRef) {
        registerInstance(this, hostRef);
    }
    RaulSimpleTable.prototype.render = function () {
        return (h("slot", null));
    };
    Object.defineProperty(RaulSimpleTable, "style", {
        get: function () { return "raul-simple-table{display:block}raul-simple-table table{width:100%;background-color:#fff;border-collapse:collapse;font-size:.875rem;color:#37474f}raul-simple-table table td,raul-simple-table table th{border-bottom-width:1px;border-color:#ebedee;padding-top:.75rem;padding-bottom:.75rem;padding-left:.5rem;padding-right:.5rem}raul-simple-table table th{text-align:left;font-size:.75rem;font-weight:700;background-color:#f7f8f9}raul-simple-table[small] table tbody td{font-size:.75rem}raul-simple-table[striped] table tbody tr:nth-child(2n){background-color:#f7f8f9}raul-simple-table[hoverable] table tbody tr:hover{background-color:#e5f4ff}"; },
        enumerable: true,
        configurable: true
    });
    return RaulSimpleTable;
}());
var RaulTabs = /** @class */ (function () {
    function RaulTabs(hostRef) {
        registerInstance(this, hostRef);
        /**
         * An array of objects representing the navigation.
         */
        this.tabs = [];
        this.raulTabChange = createEvent(this, "raulTabChange", 7);
    }
    RaulTabs.prototype.connectedCallback = function () {
        this.xsDevice = window.innerWidth < 640;
    };
    RaulTabs.prototype.handleResize = function () {
        this.xsDevice = window.innerWidth < 640;
    };
    RaulTabs.prototype.selectOptions = function () {
        return this.tabs.reduce(function (acc, cv) {
            acc = __spreadArrays(acc, [{ value: cv.name, text: cv.label }]);
            return acc;
        }, []);
    };
    RaulTabs.prototype.handleClick = function (tabName) {
        this.raulTabChange.emit(tabName);
    };
    RaulTabs.prototype.handleRaulChange = function (e) {
        this.raulTabChange.emit(e.detail.value);
    };
    RaulTabs.prototype.render = function () {
        var _this = this;
        if (!(this.selectOnMobile && this.xsDevice)) {
            return (h("div", { class: "tabs", role: "tablist" }, h("div", { class: "tabs__list" }, this.tabs.map(function (item) {
                return (h("button", { role: "tab", class: {
                        'tabs__item': true,
                        'tabs__item--active': item.name === _this.activeTab,
                        'tabs__item--disabled': item.disabled
                    }, id: item.name + "-tab", disabled: item.disabled, "aria-controls": item.id || item.name, "aria-selected": item.name === _this.activeTab, onClick: function () { return _this.handleClick(item.name); } }, item.label));
            }))));
        }
        else {
            return (h("raul-select", { options: this.selectOptions(), value: this.activeTab, onRaulChange: function (e) { return _this.handleRaulChange(e); } }));
        }
    };
    Object.defineProperty(RaulTabs, "style", {
        get: function () { return "raul-tabs{display:block}raul-tabs .tabs{border-bottom-width:1px;border-color:#ebedee;height:2.5rem;overflow:hidden}raul-tabs .tabs__list{display:-ms-flexbox;display:flex;overflow-x:auto;height:4rem}raul-tabs .tabs__item{-ms-flex:none;flex:none;font-size:.875rem;font-weight:600;border-bottom-width:2px;border-color:transparent;height:2.5rem;padding-left:1rem;padding-right:1rem;margin-top:-1px}raul-tabs .tabs__item:focus{outline:0}raul-tabs .tabs__item:focus,raul-tabs .tabs__item:hover{border-color:#c6ccd0}raul-tabs .tabs__item.tabs__item--active{border-color:#0076cc}raul-tabs .tabs__item.tabs__item--disabled{color:#9ba3a7;pointer-events:none}"; },
        enumerable: true,
        configurable: true
    });
    return RaulTabs;
}());
export { DocsElement as docs_element, DocsInterface as docs_interface, RaulCheckbox as raul_checkbox, RaulOption as raul_option, RaulSelect as raul_select, RaulSimpleTable as raul_simple_table, RaulTabs as raul_tabs };
