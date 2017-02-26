/// <reference path="../../vcrnet.tsx" />

namespace VCRNETClient.Ui {

    // React.Js Komponente zur Implementierung von Hilfeseiten.
    export abstract class HelpComponent implements App.IHelpComponent {

        // Die Überschrift der Hilfeseite.
        abstract readonly title: string;

        // Erzeugt die Oberflächenelemente der Hilfeseite.
        abstract render(page: App.IPage): JSX.Element;
    }
}
