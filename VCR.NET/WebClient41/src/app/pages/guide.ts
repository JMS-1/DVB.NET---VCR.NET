/// <reference path="page.ts" />
/// <reference path="../../../scripts/VCRServer.ts" />

namespace VCRNETClient.App {

    // Schnittstelle zum Blättern in der Programmzeitschrift.
    export interface IGuidePageNavigation {
        // Befehl zum Wechseln auf den Anfang der aktuellen Ergebnisliste.
        readonly firstPage: JMSLib.App.ICommand;

        // Befehl zum Wechseln auf die vorherige Seite der aktuellen Ergebnisliste.
        readonly prevPage: JMSLib.App.ICommand;

        // Befehl zum Wechseln auf die nächste Seite der aktuellen Ergebnisliste.
        readonly nextPage: JMSLib.App.ICommand;
    }

    // Schnittstelle zur Anzeige der Programmzeitschrift.
    export interface IGuidePage extends IPage, IGuidePageNavigation {
        // Der anzuzeigende Ausschnitt der aktuellen Ergebnisliste.
        readonly entries: Guide.GuideEntry[];

        // Alle bekannten Geräte.
        readonly profiles: JMSLib.App.IValidateStringFromList;

        // Alle Quellen auf dem aktuell ausgewählten Gerät.
        readonly sources: JMSLib.App.IValidateStringFromList;

        // Auswahl des Verschlüsselungsfilters.
        readonly encrpytion: JMSLib.App.IValueFromList<VCRServer.GuideEncryption>;

        // Gesetzt, wenn der Verschlüsselungsfilter angezeigt werden soll.
        readonly showEncryption: boolean;

        // Auswahl der Einschränkung auf die Art der Quelle.
        readonly sourceType: JMSLib.App.IValueFromList<VCRServer.GuideSource>;

        // Gesetzt, wenn die Einschränkung der Art der Quelle angezeigt werden soll.
        readonly showSourceType: boolean;

        // Setzt den Anfang der Ergebnisliste auf ein bestimmtes Datum.
        readonly days: JMSLib.App.IValidateStringFromList;

        // Setzt den Anfang der Ergebnisliste auf eine bestimmte Uhrzeit.
        readonly hours: JMSLib.App.IValueFromList<number>;

        // Der aktuelle Text zur Suche in allen Einträgen der Programmzeitschrift.
        readonly queryString: JMSLib.App.IValidatedString;

        // Gesetzt, wenn auch in der Beschreibung gesucht werden soll.
        readonly withContent: JMSLib.App.IValidatedFlag;

        // Befhel zum Zurücksetzen aller Einschränkungen.
        readonly resetFilter: JMSLib.App.ICommand;
    }

    // Ui View Model zur Anzeige der Programmzeitschrift.
    export class GuidePage extends Page implements IGuidePage {

        // Optionen zur Auswahl der Einschränkung auf die Verschlüsselung.
        private static _cryptOptions: JMSLib.App.IUiValue<VCRServer.GuideEncryption>[] = [
            { display: "Nur unverschlüsselt", value: VCRServer.GuideEncryption.FREE },
            { display: "Nur verschlüsselt", value: VCRServer.GuideEncryption.PAY },
            { display: "Alle Quellen", value: VCRServer.GuideEncryption.ALL }
        ];

        // Optionen zur Auswahl der Einschränkuzng auf die Art der Quelle.
        private static _typeOptions: JMSLib.App.IUiValue<VCRServer.GuideSource>[] = [
            { display: "Nur Fernsehen", value: VCRServer.GuideSource.TV },
            { display: "Nur Radio", value: VCRServer.GuideSource.RADIO },
            { display: "Alle Quellen", value: VCRServer.GuideSource.ALL }
        ];

        // Für den Start der aktuellen Ergebnisliste verfügbaren Auswahloptionen für die Uhrzeit.
        private static _hours: JMSLib.App.IUiValue<number>[] = [
            { display: "00:00", value: 0 },
            { display: "06:00", value: 6 },
            { display: "12:00", value: 12 },
            { display: "18:00", value: 18 },
            { display: "20:00", value: 20 },
            { display: "22:00", value: 22 },
        ];

        // Laufende Nummer der aktuellen Serveranfrage.
        private _queryId = 0;

