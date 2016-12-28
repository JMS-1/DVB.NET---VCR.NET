/// <reference path="generic.ts" />

namespace VCRNETClient.App.NoUi {

    export interface IDaySelectorSite {
        refresh(): void;
    }

    export interface ISelectableDay {
        display: string;

        selected: boolean;

        today: boolean;

        select(): void;
    }

    export interface IDaySelector {
        setSite(newSite: IDaySelectorSite): void;

        monthBack(): void;

        monthForward(): void;

        today(): void;

        month(newMonth?: string): string;

        readonly months: string[];

        year(newYear?: string): string;

        readonly years: string[];

        readonly dayNames: string[];

        readonly days: ISelectableDay[];
    }

    export class DayEditor extends ValueHolder<Date> implements IDaySelector {
        private static _dayNames = ["Mo", "Di", "Mi", "Do", "Fr", "Sa", "So"];

        readonly dayNames = DayEditor._dayNames;

        monthBack(): void {
        }

        monthForward(): void {
        }

        today(): void {
        }

        private _month: string;

        month(newMonth?: string): string {
            return undefined;
        }

        readonly months: string[];

        year(newYear?: string): string {
            return undefined;
        }

        readonly years: string[];

        readonly days: ISelectableDay[];

        private _site: IDaySelectorSite;

        setSite(newSite: IDaySelectorSite): void {
            this._site = newSite;
        }

        constructor(data: any, prop: string, onChange: () => void) {
            super(data, prop, onChange);

            this.refresh();
        }

        private refresh(): void {
            var day = this.val();

            if (this._site)
                this._site.refresh();
        }
    }
}