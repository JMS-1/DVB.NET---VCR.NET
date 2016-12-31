/// <reference path="../../vcrnet.tsx" />

namespace VCRNETClient.Ui {

    interface IEditChannelFlagsStatic {
        noui: App.NoUi.ISourceFlagsEditor;
    }

    interface IEditChannelFlagsDynamic {
        disabled: boolean;
    }

    export class EditChannelFlags extends React.Component<IEditChannelFlagsStatic, IEditChannelFlagsDynamic>  {
        componentWillMount(): void {
            this.componentWillReceiveProps(this.props, undefined);
        }

        componentWillReceiveProps(nextProps: IEditChannelFlagsStatic, nextContext: any): void {
            this.setState({ disabled: !nextProps.noui.isEnabled() });
        }

        render(): JSX.Element {
            return <div className="vcrnet-editchannelflags">
                <EditBoolean noui={this.props.noui.includeDolby} disabled={this.state.disabled} />
                <EditBoolean noui={this.props.noui.allLanguages} disabled={this.state.disabled} />
                <EditBoolean noui={this.props.noui.withVideotext} disabled={this.state.disabled} />
                <EditBoolean noui={this.props.noui.withSubtitles} disabled={this.state.disabled} />
            </div>;
        }
    }
}
