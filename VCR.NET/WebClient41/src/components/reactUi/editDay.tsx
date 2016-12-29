/// <reference path="../../vcrnet.tsx" />

namespace VCRNETClient.Ui {

    interface IEditDayStatic {
        noui: App.NoUi.IDaySelector;
    }

    interface IEditDayDynamic {
    }

    export class EditDay extends React.Component<IEditDayStatic, IEditDayDynamic> implements App.NoUi.IDaySelectorSite {
        componentWillMount(): void {
            this.props.noui.setSite(this);
        }

        componentWillUnmount(): void {
            this.props.noui.setSite(undefined);
        }

        // Anzeige erstellen.
        render(): JSX.Element {
            return <div className="vcrnet-editday">
                <div>
                    <Pictogram name="prev" type="gif" onClick={this._prevMonth} />
                    <select value={this.props.noui.month()} onChange={this._selectMonth} >
                        {this.props.noui.months.map(m => <option key={m} value={m}>{m}</option>)}
                    </select>
                    <select value={this.props.noui.year()} onChange={this._selectYear} >
                        {this.props.noui.years.map(m => <option key={m} value={m}>{m}</option>)}
                    </select>
                    <Pictogram name="next" type="gif" onClick={this._nextMonth} />
                </div>
                <div>
                    <table>
                        <thead>
                            <tr>{this.props.noui.dayNames.map(n => <td key={n}>{n}</td>)}</tr>
                        </thead>
                        <tbody>
                        </tbody>
                    </table>
                </div>
                <div>[Confirm]</div>
            </div>;
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

        refresh(): void {
            this.forceUpdate();
        }
    }
}
