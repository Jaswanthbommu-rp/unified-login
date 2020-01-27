import { r as registerInstance, h } from './core-9263a98c.js';

const RaulBadge = class {
    constructor(hostRef) {
        registerInstance(this, hostRef);
        /**
         * Determines the primary appearance of the badge based on its purpose.
         */
        this.variant = 'primary';
    }
    render() {
        return (h("div", { class: `r-badge__container r-badge__container--${this.variant}` }, this.icon &&
            h("div", { class: 'r-badge__icon' }, h("raul-icon", { icon: this.icon })), h("div", { class: 'r-badge__content' }, this.content)));
    }
    static get style() { return "raul-badge{display:inline-block}.r-badge__container{display:-ms-flexbox;display:flex;-ms-flex-align:center;align-items:center;-ms-flex-pack:center;justify-content:center;height:1.25rem;border-radius:9999px;font-size:.75rem;padding-left:.5rem;padding-right:.5rem;padding-top:.25rem;padding-bottom:.25rem;min-width:30px}.r-badge__container--primary{color:#fff;background-color:#0076cc}.r-badge__container--warning{color:#37474f;background-color:#fec12d}.r-badge__container--error{color:#fff;background-color:#d01a1f}.r-badge__container--success{color:#fff;background-color:#139c3e}.r-badge__icon{position:relative;margin-right:.25rem}.r-badge__content{white-space:nowrap}"; }
};

export { RaulBadge as raul_badge };
