namespace VCRNETClient.Ui.HelpPages {

    // React.Js Komponente für ein Bild in der Hilfe.
    export class ScreenShot extends React.Component<JMSLib.ReactUi.IPictogram, JMSLib.ReactUi.IEmpty>  {

        // Erzeugt die Oberflächenelemente.
        render(): JSX.Element {
            return <span className="vcrnet-screenshot"><JMSLib.ReactUi.Pictogram {...this.props} /></span>;
        }

    }
}
