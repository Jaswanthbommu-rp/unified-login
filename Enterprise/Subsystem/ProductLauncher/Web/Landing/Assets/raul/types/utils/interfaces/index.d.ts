export interface TabInterface {
    label: string;
    name: string;
    id?: string;
    disabled?: boolean;
}
export interface ToggleInterface extends TabInterface {
}
export interface ListGroupItemInterface {
    title: string;
    description?: string;
    value: string;
    disabled?: boolean;
    items?: ListGroupItemInterface[];
}
