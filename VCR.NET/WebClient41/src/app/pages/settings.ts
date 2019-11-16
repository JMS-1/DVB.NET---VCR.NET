/// <reference path="page.ts" />

namespace VCRNETClient.App {

    // Schnittstelle zur Pflege der Benutzereinstellung.
    export interface ISettingsPage extends IPage {
        // Die Anzahl von Tagen im Aufzeichnungsplan.
        readonly planDays: JMSLib.App.INumber;

        // Die maximale Anzahl von Quellen in der Liste zuletzt verwendeter Quellen.
        readonly maxFavorites: JMSLib.App.INumber;

        // Die Anzahl der Einträge auf einer Seite der Programmzeitschrift.
        readonly guideRows: JMSLib.App.INumber;

        // Die Vorlaufzeit für neue Aufzeichnung, die aus der Programmzeitschrift angelegt werden (in Minuten).
        readonly preGuide: JMSLib.App.INumber;

        // Die Nachlaufzeit für neue Aufzeichnung, die aus der Programmzeitschrift angelegt werden (in Minuten).
        readonly postGuide: JMSLib.App.INumber;

        // Gesetzt, wenn bevorzugt das Dolby-Digital Tonsignal mit aufgezeichnet werden soll.
        readonly dolby: JMSLib.App.IFlag;

        // Gesetzt, wenn bevorzugt alle Sprachen aufgezeichnet werden sollen.
        readonly allAudio: JMSLib.App.IFlag;

        // Gesetzt, wenn bevorzugt der Videotext mit aufgezeichnet werden soll.
        readonly ttx: JMSLib.App.IFlag;

        // Gesetzt, wenn bevorzugt die DVB Untertitel mit aufgezeichnet werden soll.
        readonly subs: JMSLib.App.IFlag;

        // Gesetzt, wenn beim Abbruch einer laufenden Aufzeichnung bevorzugt der Schlafzustand unterdrückt werden soll.
        readonly noSleep: JMSLib.App.IFlag;

        // Gesetzt, wenn nach dem Anlegen einer neuen Aufzeichnung aus der Programmzeitschrift in diese zurück gekehrt werden soll.
        readonly backToGuide: JMSLib.App.IFlag;

        // Die bevorzugte Einschränkung auf die Art der Quellen bei der Auswahl einer Quelle für eine Aufzeichnung.
        readonly sourceType: JMSLib.App.IValueFromList<string>;

        // Die bevorzugte Einschränkung auf die Verschlüsselung der Quellen bei der Auswahl einer Quelle für eine Aufzeichnung.
        readonly encryption: JMSLib.App.IValueFromList<string>;

        // Befehl zur Aktualisierung der Einstellungen.
        readonly update: JMSLib.App.ICommand;
    }

    // Präsentationsmodell zur Pflege der Einstellungen des Anwenders.
    export class SettingsPage extends Page implements ISettingsPage {

        // Alle Einschränkungen auf die Art der Quellen.
        private static readonly _types = [
            JMSLib.App.uiValue("RT", "Alle Quellen"),
            JMSLib.App.uiValue("R", "Nur Radio"),
            JMSLib.App.uiValue("T", "Nur Fernsehen")
        ];

        // Alle Einschränkungen auf die Verschlüsselung der Quellen.
        private static readonly _encryptions = [
            JMSLib.App.uiValue("FP", "Alle Quellen"),
            JMSLib.App.uiValue("P", "Nur verschlüsselte Quellen"),
            JMSLib.App.uiValue("F", "Nur unverschlüsselte Quellen")
        ];

        // Befehl zur Aktualisierung der Einstellungen.
        readonly update = new JMSLib.App.Command(() => this.save(), "Aktualisieren", () => this.isValid);

        // Die Anzahl von Tagen im Aufzeichnungsplan.
        readonly planDays = new JMSLib.App.Number({}, "planDays", "Anzahl der Vorschautage im Aufzeichnungsplan", () => this.update.refreshUi())
            .addRequiredValidator()
            .addMinValidator(1)
            .addMaxValidator(50);

        // Die maximale Anzahl von Quellen in der Liste zuletzt verwendeter Quellen.
        readonly maxFavorites = new JMSLib.App.Number({}, "recentSourceLimit", "Maximale Größe der Liste zuletzt verwendeter Sendern", () => this.update.refreshUi())
            .addRequiredValidator()
            .addMinValidator(1)
            .addMaxValidator(50);

        // Die Anzahl der Einträge auf einer Seite der Programmzeitschrift.
        readonly guideRows = new JMSLib.App.Number({}, "guideRows", "Anzahl der Einträge pro Seite in der Programmzeitschrift", () => this.update.refreshUi())
            .addRequiredValidator()
            .addMinValidator(10)
            .addMaxValidator(100);

