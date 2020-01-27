export declare class RaulRangePicker {
    el: HTMLElement;
    firstPicker: HTMLRaulDatePickerElement;
    secondPicker: HTMLRaulDatePickerElement;
    months: string[];
    startDate: Date;
    endDate: Date;
    targetDate: Date;
    handleDateSelected(e: any): Promise<void>;
    formatDateForDisplay: (date: Date) => string;
    clearFirstPicker(): Promise<void>;
    clearSecondPicker(): Promise<void>;
    render(): any;
}
