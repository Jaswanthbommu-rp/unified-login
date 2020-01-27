'use strict';

Object.defineProperty(exports, '__esModule', { value: true });

const core = require('./core-40bc5f02.js');

const DocsRaulSelect = class {
    constructor(hostRef) {
        core.registerInstance(this, hostRef);
        this.singleSelectionOptions = [
            {
                text: 'Red',
                value: 'red'
            },
            {
                text: 'Green',
                value: 'green'
            },
            {
                text: 'Blue',
                value: 'blue'
            }
        ];
        this.singleSelectionOptionsValidation = [
            {
                text: 'Red',
                value: 'red'
            },
            {
                text: 'Green',
                value: 'green'
            },
            {
                text: 'Blue',
                value: 'blue'
            }
        ];
        this.multipleSelectionsOptions = [
            {
                text: 'Cat',
                value: 'cat'
            },
            {
                text: 'Dog',
                value: 'dog'
            },
            {
                text: 'Elephant',
                value: 'elephant'
            }
        ];
        this.groupedOptions = [
            {
                text: 'Colors',
                options: [
                    {
                        text: 'Red',
                        value: 'red'
                    },
                    {
                        text: 'Green',
                        value: 'green'
                    },
                    {
                        text: 'Blue',
                        value: 'blue'
                    }
                ]
            },
            {
                text: 'Animals',
                options: [
                    {
                        text: 'Cat',
                        value: 'cat'
                    },
                    {
                        text: 'Dog',
                        value: 'dog'
                    },
                    {
                        text: 'Elephant',
                        value: 'elephant'
                    }
                ]
            }
        ];
        this.disabledOptions = [
            {
                text: 'Red',
                value: 'red',
                disabled: true
            },
            {
                text: 'Green',
                value: 'green'
            },
            {
                text: 'Blue',
                value: 'blue'
            }
        ];
        this.disabledGroupedOptions = [
            {
                text: 'Colors',
                options: [
                    {
                        text: 'Red',
                        value: 'red'
                    },
                    {
                        text: 'Green',
                        value: 'green',
                        disabled: true
                    },
                    {
                        text: 'Blue',
                        value: 'blue'
                    }
                ]
            },
            {
                text: 'Animals',
                disabled: true,
                options: [
                    {
                        text: 'Cat',
                        value: 'cat'
                    },
                    {
                        text: 'Dog',
                        value: 'dog'
                    },
                    {
                        text: 'Elephant',
                        value: 'elephant'
                    }
                ]
            }
        ];
        this.descriptionAndImageOptions = [
            {
                text: 'Red',
                value: 'red',
                description: 'Red color description',
                icon: 'interface-alert-diamond'
            },
            {
                text: 'Green',
                value: 'green',
                description: 'Green color description',
                icon: 'check-mark-success'
            },
            {
                text: 'Blue',
                value: 'blue',
                description: 'Blue color description',
                icon: 'night-clear-sky'
            }
        ];
        this.singleSelectValue = '';
        this.singleSelectValueValidation = '';
        this.multipleSelectValue = [];
        this.searchableValue = '';
        this.borderlessValue = '';
        this.groupedSingleValue = '';
        this.groupedMultipleValue = [];
        this.disabledSingleValue = '';
        this.disabledMultipleValue = [];
        this.disabledGroupedSingleValue = '';
        this.disabledGroupedMultipleValue = [];
        this.descriptionAndImageValue = '';
    }
    handleRaulChange(e, value) {
        this[value] = e.detail.value;
    }
    render() {
        return (core.h("docs-element", { title: "Select" }, core.h("div", { slot: "overview" }, core.h("docs-showcase", null, core.h("div", { class: "mb-md" }, core.h("raul-select", { label: "Single Select", placeholder: "Pick a color", options: this.singleSelectionOptions, value: this.singleSelectValue, onRaulChange: e => this.handleRaulChange(e, 'singleSelectValue') })), core.h("div", { class: "mb-md" }, core.h("raul-select", { label: "Single Select with Validation", placeholder: "Pick a color", options: this.singleSelectionOptionsValidation, value: this.singleSelectValueValidation, onRaulChange: e => this.handleRaulChange(e, 'singleSelectValueValidation'), error: "This flied is required." })), core.h("div", { class: "mb-md" }, core.h("raul-select", { label: "Multiple Select", multiple: true, placeholder: "Pick some colors", onRaulChange: e => this.handleRaulChange(e, 'multipleSelectValue'), value: this.multipleSelectValue, options: this.multipleSelectionsOptions })), core.h("div", { class: "mb-md" }, core.h("raul-select", { label: "Searchable", searchable: true, options: this.singleSelectionOptions, value: this.searchableValue, onRaulChange: e => this.handleRaulChange(e, 'searchableValue') })), core.h("div", { class: "mb-md" }, core.h("raul-select", { label: "Disabled", disabled: true, options: this.singleSelectionOptions })), core.h("div", { class: "mb-md" }, core.h("raul-select", { label: "Borderless", borderless: true, options: this.singleSelectionOptions, value: this.borderlessValue, onRaulChange: e => this.handleRaulChange(e, 'borderlessValue') })), core.h("div", { class: "mb-md" }, core.h("raul-select", { label: "Grouped Single", searchable: true, options: this.groupedOptions, value: this.groupedSingleValue, onRaulChange: e => this.handleRaulChange(e, 'groupedSingleValue') })), core.h("div", { class: "mb-md" }, core.h("raul-select", { label: "Grouped Multi", searchable: true, multiple: true, options: this.groupedOptions, value: this.groupedMultipleValue, onRaulChange: e => this.handleRaulChange(e, 'groupedMultipleValue') })), core.h("div", { class: "mb-md" }, core.h("raul-select", { label: "Disabled Single", options: this.disabledOptions, value: this.disabledSingleValue, onRaulChange: e => this.handleRaulChange(e, 'disabledSingleValue') })), core.h("div", { class: "mb-md" }, core.h("raul-select", { label: "Disabled Multi", multiple: true, options: this.disabledOptions, value: this.disabledMultipleValue, onRaulChange: e => this.handleRaulChange(e, 'disabledMultipleValue') })), core.h("div", { class: "mb-md" }, core.h("raul-select", { label: "Disabled Group", options: this.disabledGroupedOptions, value: this.disabledGroupedSingleValue, onRaulChange: e => this.handleRaulChange(e, 'disabledGroupedSingleValue') })), core.h("div", { class: "mb-md" }, core.h("raul-select", { label: "Disabled Multi Group", multiple: true, options: this.disabledGroupedOptions, value: this.disabledGroupedMultipleValue, onRaulChange: e => this.handleRaulChange(e, 'disabledGroupedMultipleValue') })), core.h("div", { class: "mb-md" }, core.h("raul-select", { label: "Description and Image", options: this.descriptionAndImageOptions, value: this.descriptionAndImageValue, onRaulChange: e => this.handleRaulChange(e, 'descriptionAndImageValue') })))), core.h("div", { slot: "design" }, "Design Guidelines"), core.h("div", { slot: "api" }, core.h("docs-interface", { component: "raul-select" }))));
    }
};

exports.docs_raul_select = DocsRaulSelect;
