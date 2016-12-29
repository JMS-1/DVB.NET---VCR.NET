/// <reference path="generic.ts" />

namespace VCRNETClient.App.NoUi {

    export interface IDaySelectorSite {
        refresh(): void;
    }

    export interface ISelectableDay {
        readonly display: string;

        readonly current: boolean;

        readonly today: boolean;

        select(): void;
    }

    export interface IDaySelector {
        setSite(newSite: IDaySelectorSite): void;

        monthBackward(): void;

        monthForward(): void;

        today(): void;

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

        readonly days: ISelectableDay[];

        private _site: IDaySelectorSite;

        setSite(newSite: IDaySelectorSite): void {
            if (!(this._site = newSite))
                return;

            var date = new Date(this.val());

            this._month = this.months[date.getMonth()];
            this._year = `${date.getFullYear()}`;

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

        constructor(data: any, prop: string, onChange: () => void) {
            super(data, prop, onChange);
        }

        private refresh(): void {
            this.fillYearSelector();

            if (this._site)
                this._site.refresh();
        }
    }
}