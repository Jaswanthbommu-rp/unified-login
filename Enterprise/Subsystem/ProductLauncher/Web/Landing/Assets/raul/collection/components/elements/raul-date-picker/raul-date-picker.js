import { h } from "@stencil/core";
import Pikaday from 'pikaday';
import moment from 'moment';
//TEST COMMIT
export class RaulDatePicker {
    constructor() {
        this.top = false;
        this.months = ['January', 'February', 'March', 'April', 'May', 'June', 'July', 'August', 'September', 'October', 'November', 'December'];
        this.weekdays = ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'];
        this.weekdaysShort = ['S', 'M', 'T', 'W', 'T', 'F', 'S'];
        this.today = new Date();
        this.focusedDate = null;
        this.showFocusRing = false;
        /**
       * Can be `date` or `range`. Defaults to `date`
       */
        this.variant = 'date';
        /**
       * If `true`, the date picker is disabled
       */
        this.disabled = false;
        /**
      * A string that will be shown above the date picker
      */
        this.label = '';
        /**
       * This is used internally by `raul-range-picker`
       */
        this.isInRangePicker = false; // used internally by raul-range-picker
        this.activeMonth = (new Date()).getMonth();
        this.activeYear = (new Date()).getFullYear();
        this.yearsRangeStart = 2014;
        this.view = 'dates';
        this.showPicker = false;
        this.showPickerMutated = false;
        //variant='date', 'inline'
        /**
      * A javascript Date object that represents the initial date of the date picker
      */
        this.initialDate = null;
        this.date = new Date();
        this.oldDate = null;
        this.display = '';
        this.hasDate = false;
        this.hasSetADate = false;
        //variant='range'
        /**
      * A javascript Date object that represents the initial start date of the range picker
      */
        this.initialStartDate = null;
        /**
      * A javascript Date object that represents the initial end date of the range picker
      */
        this.initialEndDate = null;
        this.startDate = null;
        this.endDate = null;
        this.keyboardStartDate = new Date();
        this.selectedDate = new Date();
        this.mouseOverListenerAdded = false;
        this.handleKeyDown = e => {
            if (!this.showPicker && !(['Esc', 'Escape'].includes(e.key))) {
                e.stopPropagation();
            }
            if (this.variant === 'date' || this.variant === 'inline') {
                if (this.view === 'dates') {
                    if (!this.oldDate) {
                        this.oldDate = this.date;
                        this.addClassToSelected();
                    }
                    if (this.date.getMonth() === this.activeMonth) {
                        switch (e.key) {
                            case 'ArrowDown':
                                e.preventDefault();
                                //@ts-ignore
                                this.handleDateChange(moment(this.date).add(1, 'weeks')._d, true);
                                this.hasDate = false;
                                if (this.hasSetADate) {
                                    this.addClassToSelected();
                                }
                                break;
                            case 'ArrowUp':
                                e.preventDefault();
                                //@ts-ignore
                                this.handleDateChange(moment(this.date).subtract(1, 'weeks')._d, true);
                                this.hasDate = false;
                                if (this.hasSetADate) {
                                    this.addClassToSelected();
                                }
                                break;
                            case 'ArrowLeft':
                                e.preventDefault();
                                //@ts-ignore
                                this.handleDateChange(moment(this.date).subtract(1, 'days')._d, true);
                                this.hasDate = false;
                                if (this.hasSetADate) {
                                    this.addClassToSelected();
                                }
                                break;
                            case 'ArrowRight':
                                e.preventDefault();
                                //@ts-ignore
                                this.handleDateChange(moment(this.date).add(1, 'days')._d, true);
                                this.hasDate = false;
                                if (this.hasSetADate) {
                                    this.addClassToSelected();
                                }
                                break;
                            case 'Enter':
                                e.preventDefault();
                                this.handleDateChange(this.date, false);
                                this.hasDate = true;
                                this.addClassToSelected();
                        }
                    }
                    else {
                        this.date = new Date(this.activeYear, this.activeMonth, 1);
                        this.handleKeyDown(e);
                    }
                    return;
                }
                if (this.view === 'months') {
                    switch (e.key) {
                        case 'ArrowDown':
                            e.preventDefault();
                            // console.log(this.focusedDate || this.date)
                            //@ts-ignore
                            // this.handleDateChange(moment(this.date).add(4, 'months')._d, true)
                            this.focusedDate = moment(this.focusedDate || this.date).add(4, 'months')._d;
                            this.activeYear = this.focusedDate.getFullYear();
                            // this.hasDate = false
                            // this.
                            break;
                        case 'ArrowUp':
                            e.preventDefault();
                            //@ts-ignore
                            // this.handleDateChange(moment(this.date).subtract(4, 'months')._d, true)
                            this.focusedDate = moment(this.focusedDate || this.date).subtract(4, 'months')._d;
                            this.activeYear = this.focusedDate.getFullYear();
                            // this.hasDate = false
                            break;
                        case 'ArrowLeft':
                            e.preventDefault();
                            //@ts-ignore
                            // this.handleDateChange(moment(this.date).subtract(1, 'months')._d, true)
                            this.focusedDate = moment(this.focusedDate || this.date).subtract(1, 'months')._d;
                            this.activeYear = this.focusedDate.getFullYear();
                            // this.hasDate = false
                            break;
                        case 'ArrowRight':
                            e.preventDefault();
                            //@ts-ignore
                            // this.handleDateChange(moment(this.date).add(1, 'months')._d, true)
                            this.focusedDate = moment(this.focusedDate || this.date).add(1, 'months')._d;
                            this.activeYear = this.focusedDate.getFullYear();
                            // this.hasDate = false
                            break;
                        case 'Enter':
                            e.preventDefault();
                            this.handleDateChange(this.focusedDate, false);
                            this.view = 'dates';
                            // this.display = `${this.months[this.date.getMonth()]}, ${this.date.getFullYear()}`
                            setTimeout(() => { this.el.querySelector('.pika-single').focus(); }, 100);
                    }
                }
                if (this.view === 'years') {
                    switch (e.key) {
                        case 'ArrowDown':
                            e.preventDefault();
                            //@ts-ignore
                            // this.handleDateChange(moment(this.date).add(4, 'years')._d, true)
                            this.focusedDate = moment(this.focusedDate || this.date).add(4, 'years')._d;
                            if (this.focusedDate.getFullYear() < this.yearsRangeStart)
                                this.handlePreviousYearsRange();
                            if (this.focusedDate.getFullYear() >= (this.yearsRangeStart + 12))
                                this.handleNextYearsRange();
                            break;
                        case 'ArrowUp':
                            e.preventDefault();
                            //@ts-ignore
                            // this.handleDateChange(moment(this.date).subtract(4, 'years')._d, true)
                            this.focusedDate = moment(this.focusedDate || this.date).subtract(4, 'years')._d;
                            if (this.focusedDate.getFullYear() < this.yearsRangeStart)
                                this.handlePreviousYearsRange();
                            if (this.focusedDate.getFullYear() >= (this.yearsRangeStart + 12))
                                this.handleNextYearsRange();
                            break;
                        case 'ArrowLeft':
                            e.preventDefault();
                            //@ts-ignore
                            // this.handleDateChange(moment(this.date).subtract(1, 'years')._d, true)
                            this.focusedDate = moment(this.focusedDate || this.date).subtract(1, 'years')._d;
                            if (this.focusedDate.getFullYear() < this.yearsRangeStart)
                                this.handlePreviousYearsRange();
                            if (this.focusedDate.getFullYear() >= (this.yearsRangeStart + 12))
                                this.handleNextYearsRange();
                            break;
                        case 'ArrowRight':
                            e.preventDefault();
                            //@ts-ignore
                            // this.handleDateChange(moment(this.date).add(1, 'years')._d, true)
                            this.focusedDate = moment(this.focusedDate || this.date).add(1, 'years')._d;
                            if (this.focusedDate.getFullYear() < this.yearsRangeStart)
                                this.handlePreviousYearsRange();
                            if (this.focusedDate.getFullYear() >= (this.yearsRangeStart + 12))
                                this.handleNextYearsRange();
                            break;
                        case 'Enter':
                            e.preventDefault();
                            this.handleDateChange(this.focusedDate, false);
                            this.view = 'months';
                            // this.display = this.date.getFullYear().toString()
                            setTimeout(() => { this.el.querySelector('.r-date-picker__months-view__months-container').focus(); }, 100);
                    }
                }
            }
            if (this.variant === 'range') {
                if (this.view === 'dates') {
                    switch (e.key) {
                        case 'ArrowLeft':
                            e.preventDefault();
                            if (!this.startDate) {
                                //@ts-ignore
                                this.keyboardStartDate = moment(this.keyboardStartDate).subtract(1, 'days')._d;
                                this.picker.setDate(this.keyboardStartDate, true);
                                this.activeMonth = this.keyboardStartDate.getMonth();
                                this.activeYear = this.keyboardStartDate.getFullYear();
                                return;
                            }
                            if (this.startDate && !this.endDate) {
                                //@ts-ignore
                                this.keyboardStartDate = moment(this.keyboardStartDate).subtract(1, 'days')._d;
                                this.handleRangeChange(this.keyboardStartDate, true);
                                this.activeMonth = this.keyboardStartDate.getMonth();
                                this.activeYear = this.keyboardStartDate.getFullYear();
                                return;
                            }
                            if (this.startDate && this.endDate) {
                                // ???
                                //@ts-ignore
                                this.keyboardStartDate = moment(this.startDate).subtract(1, 'days')._d;
                                this.picker.setStartRange(null);
                                this.picker.setEndRange(null);
                                this.startDate = null;
                                this.endDate = null;
                                this.picker.setDate(this.keyboardStartDate, true);
                                this.activeMonth = this.keyboardStartDate.getMonth();
                                this.activeYear = this.keyboardStartDate.getFullYear();
                                // this.revertPicker()
                                this.syncRangePicker();
                                return;
                            }
                            break;
                        case 'ArrowRight':
                            e.preventDefault();
                            if (!this.startDate) {
                                //@ts-ignore
                                this.keyboardStartDate = moment(this.keyboardStartDate).add(1, 'days')._d;
                                this.picker.setDate(this.keyboardStartDate, true);
                                this.activeMonth = this.keyboardStartDate.getMonth();
                                this.activeYear = this.keyboardStartDate.getFullYear();
                                return;
                            }
                            if (this.startDate && !this.endDate) {
                                //@ts-ignore
                                this.keyboardStartDate = moment(this.keyboardStartDate).add(1, 'days')._d;
                                this.handleRangeChange(this.keyboardStartDate, true);
                                this.activeMonth = this.keyboardStartDate.getMonth();
                                this.activeYear = this.keyboardStartDate.getFullYear();
                                return;
                            }
                            if (this.startDate && this.endDate) {
                                //@ts-ignore
                                this.keyboardStartDate = moment(this.keyboardStartDate).add(1, 'days')._d;
                                this.picker.setStartRange(null);
                                this.picker.setEndRange(null);
                                this.startDate = null;
                                this.endDate = null;
                                this.picker.setDate(this.keyboardStartDate, true);
                                this.activeMonth = this.keyboardStartDate.getMonth();
                                this.activeYear = this.keyboardStartDate.getFullYear();
                                // this.revertPicker()
                                this.syncRangePicker();
                                return;
                            }
                            break;
                        case 'ArrowUp':
                            e.preventDefault();
                            if (!this.startDate) {
                                //@ts-ignore
                                this.keyboardStartDate = moment(this.keyboardStartDate).subtract(1, 'weeks')._d;
                                this.picker.setDate(this.keyboardStartDate, true);
                                this.activeMonth = this.keyboardStartDate.getMonth();
                                this.activeYear = this.keyboardStartDate.getFullYear();
                            }
                            if (this.startDate && !this.endDate) {
                                //@ts-ignore
                                this.keyboardStartDate = moment(this.keyboardStartDate).subtract(1, 'weeks')._d;
                                this.handleRangeChange(this.keyboardStartDate, true);
                                this.activeMonth = this.keyboardStartDate.getMonth();
                                this.activeYear = this.keyboardStartDate.getFullYear();
                                // this.syncRangePicker()
                                return;
                            }
                            if (this.startDate && this.endDate) {
                                // ???
                                //@ts-ignore
                                this.keyboardStartDate = moment(this.keyboardStartDate).subtract(1, 'weeks')._d;
                                this.picker.setStartRange(null);
                                this.picker.setEndRange(null);
                                this.startDate = null;
                                this.endDate = null;
                                this.picker.setDate(this.keyboardStartDate, true);
                                this.activeMonth = this.keyboardStartDate.getMonth();
                                this.activeYear = this.keyboardStartDate.getFullYear();
                                // this.revertPicker()
                                this.syncRangePicker();
                                return;
                            }
                            break;
                        case 'ArrowDown':
                            e.preventDefault();
                            if (!this.startDate) {
                                //@ts-ignore
                                this.keyboardStartDate = moment(this.keyboardStartDate).add(1, 'weeks')._d;
                                this.picker.setDate(this.keyboardStartDate, true);
                                this.activeMonth = this.keyboardStartDate.getMonth();
                                this.activeYear = this.keyboardStartDate.getFullYear();
                                return;
                            }
                            if (this.startDate && !this.endDate) {
                                //@ts-ignore
                                this.keyboardStartDate = moment(this.keyboardStartDate).add(1, 'weeks')._d;
                                this.handleRangeChange(this.keyboardStartDate, true);
                                this.activeMonth = this.keyboardStartDate.getMonth();
                                this.activeYear = this.keyboardStartDate.getFullYear();
                                return;
                            }
                            if (this.startDate && this.endDate) {
                                // ???
                                //@ts-ignore
                                this.keyboardStartDate = moment(this.keyboardStartDate).add(1, 'weeks')._d;
                                this.picker.setStartRange(null);
                                this.picker.setEndRange(null);
                                this.startDate = null;
                                this.endDate = null;
                                this.picker.setDate(this.keyboardStartDate, true);
                                this.activeMonth = this.keyboardStartDate.getMonth();
                                this.activeYear = this.keyboardStartDate.getFullYear();
                                // this.revertPicker()
                                this.syncRangePicker();
                                return;
                            }
                            break;
                        case 'Enter':
                            e.preventDefault();
                            if (!this.startDate) {
                                this.display = '';
                                this.startDate = this.keyboardStartDate;
                                return;
                            }
                            if (this.startDate && !this.endDate) {
                                if (this.keyboardStartDate > this.startDate) {
                                    this.endDate = this.keyboardStartDate;
                                    this.display = this.formatRangeForDisplay(this.startDate, this.endDate);
                                    this.showPicker = false;
                                    this.picker.setStartRange(this.startDate);
                                    this.rangeSelected.emit({
                                        startDate: this.startDate,
                                        endDate: this.endDate
                                    });
                                    this.toggle.focus();
                                    return;
                                }
                                if (this.keyboardStartDate < this.startDate) {
                                    this.endDate = this.startDate;
                                    this.startDate = this.keyboardStartDate;
                                    this.display = this.formatRangeForDisplay(this.startDate, this.endDate);
                                    this.showPicker = false;
                                    this.picker.setStartRange(this.startDate);
                                    this.rangeSelected.emit({
                                        startDate: this.startDate,
                                        endDate: this.endDate
                                    });
                                    this.toggle.focus();
                                    return;
                                }
                            }
                            break;
                    }
                    return;
                }
                if (this.view === 'months') {
                    switch (e.key) {
                        case 'ArrowLeft':
                            e.preventDefault();
                            //@ts-ignore
                            this.focusedDate = moment(this.focusedDate || this.keyboardStartDate).subtract(1, 'months')._d;
                            this.activeYear = this.focusedDate.getFullYear();
                            if (!this.startDate) {
                                //@ts-ignore
                                this.keyboardStartDate = moment(this.keyboardStartDate).subtract(1, 'month')._d;
                                this.picker.setDate(this.keyboardStartDate, true);
                                // this.activeMonth = this.keyboardStartDate.getMonth()
                                this.activeYear = this.keyboardStartDate.getFullYear();
                                return;
                            }
                            if (this.startDate && !this.endDate) {
                                //@ts-ignore
                                this.keyboardStartDate = moment(this.keyboardStartDate).subtract(1, 'months')._d;
                                this.handleRangeChange(this.keyboardStartDate, true);
                                // this.activeMonth = this.keyboardStartDate.getMonth()
                                this.activeYear = this.keyboardStartDate.getFullYear();
                                return;
                            }
                            if (this.startDate && !this.endDate) {
                                //@ts-ignore
                                this.keyboardStartDate = moment(this.startDate).subtract(1, 'months')._d;
                                this.picker.setStartRange(null);
                                this.picker.setEndRange(null);
                                this.startDate = null;
                                this.endDate = null;
                                this.picker.setDate(this.keyboardStartDate, true);
                                // this.activeMonth = this.keyboardStartDate.getMonth()
                                this.activeYear = this.keyboardStartDate.getFullYear();
                                // this.revertPicker()
                                this.syncRangePicker();
                                return;
                            }
                            break;
                        case 'ArrowRight':
                            e.preventDefault();
                            //@ts-ignore
                            this.focusedDate = moment(this.focusedDate || this.keyboardStartDate).add(1, 'months')._d;
                            this.activeYear = this.focusedDate.getFullYear();
                            if (!this.startDate) {
                                //@ts-ignore
                                this.keyboardStartDate = moment(this.keyboardStartDate).add(1, 'months')._d;
                                this.picker.setDate(this.keyboardStartDate, true);
                                // this.activeMonth = this.keyboardStartDate.getMonth()
                                this.activeYear = this.keyboardStartDate.getFullYear();
                                return;
                            }
                            if (this.startDate && !this.endDate) {
                                //@ts-ignore
                                this.keyboardStartDate = moment(this.keyboardStartDate).add(1, 'months')._d;
                                this.handleRangeChange(this.keyboardStartDate, true);
                                // this.activeMonth = this.keyboardStartDate.getMonth()
                                this.activeYear = this.keyboardStartDate.getFullYear();
                                return;
                            }
                            if (this.startDate && this.endDate) {
                                // ???
                                //@ts-ignore
                                this.keyboardStartDate = moment(this.keyboardStartDate).add(1, 'months')._d;
                                this.picker.setStartRange(null);
                                this.picker.setEndRange(null);
                                this.startDate = null;
                                this.endDate = null;
                                this.picker.setDate(this.keyboardStartDate, true);
                                // this.activeMonth = this.keyboardStartDate.getMonth()
                                this.activeYear = this.keyboardStartDate.getFullYear();
                                // this.revertPicker()
                                this.syncRangePicker();
                                return;
                            }
                            break;
                        case 'ArrowUp':
                            e.preventDefault();
                            //@ts-ignore
                            this.focusedDate = moment(this.focusedDate || this.keyboardStartDate).subtract(4, 'months')._d;
                            this.activeYear = this.focusedDate.getFullYear();
                            if (!this.startDate) {
                                //@ts-ignore
                                this.keyboardStartDate = moment(this.keyboardStartDate).subtract(4, 'months')._d;
                                this.picker.setDate(this.keyboardStartDate, true);
                                // this.activeMonth = this.keyboardStartDate.getMonth()
                                this.activeYear = this.keyboardStartDate.getFullYear();
                            }
                            if (this.startDate && !this.endDate) {
                                //@ts-ignore
                                this.keyboardStartDate = moment(this.keyboardStartDate).subtract(4, 'months')._d;
                                this.handleRangeChange(this.keyboardStartDate, true);
                                // this.activeMonth = this.keyboardStartDate.getMonth()
                                this.activeYear = this.keyboardStartDate.getFullYear();
                                // this.syncRangePicker()
                                return;
                            }
                            if (this.startDate && this.endDate) {
                                // ???
                                //@ts-ignore
                                this.keyboardStartDate = moment(this.keyboardStartDate).subtract(4, 'months')._d;
                                this.picker.setStartRange(null);
                                this.picker.setEndRange(null);
                                this.startDate = null;
                                this.endDate = null;
                                this.picker.setDate(this.keyboardStartDate, true);
                                // this.activeMonth = this.keyboardStartDate.getMonth()
                                this.activeYear = this.keyboardStartDate.getFullYear();
                                // this.revertPicker()
                                this.syncRangePicker();
                                return;
                            }
                            break;
                        case 'ArrowDown':
                            e.preventDefault();
                            //@ts-ignore
                            this.focusedDate = moment(this.focusedDate || this.keyboardStartDate).add(4, 'months')._d;
                            this.activeYear = this.focusedDate.getFullYear();
                            if (!this.startDate) {
                                //@ts-ignore
                                this.keyboardStartDate = moment(this.keyboardStartDate).add(4, 'months')._d;
                                this.picker.setDate(this.keyboardStartDate, true);
                                // this.activeMonth = this.keyboardStartDate.getMonth()
                                this.activeYear = this.keyboardStartDate.getFullYear();
                                return;
                            }
                            if (this.startDate && !this.endDate) {
                                //@ts-ignore
                                this.keyboardStartDate = moment(this.keyboardStartDate).add(4, 'months')._d;
                                this.handleRangeChange(this.keyboardStartDate, true);
                                // this.activeMonth = this.keyboardStartDate.getMonth()
                                this.activeYear = this.keyboardStartDate.getFullYear();
                                return;
                            }
                            if (this.startDate && this.endDate) {
                                // ???
                                //@ts-ignore
                                this.keyboardStartDate = moment(this.keyboardStartDate).add(4, 'months')._d;
                                this.picker.setStartRange(null);
                                this.picker.setEndRange(null);
                                this.startDate = null;
                                this.endDate = null;
                                this.picker.setDate(this.keyboardStartDate, true);
                                // this.activeMonth = this.keyboardStartDate.getMonth()
                                this.activeYear = this.keyboardStartDate.getFullYear();
                                // this.revertPicker()
                                this.syncRangePicker();
                                return;
                            }
                            break;
                        case 'Enter':
                            e.preventDefault();
                            this.view = 'dates';
                            this.activeMonth = this.focusedDate.getMonth();
                            this.selectedDate = this.focusedDate;
                            setTimeout(() => { this.el.querySelector('.pika-single').focus(); }, 100);
                    }
                    return;
                }
                if (this.view === 'years') {
                    switch (e.key) {
                        case 'ArrowLeft':
                            e.preventDefault();
                            if (!this.startDate) {
                                //@ts-ignore
                                this.keyboardStartDate = moment(this.keyboardStartDate).subtract(1, 'year')._d;
                                this.picker.setDate(this.keyboardStartDate, true);
                                this.activeMonth = this.keyboardStartDate.getMonth();
                                //@ts-ignore
                                // this.handleDateChange(moment(this.date).subtract(1, 'years')._d, true)
                                this.focusedDate = this.keyboardStartDate;
                                if (this.focusedDate.getFullYear() < this.yearsRangeStart)
                                    this.handlePreviousYearsRange();
                                if (this.focusedDate.getFullYear() >= (this.yearsRangeStart + 12))
                                    this.handleNextYearsRange();
                                return;
                            }
                            if (this.startDate && !this.endDate) {
                                //@ts-ignore
                                this.keyboardStartDate = moment(this.keyboardStartDate).subtract(1, 'year')._d;
                                this.handleRangeChange(this.keyboardStartDate, true);
                                this.activeMonth = this.keyboardStartDate.getMonth();
                                this.focusedDate = this.keyboardStartDate;
                                if (this.focusedDate.getFullYear() < this.yearsRangeStart)
                                    this.handlePreviousYearsRange();
                                if (this.focusedDate.getFullYear() >= (this.yearsRangeStart + 12))
                                    this.handleNextYearsRange();
                                return;
                            }
                            if (this.startDate && !this.endDate) {
                                //@ts-ignore
                                this.keyboardStartDate = moment(this.startDate).subtract(1, 'year')._d;
                                this.picker.setStartRange(null);
                                this.picker.setEndRange(null);
                                this.startDate = null;
                                this.endDate = null;
                                this.picker.setDate(this.keyboardStartDate, true);
                                this.activeMonth = this.keyboardStartDate.getMonth();
                                this.focusedDate = this.keyboardStartDate;
                                if (this.focusedDate.getFullYear() < this.yearsRangeStart)
                                    this.handlePreviousYearsRange();
                                if (this.focusedDate.getFullYear() >= (this.yearsRangeStart + 12))
                                    this.handleNextYearsRange(); // this.revertPicker()
                                this.syncRangePicker();
                                return;
                            }
                            break;
                        case 'ArrowRight':
                            e.preventDefault();
                            if (!this.startDate) {
                                //@ts-ignore
                                this.keyboardStartDate = moment(this.keyboardStartDate).add(1, 'year')._d;
                                this.picker.setDate(this.keyboardStartDate, true);
                                this.activeMonth = this.keyboardStartDate.getMonth();
                                this.focusedDate = this.keyboardStartDate;
                                if (this.focusedDate.getFullYear() < this.yearsRangeStart)
                                    this.handlePreviousYearsRange();
                                if (this.focusedDate.getFullYear() >= (this.yearsRangeStart + 12))
                                    this.handleNextYearsRange();
                                return;
                            }
                            if (this.startDate && !this.endDate) {
                                //@ts-ignore
                                this.keyboardStartDate = moment(this.keyboardStartDate).add(1, 'year')._d;
                                this.handleRangeChange(this.keyboardStartDate, true);
                                this.activeMonth = this.keyboardStartDate.getMonth();
                                this.focusedDate = this.keyboardStartDate;
                                if (this.focusedDate.getFullYear() < this.yearsRangeStart)
                                    this.handlePreviousYearsRange();
                                if (this.focusedDate.getFullYear() >= (this.yearsRangeStart + 12))
                                    this.handleNextYearsRange();
                                return;
                            }
                            if (this.startDate && this.endDate) {
                                // ???
                                //@ts-ignore
                                this.keyboardStartDate = moment(this.keyboardStartDate).add(1, 'year')._d;
                                this.picker.setStartRange(null);
                                this.picker.setEndRange(null);
                                this.startDate = null;
                                this.endDate = null;
                                this.picker.setDate(this.keyboardStartDate, true);
                                this.activeMonth = this.keyboardStartDate.getMonth();
                                this.focusedDate = this.keyboardStartDate;
                                if (this.focusedDate.getFullYear() < this.yearsRangeStart)
                                    this.handlePreviousYearsRange();
                                if (this.focusedDate.getFullYear() >= (this.yearsRangeStart + 12))
                                    this.handleNextYearsRange(); // this.revertPicker()
                                this.syncRangePicker();
                                return;
                            }
                            break;
                        case 'ArrowUp':
                            e.preventDefault();
                            if (!this.startDate) {
                                //@ts-ignore
                                this.keyboardStartDate = moment(this.keyboardStartDate).subtract(4, 'years')._d;
                                this.picker.setDate(this.keyboardStartDate, true);
                                this.activeMonth = this.keyboardStartDate.getMonth();
                                this.focusedDate = this.keyboardStartDate;
                                if (this.focusedDate.getFullYear() < this.yearsRangeStart)
                                    this.handlePreviousYearsRange();
                                if (this.focusedDate.getFullYear() >= (this.yearsRangeStart + 12))
                                    this.handleNextYearsRange();
                            }
                            if (this.startDate && !this.endDate) {
                                //@ts-ignore
                                this.keyboardStartDate = moment(this.keyboardStartDate).subtract(4, 'years')._d;
                                this.handleRangeChange(this.keyboardStartDate, true);
                                this.activeMonth = this.keyboardStartDate.getMonth();
                                this.focusedDate = this.keyboardStartDate;
                                if (this.focusedDate.getFullYear() < this.yearsRangeStart)
                                    this.handlePreviousYearsRange();
                                if (this.focusedDate.getFullYear() >= (this.yearsRangeStart + 12))
                                    this.handleNextYearsRange(); // this.syncRangePicker()
                                return;
                            }
                            if (this.startDate && this.endDate) {
                                // ???
                                //@ts-ignore
                                this.keyboardStartDate = moment(this.keyboardStartDate).subtract(4, 'years')._d;
                                this.picker.setStartRange(null);
                                this.picker.setEndRange(null);
                                this.startDate = null;
                                this.endDate = null;
                                this.picker.setDate(this.keyboardStartDate, true);
                                this.activeMonth = this.keyboardStartDate.getMonth();
                                this.focusedDate = this.keyboardStartDate;
                                if (this.focusedDate.getFullYear() < this.yearsRangeStart)
                                    this.handlePreviousYearsRange();
                                if (this.focusedDate.getFullYear() >= (this.yearsRangeStart + 12))
                                    this.handleNextYearsRange(); // this.revertPicker()
                                this.syncRangePicker();
                                return;
                            }
                            break;
                        case 'ArrowDown':
                            e.preventDefault();
                            if (!this.startDate) {
                                //@ts-ignore
                                this.keyboardStartDate = moment(this.keyboardStartDate).add(4, 'years')._d;
                                this.picker.setDate(this.keyboardStartDate, true);
                                this.activeMonth = this.keyboardStartDate.getMonth();
                                this.focusedDate = this.keyboardStartDate;
                                if (this.focusedDate.getFullYear() < this.yearsRangeStart)
                                    this.handlePreviousYearsRange();
                                if (this.focusedDate.getFullYear() >= (this.yearsRangeStart + 12))
                                    this.handleNextYearsRange();
                                return;
                            }
                            if (this.startDate && !this.endDate) {
                                //@ts-ignore
                                this.keyboardStartDate = moment(this.keyboardStartDate).add(4, 'years')._d;
                                this.handleRangeChange(this.keyboardStartDate, true);
                                this.activeMonth = this.keyboardStartDate.getMonth();
                                this.focusedDate = this.keyboardStartDate;
                                if (this.focusedDate.getFullYear() < this.yearsRangeStart)
                                    this.handlePreviousYearsRange();
                                if (this.focusedDate.getFullYear() >= (this.yearsRangeStart + 12))
                                    this.handleNextYearsRange();
                                return;
                            }
                            if (this.startDate && this.endDate) {
                                // ???
                                //@ts-ignore
                                this.keyboardStartDate = moment(this.keyboardStartDate).add(4, 'years')._d;
                                this.picker.setStartRange(null);
                                this.picker.setEndRange(null);
                                this.startDate = null;
                                this.endDate = null;
                                this.picker.setDate(this.keyboardStartDate, true);
                                this.activeMonth = this.keyboardStartDate.getMonth();
                                this.focusedDate = this.keyboardStartDate;
                                if (this.focusedDate.getFullYear() < this.yearsRangeStart)
                                    this.handlePreviousYearsRange();
                                if (this.focusedDate.getFullYear() >= (this.yearsRangeStart + 12))
                                    this.handleNextYearsRange(); // this.revertPicker()
                                this.syncRangePicker();
                                return;
                            }
                            break;
                        case 'Enter':
                            e.preventDefault();
                            this.view = 'months';
                            this.activeYear = this.focusedDate.getFullYear();
                            setTimeout(() => { this.el.querySelector('.r-date-picker__months-view__months-container').focus(); }, 100);
                    }
                }
            }
        };
        this.handleHeaderKeyDown = e => {
            if (!this.showPicker && !(['Esc', 'Escape'].includes(e.key))) {
                e.stopPropagation();
            }
            switch (e.key) {
                case 'ArrowLeft':
                    e.preventDefault();
                    if (this.view === 'dates') {
                        this.handlePreviousMonth();
                    }
                    break;
                case 'ArrowRight':
                    e.preventDefault();
                    if (this.view === 'dates') {
                        this.handleNextMonth();
                    }
                    break;
                case 'Enter':
                    e.preventDefault();
                    this.handleMonthYearClick();
                    break;
                case 'ArrowDown':
                    e.preventDefault();
                    this.el.querySelector('.pika-single').focus();
            }
        };
        this.handleYearFocusedKeyDown = e => {
            e.preventDefault();
            if (!this.showPicker && !(['Esc', 'Escape'].includes(e.key))) {
                e.stopPropagation();
            }
            switch (e.key) {
                case 'Enter':
                    e.preventDefault();
                    this.handleMonthYearClick();
                    break;
                case 'ArrowLeft':
                    //@ts-ignore
                    this.handleDateChange(moment(this.date).subtract(1, 'years')._d, true);
                    break;
                case 'ArrowRight':
                    //@ts-ignore
                    this.handleDateChange(moment(this.date).add(1, 'years')._d, true);
            }
        };
        this.handleMouseOver = e => {
            if (e.target.tagName === 'BUTTON') {
                const date = new Date(e.target.dataset.pikaYear, e.target.dataset.pikaMonth, e.target.dataset.pikaDay);
                if (this.startDate && !this.endDate) {
                    if (date > this.startDate) {
                        this.picker.setStartRange(this.startDate);
                        this.picker.setEndRange(date);
                        this.syncRangePicker();
                        this.reDraw();
                    }
                    if (date < this.startDate) {
                        this.picker.setEndRange(this.startDate);
                        this.picker.setStartRange(date);
                        this.syncRangePicker();
                        this.reDraw();
                    }
                }
            }
        };
        this.togglePicker = () => {
            this.showPicker = !this.showPicker || (this.variant === 'inline');
            this.revertDatePicker();
        };
        this.reDraw = () => {
            this.showPicker = false;
            this.showPicker = true;
        };
        this.handleDateChange = (date, sourceIsKeyboard) => {
            if (this.variant === 'date' || this.variant === 'inline') {
                this.date = date;
                if (!sourceIsKeyboard) {
                    this.showPicker = !this.showPicker;
                    this.dateSelected.emit(date);
                    this.toggle.focus();
                    this.oldDate = date;
                    this.hasDate = true;
                    this.hasSetADate = true;
                    this.display = this.formatDateForDisplay(date);
                    this.reDraw();
                }
                else {
                    this.picker.setDate(this.date, true);
                    if (this.view === 'months') {
                        // this.display = `${this.months[this.date.getMonth()]}, ${this.date.getFullYear()}`
                    }
                    if (this.view === 'years') {
                        // this.display = `${this.date.getFullYear()}`
                    }
                }
                this.revertDatePicker();
            }
        };
        this.handleRangeChange = (date, isSourceKeyboard) => {
            this.UNSAFE_dateSelected.emit(date);
            if (date.getFullYear() < this.yearsRangeStart)
                this.handlePreviousYearsRange();
            if (date.getFullYear() >= (this.yearsRangeStart + 12))
                this.handleNextYearsRange();
            if (!this.startDate && !this.endDate) {
                this.startDate = date;
                this.picker.setStartRange(date);
                this.syncRangePicker();
                this.reDraw();
                return;
            }
            if (this.startDate && !this.endDate) {
                if (!isSourceKeyboard) {
                    if (date > this.startDate) {
                        this.endDate = date;
                        this.picker.setEndRange(date);
                        this.syncRangePicker();
                        this.display = this.formatRangeForDisplay(this.startDate, this.endDate);
                        this.addClass(this.endDate, 'is-endrange');
                        this.addClass(this.startDate, 'is-startrange');
                        this.showPicker = false;
                        this.rangeSelected.emit({
                            startDate: this.startDate,
                            endDate: this.endDate
                        });
                        this.toggle.focus();
                        return;
                    }
                    if (date < this.startDate) {
                        this.endDate = this.startDate;
                        this.startDate = date;
                        this.picker.setStartRange(this.startDate);
                        this.picker.setEndRange(this.endDate);
                        this.display = this.formatRangeForDisplay(this.startDate, this.endDate);
                        this.showPicker = false;
                        this.addClass(this.endDate, 'is-endrange');
                        this.addClass(this.startDate, 'is-startrange');
                        this.rangeSelected.emit({
                            startDate: this.startDate,
                            endDate: this.endDate
                        });
                        this.toggle.focus();
                        return;
                    }
                }
                if (isSourceKeyboard) {
                    if (this.startDate && !this.endDate) {
                        if (date > this.startDate) {
                            this.picker.setStartRange(this.startDate);
                            this.picker.setEndRange(date);
                            // this.endDate = date
                            this.syncRangePicker();
                            if (this.view === 'dates')
                                this.reDraw();
                            this.addClass(this.startDate, 'is-startrange');
                            this.removeClass(this.startDate, 'is-inrange');
                            this.addClass(date, 'is-endrange');
                            this.removeClass(date, 'is-inrange');
                            return;
                        }
                        if (date < this.startDate) {
                            this.picker.setEndRange(this.startDate);
                            this.picker.setStartRange(date);
                            this.syncRangePicker();
                            if (this.view === 'dates')
                                this.reDraw();
                            this.addClass(this.startDate, 'is-endrange');
                            this.removeClass(this.date, 'is-inrange');
                            this.addClass(date, 'is-startrange');
                            this.removeClass(date, 'is-inrange');
                            return;
                        }
                    }
                }
            }
            if (this.startDate && this.endDate) {
                if (!isSourceKeyboard) {
                    this.startDate = date;
                    this.endDate = null;
                    this.picker.setStartRange(date);
                    this.picker.setEndRange(null);
                    this.display = '';
                    this.syncRangePicker();
                    this.reDraw();
                    return;
                }
            }
            this.revertDatePicker();
        };
        this.syncDatePicker = () => {
            this.picker.gotoYear(this.activeYear);
            this.picker.gotoMonth(this.activeMonth);
        };
        this.syncRangePicker = () => {
            this.picker.gotoYear(this.activeYear);
            this.picker.gotoMonth(this.activeMonth);
        };
        this.revertDatePicker = () => {
            this.activeMonth = this.variant === 'range' ? this.startDate ? this.startDate.getMonth() : this.activeMonth : this.date.getMonth();
            this.activeYear = this.variant === 'range' ? this.startDate ? this.startDate.getFullYear() : this.activeYear : this.date.getFullYear();
            this.syncDatePicker();
        };
        this.handlePreviousMonth = () => {
            if (this.activeMonth === 0) {
                this.activeMonth = 11;
                this.activeYear--;
            }
            else {
                this.activeMonth--;
            }
            this.syncDatePicker();
        };
        this.handleNextMonth = () => {
            if (this.activeMonth === 11) {
                this.activeMonth = 0;
                this.activeYear++;
            }
            else {
                this.activeMonth++;
            }
            this.syncDatePicker();
        };
        this.setMonth = (index) => {
            this.activeMonth = index;
            // this.display = this.months[this.activeMonth]
            this.syncDatePicker();
            this.view = 'dates';
        };
        this.setYear = (year) => {
            this.activeYear = year;
            this.activeMonth = null;
            // this.display = this.activeYear.toString()
            this.syncDatePicker();
            this.view = 'months';
        };
        this.handleMonthYearClick = () => {
            if (this.view === 'dates') {
                // if (this.variant === 'date') this.display = `${this.months[this.activeMonth]}, ${this.date.getFullYear()}`
                this.view = 'months';
                setTimeout(() => { this.el.querySelector('.r-date-picker__months-view__months-container').focus(); }, 100);
            }
            else if (this.view === 'months') {
                // if (this.variant === 'date') this.display = this.activeYear.toString()
                this.view = 'years';
                setTimeout(() => { this.el.querySelector('.r-date-picker__years-view__years-container').focus(); }, 100);
            }
        };
        this.handlePreviousYear = () => {
            this.activeYear--;
        };
        this.handleNextYear = () => {
            this.activeYear++;
        };
        this.handlePreviousYearsRange = () => {
            this.yearsRangeStart -= 12;
        };
        this.handleNextYearsRange = () => {
            this.yearsRangeStart += 12;
        };
        this.formatDateForDisplay = (date) => `${this.months[date.getMonth()].substring(0, 3)} ${date.getDate()}, ${date.getFullYear()}`;
        this.formatRangeForDisplay = (firstDate, secondDate) => firstDate.getFullYear() === secondDate.getFullYear() ?
            `${this.months[firstDate.getMonth()].substring(0, 3)} ${firstDate.getDate()} - ${this.months[secondDate.getMonth()].substring(0, 3)} ${secondDate.getDate()}, ${secondDate.getFullYear()}`
            :
                `${this.months[firstDate.getMonth()].substring(0, 3)} ${firstDate.getDate()}, ${firstDate.getFullYear()} - ${this.months[secondDate.getMonth()].substring(0, 3)} ${secondDate.getDate()}, ${secondDate.getFullYear()}`;
    }
    /**
  * Method used to programatically clear the picker
  */
    async clearPicker() {
        if (this.variant === 'range') {
            this.display = '';
            this.startDate = null;
            this.endDate = null;
            this.picker.setStartRange(null);
            this.picker.setEndRange(null);
            this.picker.setDate(null);
            this.keyboardStartDate = new Date();
            // this.picker.clear()
            return;
        }
        else {
            this.display = '';
            this.date = new Date();
            this.picker.setDate(null);
            this.oldDate = null;
            this.activeMonth = (new Date()).getMonth();
            this.activeYear = (new Date()).getFullYear();
            // this.picker.clear()
            return;
        }
    }
    /**
  * A method to programatically close the picker
  */
    async closePicker() {
        this.showPicker = false;
    }
    /**
  * A method to programatically open the picker
  */
    async openPicker() {
        this.showPicker = true;
    }
    /**
  * This is used internally by `raul-range-picker`
  */
    async setStateDate(date) {
        this.date = date;
    }
    /**
  * This is used internally by `raul-range-picker`
  */
    async setPickerDate(date) {
        this.picker.setDate(date);
    }
    /**
  * This is used internally by `raul-range-picker`
  */
    async setStateStartDate(date) {
        this.startDate = date;
    }
    /**
  * This is used internally by `raul-range-picker`
  */
    async setStateEndDate(date) {
        this.endDate = date;
    }
    /**
  * This is used internally by `raul-range-picker`
  */
    async setPickerStartDate(date) {
        this.picker.setStartRange(date);
    }
    /**
  * This is used internally by `raul-range-picker`
  */
    async setPickerEndDate(date) {
        this.picker.setEndRange(date);
    }
    /**
  * This is used internally by `raul-range-picker`
  */
    async setDisplay(text) {
        this.display = text;
    }
    /**
  * This is used internally by `raul-range-picker`
  */
    async clearDisplay() {
        this.display = '';
    }
    /**
  * This is used internally by `raul-range-picker`
  */
    async revertPicker() {
        this.revertDatePicker();
    }
    componentDidLoad() {
        this.picker = new Pikaday({
            onSelect: this.variant === 'range' ? (date, isSourceKeyboard) => this.handleRangeChange(date, isSourceKeyboard) : (date, sourceIsKeyboard) => this.handleDateChange(date, sourceIsKeyboard),
            field: this.el.querySelector('.r-date-picker__header'),
            bound: false,
            showDaysInNextAndPreviousMonths: true,
            enableSelectionDaysInNextAndPreviousMonths: false,
            defaultDate: this.date,
            i18n: {
                months: this.months,
                weekdays: this.weekdays,
                weekdaysShort: this.weekdaysShort
            },
            keyboardInput: false,
        });
        if (this.variant !== 'inline') {
            //click away listener to close picker
            window.addEventListener('click', (e) => {
                // @ts-ignore
                if (this.showPicker && !this.dropdown.contains(e.target) && !this.toggle.contains(e.target)) {
                    this.showPicker = false;
                    this.view = 'dates';
                    this.display = this.variant === 'date' ? this.hasSetADate ? this.hasDate ? this.formatDateForDisplay(this.date) : this.oldDate ? this.formatDateForDisplay(this.oldDate) : '' : '' : this.display;
                }
            }, true);
        }
        if ((this.variant === 'inline')) {
            this.activeMonth = this.initialDate.getMonth();
            this.activeYear = this.initialDate.getFullYear();
            this.handleDateChange(this.initialDate || new Date(), false);
            this.showPicker = true;
        }
        if ((this.variant === 'date') && this.initialDate) {
            this.activeMonth = this.initialDate.getMonth();
            this.activeYear = this.initialDate.getFullYear();
            this.handleDateChange(this.initialDate, false);
            this.showPicker = false;
        }
        if ((this.variant === 'range') && this.initialStartDate && this.initialEndDate) {
            this.startDate = this.initialStartDate;
            this.endDate = this.initialEndDate;
            this.picker.setStartRange(this.initialStartDate);
            this.picker.setEndRange(this.initialEndDate);
            this.display = this.formatRangeForDisplay(this.initialStartDate, this.initialEndDate);
            this.addClass(this.endDate, 'is-endrange');
            this.addClass(this.startDate, 'is-startrange');
        }
    }
    handleShowPickerChange(newVal, oldVal) {
        if (newVal && !oldVal) {
            this.showPickerMutated = true;
        }
        if (oldVal) {
            this.top = false;
        }
        if (newVal) {
            if (this.variant === 'date' || this.variant === 'inline') {
                if (this.oldDate) {
                    this.date = this.oldDate;
                    this.removeClassFromSelected();
                    this.picker.setDate(this.date, true);
                    this.hasDate = true;
                }
            }
            const el = this.el.querySelector('.pika-single');
            el.setAttribute('tabindex', '0');
            el.onkeydown = this.handleKeyDown;
            setTimeout(() => {
                el.focus();
            }, 1);
            if (this.variant === 'range') {
                setTimeout(() => {
                    el.focus();
                    if (this.startDate && this.endDate) {
                        this.addClass(this.startDate, 'is-startrange');
                        this.addClass(this.endDate, 'is-endrange');
                        this.removeClass(this.endDate, 'is-inrange');
                    }
                }, 1);
            }
        }
    }
    handleFocusDateChange() {
        this.showFocusRing = true;
    }
    handleActiveMonthChange() {
        if (this.variant === 'range' && this.keyboardStartDate) {
            this.syncRangePicker();
            if (!this.startDate || !this.endDate) {
                setTimeout(() => {
                    if (this.keyboardStartDate > this.startDate) {
                        if (this.startDate)
                            this.addClass(this.startDate, 'is-startrange');
                        this.addClass(this.keyboardStartDate, 'is-endrange');
                    }
                    if (this.keyboardStartDate < this.startDate) {
                        if (this.startDate)
                            this.addClass(this.startDate, 'is-endrange');
                        this.addClass(this.keyboardStartDate, 'is-startrange');
                    }
                    // this.reDraw()
                }, 1);
            }
        }
    }
    handleActiveYearChange() {
        if (this.variant === 'range') {
            this.syncRangePicker();
            setTimeout(() => {
                this.addClass(this.startDate, 'is-startrange');
                this.addClass(this.endDate, 'is-endrange');
                // this.reDraw()
            }, 1);
        }
    }
    handleViewChange(newVal) {
        this.showFocusRing = false;
        if (newVal === 'dates') {
            this.addClass(this.startDate, 'is-startrange');
            this.addClass(this.endDate, 'is-endrange');
        }
    }
    componentDidRender() {
        if (this.variant === 'range') {
            const table = this.el.querySelector('table');
            if (table)
                table.addEventListener('mouseover', this.handleMouseOver);
        }
        if (this.showPicker && this.showPickerMutated) {
            this.checkViewportCollision();
        }
        this.showPickerMutated = false;
    }
    checkViewportCollision() {
        const rect = this.dropdown.getBoundingClientRect();
        this.top = rect.bottom > window.innerHeight;
    }
    addClassToSelected() {
        if ((this.oldDate.getMonth() === this.activeMonth) && (this.oldDate.getFullYear() === this.activeYear)) {
            this.el.querySelector(`td[data-day='${this.oldDate.getDate()}']:not(.is-outside-current-month)`).classList.add('is-current-selection');
        }
    }
    removeClassFromSelected() {
        this.el.querySelector(`td[data-day='${this.oldDate.getDate()}']`).classList.remove('is-current-selection');
    }
    addClass(date, className) {
        if (date) {
            if ((date.getMonth() === this.activeMonth) && (date.getFullYear() === this.activeYear)) {
                this.el.querySelector(`td[data-day='${date.getDate()}']:not(.is-outside-current-month)`).classList.add(className);
            }
        }
    }
    removeClass(date, className) {
        if ((date.getMonth() === this.activeMonth) && (date.getFullYear() === this.activeYear)) {
            this.el.querySelector(`td[data-day='${date.getDate()}']:not(.is-outside-current-month)`).classList.remove(className);
        }
    }
    //test
    render() {
        const PickerToggle = () => (h("div", { class: {
                'r-date-picker__toggle': true,
                'r-date-picker__toggle--disabled': this.disabled,
                'r-date-picker__toggle--open': this.variant === 'inline' || this.showPicker
            }, ref: el => this.toggle = el, onClick: this.togglePicker, tabindex: !this.disabled && 0, onKeyDown: !this.disabled && (e => { if (!this.showPicker && (e.key === 'Enter')) {
                this.togglePicker();
            } }) },
            h("div", { class: 'r-date-picker__toggle__value' }, this.display),
            h("div", { class: 'r-date-picker__toggle__icon' },
                h("raul-icon", { icon: 'calendar-2' }))));
        const PickerHeader = () => (h("div", null,
            h("div", { class: 'r-date-picker__header' },
                h("raul-icon", { icon: 'arrow-left-v', onClick: this.handlePreviousMonth }),
                h("div", { role: 'button', class: 'r-date-picker__header__month-year-label', onClick: this.handleMonthYearClick, tabindex: 0, onKeyDown: this.handleHeaderKeyDown },
                    h("div", { class: 'r-date-picker__header__month-year-label__focus-utility', tabindex: -1 }, `${this.months[this.activeMonth]} ${this.activeYear}`)),
                h("raul-icon", { icon: 'arrow-right-v', onClick: this.handleNextMonth }))));
        const DatePicker = () => (h("div", { class: {
                'r-date-picker__container': true,
                'r-date-picker__container--show': (this.showPicker || this.variant === 'inline') && this.view === 'dates',
                'r-date-picker__container--has-date': this.hasDate
            } },
            h(PickerHeader, null)));
        const MonthPicker = () => (h("div", { class: 'r-date-picker__months-view' },
            h("div", { class: 'r-date-picker__months-view__header' },
                h("raul-icon", { icon: 'arrow-left-v', onClick: this.handlePreviousYear }),
                h("div", { class: 'r-date-picker__months-view__year-label', onClick: this.handleMonthYearClick, tabindex: 0, onKeyDown: this.handleYearFocusedKeyDown },
                    h("div", { class: 'r-date-picker__months-view__year-label__focus-utility' }, this.activeYear)),
                h("raul-icon", { icon: 'arrow-right-v', onClick: this.handleNextYear })),
            h("div", { class: 'r-date-picker__months-view__months-container', tabindex: 0, onKeyDown: this.handleKeyDown }, this.months.map((month, index) => (h("div", { class: {
                    'r-date-picker__months-view__single-month-container': true,
                    // 'r-date-picker__months-view__single-month-container--focused': true
                    'r-date-picker__months-view__single-month-container--focused': this.showFocusRing && (!!this.focusedDate && (this.activeYear === this.focusedDate.getFullYear())) && (index === this.focusedDate.getMonth())
                } },
                h("div", { class: {
                        'r-date-picker__months-view__month': true,
                        'r-date-picker__months-view__month--selected': this.variant === 'range' ? (this.activeYear === this.selectedDate.getFullYear()) && (index === this.selectedDate.getMonth()) : (this.activeYear === this.date.getFullYear()) && (index === this.activeMonth),
                        'r-date-picker__months-view__month--current': (this.activeYear === this.today.getFullYear()) && (index === this.today.getMonth()),
                    }, onClick: () => this.setMonth(index) }, month.slice(0, 3).toUpperCase())))))));
        const YearPicker = () => (h("div", { class: 'r-date-picker__years-view' },
            h("div", { class: 'r-date-picker__years-view__header' },
                h("raul-icon", { icon: 'arrow-left-v', onClick: this.handlePreviousYearsRange }),
                h("div", { class: 'r-date-picker__months-view__year-label' },
                    h("div", { class: 'r-date-picker__months-view__year-label__focus-utility', tabindex: -1 }, `${this.yearsRangeStart} - ${this.yearsRangeStart + 11}`)),
                h("raul-icon", { icon: 'arrow-right-v', onClick: this.handleNextYearsRange })),
            h("div", { class: 'r-date-picker__years-view__years-container', tabindex: 0, onKeyDown: this.handleKeyDown }, Array.from({ length: 12 }).map((_, index) => h("div", { class: {
                    'r-date-picker__years-view__year-container': true,
                    'r-date-picker__years-view__year-container--focused': this.showFocusRing && !!this.focusedDate && ((this.yearsRangeStart + index) === this.focusedDate.getFullYear())
                } },
                h("div", { class: {
                        'r-date-picker__years-view__year': true,
                        'r-date-picker__years-view__year--selected': (this.yearsRangeStart + index) === this.activeYear,
                        'r-date-picker__years-view__year--current': (this.yearsRangeStart + index) === this.today.getFullYear()
                    }, onClick: () => this.setYear(this.yearsRangeStart + index) }, this.yearsRangeStart + index))))));
        return (h("div", { ref: el => this.el = el, onKeyDown: e => {
                if (this.showPicker && (['Esc', 'Escape'].includes(e.key) && this.variant !== 'inline')) {
                    this.showPicker = false;
                    this.date = null;
                    this.view = 'dates';
                    this.display = this.variant === 'range' ? this.display : this.hasSetADate ? this.hasDate ? this.formatDateForDisplay(this.date) : this.oldDate ? this.formatDateForDisplay(this.oldDate) : '' : '';
                    this.toggle.focus();
                }
            }, class: {
                'r-date-picker__inline': this.variant == 'inline',
                'r-date-picker__wrapper': true,
                'r-date-picker__wrapper--show': this.variant === 'inline' || this.showPicker,
                'r-date-picker__wrapper--invalid': !!this.error
            } },
            this.label && h("label", { class: "r-date-picker__label" }, this.label),
            h("div", { class: 'r-date-picker' },
                h(PickerToggle, null),
                h("div", { class: 'r-date-picker__container__responsive-utility-wrapper' },
                    h("div", { ref: el => this.dropdown = el, class: {
                            'r-date-picker__dropdown': true,
                            'r-date-picker__dropdown--top': this.top
                        } },
                        h(DatePicker, null),
                        this.view === 'months' && h(MonthPicker, null),
                        this.view === 'years' && h(YearPicker, null)))),
            this.hint && h("small", { class: "r-date-picker__hint" }, this.hint),
            this.error &&
                h("div", { class: "r-date-picker__error" },
                    h("raul-icon", { icon: "interface-alert-diamond", class: "r-date-picker__error__icon" }),
                    " ",
                    this.error)));
    }
    static get is() { return "raul-date-picker"; }
    static get originalStyleUrls() { return {
        "$": ["raul-date-picker.scss"]
    }; }
    static get styleUrls() { return {
        "$": ["raul-date-picker.css"]
    }; }
    static get properties() { return {
        "variant": {
            "type": "string",
            "mutable": false,
            "complexType": {
                "original": "'inline' | 'date' | 'range'",
                "resolved": "\"date\" | \"inline\" | \"range\"",
                "references": {}
            },
            "required": false,
            "optional": false,
            "docs": {
                "tags": [],
                "text": "Can be `date` or `range`. Defaults to `date`"
            },
            "attribute": "variant",
            "reflect": false,
            "defaultValue": "'date'"
        },
        "disabled": {
            "type": "boolean",
            "mutable": false,
            "complexType": {
                "original": "boolean",
                "resolved": "boolean",
                "references": {}
            },
            "required": false,
            "optional": false,
            "docs": {
                "tags": [],
                "text": "If `true`, the date picker is disabled"
            },
            "attribute": "disabled",
            "reflect": false,
            "defaultValue": "false"
        },
        "label": {
            "type": "string",
            "mutable": false,
            "complexType": {
                "original": "string",
                "resolved": "string",
                "references": {}
            },
            "required": false,
            "optional": false,
            "docs": {
                "tags": [],
                "text": "A string that will be shown above the date picker"
            },
            "attribute": "label",
            "reflect": false,
            "defaultValue": "''"
        },
        "hint": {
            "type": "string",
            "mutable": false,
            "complexType": {
                "original": "string",
                "resolved": "string",
                "references": {}
            },
            "required": false,
            "optional": false,
            "docs": {
                "tags": [],
                "text": "Optional hint text."
            },
            "attribute": "hint",
            "reflect": false
        },
        "error": {
            "type": "string",
            "mutable": false,
            "complexType": {
                "original": "string",
                "resolved": "string",
                "references": {}
            },
            "required": false,
            "optional": false,
            "docs": {
                "tags": [],
                "text": "Makes the datepicker visually invalid and shows the feedback message."
            },
            "attribute": "error",
            "reflect": false
        },
        "isInRangePicker": {
            "type": "boolean",
            "mutable": false,
            "complexType": {
                "original": "boolean",
                "resolved": "boolean",
                "references": {}
            },
            "required": false,
            "optional": false,
            "docs": {
                "tags": [],
                "text": "This is used internally by `raul-range-picker`"
            },
            "attribute": "is-in-range-picker",
            "reflect": false,
            "defaultValue": "false"
        },
        "initialDate": {
            "type": "unknown",
            "mutable": false,
            "complexType": {
                "original": "Date",
                "resolved": "Date",
                "references": {
                    "Date": {
                        "location": "global"
                    }
                }
            },
            "required": false,
            "optional": false,
            "docs": {
                "tags": [],
                "text": "A javascript Date object that represents the initial date of the date picker"
            },
            "defaultValue": "null"
        },
        "initialStartDate": {
            "type": "unknown",
            "mutable": false,
            "complexType": {
                "original": "Date",
                "resolved": "Date",
                "references": {
                    "Date": {
                        "location": "global"
                    }
                }
            },
            "required": false,
            "optional": false,
            "docs": {
                "tags": [],
                "text": "A javascript Date object that represents the initial start date of the range picker"
            },
            "defaultValue": "null"
        },
        "initialEndDate": {
            "type": "unknown",
            "mutable": false,
            "complexType": {
                "original": "Date",
                "resolved": "Date",
                "references": {
                    "Date": {
                        "location": "global"
                    }
                }
            },
            "required": false,
            "optional": false,
            "docs": {
                "tags": [],
                "text": "A javascript Date object that represents the initial end date of the range picker"
            },
            "defaultValue": "null"
        }
    }; }
    static get states() { return {
        "top": {},
        "focusedDate": {},
        "showFocusRing": {},
        "activeMonth": {},
        "activeYear": {},
        "yearsRangeStart": {},
        "view": {},
        "showPicker": {},
        "date": {},
        "oldDate": {},
        "display": {},
        "hasDate": {},
        "hasSetADate": {},
        "startDate": {},
        "endDate": {},
        "keyboardStartDate": {},
        "selectedDate": {}
    }; }
    static get events() { return [{
            "method": "dateSelected",
            "name": "dateSelected",
            "bubbles": true,
            "cancelable": true,
            "composed": true,
            "docs": {
                "tags": [],
                "text": "Event emitted when a date is selected. Event.detail will be a javascript Date object"
            },
            "complexType": {
                "original": "any",
                "resolved": "any",
                "references": {}
            }
        }, {
            "method": "rangeSelected",
            "name": "rangeSelected",
            "bubbles": true,
            "cancelable": true,
            "composed": true,
            "docs": {
                "tags": [],
                "text": "Event emitted when a range is selected. Event.detail will be {startDate: Date, endDate: Date}, where `Date` will be a javascript Date object"
            },
            "complexType": {
                "original": "any",
                "resolved": "any",
                "references": {}
            }
        }, {
            "method": "UNSAFE_dateSelected",
            "name": "UNSAFE_dateSelected",
            "bubbles": true,
            "cancelable": true,
            "composed": true,
            "docs": {
                "tags": [],
                "text": "To be used internally by `raul-range-picker`"
            },
            "complexType": {
                "original": "any",
                "resolved": "any",
                "references": {}
            }
        }]; }
    static get methods() { return {
        "clearPicker": {
            "complexType": {
                "signature": "() => Promise<void>",
                "parameters": [],
                "references": {
                    "Promise": {
                        "location": "global"
                    }
                },
                "return": "Promise<void>"
            },
            "docs": {
                "text": "Method used to programatically clear the picker",
                "tags": []
            }
        },
        "closePicker": {
            "complexType": {
                "signature": "() => Promise<void>",
                "parameters": [],
                "references": {
                    "Promise": {
                        "location": "global"
                    }
                },
                "return": "Promise<void>"
            },
            "docs": {
                "text": "A method to programatically close the picker",
                "tags": []
            }
        },
        "openPicker": {
            "complexType": {
                "signature": "() => Promise<void>",
                "parameters": [],
                "references": {
                    "Promise": {
                        "location": "global"
                    }
                },
                "return": "Promise<void>"
            },
            "docs": {
                "text": "A method to programatically open the picker",
                "tags": []
            }
        },
        "setStateDate": {
            "complexType": {
                "signature": "(date: Date) => Promise<void>",
                "parameters": [{
                        "tags": [],
                        "text": ""
                    }],
                "references": {
                    "Promise": {
                        "location": "global"
                    },
                    "Date": {
                        "location": "global"
                    }
                },
                "return": "Promise<void>"
            },
            "docs": {
                "text": "This is used internally by `raul-range-picker`",
                "tags": []
            }
        },
        "setPickerDate": {
            "complexType": {
                "signature": "(date: Date) => Promise<void>",
                "parameters": [{
                        "tags": [],
                        "text": ""
                    }],
                "references": {
                    "Promise": {
                        "location": "global"
                    },
                    "Date": {
                        "location": "global"
                    }
                },
                "return": "Promise<void>"
            },
            "docs": {
                "text": "This is used internally by `raul-range-picker`",
                "tags": []
            }
        },
        "setStateStartDate": {
            "complexType": {
                "signature": "(date: Date) => Promise<void>",
                "parameters": [{
                        "tags": [],
                        "text": ""
                    }],
                "references": {
                    "Promise": {
                        "location": "global"
                    },
                    "Date": {
                        "location": "global"
                    }
                },
                "return": "Promise<void>"
            },
            "docs": {
                "text": "This is used internally by `raul-range-picker`",
                "tags": []
            }
        },
        "setStateEndDate": {
            "complexType": {
                "signature": "(date: Date) => Promise<void>",
                "parameters": [{
                        "tags": [],
                        "text": ""
                    }],
                "references": {
                    "Promise": {
                        "location": "global"
                    },
                    "Date": {
                        "location": "global"
                    }
                },
                "return": "Promise<void>"
            },
            "docs": {
                "text": "This is used internally by `raul-range-picker`",
                "tags": []
            }
        },
        "setPickerStartDate": {
            "complexType": {
                "signature": "(date: Date) => Promise<void>",
                "parameters": [{
                        "tags": [],
                        "text": ""
                    }],
                "references": {
                    "Promise": {
                        "location": "global"
                    },
                    "Date": {
                        "location": "global"
                    }
                },
                "return": "Promise<void>"
            },
            "docs": {
                "text": "This is used internally by `raul-range-picker`",
                "tags": []
            }
        },
        "setPickerEndDate": {
            "complexType": {
                "signature": "(date: Date) => Promise<void>",
                "parameters": [{
                        "tags": [],
                        "text": ""
                    }],
                "references": {
                    "Promise": {
                        "location": "global"
                    },
                    "Date": {
                        "location": "global"
                    }
                },
                "return": "Promise<void>"
            },
            "docs": {
                "text": "This is used internally by `raul-range-picker`",
                "tags": []
            }
        },
        "setDisplay": {
            "complexType": {
                "signature": "(text: string) => Promise<void>",
                "parameters": [{
                        "tags": [],
                        "text": ""
                    }],
                "references": {
                    "Promise": {
                        "location": "global"
                    }
                },
                "return": "Promise<void>"
            },
            "docs": {
                "text": "This is used internally by `raul-range-picker`",
                "tags": []
            }
        },
        "clearDisplay": {
            "complexType": {
                "signature": "() => Promise<void>",
                "parameters": [],
                "references": {
                    "Promise": {
                        "location": "global"
                    }
                },
                "return": "Promise<void>"
            },
            "docs": {
                "text": "This is used internally by `raul-range-picker`",
                "tags": []
            }
        },
        "revertPicker": {
            "complexType": {
                "signature": "() => Promise<void>",
                "parameters": [],
                "references": {
                    "Promise": {
                        "location": "global"
                    }
                },
                "return": "Promise<void>"
            },
            "docs": {
                "text": "This is used internally by `raul-range-picker`",
                "tags": []
            }
        }
    }; }
    static get watchers() { return [{
            "propName": "showPicker",
            "methodName": "handleShowPickerChange"
        }, {
            "propName": "focusedDate",
            "methodName": "handleFocusDateChange"
        }, {
            "propName": "activeMonth",
            "methodName": "handleActiveMonthChange"
        }, {
            "propName": "activeYear",
            "methodName": "handleActiveYearChange"
        }, {
            "propName": "view",
            "methodName": "handleViewChange"
        }]; }
}