        // Die aktuellen Einschränkungen.
        private _filter: VCRServer.GuideFilterContract =
        {
            cryptFilter: VCRServer.GuideEncryption.ALL,
            typeFilter: VCRServer.GuideSource.ALL,
            content: null,
            device: null,
            start: null,
            title: null,
            station: "",
            size: 20,
            index: 0
        };

        // Schnittstelle zur Auswahl des zu betrachtenden Gerätes.
        readonly profiles = new JMSLib.App.EditStringFromList(this._filter, "device", () => this.onDeviceChanged(true), "Gerät", false, []);

        // Schnittstelle zur Auswahl der Quelle.
        readonly sources = new JMSLib.App.EditStringFromList(this._filter, "station", () => this.query(), "Quelle", false, []);

        // Schnittstelle zur Auswahl der Einschränkung auf die Verschlüsselung.
        readonly encrpytion = new JMSLib.App.EditFromList<VCRServer.GuideEncryption>(this._filter, "cryptFilter", () => this.query(), null, GuidePage._cryptOptions);

        // Schnittstelle zur Auswahl der Einschränkung auf die Art der Quelle.
        readonly sourceType = new JMSLib.App.EditFromList<VCRServer.GuideSource>(this._filter, "typeFilter", () => this.query(), null, GuidePage._typeOptions);

        // Schnittstelle zum Setzen eines bestimmten Tags für den Anfang der Ergebnisliste.
        readonly days = new JMSLib.App.EditStringFromList(this._filter, "start", () => this.resetIndexAndQuery(), "Datum", false, []);

        // Bei der nächsten Abfrage zu setzende Uhrzeit für den Anfang der Ergebnisliste.
        private _hour = -1;

        // Schnittstelle zum Setzen einer bestimmten Uhrzeit für den Anfange der Ergebnisliste.
        readonly hours = new JMSLib.App.EditFromList<number>(this, "_hour", () => this.resetIndexAndQuery(), "Start ab", GuidePage._hours);

        // Die aktuelle Freitextsucheingabe.
        private _query = "";

        // Schnittstelle zur Pflege der Freitextsuchbedingung.
        readonly queryString = new JMSLib.App.EditString(this, "_query", () => this.delayedQuery(), "Suche nach", false);

        // Gesetzt, wenn auch eine Suche auf die Beschreibung erfolgen soll.
        private _withContent = true;

        // Schnittstelle zur Pflege der Auswahl der Freitextsuche auf die Beschreibung.
        readonly withContent = new JMSLib.App.EditFlag(this, "_withContent", () => this.query(), "Auch in Beschreibung suchen");

        // Aktuelle Anmeldung für verzögerte Suchanfragen.
        private _timeout: number;

        // Befehl zur Anzeige des Anfangs der Ergebnisliste.
        readonly firstPage = new JMSLib.App.Command(() => this.changePage(-this._filter.index), "Erste Seite", () => this._filter.index > 0);

        // Befehl zur Anzeige der vorherigen Seite der Ergebnisliste.
        readonly prevPage = new JMSLib.App.Command(() => this.changePage(-1), "Vorherige Seite", () => this._filter.index > 0);

        // Befehl zur Anzeige der nächsten Seite der Ergebnisliste.
        readonly nextPage = new JMSLib.App.Command(() => this.changePage(+1), "Nächste Seite", () => this._hasMore);

        // Befehl zum Zurücksetzen aller aktuellen Einschränkungen.
        readonly resetFilter = new JMSLib.App.Command(() => this.resetAllAndQuery(), "Neue Suche");

        // Meldet, ob die Auswahl der Verschlüsselung angeboten werden soll.
        get showEncryption(): boolean {
            return !this._filter.station;
        }

        // Meldet, ob die Auswahl der Art der Quelle angeboten werden soll.
        get showSourceType(): boolean {
            return this.showEncryption;
        }

        // Der aktuell anzuzeigende Ausschnitt aus der Ergebnisliste.
        entries: Guide.GuideEntry[] = [];

        // Der aktuell ausgewählte Auftrag.
        private _selectedJob = "*";

        // Die aktuelle Liste der für das Gerät angelegten Aufträg.
        private _jobSelector = new JMSLib.App.EditStringFromList(this, "_selectedJob", null, "zum Auftrag", true, []);

        // Gesetzt, wenn eine nächste Seite der Ergebnisliste existiert.
        private _hasMore = false;

        // Beschreibt den Gesamtauszug der Programmzeitschrift zum aktuell ausgewählten Gerät.
        private _profileInfo: VCRServer.GuideInfoContract;

