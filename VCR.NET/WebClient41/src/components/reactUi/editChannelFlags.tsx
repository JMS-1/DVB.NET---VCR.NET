/// <reference path="../../vcrnet.tsx" />

namespace VCRNETClient.Ui {

    interface IEditChannelFlagsStatic {
        noui: App.NoUi.JobScheduleEditor<any>;
    }

    interface IEditChannelFlagsDynamic {
        disabled: boolean;
    }

    export class EditChannelFlags extends React.Component<IEditChannelFlagsStatic, IEditChannelFlagsDynamic>  {
        componentWillMount(): void {
            this.componentWillReceiveProps(this.props, undefined);
        }

        componentWillReceiveProps(nextProps: IEditChannelFlagsStatic, nextContext: any): void {
            this.setState({ disabled: (nextProps.noui.source.val() || "").trim().length < 1 });
        }

        render(): JSX.Element {
            return <div className="vcrnet-editchannelflags">
                <EditBoolean noui={this.props.noui.includeDolby} disabled={this.state.disabled}>Dolby Digital (AC3)</EditBoolean>
                <EditBoolean noui={this.props.noui.allLanguages} disabled={this.state.disabled}>Alle Sprachen</EditBoolean>
                <EditBoolean noui={this.props.noui.withVideotext} disabled={this.state.disabled}>Videotext</EditBoolean>
                <EditBoolean noui={this.props.noui.withSubtitles} disabled={this.state.disabled}>DVB Untertitel</EditBoolean>
            </div>;
        }
    }
}
