import { EventEmitter } from '../../../../stencil.core';
export declare class DocsPreviewProps {
    propDocs: any;
    component: string;
    docsPropChange: EventEmitter;
    componentWillLoad(): void;
    renderPropControl(doc: any): any;
    render(): any;
}