        // Erstellt eine neue Instanz zur Anzeige der Programmzeitschrift.
        constructor(application: Application) {
            super("guide", application);

            // Navigation abweichend vom Standard konfigurieren.
            this.navigation.favorites = true;
            this.navigation.guide = false;
        }

        // Wird aufgerufen wenn in der Oberfläche die Programmzeitschrift angezeigt werden soll.
        reset(sections: string[]): void {
            // Sicherstellen, dass alte Serveranfragen verworfen werden.
            this._queryId++;

            // Anzeige löschen.
            this.entries = [];
            this._hasMore = false;

            // Größe der Anzeigeliste auf den neusten Stand bringen - alle anderen Einschränkungen bleiben erhalten!
            this._filter.size = this.application.profile.guideRows;

            // Die Liste aller bekannten Geräte ermitteln.
            VCRServer.ProfileCache.getAllProfiles().then(profiles => {
                // Auswahl aktualisieren.
                this.profiles.allowedValues = (profiles || []).map(p => <JMSLib.App.IUiValue<string>>{ display: p.name, value: p.name });
                this.profiles.validate();

                // Erstes Gerät vorauswählen.
                if (!this._filter.device || this.profiles.message)
                    this._filter.device = this.profiles.allowedValues[0].value;

                // Ergebnisliste aktualisieren.
                this.onDeviceChanged(false);
            });
        }

        // Meldet die Überschrift der Seite.
        get title(): string {
            return "Programmzeitschrift";
        }

        // Alle Einschränkungen entfernen.
        private clearFilter(): void {
            this._filter.cryptFilter = VCRServer.GuideEncryption.ALL;
            this._filter.typeFilter = VCRServer.GuideSource.ALL;
            this._filter.content = null;
            this._filter.station = "";
            this._filter.start = null;
            this._filter.title = null;
            this._withContent = true;
            this._filter.index = 0;
            this._query = "";
            this._hour = -1;
        }

        // Nach der Auswahl des Gerätes alle Listen aktualisieren.
        private onDeviceChanged(deviceHasChanged: boolean) {
            VCRServer.GuideInfoCache.getPromise(this._filter.device).then(info => {
                // Informationen zur Programmzeitschrift des Gerätes festhalten.
                this._profileInfo = info;

                // Daraus die Liste der Quellen und möglichen Starttage ermitteln.
                this.refreshSources();
                this.refreshDays();

                // Liste der Aufträge laden.
                return VCRServer.getProfileJobInfos(this._filter.device);
            }).then(jobs => {
                // Liste der bekannten Aufträge aktualisieren.
                var selection = jobs.map(job => <JMSLib.App.IUiValue<string>>{ display: job.name, value: job.id });

                selection.unshift(<JMSLib.App.IUiValue<string>>{ display: "(neuen Auftrag anlegen)", value: "" });

                this._jobSelector.allowedValues = selection;

                // Ergebnisliste neu laden - bei Wechsel des Gerätes werden alle Einschränkungen entfernt.
                if (deviceHasChanged)
                    this.resetAllAndQuery();
                else
                    this.query();
            });
        }

        // Die Liste der Quellen des aktuell ausgewählten Gerätes neu ermitteln.
        private refreshSources(): void {
            var sources = (this._profileInfo.stations || []).map(s => <JMSLib.App.IUiValue<string>>{ display: s, value: s });

            // Der erste Eintrag erlaubt immer die Anzeige ohne vorausgewählter Quelle.
            sources.unshift({ display: "(Alle Sender)", value: "" });

            this.sources.allowedValues = sources;
        }

        // Die Liste der möglichen Starttage ermitteln.
        private refreshDays(): void {
            var days: JMSLib.App.IUiValue<string>[] = [];

            // Als Basis kann immer die aktuelle Uhrzeit verwendet werden.
            days.push({ display: "Jetzt", value: null });

            // Das geht nur, wenn mindestens ein Eintrag in der Programmzeitschrift der aktuellen Quelle vorhanden ist.
            if (this._profileInfo.first && this._profileInfo.last) {
                // Die Zeiten werden immer in UTC gemeldet, die Anzeige erfolgt aber immer lokal - das kann am ersten Tag zu fehlenden Einträgen führen.
                var first = new Date(this._profileInfo.first);
                var last = new Date(this._profileInfo.last);

                // Es werden maximal 14 mögliche Starttage angezeigt.
                for (var i = 0; (i < 14) && (first.getTime() <= last.getTime()); i++) {
                    // Korrigieren.
                    var start = new Date(first.getFullYear(), first.getMonth(), first.getDate());

                    // Auswahlelement anlegen.
                    days.push({ display: JMSLib.App.DateFormatter.getShortDate(start), value: start.toISOString() });

                    // Nächsten Tag auswählen.
                    first = new Date(start.getFullYear(), start.getMonth(), start.getDate() + 1);
                }
            }

            this.days.allowedValues = days;
        }

