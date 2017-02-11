namespace JMSLib.App {

    // Beschreibt ein Oberflächenelement mit einem Anzeigetext, der eventuell sprachabhängig ist.
    export interface IDisplay {
        // Der in der Oberfläche zu verwendende Anzeigetext.
        readonly text: string;
    }
}