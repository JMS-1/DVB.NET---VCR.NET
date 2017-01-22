/// <reference path="../lib/reactUi.tsx" />

namespace VCRNETClient {

    // React.Js Komponente zur Anzeige der Hilfeseite.
    export class Help extends JMSLib.ReactUi.Component<App.IHelpPage> {
        // Erstellt die Anzeigeelemente der Oberfläche.
        render(): JSX.Element {
            // Ermittelt die Anzeige des gewählten Aspektes.
            var element = this.props.noui.getHelpComponent<HelpComponent>();

            return <div className="vcrnet-faq">
                {(element && element.render(this.props.noui)) || null}
            </div>;
        }
    }
}
