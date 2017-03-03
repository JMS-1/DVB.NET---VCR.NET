namespace JMSLib.ReactUi {

    // Eine React.Js Komponente zur Anzeige einer Zeitschiene.
    export class TimeBar extends Component<App.ITimeBar>  {

        // Erstellt die Oberflächenelemente zur Anzeige der Zeitschiene.
        render(): JSX.Element {
            return <div className="jmslib-timebar">
                <div>
                    {(this.props.uvm.prefixTime > 0) && <div style={{ width: `${this.props.uvm.prefixTime}%` }}></div>}
                    <div style={{ width: `${this.props.uvm.time}%`, left: `${this.props.uvm.prefixTime}%` }} className={this.props.uvm.timeIsComplete ? `jmslib-timebar-good` : `jmslib-timebar-bad`}></div>
                    {(this.props.uvm.suffixTime > 0) && <div style={{ width: `${this.props.uvm.suffixTime}%`, left: `${this.props.uvm.prefixTime + this.props.uvm.time}%` }}></div>}
                </div>
                <div style={(this.props.uvm.currentTime === undefined) ? { display: `none` } : { left: `${this.props.uvm.currentTime}%` }}></div>
            </div>;
        }

    }
}
