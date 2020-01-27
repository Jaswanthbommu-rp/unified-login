import { h } from "@stencil/core";
import initPage from '../init-page';
import toast from '../../../../global/commands/toast';
export class DocsRaulSnackbar {
    constructor() {
        this.counter = 0;
        this.handleClick = () => {
            this.counter++;
            toast({
                timeout: 3000,
                heading: `Snackbar #${this.counter}`,
                content: 'Lorem ipsum dolor sit amet, consectetur adipiscing elit.',
                ctaMessage: 'Click me!',
                tagName: 'raul-snackbar',
                dismissable: true
            });
        };
    }
    componentDidLoad() {
        initPage('Snackbar');
    }
    render() {
        return (h("docs-element", { title: 'Snackbar' },
            h("div", { slot: 'overview' },
                h("docs-readme", { component: 'raul-snackbar' }),
                h("docs-showcase", null,
                    h("div", { class: 'mb-10' },
                        h("raul-snackbar", { dismissable: true, variant: 'information', heading: 'This is an information snackbar' })),
                    h("div", { class: 'mb-10' },
                        h("raul-snackbar", { dismissable: true, variant: 'success', heading: 'This is a success snackbar' })),
                    h("div", { class: 'mb-10' },
                        h("raul-snackbar", { dismissable: true, variant: 'warning', heading: 'This a warning snackbar' })),
                    h("div", { class: 'mb-10' },
                        h("raul-snackbar", { dismissable: true, variant: 'danger', heading: 'This is a danger snackbar' })),
                    h("div", { class: 'mb-10' },
                        h("raul-snackbar", { dismissable: true, variant: 'information', heading: 'This is a complex snackbar', content: 'Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed dictum dui ut ante mollis, in viverra ipsum aliquam.', ctaMessage: 'Go to Google', ctaUrl: 'https://google.com' })),
                    h("raul-button", { onClick: this.handleClick }, "Show snackbar"))),
            h("div", { slot: 'design' }),
            h("div", { slot: 'api' },
                h("docs-preview", { component: "raul-snackbar" }),
                h("docs-interface", { component: "raul-snackbar" }))));
    }
    static get is() { return "docs-raul-snackbar"; }
}
