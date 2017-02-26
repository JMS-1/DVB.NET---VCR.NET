/// <reference path="section.ts" />

namespace VCRNETClient.App.Admin {

    // Schnittstelle zur Pflege der Konfiguration der Programmzeitschrift.
    export interface IAdminGuidePage extends ISection {
        // Gesetzt, wenn die automatische Aktualisierung der Programmzeitschrift aktiviert wurde.
        readonly isActive: JMSLib.App.IFlag;

        // Die Liste der Stunden, an denen eine automatische Aktivierung stattfinden soll.
        readonly hours: JMSLib.App.IMultiValueFromList<number>;

        // Alle Quellen, deren Programmzeitschrift ausgelesen werden soll.
        readonly sources: JMSLib.App.IMultiValueFromList<string>;

        // Gesetzt, wenn auch die englische programmzeitschrift eingeschlossen werden soll.
        readonly ukTv: JMSLib.App.IFlag;

        // Die Auswahl eines Geräte für die folgende Auswahl einer Quelle.
        readonly device: JMSLib.App.IValueFromList<string>;

        // Die Auswahl einer Quelle des aktuell ausgewählten Gerätes.
        readonly source: IChannelSelector;

        // Entfernt die ausgewählten Quellen aus der Liste der zu untersuchenden Quellen.
        readonly remove: JMSLib.App.ICommand;

        // Fügt eine Quelle zur Liste der zu untersuchenden Quellen hinzu.
        readonly add: JMSLib.App.ICommand;

        // Maximale Dauer für die Sammlung der Programmzeitschrift (in Minuten).
        readonly duration: JMSLib.App.INumber;

        // Minimale Dauer zwischen zwei Sammlungen (in Minuten).
        readonly delay: JMSLib.App.INumber;

        // Interval für die vorgezogene Sammlung (in Minuten).
        readonly latency: JMSLib.App.INumber;
    }

    // Präsentationsmodell zur Pflege der Konfiguration der Aktualisierung der Programmzeitschrift.
    export class GuideSection extends Section implements IAdminGuidePage {

        // Gesetzt, wenn die automatische Aktualisierung der Programmzeitschrift aktiviert wurde.
        readonly isActive = new JMSLib.App.Flag({}, "value", "Aktualisierung aktivieren", () => this.refreshUi());

        // Die Liste der Stunden, an denen eine automatische Aktivierung stattfinden soll.
        readonly hours = new JMSLib.App.SelectMultipleFromList<number>({}, "hours", "Uhrzeiten", null, AdminPage.hoursOfDay);

        // Alle Quellen, deren Programmzeitschrift ausgelesen werden soll.
        readonly sources = new JMSLib.App.SelectMultipleFromList<string>({}, "value", null, () => this.remove && this.remove.refreshUi());

        // Gesetzt, wenn auch die englische programmzeitschrift eingeschlossen werden soll.
        readonly ukTv = new JMSLib.App.Flag({}, "includeUK", "Sendungsvorschau englischer Sender (FreeSat UK) abrufen");

        // Entfernt die ausgewählten Quellen aus der Liste der zu untersuchenden Quellen.
        readonly remove = new JMSLib.App.Command(() => this.removeSources(), "Entfernen", () => this.sources.value.length > 0);

        // Die Auswahl eines Geräte für die folgende Auswahl einer Quelle.
        readonly device = new JMSLib.App.SelectSingleFromList<string>({}, "value", "Quellen des Gerätes", () => !this.page.application.isBusy && this.loadSources());

        // Die Auswahl einer Quelle des aktuell ausgewählten Gerätes.
        readonly source: ChannelEditor;

        // Fügt eine Quelle zur Liste der zu untersuchenden Quellen hinzu.
        readonly add = new JMSLib.App.Command(() => this.addSource(), "Hinzufügen", () => this.source.value && this.sources.allowedValues.every(v => v.value !== this.source.value));

        // Maximale Dauer für die Sammlung der Programmzeitschrift (in Minuten).
        readonly duration = new JMSLib.App.Number({}, "duration", "Maximale Laufzeit einer Aktualisierung in Minuten", () => this.update.refreshUi())
            .addRequiredValidator()
            .addMinValidator(5)
            .addMaxValidator(55);

