import { r as registerInstance, h } from './core-9263a98c.js';

const RaulChip = class {
    constructor(hostRef) {
        registerInstance(this, hostRef);
        /**
         * Status variant.
         */
        this.variant = 'default';
    }
    componentDidLoad() {
        this.title = this.statusTextEl.textContent;
    }
    render() {
        return (h("div", { class: {
                'status': true,
                [`status--${this.variant}`]: this.variant !== 'default'
            } }, h("div", { class: "status__text", title: this.title, ref: el => this.statusTextEl = el }, h("slot", null))));
    }
    static get style() { return "raul-status{display:inline-block;max-width:100%}raul-status .status{position:relative;display:-ms-flexbox;display:flex;background-color:#ebedee;font-size:.75rem;font-weight:500;color:#37474f;text-align:left;line-height:1.25;border-radius:.125rem;padding-left:.5rem;padding-right:.5rem;padding-top:.25rem;padding-bottom:.25rem;max-width:100%}raul-status .status--destructive{background-color:#fae8e9;color:#d01a1f}raul-status .status--success{background-color:#e7f3eb;color:#139c3e}raul-status .status--warning{background-color:#fef8ea;color:#bc8701}"; }
};

export { RaulChip as raul_status };
