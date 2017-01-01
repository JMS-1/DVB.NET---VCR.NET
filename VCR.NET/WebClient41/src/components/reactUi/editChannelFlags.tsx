/// <reference path="../../vcrnet.tsx" />

namespace VCRNETClient.Ui {

    // Schnittstelle zur Pflege der in eine Aufzeichnung zu integrierenden Sonderdaten.
    interface IEditChannelFlagsStatic {
        // Zugriff auf die einzelnen Wahrheitswerte.
        noui: App.NoUi.ISourceFlagsEditor;
    }

    // React.Js Komponente zur visuellen Darstellung der Aufzeichnungsoptionen.
    export class EditChannelFlags extends React.Component<IEditChannelFlagsStatic, INoDynamicState>  {

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
