namespace VCRNETClient.App.NoUi {

    // Beschreibt ein Oberflächenelement mit einem Anzeigetext, der eventuell sprachabhängig ist.
    export interface IDisplayText {
        // Der in der Oberfläche zu verwendende Anzeigetext.
        readonly text: string;
    }
}