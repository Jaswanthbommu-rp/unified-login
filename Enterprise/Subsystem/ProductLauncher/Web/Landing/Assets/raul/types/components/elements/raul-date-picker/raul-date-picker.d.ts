import { EventEmitter } from '../../../stencil.core';
export declare class RaulDatePicker {
    el: HTMLElement;
    toggle: HTMLElement;
    dropdown: HTMLElement;
    top: boolean;
    picker: any;
    months: string[];
    weekdays: string[];
    weekdaysShort: string[];
    today: Date;
    focusedDate: Date;
    showFocusRing: boolean;
    /**
   * Can be `date` or `range`. Defaults to `date`
   */
    variant: 'inline' | 'date' | 'range';
    /**
   * If `true`, the date picker is disabled
   */
    disabled: boolean;
    /**
  * A string that will be shown above the date picker
  */
    label: string;
    /**
     * Optional hint text.
     */
    hint: string;
    /**
     * Makes the datepicker visually invalid and shows the feedback message.
     */
    error: string;
    /**
   * This is used internally by `raul-range-picker`
   */
    isInRangePicker: boolean;
    activeMonth: number;
    activeYear: number;
    yearsRangeStart: number;
    view: 'dates' | 'months' | 'years';
    showPicker: boolean;
    showPickerMutated: boolean;
    /**
  * A javascript Date object that represents the initial date of the date picker
  */
    initialDate: Date;
    date: Date;
    oldDate: Date;
    display: string;
    hasDate: boolean;
    hasSetADate: boolean;
    /**
  * Event emitted when a date is selected. Event.detail will be a javascript Date object
  */
    dateSelected: EventEmitter;
    /**
  * A javascript Date object that represents the initial start date of the range picker
  */
    initialStartDate: Date;
    /**
  * A javascript Date object that represents the initial end date of the range picker
  */
    initialEndDate: Date;
    startDate: Date;
    endDate: Date;
    keyboardStartDate: Date;
    selectedDate: Date;
    /**
  * Event emitted when a range is selected. Event.detail will be {startDate: Date, endDate: Date}, where `Date` will be a javascript Date object
  */
    rangeSelected: EventEmitter;
    mouseOverListenerAdded: boolean;
    /**
  * To be used internally by `raul-range-picker`
  */
    UNSAFE_dateSelected: EventEmitter;
    /**
  * Method used to programatically clear the picker
  */
    clearPicker(): Promise<void>;
    /**
  * A method to programatically close the picker
  */
    closePicker(): Promise<void>;
    /**
  * A method to programatically open the picker
  */
    openPicker(): Promise<void>;
    /**
  * This is used internally by `raul-range-picker`
  */
    setStateDate(date: Date): Promise<void>;
    /**
  * This is used internally by `raul-range-picker`
  */
    setPickerDate(date: Date): Promise<void>;
    /**
  * This is used internally by `raul-range-picker`
  */
    setStateStartDate(date: Date): Promise<void>;
    /**
  * This is used internally by `raul-range-picker`
  */
    setStateEndDate(date: Date): Promise<void>;
    /**
  * This is used internally by `raul-range-picker`
  */
    setPickerStartDate(date: Date): Promise<void>;
    /**
  * This is used internally by `raul-range-picker`
  */
    setPickerEndDate(date: Date): Promise<void>;
    /**
  * This is used internally by `raul-range-picker`
  */
    setDisplay(text: string): Promise<void>;
    /**
  * This is used internally by `raul-range-picker`
  */
    clearDisplay(): Promise<void>;
    /**
  * This is used internally by `raul-range-picker`
  */
    revertPicker(): Promise<void>;
    componentDidLoad(): void;
    handleShowPickerChange(newVal: boolean, oldVal: boolean): void;
    handleFocusDateChange(): void;
    handleActiveMonthChange(): void;
    handleActiveYearChange(): void;
    handleViewChange(newVal: any): void;
    handleKeyDown: (e: any) => void;
    handleHeaderKeyDown: (e: any) => void;
    handleYearFocusedKeyDown: (e: any) => void;
    componentDidRender(): void;
    handleMouseOver: (e: any) => void;
    checkViewportCollision(): void;
    togglePicker: () => void;
    reDraw: () => void;
    handleDateChange: (date: Date, sourceIsKeyboard: boolean) => void;
    addClassToSelected(): void;
    removeClassFromSelected(): void;
    addClass(date: Date, className: string): void;
    removeClass(date: Date, className: string): void;
    handleRangeChange: (date: Date, isSourceKeyboard: boolean) => void;
    syncDatePicker: () => void;
    syncRangePicker: () => void;
    revertDatePicker: () => void;
    handlePreviousMonth: () => void;
    handleNextMonth: () => void;
    setMonth: (index: number) => void;
    setYear: (year: number) => void;
    handleMonthYearClick: () => void;
    handlePreviousYear: () => void;
    handleNextYear: () => void;
    handlePreviousYearsRange: () => void;
    handleNextYearsRange: () => void;
    formatDateForDisplay: (date: Date) => string;
    formatRangeForDisplay: (firstDate: Date, secondDate: Date) => string;
    render(): any;
}
