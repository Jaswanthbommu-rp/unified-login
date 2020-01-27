import { PopperPlacements } from '../../../utils/types';
export declare class RaulTooltip {
    private tooltipId;
    private popper;
    el: HTMLRaulTooltipElement;
    text: string;
    placement: PopperPlacements;
    disabledHoverListener: boolean;
    disabledFocusListener: boolean;
    handleMouseEnter(): void;
    handleMouseLeave(): void;
    handleFocusIn(): void;
    handleFocusOut(): void;
    show(): Promise<void>;
    hide(): Promise<void>;
    private tooltipRef;
    private tooltipArrowRef;
    private tooltipElement;
    private popperOptions;
    private createTooltip;
    private removeTooltip;
    render(): any;
}
