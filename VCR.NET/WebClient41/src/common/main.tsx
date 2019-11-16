/// <reference path="../vcrnet.tsx" />

namespace VCRNETClient.Ui {

    // React.Js Komponente für die Hauptseite der Anwendung - im Prinzip der gesamte sichtbare Bereich im Browser.
    export class Main extends React.Component<JMSLib.ReactUi.IEmpty, JMSLib.ReactUi.IEmpty> implements App.IApplicationSite {

        // Alle bekannten Hilfeseiten.
        private readonly _topics: { [section: string]: App.IHelpComponent; } = {
            repeatingschedules: new HelpPages.RepeatingSchedules(),
            parallelrecording: new HelpPages.ParallelRecording(),
            jobsandschedules: new HelpPages.JobsAndSchedules(),
            customschedule: new HelpPages.CustomSchedule(),
            configuration: new HelpPages.Configuration(),
            controlcenter: new HelpPages.ControlCenter(),
            currentstream: new HelpPages.CurrentStream(),
            epgconfig: new HelpPages.AdminProgramGuide(),
            numberoffiles: new HelpPages.NumberOfFiles(),
            sourcechooser: new HelpPages.SourceChooser(),
            filecontents: new HelpPages.FileContents(),
            psiconfig: new HelpPages.AdminSourceScan(),
            editcurrent: new HelpPages.EditCurrent(),
            hibernation: new HelpPages.Hibernation(),
            sourcelimit: new HelpPages.SourceLimit(),
            websettings: new HelpPages.WebSettings(),
            decryption: new HelpPages.Decryption(),
            streaming: new HelpPages.Streaming(),
            overview: new HelpPages.Overview(),
            tsplayer: new HelpPages.TsPlayer(),
            epg: new HelpPages.ProgramGuide(),
            archive: new HelpPages.Archive(),
            dvbnet: new HelpPages.DvbNet(),
            nexus: new HelpPages.Nexus(),
            tasks: new HelpPages.Tasks(),
            log: new HelpPages.Log(),
        };

        // Das Präsentationsmodell der Anwendung.
        private readonly _application: App.IApplication = new App.Application(this);

        // Wird ausgelöst, wenn sich der Navigationsberich ändert.
        private readonly _onhashchange: () => void = this.onhashchange.bind(this);

        // Erstellt eine neue Komponente.
        constructor(props) {
            super(props);

            // Initialen Navigationsbereich in Abhängigkeit von der URL aufrufen.
            this.onhashchange();
        }

        // Anmeldung beim Anbinden der React.Js Komponente ins DOM.
        componentDidMount(): void {
            window.addEventListener("hashchange", this._onhashchange);
        }

        // Abmelden beim Entfernen aus dem DOM - tatsächlich passiert dies nie.
        componentWillUnmount(): void {
            window.removeEventListener("hashchange", this._onhashchange);
        }

        // React.Js zur Aktualisierung der Oberfläche auffordern.
        refreshUi(): void {
            this.forceUpdate();
        }

        // Oberflächenelemente erstellen.
        render(): JSX.Element {
            // Überschrift ermitteln.
            var title = this._application.title;
            var page = this._application.page;

            if (document.title !== title)
                document.title = title;

            // Anzeige erstellen.
            return <div className="vcrnet-main">
                {this._application.isRestarting ?
                    <div className="vcrnet-restart">Der VCR.NET Recording Service startet nun neu und steht in Kürze wieder zur Verfügung.</div> :
                    (page ?
                        <div><h1>{page ? page.title : title}</h1><Navigation uvm={page} /><View uvm={page} /></div> :
                        <div><h1>(Bitte etwas Geduld)</h1></div>)}
                    {this._application.isBusy && <div className="vcrnet-main-busy"></div>}
            </div>;
        }

        // Wird zur Aktualisierung des Navigationsbereichs aufgerufen.
        private onhashchange(): void {
            // Auslesen der Kennung - für FireFox ist es nicht möglich, .hash direkt zu verwenden, da hierbei eine Decodierung durchgeführt wird
            var query = window.location.href.split("#");
            var hash = (query.length > 1) ? query[1] : "";

            // Erst mal auf die Einstiegsseite prüfen.
            if (hash.length < 1)
                this.setPage();
            else {
                // Ansonsten den Navigationsbereich mit Parametern aufrufen.
                var sections = hash.split(";");

                this.setPage(sections[0], sections.slice(1));
            }
        }

        // Den Navigationsbereich wechseln.
        private setPage(name: string = "", sections?: string[]) {
            this._application.switchPage(name, sections);
        }

        // Den Navigationsberecich über den Browser ändern.
        goto(name: string): void {
            window.location.href = name ? `#${name}` : `#`;
        }

        // Die Verwaltung der Hilfeseiten melden.
        getHelpComponentProvider<TComponentType extends App.IHelpComponent>(): App.IHelpComponentProvider<TComponentType> {
            return this._topics as App.IHelpComponentProvider<TComponentType>;
        }

    }
}
