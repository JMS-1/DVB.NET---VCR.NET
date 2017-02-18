namespace JMSLib.ReactUi {

    export class TimeBar extends Component<App.ITimeBar>  {
        render(): JSX.Element {
            return <div className="jmslib-timebar">
                <div>
                    <div style={{ width: `${this.props.noui.prePercent}%` }}></div>
                    <div className={this.props.noui.recClass} style={{ width: `${this.props.noui.recPercent}%`, left: `${this.props.noui.prePercent}%` }}></div>
                    <div style={{ width: `${this.props.noui.postPercent}%`, left: `${this.props.noui.prePercent + this.props.noui.recPercent}%` }}></div>
                </div>
                <div style={(this.props.noui.nowPercent === undefined) ? { display: `none` } : { left: `${this.props.noui.nowPercent}%` }}></div>
            </div>;
        }
    }
}
