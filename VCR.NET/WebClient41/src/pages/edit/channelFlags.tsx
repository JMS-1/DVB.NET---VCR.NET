/// <reference path="../../lib.react/reactUi.tsx" />

namespace VCRNETClient.Ui {

    // React.Js Komponente zur visuellen Darstellung der Aufzeichnungsoptionen.
    export class EditChannelFlags extends JMSLib.ReactUi.Component<App.Edit.ISourceFlagsEditor>  {

        // Erzeugt die visuelle Darstellung.
        render(): JSX.Element {
            return <div className="vcrnet-editchannelflags">
                <JMSLib.ReactUi.EditBoolean uvm={this.props.uvm.includeDolby} />
                <JMSLib.ReactUi.EditBoolean uvm={this.props.uvm.allLanguages} />
                <JMSLib.ReactUi.EditBoolean uvm={this.props.uvm.withVideotext} />
                <JMSLib.ReactUi.EditBoolean uvm={this.props.uvm.withSubtitles} />
            </div>;
        }

    }
}
