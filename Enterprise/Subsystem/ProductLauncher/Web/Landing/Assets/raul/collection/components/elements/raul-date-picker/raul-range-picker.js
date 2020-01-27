import { h } from "@stencil/core";
export class RaulRangePicker {
    constructor() {
        this.months = ['January', 'February', 'March', 'April', 'May', 'June', 'July', 'August', 'September', 'October', 'November', 'December'];
        this.startDate = null;
        this.endDate = null;
        this.targetDate = null;
        this.formatDateForDisplay = (date) => `${this.months[date.getMonth()].substring(0, 3)} ${date.getDate()}, ${date.getFullYear()}`;
    }
    async handleDateSelected(e) {
        const date = e.detail;
        if (e.target.id === 'first') {
            if (!this.startDate) {
                await this.firstPicker.closePicker();
                this.startDate = date;
                this.targetDate = date;
                this.firstPicker.setDisplay(this.formatDateForDisplay(date));
                await this.secondPicker.setStateStartDate(date);
                await this.secondPicker.setPickerStartDate(date);
                await this.secondPicker.revertPicker();
                await this.secondPicker.openPicker();
            }
            else {
                await this.clearFirstPicker();
                await this.clearSecondPicker();
                this.startDate = date;
                this.targetDate = date;
                await this.firstPicker.setDisplay(this.formatDateForDisplay(date));
                await this.secondPicker.setDisplay('');
                await this.firstPicker.closePicker();
                await this.secondPicker.setStateStartDate(date);
                await this.secondPicker.setPickerStartDate(date);
                await this.secondPicker.revertPicker();
                await this.secondPicker.openPicker();
            }
        }
        if (e.target.id === 'second') {
            if (!this.endDate) {
                if (date > this.startDate) {
                    this.endDate = date;
                    this.firstPicker.setStateEndDate(date);
                    this.firstPicker.setPickerEndDate(date);
                    await this.secondPicker.setDisplay(this.formatDateForDisplay(date));
                }
                if (date < this.startDate) {
                    this.endDate = this.startDate;
                    this.startDate = date;
                    await this.secondPicker.setStateStartDate(this.startDate);
                    await this.secondPicker.setPickerStartDate(this.startDate);
                    await this.secondPicker.setStateEndDate(this.endDate);
                    await this.secondPicker.setPickerEndDate(this.endDate);
                    await this.firstPicker.setStateStartDate(this.startDate);
                    await this.firstPicker.setPickerStartDate(this.startDate);
                    await this.firstPicker.setStateEndDate(this.endDate);
                    await this.firstPicker.setPickerEndDate(this.endDate);
                    await this.firstPicker.setDisplay(this.formatDateForDisplay(this.startDate));
                    await this.secondPicker.setDisplay(this.formatDateForDisplay(this.endDate));
                    await this.firstPicker.revertPicker();
                    await this.secondPicker.revertPicker();
                }
            }
            else {
                if (date > this.targetDate) {
                    this.endDate = date;
                    await this.secondPicker.setStateStartDate(this.targetDate);
                    await this.secondPicker.setPickerStartDate(this.targetDate);
                    await this.secondPicker.setStateEndDate(this.endDate);
                    await this.secondPicker.setPickerEndDate(this.endDate);
                    await this.secondPicker.setDisplay(this.formatDateForDisplay(this.endDate));
                    await this.secondPicker.revertPicker();
                    await this.firstPicker.setStateStartDate(this.targetDate);
                    await this.firstPicker.setPickerStartDate(this.targetDate);
                    await this.firstPicker.setStateEndDate(this.endDate);
                    await this.firstPicker.setPickerEndDate(this.endDate);
                    await this.firstPicker.setDisplay(this.formatDateForDisplay(this.targetDate));
                    await this.firstPicker.revertPicker();
                }
                if (date < this.targetDate) {
                    this.endDate = this.targetDate;
                    this.startDate = date;
                    await this.secondPicker.setStateStartDate(this.startDate);
                    await this.secondPicker.setPickerStartDate(this.startDate);
                    await this.secondPicker.setStateEndDate(this.targetDate);
                    await this.secondPicker.setPickerEndDate(this.targetDate);
                    await this.secondPicker.setDisplay(this.formatDateForDisplay(this.targetDate));
                    await this.secondPicker.revertPicker();
                    await this.firstPicker.setStateStartDate(this.startDate);
                    await this.firstPicker.setPickerStartDate(this.startDate);
                    await this.firstPicker.setStateEndDate(this.targetDate);
                    await this.firstPicker.setPickerEndDate(this.targetDate);
                    await this.firstPicker.setDisplay(this.formatDateForDisplay(this.startDate));
                    await this.firstPicker.revertPicker();
                }
            }
        }
    }
    async clearFirstPicker() {
        await this.firstPicker.setStateStartDate(null);
        await this.firstPicker.setPickerStartDate(null);
        await this.firstPicker.setStateEndDate(null);
        await this.firstPicker.setPickerEndDate(null);
        await this.firstPicker.setStateEndDate(null);
        await this.firstPicker.setPickerDate(null);
    }
    async clearSecondPicker() {
        await this.secondPicker.setStateStartDate(null);
        await this.secondPicker.setPickerStartDate(null);
        await this.secondPicker.setStateEndDate(null);
        await this.secondPicker.setPickerEndDate(null);
        await this.secondPicker.setPickerDate(null);
    }
    render() {
        return (h("div", { style: { maxWidth: '550px' } },
            h("raul-content", null, "Start & End range [WIP]"),
            h("div", { class: 'flex flex-row justify-start' },
                h("div", { class: 'flex-1' },
                    h("raul-date-picker", { variant: 'range', ref: el => this.firstPicker = el, id: 'first', isInRangePicker: true, label: 'Start date' })),
                h("div", { class: 'flex-1 ml-2' },
                    h("raul-date-picker", { variant: 'range', ref: el => this.secondPicker = el, id: 'second', isInRangePicker: true, label: 'End date' })))));
    }
    static get is() { return "raul-range-picker"; }
    static get originalStyleUrls() { return {
        "$": ["raul-date-picker.scss"]
    }; }
    static get styleUrls() { return {
        "$": ["raul-date-picker.css"]
    }; }
    static get states() { return {
        "startDate": {},
        "endDate": {},
        "targetDate": {}
    }; }
    static get listeners() { return [{
            "name": "UNSAFE_dateSelected",
            "method": "handleDateSelected",
            "target": undefined,
            "capture": false,
            "passive": false
        }]; }
}
