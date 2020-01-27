export declare type ScreenSize = 'sm' | 'md' | 'lg';
export declare type NavigationKeys = 'ArrowDown' | 'Down' | 'ArrowUp' | 'Up';
export declare type PopperPlacements = 'auto' | 'top' | 'right' | 'bottom' | 'left' | 'auto-start' | 'auto-end' | 'top-start' | 'top-end' | 'right-start' | 'right-end' | 'bottom-start' | 'bottom-end' | 'left-start' | 'left-end';
export interface DropdownMenuItem {
    icon?: string;
    title?: string;
    url?: string;
    payload?: unknown;
    onClickCallback?: Function;
    onBlurCallback?: Function;
    active?: boolean;
    disabled?: boolean;
    event?: unknown;
}
