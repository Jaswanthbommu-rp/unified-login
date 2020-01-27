'use strict';

Object.defineProperty(exports, '__esModule', { value: true });

const core = require('./core-40bc5f02.js');
const MenuItem = require('./MenuItem-52346884.js');

const RaulDropdownMenu = class {
    constructor(hostRef) {
        core.registerInstance(this, hostRef);
        this.open = false;
        this.top = false;
        this.right = false;
        this.openMutated = false;
        this.dividers = false;
        this.emphasizeFinal = false;
        this.color = 'primary';
        this.disabled = false;
        this.handleToggleClick = () => {
            this.open = !this.open;
        };
        this.handleKeyDown = e => {
            if (e.key === 'Enter')
                this.open = !this.open;
        };
        this.handleMenuItemClick = payload => {
            this.optionSelected.emit(payload);
        };
        this.handleMenuItemBlur = (e) => {
            requestAnimationFrame(() => {
                let length = this.dropdown.querySelectorAll('.r-dropdown-menu__menu-item').length;
                if ((e.target === this.dropdown.querySelectorAll('.r-dropdown-menu__menu-item')[length - 1])
                    &&
                        (document.activeElement !== this.dropdown.querySelectorAll('.r-dropdown-menu__menu-item')[length - 2])) {
                    this.open = false;
                }
            });
        };
        this.handleEscape = e => {
            if (['Escape', 'Esc'].includes(e.key)) {
                this.open = false;
                this.toggle.focus();
            }
        };
        this.optionSelected = core.createEvent(this, "optionSelected", 7);
    }
    componentDidLoad() {
        window.addEventListener('click', (e) => {
            // @ts-ignore
            if (this.open && !this.dropdown.contains(e.target) && !this.toggle.contains(e.target)) {
                this.open = false;
            }
        }, true); // third argument is capture
    }
    handleOpenChange(newValue, oldValue) {
        if (newValue && !oldValue) {
            this.openMutated = true;
        }
        if (!newValue) {
            this.top = false;
            this.right = false;
        }
    }
    componentDidRender() {
        if (this.open && this.openMutated) {
            this.checkViewportCollision();
        }
        this.openMutated = false;
    }
    checkViewportCollision() {
        const rect = this.dropdown.getBoundingClientRect();
        this.top = rect.bottom > window.innerHeight;
        this.right = rect.right < rect.width;
    }
    async closeMenu() {
        this.open = false;
    }
    render() {
        return (core.h("div", { class: {
                'r-dropdown-menu': true,
                'r-dropdown-menu--dividers': this.dividers,
                'r-dropdown-menu--emphasize-final': this.emphasizeFinal,
                'r-dropdown-menu--color-active': this.color === 'active',
                'r-dropdown-menu--show': this.open
            }, onKeyDown: this.handleEscape }, core.h("div", { onClick: this.handleToggleClick, class: 'r-dropdown-menu__toggle', role: "button", tabIndex: 0, onKeyDown: this.handleKeyDown, ref: el => this.toggle = el }, core.h("div", { class: 'r-dropdown-menu__toggle__focus-utility', tabindex: -1 }, core.h("slot", { name: "toggle" }))), core.h("div", { ref: el => this.dropdown = el, class: {
                'r-dropdown-menu__dropdown': true,
                'r-dropdown-menu__dropdown--show': this.open,
                'r-dropdown-menu__dropdown--default': !this.top && !this.right,
                'r-dropdown-menu__dropdown--top': this.top,
                'r-dropdown-menu__dropdown--right': this.right
            } }, this.items && this.items.map(item => core.h(MenuItem.MenuItem, Object.assign({}, item, { onBlurCallback: this.handleMenuItemBlur, onClickCallback: this.handleMenuItemClick, disabled: this.disabled }))), core.h("slot", null))));
    }
    static get watchers() { return {
        "open": ["handleOpenChange"]
    }; }
    static get style() { return "raul-dropdown-menu .r-dropdown-menu{opacity:1;-webkit-user-select:none;-moz-user-select:none;-ms-user-select:none;user-select:none;position:relative;display:inline-block}raul-dropdown-menu .r-dropdown-menu .r-dropdown-menu__toggle{position:relative;height:100%;width:100%;display:-ms-flexbox;display:flex;-ms-flex-pack:center;justify-content:center;-ms-flex-align:center;align-items:center;cursor:pointer}raul-dropdown-menu .r-dropdown-menu .r-dropdown-menu__toggle:focus{outline:none}raul-dropdown-menu .r-dropdown-menu .r-dropdown-menu__toggle:focus .r-dropdown-menu__toggle__focus-utility{outline:1px solid #0076cc}raul-dropdown-menu .r-dropdown-menu .r-dropdown-menu__toggle .r-dropdown-menu__toggle__focus-utility{width:100%;height:100%;display:-ms-flexbox;display:flex;-ms-flex-pack:center;justify-content:center;-ms-flex-align:center;align-items:center}raul-dropdown-menu .r-dropdown-menu .r-dropdown-menu__toggle .r-dropdown-menu__toggle__focus-utility:focus{outline:none}raul-dropdown-menu .r-dropdown-menu .r-dropdown-menu__dropdown{position:absolute;display:none;border-radius:.125rem;-webkit-box-shadow:0 8px 16px 0 rgba(82,97,115,.18);box-shadow:0 8px 16px 0 rgba(82,97,115,.18);padding-top:.5rem;padding-bottom:.5rem;background-color:#fff;max-width:180px;z-index:2000}raul-dropdown-menu .r-dropdown-menu .r-dropdown-menu__dropdown--show{display:-ms-flexbox;display:flex;-ms-flex-direction:column;flex-direction:column}raul-dropdown-menu .r-dropdown-menu .r-dropdown-menu__dropdown--default{top:32px;right:0;left:auto}raul-dropdown-menu .r-dropdown-menu .r-dropdown-menu__dropdown--top{bottom:32px;right:0;left:auto}raul-dropdown-menu .r-dropdown-menu .r-dropdown-menu__dropdown--right{left:0;right:auto}raul-dropdown-menu .r-dropdown-menu .r-dropdown-menu__dropdown .r-dropdown-menu__menu-item{color:#37474f;cursor:pointer;-webkit-user-select:none;-moz-user-select:none;-ms-user-select:none;user-select:none;text-decoration:none;min-height:32px;display:-ms-flexbox;display:flex;-ms-flex-direction:column;flex-direction:column;-ms-flex-pack:space-evenly;justify-content:space-evenly;-ms-flex-align:start;align-items:flex-start;opacity:1!important}raul-dropdown-menu .r-dropdown-menu .r-dropdown-menu__dropdown .r-dropdown-menu__menu-item .r-dropdown-menu-item__focus-utility{border-width:1px;border-color:transparent;width:100%;min-height:32px;display:-ms-flexbox;display:flex;-ms-flex-direction:column;flex-direction:column;-ms-flex-pack:distribute;justify-content:space-around}raul-dropdown-menu .r-dropdown-menu .r-dropdown-menu__dropdown .r-dropdown-menu__menu-item .r-dropdown-menu-item__focus-utility:focus{outline:none}raul-dropdown-menu .r-dropdown-menu .r-dropdown-menu__dropdown .r-dropdown-menu__menu-item .r-dropdown-menu-item__focus-utility .r-dropdown-menu__menu-item__label{color:#37474f;opacity:1!important}raul-dropdown-menu .r-dropdown-menu .r-dropdown-menu__dropdown .r-dropdown-menu__menu-item .r-dropdown-menu-item__focus-utility .r-dropdown-menu__menu-item__label:hover{text-decoration:underline}raul-dropdown-menu .r-dropdown-menu .r-dropdown-menu__dropdown .r-dropdown-menu__menu-item:focus{outline:none}raul-dropdown-menu .r-dropdown-menu .r-dropdown-menu__dropdown .r-dropdown-menu__menu-item:focus .r-dropdown-menu-item__focus-utility{border-width:1px;border-color:#0076cc}raul-dropdown-menu .r-dropdown-menu .r-dropdown-menu__dropdown .r-dropdown-menu__menu-item .r-dropdown-menu__menu-item__container{color:#37474f;width:100%;padding:0 8px;display:-ms-flexbox;display:flex;-ms-flex-direction:row;flex-direction:row;-ms-flex-pack:start;justify-content:flex-start;-ms-flex-align:center;align-items:center}raul-dropdown-menu .r-dropdown-menu .r-dropdown-menu__dropdown .r-dropdown-menu__menu-item .r-dropdown-menu__menu-item__container .r-dropdown-menu__menu-item__label{padding:0 8px;line-height:18px!important;font-size:12px!important;text-transform:capitalize;white-space:nowrap;overflow:hidden;text-overflow:ellipsis}raul-dropdown-menu .r-dropdown-menu .r-dropdown-menu__dropdown .r-dropdown-menu__menu-item--disabled{pointer-events:none}raul-dropdown-menu .r-dropdown-menu .r-dropdown-menu__dropdown .r-dropdown-menu__menu-item--disabled .r-dropdown-menu__menu-item__label{color:#9ba3a7!important}raul-dropdown-menu .r-dropdown-menu .r-dropdown-menu__dropdown .r-dropdown-menu__menu-item--disabled:focus{outline:none}raul-dropdown-menu .r-dropdown-menu--color-active .r-dropdown-menu__menu-item__container{color:#0076cc}raul-dropdown-menu .r-dropdown-menu--emphasize-final raul-action-menu-item:last-child .r-dropdown-menu__menu-item:before{border-top-width:1px;border-color:#f7f8f9;content:\"\";display:block;width:100%;margin-top:8px;margin-bottom:8px}raul-dropdown-menu .r-dropdown-menu--dividers .r-dropdown-menu__dropdown{padding:0}raul-dropdown-menu .r-dropdown-menu--dividers .r-dropdown-menu__dropdown .r-dropdown-menu__menu-item{border-bottom-width:1px;border-color:#f7f8f9}raul-dropdown-menu .r-dropdown-menu--dividers .r-dropdown-menu__dropdown .r-dropdown-menu__menu-item:last-child{border-bottom:none}"; }
};

exports.raul_dropdown_menu = RaulDropdownMenu;
