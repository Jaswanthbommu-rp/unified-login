import { EventEmitter } from '../../../stencil.core';
export declare class RaulAvatar {
    static: boolean;
    label: string;
    hint: string;
    value: string;
    color: 'primary' | 'warning' | 'danger' | 'success';
    raulProgressRemove: EventEmitter;
    private handleProgressRemove;
    render(): any;
}
