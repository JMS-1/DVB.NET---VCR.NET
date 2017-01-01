/// <reference path="../../vcrnet.tsx" />

namespace VCRNETClient.Ui {

    // Schnittstelle zur Pflege der in eine Aufzeichnung zu integrierenden Sonderdaten.
    interface IEditChannelFlagsStatic {
        noui: App.NoUi.ISourceFlagsEditor;
    }

    export class EditChannelFlags extends React.Component<IEditChannelFlagsStatic, INoDynamicState>  {
        render(): JSX.Element {
            return <div className="vcrnet-editchannelflags">
                <EditBoolean noui={this.props.noui.includeDolby} />
                <EditBoolean noui={this.props.noui.allLanguages} />
                <EditBoolean noui={this.props.noui.withVideotext} />
                <EditBoolean noui={this.props.noui.withSubtitles} />
            </div>;
        }
    }
}
