'use strict';

Object.defineProperty(exports, '__esModule', { value: true });

const core = require('./core-40bc5f02.js');

const loremIpsum = `
Lorem ipsum dolor sit amet, consectetur adipiscing elit. Cras elementum pellentesque nisl ut sodales. Donec tempus nulla eget arcu ultrices, non molestie turpis rhoncus. Vivamus est tellus, varius at ante quis, ultricies lobortis mi. Suspendisse vitae neque id enim auctor pellentesque et id lacus. Curabitur nec nulla tortor. Fusce fringilla eleifend gravida. Duis tincidunt mauris vel varius vestibulum. Donec pharetra sem et ipsum ultrices, tempor varius sapien congue. Phasellus at mi dui. Nunc in orci sed dolor facilisis commodo ut quis urna. Integer suscipit bibendum est eget placerat. Maecenas felis ante, hendrerit nec rhoncus nec, molestie quis mauris. Curabitur felis nibh, dapibus eu ex id, blandit tristique metus. Sed consequat sodales diam, ac bibendum ipsum condimentum sit amet. Nam ut ante non urna convallis vulputate.

In metus mauris, tempor quis porta eu, sollicitudin eget tellus. Phasellus nec dui vitae lacus facilisis luctus id non nisi. Donec viverra vehicula ultrices. Proin vel posuere mauris. Cras nunc tellus, rutrum ut rutrum a, dapibus eu dolor. Aliquam in bibendum nunc. In mollis ut velit ut luctus. Morbi faucibus purus tincidunt nisl fringilla accumsan. Sed a turpis nisl. Suspendisse quis lectus neque. Ut pellentesque nunc lorem, vel vestibulum ipsum eleifend ut. Vivamus consectetur magna non tellus ultricies fringilla. Suspendisse placerat, lacus non condimentum placerat, augue dui suscipit nisi, a auctor risus est sed risus. Fusce convallis est risus, et commodo libero tristique ac. Vestibulum porttitor gravida nulla et molestie.

Donec suscipit turpis a arcu feugiat semper. Phasellus lobortis tellus vel tortor fermentum vulputate. Aenean sed orci ut nibh euismod rhoncus. Vivamus at quam et erat tempor ultrices auctor quis massa. Quisque sollicitudin magna in nibh molestie, sed gravida nisi cursus. Aliquam sed tortor mattis, auctor nibh id, scelerisque tellus. Sed hendrerit urna vel lobortis iaculis. Cras dignissim neque in aliquet elementum. Donec nec pretium ex. Proin ut rhoncus ligula, ut laoreet mi.

Mauris laoreet odio eget mauris dapibus, id interdum nisi ultrices. Nam ac cursus quam. Cras rhoncus purus nibh, eget volutpat risus bibendum nec. Suspendisse at magna nulla. Vivamus bibendum magna eros, ac vulputate velit pellentesque id. Ut sit amet consectetur elit. Pellentesque fringilla interdum faucibus. Nulla vulputate nunc at mattis interdum. Proin dolor quam, tristique ut massa eget, fermentum mattis mi. Cras eget porttitor urna. Sed eu luctus odio. Phasellus nec elit ut erat porta egestas. Integer vel iaculis augue, in commodo est. Cras id consequat lacus.

Vestibulum fringilla lorem non nulla varius, ac interdum neque ornare. Vivamus tempus at tellus in dapibus. Proin sit amet felis non nisi lobortis convallis. Donec consequat viverra nulla, sodales posuere velit auctor vitae. Vestibulum bibendum vestibulum enim et dignissim. Ut luctus nibh pulvinar, fringilla dolor at, convallis erat. Pellentesque cursus lobortis risus consequat viverra. Ut suscipit, urna vitae tempor efficitur, lacus quam pulvinar urna, ut tempus massa nunc vitae odio.

Donec id sollicitudin purus. Fusce non turpis tincidunt justo aliquam dapibus. Nulla eu pellentesque lorem, sed rhoncus tellus. Integer sapien sapien, efficitur at lectus non, tristique molestie nulla. Vivamus sed interdum nunc. Suspendisse porttitor vitae nisl pretium faucibus. Proin a massa non massa fringilla finibus. Duis a augue odio.

Pellentesque congue, neque vel viverra lobortis, magna urna laoreet sapien, nec rutrum nisl lorem non est. Donec luctus purus sit amet mattis mattis. Cras at neque ornare, accumsan arcu non, porta massa. Phasellus vel lectus urna. Etiam ultrices consectetur blandit. Nam eu lectus convallis, dignissim elit at, dignissim turpis. Aenean tincidunt nec ipsum vel faucibus. Sed in sollicitudin dolor. Pellentesque dui dolor, bibendum vel dictum sit amet, ultrices in risus. Etiam suscipit justo sed dui euismod, id condimentum est maximus. Vivamus fringilla mollis est.

Fusce a augue tincidunt, bibendum elit sit amet, viverra dolor. Morbi id consectetur erat, eu porta mi. Donec a aliquet orci. Nunc dictum arcu condimentum consectetur pharetra. Aenean egestas id nulla ac convallis. Sed sed fermentum nisl, vitae dapibus ex. Praesent et magna volutpat sapien sagittis pretium a non ligula. Proin pretium neque nulla, sed tempus tellus laoreet sed. Phasellus purus lacus, faucibus ut iaculis non, tincidunt non nibh. Sed vehicula justo in ipsum faucibus, a euismod orci mattis. Integer purus massa, dictum at quam eu, egestas pellentesque ex.

Proin tellus magna, sagittis quis luctus vitae, commodo non metus. Nulla semper elit massa, ut euismod ex pretium sit amet. Mauris a velit mauris. Proin cursus scelerisque ex, sit amet consequat mi rhoncus pretium. Etiam et purus a risus ultrices eleifend vel ac ante. Sed tincidunt libero non ante finibus sagittis. Donec a sodales sapien. Cras faucibus orci quis metus ultricies molestie. Curabitur id lacus libero. Maecenas viverra elementum rhoncus. Sed ac sodales mauris. Morbi vel sollicitudin nisi, eget finibus nisi. Suspendisse enim est, viverra non ullamcorper in, mattis quis lorem. Mauris non augue quis quam accumsan dignissim.

In hac habitasse platea dictumst. Nam et leo odio. Maecenas sit amet varius neque. Integer sit amet suscipit ante, a tristique elit. Duis accumsan ornare nisi a dictum. Nullam blandit ante ut sapien molestie mollis. Nulla quis massa id dui porta luctus.
`;