        // Minimale Dauer zwischen zwei Sammlungen (in Minuten).
        readonly delay = new JMSLib.App.Number({}, "minDelay", "Wartezeit zwischen zwei Aktualisierungen in Stunden (optional)", () => this.update.refreshUi())
            .addMinValidator(1)
            .addMaxValidator(23);

        // Interval für die vorgezogene Sammlung (in Minuten).
        readonly latency = new JMSLib.App.Number({}, "joinHours", "Latenzzeit für vorgezogene Aktualisierungen in Stunden (optional)", () => this.update.refreshUi())
            .addMinValidator(1)
            .addMaxValidator(23);

        // Erstellt ein neues Präsentationsmodell.
        constructor(page: AdminPage) {
            super(page);

            // Auswahl der Quelle vorbereiten.
            this.source = new ChannelEditor({}, "value", this.page.application.profile.recentSources || [], () => this.refreshUi());
        }

        // Forder die Konfiguration zur Pflege der Programmzeitschrift an.
        protected loadAsync(): void {
            // Neu initialisieren.
            this.add.reset();
            this.remove.reset();

            this.device.value = null;
            this.source.allSources = [];
            this.source.value = ``;

            // Daten vom VCR.NET Recording Service abrufen.
            VCRServer.getGuideSettings().then(settings => {
                // Daten mit den Präsentationsmodellen verbinden.
                this.isActive.value = (settings.duration > 0);
                this.duration.data = settings;
                this.latency.data = settings;
                this.hours.data = settings;
                this.delay.data = settings;
                this.ukTv.data = settings;

                // Die aktuelle Liste der Quellen laden.
                this.sources.allowedValues = settings.sources.map(s => JMSLib.App.uiValue(s));

                // Liste der Geräteprofile anfordern.
                return VCRServer.ProfileCache.getAllProfiles();
            }).then(profiles => {
                // Alle bekannten Geräteprofile.
                this.device.allowedValues = profiles.map(p => JMSLib.App.uiValue(p.name));

                // Das erste Profil auswählen.
                if (this.device.allowedValues.length > 0)
                    this.device.value = this.device.allowedValues[0].value;

                // Die Liste der Quellen aktualisieren.
                this.loadSources();
            });
        }

        // Prüft, ob ein Speichern möglich ist.
        protected get isValid(): boolean {
            // Immer, wenn die automatische Aktualisierung deaktiviert ist.
            if (!this.isActive.value)
                return true;

            // Alle Zahlen müssen fehlerfrei sein.
            if (this.duration.message)
                return false;
            if (this.latency.message)
                return false;
            if (this.delay.message)
                return false;

            // Dann können wir auch speichern.
            return true;
        }

        // Fordert die Liste der Quellen vom aktuellen ausgewählten Gerät an.
        private loadSources(): void {
            VCRServer.ProfileSourcesCache.getSources(this.device.value).then(sources => {
                // Auswahlliste setzen.
                this.source.allSources = sources;

                // Anwendung zur Benutzung freischalten.
                this.page.application.isBusy = false;

                // Oberfläche zur Aktualisierung auffordern.
                this.refreshUi();
            });
        }

        // Alle ausgewählten Quellen entfernen.
        private removeSources(): void {
            this.sources.allowedValues = this.sources.allowedValues.filter(v => !v.isSelected);
        }

        // Neue Quelle zur Liste der zu berücksichtigenden Quellen hinzufügen.
        private addSource(): void {
            this.sources.allowedValues = this.sources.allowedValues.concat([JMSLib.App.uiValue(this.source.value)]);

            // Die Auswahl setzen wir aber direkt wieder zurück.
            this.source.value = ``;
        }

        // Die Konfiguration zur Aktualisierung an den VCR.NET Recording Service übertragen.
        protected saveAsync(): JMSLib.App.IHttpPromise<boolean> {
            // Die Auswahlliste der Quellen ist die Liste der zu berücksichtigenden Quellen.
            var settings = <VCRServer.GuideSettingsContract>this.hours.data;

            settings.sources = this.sources.allowedValues.map(v => v.value);

            // Die Aktivierung der Aktualisierung wird über die Dauer gesteuert.
            if (!this.isActive.value)
                settings.duration = 0;

            // Speicherung anfordern.
            return VCRServer.setGuideSettings(settings);
        }
    }
}