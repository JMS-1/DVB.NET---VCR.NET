﻿/// <reference path="page.ts" />
/// <reference path="../../web/GuideSource.ts" />
/// <reference path="../../web/GuideEncryption.ts" />

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
        readonly profiles: JMSLib.App.IValueFromList<string>;

        // Alle Quellen auf dem aktuell ausgewählten Gerät.
        readonly sources: JMSLib.App.IValueFromList<string>;

        // Auswahl des Verschlüsselungsfilters.
        readonly encrpytion: JMSLib.App.IValueFromList<VCRServer.GuideEncryption>;

        // Gesetzt, wenn der Verschlüsselungsfilter angezeigt werden soll.
        readonly showEncryption: boolean;

        // Auswahl der Einschränkung auf die Art der Quelle.
        readonly sourceType: JMSLib.App.IValueFromList<VCRServer.GuideSource>;

        // Gesetzt, wenn die Einschränkung der Art der Quelle angezeigt werden soll.
        readonly showSourceType: boolean;

        // Setzt den Anfang der Ergebnisliste auf ein bestimmtes Datum.
        readonly days: JMSLib.App.IValueFromList<string>;

        // Setzt den Anfang der Ergebnisliste auf eine bestimmte Uhrzeit.
        readonly hours: JMSLib.App.IValueFromList<number>;

        // Der aktuelle Text zur Suche in allen Einträgen der Programmzeitschrift.
        readonly queryString: JMSLib.App.IString;

        // Gesetzt, wenn auch in der Beschreibung gesucht werden soll.
        readonly withContent: JMSLib.App.IFlag;

        // Befhel zum Zurücksetzen aller Einschränkungen.
        readonly resetFilter: JMSLib.App.ICommand;

        readonly addFavorite: JMSLib.App.ICommand;
    }

    // Ui View Model zur Anzeige der Programmzeitschrift.
    export class GuidePage extends Page implements IGuidePage {

        // Optionen zur Auswahl der Einschränkung auf die Verschlüsselung.
        private static _cryptOptions = [
            JMSLib.App.uiValue(VCRServer.GuideEncryption.FREE, `Nur unverschlüsselt`),
            JMSLib.App.uiValue(VCRServer.GuideEncryption.PAY, `Nur verschlüsselt`),
            JMSLib.App.uiValue(VCRServer.GuideEncryption.ALL, `Alle Quellen`)
        ];

        // Optionen zur Auswahl der Einschränkuzng auf die Art der Quelle.
        private static _typeOptions = [
            JMSLib.App.uiValue(VCRServer.GuideSource.TV, `Nur Fernsehen`),
            JMSLib.App.uiValue(VCRServer.GuideSource.RADIO, `Nur Radio`),
            JMSLib.App.uiValue(VCRServer.GuideSource.ALL, `Alle Quellen`)
        ];

        // Für den Start der aktuellen Ergebnisliste verfügbaren Auswahloptionen für die Uhrzeit.
        private static _hours = [
            JMSLib.App.uiValue(0, `00:00`),
            JMSLib.App.uiValue(6, `06:00`),
            JMSLib.App.uiValue(12, `12:00`),
            JMSLib.App.uiValue(18, `18:00`),
            JMSLib.App.uiValue(20, `20:00`),
            JMSLib.App.uiValue(22, `22:00`)
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
            station: ``,
            size: 20,
            index: 0
        };

        // Schnittstelle zur Auswahl des zu betrachtenden Gerätes.
        readonly profiles = new JMSLib.App.SelectSingleFromList<string>(this._filter, `device`, `Gerät`, () => this.onDeviceChanged(true), false, []);

        // Schnittstelle zur Auswahl der Quelle.
        readonly sources = new JMSLib.App.SelectSingleFromList<string>(this._filter, `station`, `Quelle`, () => this.query(), false, []);

        // Schnittstelle zur Auswahl der Einschränkung auf die Verschlüsselung.
        readonly encrpytion = new JMSLib.App.SelectSingleFromList<VCRServer.GuideEncryption>(this._filter, `cryptFilter`, null, () => this.query(), false, GuidePage._cryptOptions);

        // Schnittstelle zur Auswahl der Einschränkung auf die Art der Quelle.
        readonly sourceType = new JMSLib.App.SelectSingleFromList<VCRServer.GuideSource>(this._filter, `typeFilter`, null, () => this.query(), false, GuidePage._typeOptions);

        // Schnittstelle zum Setzen eines bestimmten Tags für den Anfang der Ergebnisliste.
        readonly days = new JMSLib.App.SelectSingleFromList<string>(this._filter, `start`, `Datum`, () => this.resetIndexAndQuery(), false, []);

        // Bei der nächsten Abfrage zu setzende Uhrzeit für den Anfang der Ergebnisliste.
        private _hour = -1;

        // Schnittstelle zum Setzen einer bestimmten Uhrzeit für den Anfange der Ergebnisliste.
        readonly hours = new JMSLib.App.SelectSingleFromList<number>(this, `_hour`, `Start ab`, () => this.resetIndexAndQuery(), false, GuidePage._hours);

        // Die aktuelle Freitextsucheingabe.
        private _query = ``;

        // Schnittstelle zur Pflege der Freitextsuchbedingung.
        readonly queryString = new JMSLib.App.String(this, `_query`, `Suche nach`, () => this.delayedQuery());

        // Gesetzt, wenn auch eine Suche auf die Beschreibung erfolgen soll.
        private _withContent = true;

        // Schnittstelle zur Pflege der Auswahl der Freitextsuche auf die Beschreibung.
        readonly withContent = new JMSLib.App.Flag(this, `_withContent`, `Auch in Beschreibung suchen`, () => this.query());

        // Aktuelle Anmeldung für verzögerte Suchanfragen.
        private _timeout: number;

        // Befehl zur Anzeige des Anfangs der Ergebnisliste.
        readonly firstPage = new JMSLib.App.Command(() => this.changePage(-this._filter.index), `Erste Seite`, () => this._filter.index > 0);

        // Befehl zur Anzeige der vorherigen Seite der Ergebnisliste.
        readonly prevPage = new JMSLib.App.Command(() => this.changePage(-1), `Vorherige Seite`, () => this._filter.index > 0);

        // Befehl zur Anzeige der nächsten Seite der Ergebnisliste.
        readonly nextPage = new JMSLib.App.Command(() => this.changePage(+1), `Nächste Seite`, () => this._hasMore);

        // Befehl zum Zurücksetzen aller aktuellen Einschränkungen.
        readonly resetFilter = new JMSLib.App.Command(() => this.resetAllAndQuery(), `Neue Suche`);

        readonly addFavorite = new JMSLib.App.Command(() => this.createFavorite(), `Aktuelle Suche als Favorit hinzufügen`, () => (this._query || ``).trim() !== ``);

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

        // Die aktuelle Liste der für das Gerät angelegten Aufträg.
        private _jobSelector = new JMSLib.App.SelectSingleFromList<string>({}, `value`, `zum Auftrag`, null, true);

        // Gesetzt, wenn eine nächste Seite der Ergebnisliste existiert.
        private _hasMore = false;

        // Beschreibt den Gesamtauszug der Programmzeitschrift zum aktuell ausgewählten Gerät.
        private _profileInfo: VCRServer.GuideInfoContract;

        // Zeigt an, dass die Präsentation gearade die Daten für die initiale Ansicht sammelt.
        private _startup: boolean;

        // Die konkrete Art der Suche.
        private _fulltextQuery = true;

        // Erstellt eine neue Instanz zur Anzeige der Programmzeitschrift.
        constructor(application: Application) {
            super(`guide`, application);

            // Navigation abweichend vom Standard konfigurieren.
            this.navigation.favorites = true;
            this.navigation.guide = false;
        }

        // Wird aufgerufen wenn in der Oberfläche die Programmzeitschrift angezeigt werden soll.
        reset(sections: string[]): void {
            // Sicherstellen, dass alte Serveranfragen verworfen werden.
            this._startup = true;
            this._queryId++;

            // Anzeige löschen.
            this.entries = [];
            this._hasMore = false;
            this.nextPage.reset();
            this.prevPage.reset();
            this.firstPage.reset();
            this.resetFilter.reset();

            // Größe der Anzeigeliste auf den neusten Stand bringen - alle anderen Einschränkungen bleiben erhalten!
            this._filter.size = this.application.profile.guideRows;

            // Die Liste aller bekannten Geräte ermitteln.
            VCRServer.ProfileCache.getAllProfiles().then(profiles => {
                // Auswahl aktualisieren.
                this.profiles.allowedValues = (profiles || []).map(p => JMSLib.App.uiValue(p.name));
                this.profiles.validate();

                // Erstes Gerät vorauswählen.
                if (!this._filter.device || this.profiles.message)
                    this._filter.device = this.profiles.allowedValues[0].value;

                // Die Startphase ist erst einmal abgeschlossen.
                this._startup = false;

                // Ergebnisliste aktualisieren.
                this.onDeviceChanged(false);
            });
        }

        // Meldet die Überschrift der Seite.
        get title(): string {
            return `Programmzeitschrift`;
        }

        // Alle Einschränkungen entfernen.
        private clearFilter(): void {
            this._filter.cryptFilter = VCRServer.GuideEncryption.ALL;
            this._filter.typeFilter = VCRServer.GuideSource.ALL;
            this._filter.content = null;
            this._fulltextQuery = true;
            this._filter.station = ``;
            this._filter.start = null;
            this._filter.title = null;
            this._withContent = true;
            this._filter.index = 0;
            this._query = ``;
            this._hour = -1;
        }

        findInGuide(model: VCRServer.GuideItemContract): void {
            this.clearFilter();

            // Textsuche auf den Namen auf der selben Karte
            this._filter.device = model.id.split(':')[1];
            this._filter.station = model.station;
            this._fulltextQuery = false;
            this._withContent = false;
            this._query = model.name;

            if (this.application.page === this)
                this.query();
            else
                this.application.gotoPage(this.route);
        }

        // Vordefinierte Suche als Suchbedingung laden.
        loadFilter(filter: VCRServer.SavedGuideQueryContract): void {
            this.clearFilter();

            var query = filter.text || ``;

            this._fulltextQuery = (query[0] === `*`);
            this._withContent = !filter.titleOnly;
            this._query = query.substr(1);

            this._filter.cryptFilter = filter.encryption;
            this._filter.typeFilter = filter.sourceType;
            this._filter.station = filter.source;

            this.application.gotoPage(this.route);
        }

        // Nach der Auswahl des Gerätes alle Listen aktualisieren.
        private onDeviceChanged(deviceHasChanged: boolean) {
            if (this._startup)
                return;

            this._startup = true;

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
                var selection = jobs.map(job => JMSLib.App.uiValue(job.id, job.name));

                selection.unshift(JMSLib.App.uiValue(``, `(neuen Auftrag anlegen)`));

                this._jobSelector.allowedValues = selection;

                this._startup = false;

                // Ergebnisliste neu laden - bei Wechsel des Gerätes werden alle Einschränkungen entfernt.
                if (deviceHasChanged)
                    this.resetAllAndQuery();
                else
                    this.query();
            });
        }

        // Die Liste der Quellen des aktuell ausgewählten Gerätes neu ermitteln.
        private refreshSources(): void {
            var sources = (this._profileInfo.stations || []).map(s => JMSLib.App.uiValue(s));

            // Der erste Eintrag erlaubt immer die Anzeige ohne vorausgewählter Quelle.
            sources.unshift(JMSLib.App.uiValue(``, `(Alle Sender)`));

            this.sources.allowedValues = sources;
        }

        // Die Liste der möglichen Starttage ermitteln.
        private refreshDays(): void {
            var days: JMSLib.App.IUiValue<string>[] = [];

            // Als Basis kann immer die aktuelle Uhrzeit verwendet werden.
            days.push(JMSLib.App.uiValue(<string>null, `Jetzt`));

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
                    days.push(JMSLib.App.uiValue(start.toISOString(), JMSLib.App.DateTimeUtils.formatShortDate(start)));

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

            // Nach Eingabe gibt es immer die Volltextsuche.
            this._fulltextQuery = true;

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
            if (this._startup)
                return;

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

            this._filter.title = (query === ``) ? null : `${this._fulltextQuery ? `*` : `=`}${query}`;
            this._filter.content = (this._withContent && this._fulltextQuery) ? this._filter.title : null;

            // Auszug aus der Programmzeitschrift abrufen.
            VCRServer.queryProgramGuide(this._filter).then(items => {
                // Es handelt sich eventuell nicht mehr um die aktuelle Anfrage.
                if (this._queryId !== queryId)
                    return;

                // Einträge im Auszug auswerten.
                var toggleDetails = this.toggleDetails.bind(this);
                var createNew = this.createNewSchedule.bind(this);
                var similiar = this.findInGuide.bind(this);

                this.entries = (items || []).slice(0, this._filter.size).map(i => new Guide.GuideEntry(i, similiar, toggleDetails, createNew, this._jobSelector));
                this._hasMore = items && (items.length > this._filter.size);

                // Anwendung zur Bedienung freischalten.
                this.application.isBusy = false;

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
            this._jobSelector.value = ``;

            this.refreshUi();
        }

        // In der Ergebnisliste bättern.
        private changePage(delta: number): void {
            // Startseite ändern und neue Suche ausführen.
            this._filter.index += delta;
            this.query();
        }

        private createFavorite(): JMSLib.App.IHttpPromise<void> {
            var query: VCRServer.SavedGuideQueryContract = {
                encryption: this._filter.station ? VCRServer.GuideEncryption.ALL : this._filter.cryptFilter,
                sourceType: this._filter.station ? VCRServer.GuideSource.ALL : this._filter.typeFilter,
                text: `${this._fulltextQuery ? `*` : `=`}${this._query}`,
                titleOnly: !this._withContent,
                source: this._filter.station,
                device: this._filter.device
            };

            return this.application.favoritesPage.add(query);
        }
    }
}