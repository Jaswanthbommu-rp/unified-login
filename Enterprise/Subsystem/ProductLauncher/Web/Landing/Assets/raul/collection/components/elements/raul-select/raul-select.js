import { h } from "@stencil/core";
import { randomUID } from '../../../utils/string';
import { UP_KEY, DOWN_KEY, NAVIGATION_KEYS, ESCAPE_KEY } from '../../../utils/constants';
import { isScreen } from '../../../utils/dom';
import cloneDeep from 'lodash/cloneDeep';
import debounce from 'lodash/debounce';
import Popper from 'popper.js';
export class RaulSelect {
    constructor() {
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
        this.name = `select-${randomUID()}`;
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
    }
    optionsChanged() {
        this.processOptions();
    }
    valueChanged() {
        this.validateValue();
    }
    componentWillLoad() {
        this.processOptions();
        this.validateValue();
        this.handleDebouncedResize = debounce(this.handleDebouncedResize, 200);
    }
    handleKeyDown(e) {
        if (NAVIGATION_KEYS.includes(e.key)) {
            e.preventDefault();
            this.opened ? this.focusOption(e.key) : this.open();
        }
        else if (ESCAPE_KEY.includes(e.key)) {
            this.close();
        }
    }
    handleClickAndFocusOutside(e) {
        if (this.opened && !this.selectEl.contains(e.target)) {
            this.close();
        }
    }
    handleDebouncedResize() {
        if (this.opened) {
            isScreen('sm') ? this.createPopper() : this.destroyPopper();
        }
    }
    /**
     * Open the select menu.
     */
    async open() {
        this.opened = true;
        setTimeout(() => {
            this.expanded = true;
            if (isScreen('sm')) {
                this.createPopper();
            }
            if (this.searchable) {
                this.clearSearch();
            }
            this.raulSelectOpen.emit({ value: this.value });
            if (!isScreen('sm'))
                document.body.classList.add('no-scroll');
        }, 100);
    }
    /**
     * Close the select menu.
     */
    async close() {
        const transitionDuration = parseFloat(window.getComputedStyle(this.selectMenuEl).transitionDuration) * 1000;
        this.expanded = false;
        setTimeout(() => {
            this.destroyPopper();
            this.opened = false;
            this.raulSelectClose.emit({ value: this.value });
        }, transitionDuration);
        document.body.classList.remove('no-scroll');
    }
    /**
     * Toggle the select menu.
     */
    async toggle() {
        this.opened ? this.close() : this.open();
    }
    hasSlot(slotName) {
        return !!this.el.querySelector(`[slot=${slotName}]`);
    }
    matchSearchQuery(text) {
        return text.toLowerCase().indexOf(this.searchQuery) !== -1;
    }
    visibleOptionsAndGroups() {
        return this.processedOptions.filter((option) => {
            if (option.options && option.options.length) {
                const someOptionsVisible = option.options.some((option) => this.matchSearchQuery(option.text));
                return this.matchSearchQuery(option.text) || someOptionsVisible;
            }
            else {
                return this.matchSearchQuery(option.text);
            }
        });
    }
    optionsWithoutGroups() {
        return this.processedOptions.reduce((acc, cv) => {
            if (cv.options && cv.options.length) {
                cv.options.forEach(option => acc.push(option));
            }
            else {
                acc.push(cv);
            }
            return acc;
        }, []);
    }
    focusableOptionsAndGroups() {
        const flattenedVisibleOptionsAndGroups = this.visibleOptionsAndGroups().reduce((acc, cv) => {
            if (!cv.disabled) {
                acc.push(cv);
                if (cv.options && cv.options.length) {
                    cv.options.forEach(option => {
                        if (!option.disabled && (this.matchSearchQuery(option.text) || this.matchSearchQuery(cv.text))) {
                            acc.push(option);
                        }
                    });
                }
            }
            return acc;
        }, []);
        return this.multiple
            ? flattenedVisibleOptionsAndGroups
            : flattenedVisibleOptionsAndGroups.filter(option => !(option.options && option.options.length));
    }
    selectedOptions() {
        const isSelected = (optionValue) => {
            return this.multiple ? this.value.includes(optionValue) : this.value === optionValue;
        };
        return this.optionsWithoutGroups().filter((option) => isSelected(option.value));
    }
    selectedOptionsText() {
        return this.selectedOptions().length > 1
            ? `${this.selectedOptions().length} options selected`
            : this.selectedOptions().map(option => option.text).join();
    }
    processOptions() {
        this.processedOptions = cloneDeep(this.options).map((option) => {
            option.id = `option-${randomUID()}`;
            if (option.options && option.options.length) {
                option.options.forEach((subOption) => {
                    subOption.id = `option-${randomUID()}`;
                });
            }
            return option;
        });
    }
    validateValue() {
        const type = this.multiple ? 'array' : 'string';
        const isStringOrArray = this.multiple ? Array.isArray(this.value) : typeof this.value === 'string';
        if (!isStringOrArray) {
            throw new Error(`value has to be ${type}`);
        }
    }
    focusOption(key) {
        let focusedOptionIndex;
        if (document.activeElement === this.searchInputEl || document.activeElement === this.toggleButtonEl) {
            focusedOptionIndex = -1;
        }
        else {
            focusedOptionIndex = this.focusedOptionId
                ? this.focusableOptionsAndGroups().findIndex(option => option.id === this.focusedOptionId)
                : -1;
        }
        const focus = (key) => {
            this.focusedOptionId = key === 'down'
                ? this.focusableOptionsAndGroups()[focusedOptionIndex + 1].id
                : this.focusableOptionsAndGroups()[focusedOptionIndex - 1].id;
            setTimeout(() => document.getElementById(this.focusedOptionId).focus());
        };
        if (DOWN_KEY.includes(key) && !(focusedOptionIndex >= this.focusableOptionsAndGroups().length - 1)) {
            focus('down');
        }
        else if (UP_KEY.includes(key) && !(focusedOptionIndex <= 0)) {
            focus('up');
        }
    }
    handleOptionChange(e, { id, selected, value }) {
        e.preventDefault();
        this.focusedOptionId = id;
        if (this.multiple) {
            this.value = selected ? [...this.value, value] : this.value.filter(optionValue => optionValue !== value);
        }
        else {
            this.value = value;
            this.close();
            this.toggleButtonEl.focus();
        }
        this.raulChange.emit({ value: this.value });
    }
    handleOptionsGroupChange(e, { id, selected, enabledOptionsValues }) {
        e.preventDefault();
        this.focusedOptionId = id;
        this.value = selected
            ? [...this.value, ...enabledOptionsValues]
            : this.value.filter(option => !enabledOptionsValues.includes(option));
        this.raulChange.emit({ value: this.value });
    }
    handleSearch(e) {
        this.searchQuery = e.target.value.toLowerCase();
    }
    createPopper() {
        if (!this.popper) {
            this.popper = new Popper(this.toggleButtonEl, this.selectMenuEl, {
                placement: this.menuPlacement,
                positionFixed: this.menuPositionFixed
            });
        }
    }
    destroyPopper() {
        if (this.popper) {
            this.popper.destroy();
            this.popper = null;
        }
    }
    clearSearch() {
        this.searchQuery = '';
        setTimeout(() => this.searchInputEl.focus(), 0);
    }
    render() {
        const renderHiddenInput = () => {
            if (this.multiple) {
                return this.value && this.value.length
                    ? this.value.map(value => h("input", { type: "hidden", name: this.name, value: value }))
                    : h("input", { type: "hidden", name: this.name, value: this.value });
            }
            else {
                return h("input", { type: "hidden", name: this.name, value: this.value });
            }
        };
        const renderSelectToggleContent = () => {
            if (!this.hasSlot('select-toggle')) {
                return (h("div", { class: {
                        'r-select__toggle__content': true,
                        'r-select__toggle__content--borderless': this.borderless,
                    } },
                    h("div", { class: "r-select__toggle__content__text" }, this.selectedOptionsText() || this.placeholder),
                    h("raul-icon", { icon: "arrow-down-v", class: "r-select__toggle__content__arrow" })));
            }
        };
        const renderSelectMenuFooter = () => {
            if (this.hasSlot('select-menu-footer')) {
                return (h("div", { class: "r-select__menu__footer" },
                    h("slot", { name: "select-menu-footer" })));
            }
        };
        const renderSelectMenuMobileLabelContent = () => {
            if (!this.hasSlot('select-mobile-label')) {
                return (h("div", { class: "r-select__mobile-label__content" },
                    h("div", { class: "r-select__mobile-label__content__button" },
                        h("span", { onClick: () => this.close() }, "Cancel")),
                    this.label &&
                        h("div", { class: "r-select__mobile-label__content__text" }, this.label),
                    this.multiple &&
                        h("div", { class: {
                                'r-select__mobile-label__content__button': true,
                                'r-select__mobile-label__content__button--right': true,
                                'r-select__mobile-label__content__button--disabled': false
                            } },
                            h("span", { onClick: () => this.close() }, "Done"))));
            }
        };
        const renderSelectMenuSearch = () => {
            if (this.searchable) {
                return (h("div", { class: "r-select__search" },
                    h("raul-icon", { icon: "search", class: "r-select__search__icon" }),
                    h("input", { type: "text", class: "r-select__search__input", placeholder: "Search", value: this.searchQuery, onInput: e => this.handleSearch(e), ref: el => this.searchInputEl = el }),
                    this.searchQuery &&
                        h("raul-icon", { icon: "remove-2", class: "r-select__search__icon-reset", onClick: () => this.clearSearch() })));
            }
        };
        const renderNoSearchResults = () => {
            if (this.processedOptions.length && !this.visibleOptionsAndGroups().length) {
                return (h("div", { class: "r-select__no-search-results" }, "No results found. Try changing your search criteria."));
            }
        };
        const renderSelectMenuHeader = () => {
            return (h("div", { class: "r-select__menu__header" },
                h("div", { class: "r-select__mobile-label" },
                    renderSelectMenuMobileLabelContent(),
                    h("slot", { name: "select-mobile-label" })),
                renderSelectMenuSearch(),
                renderNoSearchResults()));
        };
        const renderOption = (option, groupVisible = false, groupDisabled = false) => {
            const { id, text, description, disabled, icon, iconKind, variant, value } = option;
            const visible = this.matchSearchQuery(text) || groupVisible;
            const focused = id === this.focusedOptionId;
            const selected = this.multiple ? this.value.includes(value) : this.value === value;
            const isDisabled = disabled || groupDisabled;
            if (visible) {
                return (h("raul-option", { multiple: this.multiple, selected: selected, focused: focused, disabled: isDisabled, value: value, text: text, description: description, icon: icon, iconKind: iconKind, optionId: id, variant: variant, onClick: e => this.handleOptionChange(e, { id: id, selected: this.multiple ? !selected : true, value }), onKeyDown: e => e.key === 'Enter'
                        ? this.handleOptionChange(e, { id: id, selected: this.multiple ? !selected : true, value })
                        : null }));
            }
        };
        const renderOptionsGroup = (group) => {
            const { id, text, description, disabled, value, options } = group;
            const optionsValues = options.map(option => option.value);
            const enabledOptionsValues = options.filter(option => !option.disabled).map(option => option.value);
            const groupVisible = this.matchSearchQuery(text);
            const focused = id === this.focusedOptionId;
            const selected = this.multiple ? optionsValues.every(option => this.value.includes(option)) : false;
            const indeterminate = this.multiple
                ? !selected && optionsValues.some(option => this.value.includes(option))
                : false;
            const enabledOptionsSelected = this.multiple
                ? enabledOptionsValues.every(option => this.value.includes(option))
                : false;
            return (h("div", { class: "r-options-group", role: "group" },
                h("raul-option", { variant: "group", multiple: this.multiple, selected: selected, focused: focused, disabled: disabled, value: value, text: text, description: description, indeterminate: indeterminate, optionId: id, onClick: this.multiple
                        ? e => this.handleOptionsGroupChange(e, { id: id, selected: !enabledOptionsSelected, enabledOptionsValues })
                        : null, onKeyDown: e => this.multiple && e.key === 'Enter'
                        ? this.handleOptionsGroupChange(e, { id: id, selected: !enabledOptionsSelected, enabledOptionsValues })
                        : null }),
                options.map(option => renderOption(option, groupVisible, disabled))));
        };
        return [
            renderHiddenInput(),
            this.label && h("label", { class: "r-select__label" }, this.label),
            h("div", { class: {
                    'r-select': true,
                    'r-select--opened': this.opened,
                    'r-select--expanded': this.expanded,
                    'r-select--invalid': !!this.error,
                }, ref: el => this.selectEl = el },
                h("button", { type: "button", class: "r-select__toggle", disabled: this.disabled, "aria-haspopup": "true", "aria-expanded": this.opened ? 'true' : 'false', "aria-activedescendant": this.focusedOptionId, onClick: () => this.toggle(), ref: el => this.toggleButtonEl = el },
                    renderSelectToggleContent(),
                    h("slot", { name: "select-toggle" })),
                this.hint && h("small", { class: "r-select__hint" }, this.hint),
                this.error &&
                    h("div", { class: "r-select__error" },
                        h("raul-icon", { icon: "interface-alert-diamond", class: "r-select__error__icon" }),
                        " ",
                        this.error),
                h("div", { class: "r-select__menu", ref: el => this.selectMenuEl = el },
                    h("div", { class: "r-select__menu__content" },
                        renderSelectMenuHeader(),
                        h("div", { class: "r-select__menu__body", role: "listbox", tabindex: "-1", "aria-multiselectable": this.multiple ? 'true' : null, style: { maxHeight: this.optionsMaxHeight } }, this.visibleOptionsAndGroups().map(option => {
                            return option.options && option.options.length
                                ? renderOptionsGroup(option)
                                : renderOption(option);
                        })),
                        renderSelectMenuFooter())))
        ];
    }
    static get is() { return "raul-select"; }
    static get originalStyleUrls() { return {
        "$": ["raul-select.scss"]
    }; }
    static get styleUrls() { return {
        "$": ["raul-select.css"]
    }; }
    static get properties() { return {
        "disabled": {
            "type": "boolean",
            "mutable": false,
            "complexType": {
                "original": "boolean",
                "resolved": "boolean",
                "references": {}
            },
            "required": false,
            "optional": false,
            "docs": {
                "tags": [],
                "text": "If `true`, the user cannot interact with the select."
            },
            "attribute": "disabled",
            "reflect": true,
            "defaultValue": "false"
        },
        "multiple": {
            "type": "boolean",
            "mutable": false,
            "complexType": {
                "original": "boolean",
                "resolved": "boolean",
                "references": {}
            },
            "required": false,
            "optional": false,
            "docs": {
                "tags": [],
                "text": "If `true`, allows multiple selections."
            },
            "attribute": "multiple",
            "reflect": true,
            "defaultValue": "false"
        },
        "name": {
            "type": "string",
            "mutable": false,
            "complexType": {
                "original": "string",
                "resolved": "string",
                "references": {}
            },
            "required": false,
            "optional": false,
            "docs": {
                "tags": [],
                "text": "The name of the control, which is submitted with the form data."
            },
            "attribute": "name",
            "reflect": true,
            "defaultValue": "`select-${randomUID()}`"
        },
        "label": {
            "type": "string",
            "mutable": false,
            "complexType": {
                "original": "string",
                "resolved": "string",
                "references": {}
            },
            "required": false,
            "optional": false,
            "docs": {
                "tags": [],
                "text": "The text label."
            },
            "attribute": "label",
            "reflect": true,
            "defaultValue": "null"
        },
        "hint": {
            "type": "string",
            "mutable": false,
            "complexType": {
                "original": "string",
                "resolved": "string",
                "references": {}
            },
            "required": false,
            "optional": false,
            "docs": {
                "tags": [],
                "text": "Optional hint text."
            },
            "attribute": "hint",
            "reflect": false
        },
        "error": {
            "type": "string",
            "mutable": false,
            "complexType": {
                "original": "string",
                "resolved": "string",
                "references": {}
            },
            "required": false,
            "optional": false,
            "docs": {
                "tags": [],
                "text": "Makes the select visually invalid and shows the feedback message."
            },
            "attribute": "error",
            "reflect": false
        },
        "optionsMaxHeight": {
            "type": "string",
            "mutable": false,
            "complexType": {
                "original": "string",
                "resolved": "string",
                "references": {}
            },
            "required": false,
            "optional": false,
            "docs": {
                "tags": [],
                "text": "Sets the max height of the select options list, e.g. `500px`, `calc(100vh - 50px)`."
            },
            "attribute": "options-max-height",
            "reflect": false
        },
        "placeholder": {
            "type": "string",
            "mutable": false,
            "complexType": {
                "original": "string",
                "resolved": "string",
                "references": {}
            },
            "required": false,
            "optional": false,
            "docs": {
                "tags": [],
                "text": "The text to display when the select value is empty."
            },
            "attribute": "placeholder",
            "reflect": true,
            "defaultValue": "'Select an option'"
        },
        "searchable": {
            "type": "boolean",
            "mutable": false,
            "complexType": {
                "original": "boolean",
                "resolved": "boolean",
                "references": {}
            },
            "required": false,
            "optional": false,
            "docs": {
                "tags": [],
                "text": "If `true`, adds a search field at the top of the select menu."
            },
            "attribute": "searchable",
            "reflect": true,
            "defaultValue": "false"
        },
        "borderless": {
            "type": "boolean",
            "mutable": false,
            "complexType": {
                "original": "boolean",
                "resolved": "boolean",
                "references": {}
            },
            "required": false,
            "optional": false,
            "docs": {
                "tags": [],
                "text": "If `true`, removes select toggle border."
            },
            "attribute": "borderless",
            "reflect": true,
            "defaultValue": "false"
        },
        "value": {
            "type": "string",
            "mutable": true,
            "complexType": {
                "original": "string | string[]",
                "resolved": "string | string[]",
                "references": {}
            },
            "required": false,
            "optional": false,
            "docs": {
                "tags": [],
                "text": "The value of the select."
            },
            "attribute": "value",
            "reflect": false,
            "defaultValue": "this.multiple ? [] : ''"
        },
        "options": {
            "type": "unknown",
            "mutable": false,
            "complexType": {
                "original": "Object[]",
                "resolved": "Object[]",
                "references": {
                    "Object": {
                        "location": "global"
                    }
                }
            },
            "required": false,
            "optional": false,
            "docs": {
                "tags": [],
                "text": "Options and groups of the select."
            },
            "defaultValue": "[]"
        },
        "menuPlacement": {
            "type": "string",
            "mutable": false,
            "complexType": {
                "original": "PopperPlacements",
                "resolved": "\"auto\" | \"auto-end\" | \"auto-start\" | \"bottom\" | \"bottom-end\" | \"bottom-start\" | \"left\" | \"left-end\" | \"left-start\" | \"right\" | \"right-end\" | \"right-start\" | \"top\" | \"top-end\" | \"top-start\"",
                "references": {
                    "PopperPlacements": {
                        "location": "import",
                        "path": "../../../utils/types"
                    }
                }
            },
            "required": false,
            "optional": false,
            "docs": {
                "tags": [],
                "text": "Options menu's placement."
            },
            "attribute": "menu-placement",
            "reflect": false,
            "defaultValue": "'bottom-start'"
        },
        "menuPositionFixed": {
            "type": "boolean",
            "mutable": false,
            "complexType": {
                "original": "boolean",
                "resolved": "boolean",
                "references": {}
            },
            "required": false,
            "optional": false,
            "docs": {
                "tags": [],
                "text": "If `true` the menu position will be 'fixed'."
            },
            "attribute": "menu-position-fixed",
            "reflect": false,
            "defaultValue": "false"
        }
    }; }
    static get states() { return {
        "focusedOptionId": {},
        "opened": {},
        "expanded": {},
        "searchQuery": {}
    }; }
    static get events() { return [{
            "method": "raulSelectOpen",
            "name": "raulSelectOpen",
            "bubbles": true,
            "cancelable": true,
            "composed": true,
            "docs": {
                "tags": [],
                "text": "Emitted when the select menu opens."
            },
            "complexType": {
                "original": "any",
                "resolved": "any",
                "references": {}
            }
        }, {
            "method": "raulSelectClose",
            "name": "raulSelectClose",
            "bubbles": true,
            "cancelable": true,
            "composed": true,
            "docs": {
                "tags": [],
                "text": "Emitted when the select menu closes."
            },
            "complexType": {
                "original": "any",
                "resolved": "any",
                "references": {}
            }
        }, {
            "method": "raulChange",
            "name": "raulChange",
            "bubbles": true,
            "cancelable": true,
            "composed": true,
            "docs": {
                "tags": [],
                "text": "Emitted when the select value changes."
            },
            "complexType": {
                "original": "any",
                "resolved": "any",
                "references": {}
            }
        }]; }
    static get methods() { return {
        "open": {
            "complexType": {
                "signature": "() => Promise<void>",
                "parameters": [],
                "references": {
                    "Promise": {
                        "location": "global"
                    }
                },
                "return": "Promise<void>"
            },
            "docs": {
                "text": "Open the select menu.",
                "tags": []
            }
        },
        "close": {
            "complexType": {
                "signature": "() => Promise<void>",
                "parameters": [],
                "references": {
                    "Promise": {
                        "location": "global"
                    }
                },
                "return": "Promise<void>"
            },
            "docs": {
                "text": "Close the select menu.",
                "tags": []
            }
        },
        "toggle": {
            "complexType": {
                "signature": "() => Promise<void>",
                "parameters": [],
                "references": {
                    "Promise": {
                        "location": "global"
                    }
                },
                "return": "Promise<void>"
            },
            "docs": {
                "text": "Toggle the select menu.",
                "tags": []
            }
        }
    }; }
    static get elementRef() { return "el"; }
    static get watchers() { return [{
            "propName": "options",
            "methodName": "optionsChanged"
        }, {
            "propName": "value",
            "methodName": "valueChanged"
        }]; }
    static get listeners() { return [{
            "name": "keydown",
            "method": "handleKeyDown",
            "target": undefined,
            "capture": false,
            "passive": false
        }, {
            "name": "click",
            "method": "handleClickAndFocusOutside",
            "target": "window",
            "capture": false,
            "passive": false
        }, {
            "name": "focusin",
            "method": "handleClickAndFocusOutside",
            "target": "window",
            "capture": false,
            "passive": false
        }, {
            "name": "resize",
            "method": "handleDebouncedResize",
            "target": "window",
            "capture": false,
            "passive": true
        }]; }
}
