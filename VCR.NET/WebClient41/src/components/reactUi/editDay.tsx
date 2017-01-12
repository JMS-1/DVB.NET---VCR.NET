/// <reference path="../../vcrnet.tsx" />

namespace VCRNETClient.Ui {

    export class EditDay extends NoUiViewWithSite<App.NoUi.IDaySelector> implements App.NoUi.INoUiSite {
        // Anzeige erstellen.
        render(): JSX.Element {
            return <div className="vcrnet-editday">
                <div>
                    <Pictogram name="prev" type="gif" onClick={this._prevMonth} />
                    <div>
                        <select value={this.props.noui.month()} onChange={this._selectMonth} >
                            {this.props.noui.months.map(m => <option key={m} value={m}>{m}</option>)}
                        </select>
                        <select value={this.props.noui.year()} onChange={this._selectYear} >
                            {this.props.noui.years.map(m => <option key={m} value={m}>{m}</option>)}
                        </select>
                    </div>
                    <Pictogram name="next" type="gif" onClick={this._nextMonth} />
                </div>
                <table>
                    <thead>
                        <tr>{this.props.noui.dayNames.map(n => <td key={n}>{n}</td>)}</tr>
                    </thead>
                    <tbody>
                        {this.getRows(this.props.noui.days)}
                    </tbody>
                </table>
                <div>
                    <button onClick={this._today} disabled={this.props.noui.days.some(d => d.isToday)}>Heute</button>
                    <button onClick={this._reset} disabled={this.props.noui.days.some(d => d.isCurrentDay)}>Zurück</button>
                </div>
            </div>;
        }

        private readonly _reset = this.reset.bind(this);

        private reset(ev: React.FormEvent): void {
            ev.preventDefault();

            this.props.noui.reset();
        }

        private readonly _today = this.today.bind(this);

        private today(ev: React.FormEvent): void {
            ev.preventDefault();

            this.props.noui.today();
        }

        private readonly _nextMonth = this.nextMonth.bind(this);

        private nextMonth(ev: React.FormEvent): void {
            this.props.noui.monthForward();
        }

        private readonly _prevMonth = this.prevMonth.bind(this);

        private prevMonth(ev: React.FormEvent): void {
            this.props.noui.monthBackward();
        }

        private readonly _selectMonth = this.selectMonth.bind(this);

        private selectMonth(ev: React.FormEvent): void {
            this.props.noui.month((ev.target as HTMLSelectElement).value);
        }

        private readonly _selectYear = this.selectYear.bind(this);

        private selectYear(ev: React.FormEvent): void {
            this.props.noui.year((ev.target as HTMLSelectElement).value);
        }

        private getRow(days: App.NoUi.ISelectableDay[], rowKey: number): JSX.Element {
            if (days.length !== 7)
                return null;

            return <tr key={rowKey}>{days.map((day, index) => {
                var classes: string[] = [];

                if (day.isCurrentMonth)
                    classes.push("vcrnet-editday-month");
                if (day.isCurrentDay)
                    classes.push("vcrnet-editday-selected");
                if (day.isToday)
                    classes.push("vcrnet-editday-today");

                return <td onClick={day.select} key={`${index}`} className={classes.join(" ")}>{day.display}</td>;
            })}</tr>;
        }

        private getRows(days: App.NoUi.ISelectableDay[]): JSX.Element[] {
            var rows: JSX.Element[] = [];

            for (var ix = 0; ix < days.length; ix += 7)
                rows.push(this.getRow(days.slice(ix, ix + 7), ix));

            return rows;
        }
    }
}
