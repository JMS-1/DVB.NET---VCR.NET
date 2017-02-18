/// <reference path="../../lib.react/reactUi.tsx" />

namespace VCRNETClient.Ui {

    // React.Js Komponente zur visuellen Darstellung der Aufzeichnungsoptionen.
    export class EditChannelFlags extends JMSLib.ReactUi.Component<App.Edit.ISourceFlagsEditor>  {

        // Erzeugt die visuelle Darstellung.
        render(): JSX.Element {
            return <div className="vcrnet-editchannelflags">
                <JMSLib.ReactUi.EditBoolean noui={this.props.noui.includeDolby} />
                <JMSLib.ReactUi.EditBoolean noui={this.props.noui.allLanguages} />
                <JMSLib.ReactUi.EditBoolean noui={this.props.noui.withVideotext} />
                <JMSLib.ReactUi.EditBoolean noui={this.props.noui.withSubtitles} />
            </div>;
        }

    }
}
