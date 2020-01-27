import { EventEmitter } from '../../../stencil.core';
export declare class RaulVideo {
    src: string;
    autoplay: boolean;
    controls: boolean;
    videoPlay: EventEmitter;
    videoPause: EventEmitter;
    videoEnded: EventEmitter;
    videoPlayer: HTMLVideoElement;
    componentDidLoad(): void;
    playVideo(): Promise<void>;
    pauseVideo(): Promise<void>;
    stopVideo(): Promise<void>;
    progress(): Promise<number>;
    render(): any;
}
