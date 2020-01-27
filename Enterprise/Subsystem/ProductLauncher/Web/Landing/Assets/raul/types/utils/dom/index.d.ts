export declare function isElementInViewport(el: HTMLElement): boolean;
export declare function getViewportDimensions(): Dimensions;
interface Dimensions {
    height: number;
    width: number;
}
export declare function getScreenSize(): ScreenSize;
/**
 * Keep this in sync with tailwind screens theme.
 */
export declare const screens: {
    sm: number;
    md: number;
    lg: number;
    xl: number;
};
export declare function isScreen(size: any): boolean;
export declare type Screen = 'sm' | 'md' | 'lg' | 'xl';
export declare type ScreenSize = "sm" | "md" | "lg";
export declare function elementBlockHeight(el: HTMLElement): number;
export declare function collapseHeight(el: HTMLElement, transition?: boolean): void;
export declare function expandHeight(el: HTMLElement, transition?: boolean): void;
export {};
