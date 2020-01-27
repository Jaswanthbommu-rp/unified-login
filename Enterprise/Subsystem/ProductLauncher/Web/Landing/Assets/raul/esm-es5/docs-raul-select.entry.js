import { r as registerInstance, h } from './core-9263a98c.js';
var DocsRaulSelect = /** @class */ (function () {
    function DocsRaulSelect(hostRef) {
        registerInstance(this, hostRef);
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
    DocsRaulSelect.prototype.handleRaulChange = function (e, value) {
        this[value] = e.detail.value;
    };
    DocsRaulSelect.prototype.render = function () {
        var _this = this;
        return (h("docs-element", { title: "Select" }, h("div", { slot: "overview" }, h("docs-showcase", null, h("div", { class: "mb-md" }, h("raul-select", { label: "Single Select", placeholder: "Pick a color", options: this.singleSelectionOptions, value: this.singleSelectValue, onRaulChange: function (e) { return _this.handleRaulChange(e, 'singleSelectValue'); } })), h("div", { class: "mb-md" }, h("raul-select", { label: "Single Select with Validation", placeholder: "Pick a color", options: this.singleSelectionOptionsValidation, value: this.singleSelectValueValidation, onRaulChange: function (e) { return _this.handleRaulChange(e, 'singleSelectValueValidation'); }, error: "This flied is required." })), h("div", { class: "mb-md" }, h("raul-select", { label: "Multiple Select", multiple: true, placeholder: "Pick some colors", onRaulChange: function (e) { return _this.handleRaulChange(e, 'multipleSelectValue'); }, value: this.multipleSelectValue, options: this.multipleSelectionsOptions })), h("div", { class: "mb-md" }, h("raul-select", { label: "Searchable", searchable: true, options: this.singleSelectionOptions, value: this.searchableValue, onRaulChange: function (e) { return _this.handleRaulChange(e, 'searchableValue'); } })), h("div", { class: "mb-md" }, h("raul-select", { label: "Disabled", disabled: true, options: this.singleSelectionOptions })), h("div", { class: "mb-md" }, h("raul-select", { label: "Borderless", borderless: true, options: this.singleSelectionOptions, value: this.borderlessValue, onRaulChange: function (e) { return _this.handleRaulChange(e, 'borderlessValue'); } })), h("div", { class: "mb-md" }, h("raul-select", { label: "Grouped Single", searchable: true, options: this.groupedOptions, value: this.groupedSingleValue, onRaulChange: function (e) { return _this.handleRaulChange(e, 'groupedSingleValue'); } })), h("div", { class: "mb-md" }, h("raul-select", { label: "Grouped Multi", searchable: true, multiple: true, options: this.groupedOptions, value: this.groupedMultipleValue, onRaulChange: function (e) { return _this.handleRaulChange(e, 'groupedMultipleValue'); } })), h("div", { class: "mb-md" }, h("raul-select", { label: "Disabled Single", options: this.disabledOptions, value: this.disabledSingleValue, onRaulChange: function (e) { return _this.handleRaulChange(e, 'disabledSingleValue'); } })), h("div", { class: "mb-md" }, h("raul-select", { label: "Disabled Multi", multiple: true, options: this.disabledOptions, value: this.disabledMultipleValue, onRaulChange: function (e) { return _this.handleRaulChange(e, 'disabledMultipleValue'); } })), h("div", { class: "mb-md" }, h("raul-select", { label: "Disabled Group", options: this.disabledGroupedOptions, value: this.disabledGroupedSingleValue, onRaulChange: function (e) { return _this.handleRaulChange(e, 'disabledGroupedSingleValue'); } })), h("div", { class: "mb-md" }, h("raul-select", { label: "Disabled Multi Group", multiple: true, options: this.disabledGroupedOptions, value: this.disabledGroupedMultipleValue, onRaulChange: function (e) { return _this.handleRaulChange(e, 'disabledGroupedMultipleValue'); } })), h("div", { class: "mb-md" }, h("raul-select", { label: "Description and Image", options: this.descriptionAndImageOptions, value: this.descriptionAndImageValue, onRaulChange: function (e) { return _this.handleRaulChange(e, 'descriptionAndImageValue'); } })))), h("div", { slot: "design" }, "Design Guidelines"), h("div", { slot: "api" }, h("docs-interface", { component: "raul-select" }))));
    };
    return DocsRaulSelect;
}());
export { DocsRaulSelect as docs_raul_select };