        // Deaktiviert die verzögerte Ausführung einer Suchanfrage.
        private clearTimeout(): void {
            if (this._timeout === undefined)
                return;

            clearTimeout(this._timeout);

            this._timeout = undefined;
        }

        // Aktiviert die verzögerte Ausführung einer Suchanfrage.
        private delayedQuery(): void {
            this.clearTimeout();

            // Mit den Suchanfragen synchronisieren.
            var queryId = ++this._queryId;

            // Nach einer viertel Sekunde Suche starten.
            this._timeout = setTimeout(() => {
                // Suchanfrage starten, falls die Programmzeitschrift noch aktiv ist und inzwischen keine anderen Anfragen gestartet wurden.
                if (this._queryId === queryId)
                    if (this.site)
                        this.query();
            }, 250);
        }

        // Entfernt alle Einschränkungen und führt eine neue Suche aus.
        private resetAllAndQuery(): void {
            this.clearFilter();
            this.query();
        }

        // Setzt die Ergebnisliste auf den Anfang und führt eine neue Suche aus.
        private resetIndexAndQuery(): void {
            this._filter.index = 0;
            this.query();
        }

        // Führt eine Suche aus.
        private query(): void {
            // Eine eventuell ausstehende verzögerte Suche deaktivieren.
            this.clearTimeout();

            // Eindeutige Nummer für die Anfrage erstellen.
            var queryId = ++this._queryId;

            // Ausstehende Änderung der Startzeit einmischen.
            if (this._hour >= 0) {
                // Vollen Startzeitpunkt bestimmen.
                var start = this._filter.start ? new Date(this._filter.start) : new Date();

                this._filter.start = new Date(start.getFullYear(), start.getMonth(), start.getDate(), this._hour).toISOString();

                // Das machen wir immer nur einmal - die Auswahl wirkt dadurch wie eine Schaltfläche.
                this._hour = -1;
            }

            // Suchbedingung vorbereiten und übernehmen.
            var query = this._query.trim();

            this._filter.title = (query === "") ? null : `*${query}`;
            this._filter.content = this._withContent ? this._filter.title : null;

            // Auszug aus der Programmzeitschrift abrufen.
            VCRServer.queryProgramGuide(this._filter).then(items => {
                // Es handelt sich eventuell nicht mehr um die aktuelle Anfrage.
                if (this._queryId !== queryId)
                    return;

                // Einträge im Auszug auswerten.
                var toggleDetails = this.toggleDetails.bind(this);
                var createNew = this.createNewSchedule.bind(this);

                this.entries = (items || []).slice(0, this._filter.size).map(i => new Guide.GuideEntry(i, toggleDetails, createNew, this._jobSelector));
                this._hasMore = items && (items.length > this._filter.size);

                // Anwendung zur Bedienung freischalten.
                this.application.setBusy(false);

                // Anzeige aktualisieren.
                this.refreshUi();
            });
        }

        // Legt eine neue Aufzeichnung an.
        private createNewSchedule(entry: Guide.GuideEntry): void {
            this.application.gotoPage(`${this.application.editPage.route};id=*${entry.jobSelector.value};epgid=${entry.id}`);
        }

        // Aktualisiert die Detailanzeige für einen Eintrag.
        private toggleDetails(entry: Guide.GuideEntry): void {
            // Anzeige umschalten.
            var show = entry.showDetails;

            this.entries.forEach(e => e.showDetails = false);

            entry.showDetails = !show;

            // Oberfläche aktualisieren.
            this._selectedJob = "";

            this.refreshUi();
        }

        // In der Ergebnisliste bättern.
        private changePage(delta: number): void {
            // Startseite ändern und neue Suche ausführen.
            this._filter.index += delta;
            this.query();
        }
    }
}