const DocsRaulModal = class {
    constructor(hostRef) {
        core.registerInstance(this, hostRef);
        this.showBasicModal = false;
        this.showLongContentModal = false;
        this.showMediaModal = false;
    }
    handleCloseModal() {
        this.showBasicModal = false;
        this.showLongContentModal = false;
        this.showMediaModal = false;
    }
    render() {
        return (core.h("docs-element", { title: "Modal" }, core.h("div", { slot: "overview" }, core.h("docs-readme", { component: "raul-modal" }), core.h("raul-heading", { variant: 'content' }, "Examples"), core.h("raul-button", { variant: "text", size: "small", class: 'mb-4 inline-block', onClick: () => this.showBasicModal = !this.showBasicModal }, "Show a very simple modal"), core.h("div", null, this.showBasicModal &&
            core.h("raul-modal", { variant: 'normal' }, core.h("raul-modal-header", { "modal-title": 'Lorem ipsum', description: 'Dolor sit amet' }), core.h("raul-modal-footer", null, core.h("raul-button", { size: 'small', onClick: () => this.showBasicModal = false }, "Dismiss"), core.h("raul-button", { size: 'small', variant: 'primary', onClick: () => this.showBasicModal = false }, "Submit")))), core.h("raul-button", { variant: "text", size: "small", class: 'mb-4 inline-block', onClick: () => this.showLongContentModal = !this.showLongContentModal }, "Show a modal with a lot of content"), core.h("div", null, this.showLongContentModal &&
            core.h("raul-modal", { variant: 'normal' }, core.h("raul-modal-header", { "modal-title": 'Lorem ipsum', description: 'Dolor sit amet' }), core.h("raul-modal-body", null, core.h("raul-content", null, core.h("p", null, loremIpsum))), core.h("raul-modal-footer", null, core.h("raul-button", { size: 'small', onClick: () => this.showLongContentModal = false }, "Dismiss"), core.h("raul-button", { size: 'small', variant: 'primary', onClick: () => this.showLongContentModal = false }, "Submit")))), core.h("raul-button", { variant: "text", size: "small", class: 'mb-4 inline-block', onClick: () => this.showMediaModal = !this.showMediaModal }, "Show a media modal"), core.h("div", null, this.showMediaModal &&
            core.h("raul-modal", { variant: 'media' }, core.h("raul-modal-header", { "modal-title": 'Lorem ipsum', description: 'Dolor sit amet' }), core.h("raul-modal-body", null, core.h("img", { src: 'https://images.unsplash.com/photo-1569315618680-3d673b5e1514?ixlib=rb-1.2.1&ixid=eyJhcHBfaWQiOjEyMDd9&auto=format&fit=crop&w=1950&q=80' }))))), core.h("div", { slot: "design" }, "Design Guidelines Stuff"), core.h("div", { slot: "api" }, core.h("docs-interface", { component: "raul-modal" }))));
    }
};

exports.docs_raul_modal = DocsRaulModal;
