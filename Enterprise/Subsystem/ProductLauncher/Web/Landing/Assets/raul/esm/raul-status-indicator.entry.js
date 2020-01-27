import { r as registerInstance, h } from './core-9263a98c.js';

const RaulStatusIndicator = class {
    constructor(hostRef) {
        registerInstance(this, hostRef);
        this.variant = 'default';
    }
    render() {
        return (h("div", { class: {
                'status-indicator': true,
                [`status-indicator--${this.variant}`]: this.variant !== 'default'
            } }));
    }
    static get style() { return "raul-status-indicator{display:inline-block}raul-status-indicator .status-indicator{display:inline-block;border-radius:9999px;width:.5rem;height:.5rem;background-color:#ebedee}raul-status-indicator .status-indicator--destructive{background-color:#d01a1f}raul-status-indicator .status-indicator--success{background-color:#139c3e}raul-status-indicator .status-indicator--warning{background-color:#fec12d}"; }
};

export { RaulStatusIndicator as raul_status_indicator };
