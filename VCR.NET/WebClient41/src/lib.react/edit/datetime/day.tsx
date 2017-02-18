/// <reference path="../../reactUi.tsx" />

namespace JMSLib.ReactUi {

    export class EditDay extends ComponentWithSite<App.IDaySelector> {
        // Anzeige erstellen.
        render(): JSX.Element {
            return <div className="jmslib-editday">
                <div>
                    <Pictogram name="prev" type="gif" onClick={ev => this.props.noui.monthBackward()} />
                    <div>
                        <select value={this.props.noui.month()} onChange={ev => this.props.noui.month((ev.target as HTMLSelectElement).value)} >
                            {this.props.noui.months.map(m => <option key={m} value={m}>{m}</option>)}
                        </select>
                        <select value={this.props.noui.year()} onChange={ev => this.props.noui.year((ev.target as HTMLSelectElement).value)} >
                            {this.props.noui.years.map(m => <option key={m} value={m}>{m}</option>)}
                        </select>
                    </div>
                    <Pictogram name="next" type="gif" onClick={ev => this.props.noui.monthForward()} />
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
                    <button onClick={ev => this.today(ev)} disabled={this.props.noui.days.some(d => d.isToday)}>Heute</button>
                    <button onClick={ev => this.reset(ev)} disabled={this.props.noui.days.some(d => d.isCurrentDay)}>Zurück</button>
                </div>
            </div>;
        }

        private reset(ev: React.FormEvent): void {
            ev.preventDefault();

            this.props.noui.reset();
        }

        private today(ev: React.FormEvent): void {
            ev.preventDefault();

            this.props.noui.today();
        }

        private getRow(days: App.ISelectableDay[], rowKey: number): JSX.Element {
            if (days.length !== 7)
                return null;

            return <tr key={rowKey}>{days.map((day, index) => {
                var classes: string[] = [];

                if (day.isCurrentMonth)
                    classes.push("jmslib-editday-month");
                if (day.isCurrentDay)
                    classes.push("jmslib-editday-selected");
                if (day.isToday)
                    classes.push("jmslib-editday-today");

                return <td onClick={day.select} key={`${index}`} className={classes.join(" ")}>{day.display}</td>;
            })}</tr>;
        }

        private getRows(days: App.ISelectableDay[]): JSX.Element[] {
            var rows: JSX.Element[] = [];

            for (var ix = 0; ix < days.length; ix += 7)
                rows.push(this.getRow(days.slice(ix, ix + 7), ix));

            return rows;
        }
    }
}
