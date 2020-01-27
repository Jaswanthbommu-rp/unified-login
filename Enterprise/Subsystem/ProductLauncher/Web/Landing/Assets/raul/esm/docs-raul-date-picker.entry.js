import { r as registerInstance, h } from './core-9263a98c.js';
import { i as initPage } from './init-page-8d37c56f.js';

const myDate = new Date(2019, 10, 25);
const startDate = new Date(2019, 10, 6);
const endDate = new Date(2019, 11, 16);
const DocsRaulBadge = class {
    constructor(hostRef) {
        registerInstance(this, hostRef);
    }
    componentDidLoad() {
        initPage('Date picker');
    }
    render() {
        return (h("docs-element", { title: "Date picker" }, h("div", { ref: el => this.el = el, slot: 'overview' }, h("docs-readme", { component: 'raul-date-picker' }), h("docs-showcase", null, h("div", { class: 'mb-10' }, h("raul-date-picker", { variant: 'date' })), h("div", { class: 'mb-10' }, h("raul-date-picker", { variant: 'inline', label: 'An inline picker' })), h("div", { class: 'mb-10' }, h("raul-date-picker", { variant: 'date', label: 'A date picker with a label' })), h("div", { class: 'mb-10' }, h("raul-date-picker", { variant: 'date', label: 'A date picker with an initial date', initialDate: myDate })), h("div", { class: 'mb-10' }, h("raul-date-picker", { variant: 'date', label: 'A disabled date picker', disabled: true, initialDate: myDate })), h("div", { class: 'mb-10' }, h("raul-date-picker", { variant: 'date', label: 'A date picker with validation', error: "This field is a required field." })), h("div", { class: 'mb-10' }, h("raul-date-picker", { variant: 'range', label: 'A range picker' })), h("div", { class: 'mb-10' }, h("raul-date-picker", { variant: 'range', label: 'A range picker with initial range', initialStartDate: startDate, initialEndDate: endDate })), h("div", { class: 'mb-10' }, h("raul-date-picker", { variant: 'range', label: 'A range picker with validation', error: "This field is a required field." })))), h("div", { slot: 'design' }, "WIP"), h("div", { slot: 'api' }, h("docs-preview", { component: "raul-date-picker" }), h("docs-interface", { component: "raul-date-picker" }), h("docs-readme", { component: "raul-date-picker" }))));
    }
};

export { DocsRaulBadge as docs_raul_date_picker };
