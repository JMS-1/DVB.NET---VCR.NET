/// <reference path="../../reactUi.tsx" />

namespace JMSLib.ReactUi {

    // React.JS Komponente zur Auswahl eines Datums.
    export class EditDay extends ComponentWithSite<App.IDaySelector> {

        // Anzeige erstellen.
        render(): JSX.Element {
            return <div className="jmslib-editday jmslib-validatable" title={this.props.uvm.message}>
                <div>
                    <Pictogram name="prev" type="gif" onClick={ev => this.props.uvm.monthBackward.execute()} />
                    <div>
                        <select value={this.props.uvm.month} onChange={ev => this.props.uvm.month = (ev.target as HTMLSelectElement).value} >
                            {this.props.uvm.months.map(m => <option key={m} value={m}>{m}</option>)}
                        </select>
                        <select value={this.props.uvm.year} onChange={ev => this.props.uvm.year = (ev.target as HTMLSelectElement).value} >
                            {this.props.uvm.years.map(m => <option key={m} value={m}>{m}</option>)}
                        </select>
                    </div>
                    <Pictogram name="next" type="gif" onClick={ev => this.props.uvm.monthForward.execute()} />
                </div>
                <table>
                    <thead><tr>{this.props.uvm.dayNames.map(n => <td key={n}>{n}</td>)}</tr></thead>
                    <tbody>{this.getRows(this.props.uvm.days)}</tbody>
                </table>
                <div>
                    <ButtonCommand uvm={this.props.uvm.today} />
                    <ButtonCommand uvm={this.props.uvm.reset} />
                </div>
            </div>;
        }

        // Ermittelt eine Woche mit auswählbaren Tagen.
        private getRow(days: App.ISelectableDay[], rowKey: number): JSX.Element {
            // Prüfen ob genug Tage zur Verfügung stehen.
            if (days.length !== 7)
                return null;

            // Oberflächenelemente erstellen.
            return <tr key={rowKey}>{days.map((day, index) => {
                var classes: string[] = [];

                // Visualisierung der Sondertage vorbereiten.
                if (day.isCurrentMonth)
                    classes.push("jmslib-editday-month");
                if (day.isCurrentDay)
                    classes.push("jmslib-editday-selected");
                if (day.isToday)
                    classes.push("jmslib-editday-today");

                // Oberflächenelemente für einen einzelnen Tag auswählbaren erstellen.
                return <td onClick={day.select} key={`${index}`} className={classes.join(" ")}>{day.display}</td>;
            })}</tr>;
        }

        // Teilt die zur Auswahl anzubietende Tage in Häppchen zu je einer Woche.
        private getRows(days: App.ISelectableDay[]): JSX.Element[] {
            var rows: JSX.Element[] = [];

            // Tage in Schritten einer Woche zerlegen.
            for (var ix = 0; ix < days.length; ix += 7)
                rows.push(this.getRow(days.slice(ix, ix + 7), ix));

            return rows;
        }
    }
}
