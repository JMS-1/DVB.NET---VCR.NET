/// <reference path="page.ts" />

namespace VCRNETClient.App {

    // Schnittstelle zur Anzeige des Protokolls.
    export interface ILogPage extends IPage {
        // Alle benutzen Geräte.
        readonly profiles: JMSLib.App.IValueFromList<string>;

        // Auswahl des Startzeitpunkts zur Anzeige.
        readonly startDay: JMSLib.App.IValueFromList<string>;

        // Anzahl zur Anzeige von Aktualisierungen der Programmzeitschrift.
        readonly showGuide: JMSLib.App.IToggableFlag;

        // Auswahl zur Anzige der Aktualisierungen der Quellen.
        readonly showScan: JMSLib.App.IToggableFlag;

        // Auswahl zur Anzeige von LIVE Verwendung.
        readonly showLive: JMSLib.App.IToggableFlag;

        // Alle anzuzeigenden Protokolleinträge.
        readonly entries: Log.ILogEntry[];
    }

    // Präsentationmodell zur anzeige der Protokolleinträge.
    export class LogPage extends Page implements ILogPage {

        // Aktualisierung in der Initialisierungsphase unterbinden.
        private _disableLoad = true;

        // Alle benutzen Geräte.
        readonly profiles = new JMSLib.App.SelectSingleFromList<string>({}, "value", "Protokollbereich", () => this.load(), []);

        // Anzahl zur Anzeige von Aktualisierungen der Programmzeitschrift.
        readonly showGuide = new JMSLib.App.Flag({}, "value", "Programmzeitschrift", () => this.refreshUi());

        // Auswahl zur Anzige der Aktualisierungen der Quellen.
        readonly showScan = new JMSLib.App.Flag({}, "value", "Sendersuchlauf", () => this.refreshUi());

        // Auswahl zur Anzeige von LIVE Verwendung.
        readonly showLive = new JMSLib.App.Flag({}, "value", "Zapping", () => this.refreshUi());

        // Auswahl des Startzeitpunkts zur Anzeige.
        readonly startDay: JMSLib.App.SelectSingleFromList<string>;

        // Alle Protokolleinträge.
        private _entries: Log.LogEntry[] = [];

        get entries(): Log.ILogEntry[] {
            // Aktuellen Filter berücksichtigen.
            return this._entries.filter(e => {
                if (e.isGuide)
                    return this.showGuide.value;
                if (e.isScan)
                    return this.showScan.value;
                if (e.isLive)
                    return this.showLive.value;

                return true;
            });
        }

        // Erstellt ein neues Präsentationsmodell.
        constructor(application: Application) {
            super("log", application);

            // Die Liste der Starttage erstellen wir nur ein einziges Mal.
            var now = new Date();
            var start = new Date(Date.UTC(now.getFullYear(), now.getMonth(), now.getDate()));
            var days: JMSLib.App.IUiValue<string>[] = [];

            for (var i = 0; i < 10; i++) {
                // Zur Auswahl durch den Anwender.
                days.push(JMSLib.App.uiValue(start.toISOString(), JMSLib.App.DateTimeUtils.formatNumber(start.getUTCDate()) + '.' + JMSLib.App.DateTimeUtils.formatNumber(1 + start.getUTCMonth())));

                // Eine Woche zurück.
                start = new Date(Date.UTC(start.getUTCFullYear(), start.getUTCMonth(), start.getUTCDate() - 7));
            }

            // Auswahlliste aufsetzen.
            this.startDay = new JMSLib.App.SelectSingleFromList({}, "_value", null, () => this.load(), days);
        }

        // Initialisiert das Präsentationsmodell.
        reset(sections: string[]): void {
            // Kontrolliertes Laden der Protokolliste.
            this._disableLoad = true;

            // Auswahl zurücksetzen.
            this.startDay.value = this.startDay.allowedValues[0].value;

            // Liste der Geräte anfordern.
            VCRServer.ProfileCache.getAllProfiles().then(profiles => {
                // Auswahlliste vorbereiten.
                this.profiles.allowedValues = profiles.map(p => JMSLib.App.uiValue(p.name));
                this.profiles.value = profiles[0] && profiles[0].name;

                // Zurück in den Normalbetrieb.
                this._disableLoad = false;

                // Protokollliste laden.
                this.load();
            });
        }

        // Protokolliste neu laden.
        private load(): void {
            // Das dürfen wir mal eben nicht.
            if (this._disableLoad)
                return;

            // Suchparameter erstellen.
            var profile = this.profiles.value;
            var endDay = new Date(this.startDay.value);
            var startDay = new Date(endDay.getTime() - 7 * 86400000);

            // Protokolle vom VCR.NET Recording Service anfordern.
            VCRServer.getProtocolEntries(profile, startDay, endDay).then(entries => {
                // Die Anzeige erfolgt immer mit den neuesten als erstes.
                entries.reverse();

                // Präsentationsmodell erstellen.
                var toggleDetail = this.toggleDetail.bind(this);

                this._entries = entries.map(e => new Log.LogEntry(e, toggleDetail));

                // Die Anwendung darf nun verwendet werden.
                this.application.isBusy = false;

                // Oberfläche zur Aktualisierung auffordern.
                this.refreshUi();
            });
        }

        // Detailansicht eines einzelnen Protkolleintrags umschalten.
        private toggleDetail(entry: Log.LogEntry): void {
            // Beim Anschalten alle anderen Detailansichten abschalten.
            if (entry.showDetail.value)
                this._entries.forEach(e => e.showDetail.value = (e === entry));

            // Oberfläche zur Aktualisierung auffordern.
            this.refreshUi();
        }

        // Der Titel für die Anzeige des Präsentationsmodells.
        get title(): string {
            return `Aufzeichnungsprotokolle einsehen`;
        }

    }

}