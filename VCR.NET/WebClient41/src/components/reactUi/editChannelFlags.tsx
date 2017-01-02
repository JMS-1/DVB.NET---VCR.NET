/// <reference path="../../vcrnet.tsx" />

namespace VCRNETClient.Ui {

    // React.Js Komponente zur visuellen Darstellung der Aufzeichnungsoptionen.
    export class EditChannelFlags extends NoUiView<App.NoUi.ISourceFlagsEditor>  {

        // Erzeugt die visuelle Darstellung.
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
