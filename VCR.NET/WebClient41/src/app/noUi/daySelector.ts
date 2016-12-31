/// <reference path="generic.ts" />

namespace VCRNETClient.App.NoUi {

    export interface IDaySelectorSite {
        refresh(): void;
    }

    export interface ISelectableDay {
        readonly date: Date;

        readonly display: string;

        readonly isCurrentMonth: boolean;

        readonly isCurrentDay: boolean;

        readonly isToday: boolean;

        select(): void;
    }

    export interface IDaySelector {
        readonly name: string;

        setSite(newSite: IDaySelectorSite): void;

        monthBackward(): void;

        monthForward(): void;

        today(): void;

        reset(): void;

        month(newMonth?: string): string;

        readonly months: string[];

        year(newYear?: string): string;

        readonly years: string[];

        readonly dayNames: string[];

        readonly days: ISelectableDay[];
    }

    export class DayEditor extends ValueHolder<string> implements IDaySelector {
        private static _dayNames = ["Mo", "Di", "Mi", "Do", "Fr", "Sa", "So"];

        readonly dayNames = DayEditor._dayNames;

        monthBackward(): void {
            var monthIndex = this.months.indexOf(this._month);
            if (monthIndex < 0)
                return;

            if (monthIndex-- === 0) {
                monthIndex = 11;

                this._year = `${parseInt(this._year) - 1}`;
            }

            this._month = this.months[monthIndex];

            this.refresh();
        }

        monthForward(): void {
            var monthIndex = this.months.indexOf(this._month);
            if (monthIndex < 0)
                return;

            if (monthIndex++ === 11) {
                monthIndex = 0;

                this._year = `${parseInt(this._year) + 1}`;
            }

            this._month = this.months[monthIndex];

            this.refresh();
        }

        today(): void {
            this.moveTo(new Date());
        }

        reset(): void {
            this.moveTo(new Date(this.val()));
        }

        private moveTo(date: Date): void {
            var newMonth = this.months[date.getMonth()];
            var newYear = `${date.getFullYear()}`;

            if (this._month === newMonth)
                if (this._year === newYear)
                    return;

            this._month = newMonth;
            this._year = newYear;

            this.refresh();
        }

        private static _months = ["Januar", "Februar", "März", "April", "Mai", "Juni", "Juli", "August", "September", "Oktober", "November", "Dezember"];

        private _month: string;

        month(newMonth?: string): string {
            var oldMonth = this._month;

            if (newMonth !== undefined)
                if (newMonth != oldMonth) {
                    this._month = newMonth;

                    this.refresh();
                }

            return oldMonth;
        }

        readonly months: string[] = DayEditor._months;

        private _year: string;

        year(newYear?: string): string {
            var oldYear = this._year;

            if (newYear !== undefined)
                if (newYear != oldYear) {
                    this._year = newYear;

                    this.refresh();
                }

            return oldYear;
        }

        years: string[];

        days: ISelectableDay[];

        private _site: IDaySelectorSite;

        setSite(newSite: IDaySelectorSite): void {
            if (!(this._site = newSite))
                return;

            var date = new Date(this.val());

            this._month = this.months[date.getMonth()];
            this._year = `${date.getFullYear()}`;

            this.fillDayList();
            this.fillYearSelector();
        }

        private fillYearSelector(): void {
            if (this.years)
                if (this.years[5] === this._year)
                    return;

            var year = parseInt(this._year);

            this.years = [];

            for (var i = -5; i <= +5; i++)
                this.years.push(`${year + i}`);
        }

        private fillDayList(): void {
            var year = parseInt(this._year);
            var month = this.months.indexOf(this._month);
            var current = new Date(year, month, 1);
            current = new Date(year, month, 1 - (current.getDay() + 6) % 7);

            var now = new Date();
            now = new Date(now.getFullYear(), now.getMonth(), now.getDate());

            var selected = new Date(this.val());
            selected = new Date(selected.getFullYear(), selected.getMonth(), selected.getDate());

            this.days = [];

            do {
                for (var i = 7; i-- > 0; current = new Date(year, current.getMonth(), current.getDate() + 1)) {
                    let day: ISelectableDay;

                    day = {
                        isCurrentDay: current.getTime() === selected.getTime(),
                        isCurrentMonth: current.getMonth() === month,
                        isToday: current.getTime() === now.getTime(),
                        select: () => this.selectDay(day),
                        display: `${current.getDate()}`,
                        date: current
                    };

                    if (day.isCurrentDay)
                        day.select = undefined;

                    this.days.push(day);
                }
            }
            while (current.getMonth() === month);
        }

        constructor(data: any, prop: string, onChange: () => void, name: string) {
            super(data, prop, onChange, name);
        }

        private selectDay(day: ISelectableDay): void {
            var oldSelected = new Date(this.val());
            var newSelected = new Date(day.date.getFullYear(), day.date.getMonth(), day.date.getDate(), oldSelected.getHours(), oldSelected.getMinutes(), oldSelected.getSeconds(), oldSelected.getMilliseconds());

            if (newSelected.getTime() === oldSelected.getTime())
                return;

            this.val(newSelected.toISOString());

            this.reset();
            this.refresh();
        }

        private refresh(): void {
            this.fillDayList();
            this.fillYearSelector();

            if (this._site)
                this._site.refresh();
        }
    }
}