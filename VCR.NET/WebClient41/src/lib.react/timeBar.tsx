namespace JMSLib.ReactUi {

    export class TimeBar extends Component<App.ITimeBar>  {
        render(): JSX.Element {
            return <div className="jmslib-timebar">
                <div>
                    <div style={{ width: `${this.props.noui.prefixTime}%` }}></div>
                    <div className={this.props.noui.timeIsComplete ? `jmslib-timebar-good` : `jmslib-timebar-bad`} style={{ width: `${this.props.noui.time}%`, left: `${this.props.noui.prefixTime}%` }}></div>
                    <div style={{ width: `${this.props.noui.suffixTime}%`, left: `${this.props.noui.prefixTime + this.props.noui.time}%` }}></div>
                </div>
                <div style={(this.props.noui.currentTime === undefined) ? { display: `none` } : { left: `${this.props.noui.currentTime}%` }}></div>
            </div>;
        }
    }
}