        // Die Vorlaufzeit für neue Aufzeichnung, die aus der Programmzeitschrift angelegt werden (in Minuten).
        readonly preGuide = new JMSLib.App.Number({}, "guideAheadStart", "Vorlaufzeit bei Programmierung über die Programmzeitschrift (in Minuten)", () => this.update.refreshUi())
            .addRequiredValidator()
            .addMinValidator(0)
            .addMaxValidator(240);

        // Die Nachlaufzeit für neue Aufzeichnung, die aus der Programmzeitschrift angelegt werden (in Minuten).
        readonly postGuide = new JMSLib.App.Number({}, "guideBeyondEnd", "Nachlaufzeit bei Programmierung über die Programmzeitschrift (in Minuten)", () => this.update.refreshUi())
            .addRequiredValidator()
            .addMinValidator(0)
            .addMaxValidator(240);

        // Gesetzt, wenn bevorzugt das Dolby-Digital Tonsignal mit aufgezeichnet werden soll.
        readonly dolby = new JMSLib.App.Flag({}, "dolby", "Dolby Digital (AC3)");

        // Gesetzt, wenn bevorzugt alle Sprachen aufgezeichnet werden sollen.
        readonly allAudio = new JMSLib.App.Flag({}, "languages", "Alle Sprachen");

        // Gesetzt, wenn bevorzugt der Videotext mit aufgezeichnet werden soll.
        readonly ttx = new JMSLib.App.Flag({}, "videotext", "Videotext");

        // Gesetzt, wenn bevorzugt die DVB Untertitel mit aufgezeichnet werden soll.
        readonly subs = new JMSLib.App.Flag({}, "subtitles", "DVB Untertitel");

        // Gesetzt, wenn beim Abbruch einer laufenden Aufzeichnung bevorzugt der Schlafzustand unterdrückt werden soll.
        readonly noSleep = new JMSLib.App.Flag({}, "suppressHibernate", "Beim Abbrechen von Aufzeichnungen bevorzugt den Schlafzustand unterdrücken");

        // Gesetzt, wenn nach dem Anlegen einer neuen Aufzeichnung aus der Programmzeitschrift in diese zurück gekehrt werden soll.
        readonly backToGuide = new JMSLib.App.Flag({}, "backToGuide", "Nach Anlegen einer neuen Aufzeichnung zurück zur Programmzeitschrift");

        // Die bevorzugte Einschränkung auf die Art der Quellen bei der Auswahl einer Quelle für eine Aufzeichnung.
        readonly sourceType = new JMSLib.App.SelectSingleFromList({}, "typeFilter", null, null, SettingsPage._types)
            .addRequiredValidator();

        // Die bevorzugte Einschränkung auf die Verschlüsselung der Quellen bei der Auswahl einer Quelle für eine Aufzeichnung.
        readonly encryption = new JMSLib.App.SelectSingleFromList({}, "encryptionFilter", null, null, SettingsPage._encryptions)
            .addRequiredValidator();

        // Erstellt ein neues Präsentationsmodell.
        constructor(application: Application) {
            super(`settings`, application);
        }

        // Initialisiert das Präsentationsmodell.
        reset(sections: string[]): void {
            this.update.reset();

            // Einfache Kopie.
            var newProfile = { ...this.application.profile };

            // Tiefe Kopie der Liste.
            newProfile.recentSources = newProfile.recentSources.slice();

            // Binden.
            this.maxFavorites.data = newProfile;
            this.backToGuide.data = newProfile;
            this.encryption.data = newProfile;
            this.guideRows.data = newProfile;
            this.postGuide.data = newProfile;
            this.preGuide.data = newProfile;
            this.planDays.data = newProfile;
            this.allAudio.data = newProfile;
            this.noSleep.data = newProfile;
            this.dolby.data = newProfile;
            this.sourceType.data = newProfile;
            this.subs.data = newProfile;
            this.ttx.data = newProfile;

            // Die Anwendung wird nun zur Bedienung freigegeben.
            this.application.isBusy = false;
        }

        // Die Überschrift für die Anzeige des Präsentationsmodells.
        get title(): string {
            return `Individuelle Einstellungen ändern`;
        }

        // Gesetzt wenn alle Eingaben konsistenz sind.
        private get isValid(): boolean {
            if (this.planDays.message)
                return false;
            if (this.guideRows.message)
                return false;
            if (this.preGuide.message)
                return false;
            if (this.postGuide.message)
                return false;
            if (this.maxFavorites.message)
                return false;

            return true;
        }

        // Stößt die Aktualisierung der Einstellungen.
        private save(): Promise<void> {
            // Nach dem erfolgreichen Speichern geht es mit der Einstiegsseite los, dabei werden die Einstellungen immer ganz neu geladen.
            return VCRServer.setUserProfile(this.planDays.data).then(() => this.application.gotoPage(null));
        }
    }
}