namespace VCRNETClient {
    // Initialisiert die React.js Laufzeitumgebung.
    export function startup(): void {
        // Bilbliothek konfigurieren.
        JMSLib.ReactUi.Pictogram.imageRoot = `content/images/`;

        // Anwendung starten.
        ReactDOM.render(<Ui.Main />, document.querySelector(`vcrnet-spa`));
    }
}
