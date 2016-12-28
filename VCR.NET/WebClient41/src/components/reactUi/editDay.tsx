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
                <div>[Navigate]</div>
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

        refresh(): void {
            this.forceUpdate();
        }
    }
}
