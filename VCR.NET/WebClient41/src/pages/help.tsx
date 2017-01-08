/// <reference path="../vcrnet.tsx" />

namespace VCRNETClient {

    // Beschreibt die Konfiguration der Hilfeseite.
    interface IHelpStatic extends INoUiComponent<App.NoUi.IHelpPage> {
        // Alle React.Js Komponenten zur Anzeige einzelner Hilfeaspekte.
        topics: IHelpComponentProvider;
    }

    // React.Js Komponente zur Anzeige der Hilfeseite.
    export class Help extends NoUiViewEx<App.NoUi.IHelpPage, IHelpStatic> {
        // Erstellt die Anzeigeelemente der Oberfläche.
        render(): JSX.Element {
            // Ermittelt die Anzeige des gewählten Aspektes.
            var element = this.props.topics.getHelpComponent(this.props.noui.section);

            return <div className="vcrnet-faq">
                {(element && element.render(this.props.noui)) || null}
            </div>;
        }
    }
}
