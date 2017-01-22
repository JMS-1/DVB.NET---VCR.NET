namespace VCRNETClient {
    // Initialisiert die React.js Laufzeitumgebung.
    export function startup(): void {
        ReactDOM.render(<Main />, document.querySelector(`vcrnet-spa`));
    }
}
