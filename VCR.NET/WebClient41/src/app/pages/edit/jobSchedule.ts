namespace VCRNETClient.App.Edit {

    // Schnittstelle zur Pflege der gemeinsamen Daten eines Auftrags oder einer Aufzeichnung.
    export interface ISourceFlagsEditor extends JMSLib.App.IDisplay {
        // Gesetzt um alle Sprachen aufzuzeichnen
        readonly allLanguages: JMSLib.App.IFlag;

        // Gesetzt, um die Dolby Digital Tonspur aufzuzeichnen
        readonly includeDolby: JMSLib.App.IFlag;

        // Gesetzt, um den Videotext aufzuzeichnen
        readonly withVideotext: JMSLib.App.IFlag;

        // Gesetzt, um die Untertitel aufzuzeichnen
        readonly withSubtitles: JMSLib.App.IFlag;
    }

    // Schnittstelle zur Pflege der gemeinsamen Daten eines Auftrags oder einer Aufzeichnung.
    export interface IJobScheduleEditor {
        // Die zugehörige Seite der Anwendung.
        readonly page: IPage;

        // Der Name des Auftrags.
        readonly name: JMSLib.App.IString;

        // Der Name der Quelle, die aufgezeichnet werden soll.
        readonly source: IChannelSelector;

        // Aufzeichnungsoptionen.
        readonly sourceFlags: ISourceFlagsEditor;
    }

    // Bietet die gemeinsamen Daten eines Auftrags oder einer Aufzeichnung zur Pflege an.
    export abstract class JobScheduleEditor<TModelType extends VCRServer.EditJobScheduleCommonContract> implements IJobScheduleEditor {

        // Ein Muster zum Erkennen gültiger Namen von Aufzeichnungen.
        private static readonly _allowedCharacters = /^[^\\\/\:\*\?\"\<\>\|]*$/;

        // Erstelltein neues Präsentationsmodell an.
        constructor(public readonly page: IPage, protected model: TModelType, favoriteSources: string[], onChange: () => void) {
            // Prüfung auf die Auswahl einer Quelle - ohne eine solche machen die Optionen zur Aufzeichnung auch keinen Sinn.
            var noSource = () => (this.source.value || "").trim().length < 1;

            // Pflegekomponenten erstellen
            this.name = new JMSLib.App.String(this.model, "name", "Name", onChange);
            this.source = new ChannelEditor(page.application.profile, this.model, "sourceName", favoriteSources, onChange);
            this.sourceFlags = {
                includeDolby: new JMSLib.App.Flag(this.model, "includeDolby", "Dolby Digital (AC3)", onChange, noSource),
                withSubtitles: new JMSLib.App.Flag(this.model, "withSubtitles", "DVB Untertitel", onChange, noSource),
                allLanguages: new JMSLib.App.Flag(this.model, "allLanguages", "Alle Sprachen", onChange, noSource),
                withVideotext: new JMSLib.App.Flag(this.model, "withVideotext", "Videotext", onChange, noSource),
                text: "Besonderheiten"
            };

            // Zusätzliche Prüfungen einrichten.
            this.name.addPatternValidator(JobScheduleEditor._allowedCharacters, `Der Name enthält ungültige Zeichen`);

            // Initiale Prüfung.
            this.name.validate();
            this.sourceFlags.includeDolby.validate();
            this.sourceFlags.allLanguages.validate();
            this.sourceFlags.withSubtitles.validate();
            this.sourceFlags.withVideotext.validate();
        }

        // Der Name des Auftrags.
        readonly name: JMSLib.App.String;

        // Der Name der Quelle, die aufgezeichnet werden soll.
        readonly source: ChannelEditor;

        // Aufzeichnungsoptionen.
        readonly sourceFlags: {
            readonly text: string;
            readonly allLanguages: JMSLib.App.Flag;
            readonly includeDolby: JMSLib.App.Flag;
            readonly withVideotext: JMSLib.App.Flag;
            readonly withSubtitles: JMSLib.App.Flag;
        };

        // Gesetzt, wenn die Einstellungen der Quelle gültig sind.
        isValid(): boolean {
            // Wir fragen einfach alle unsere Präsentationsmodelle.
            if (this.name.message)
                return false;
            if (this.source.sourceName.message)
                return false;
            if (this.sourceFlags.allLanguages.message)
                return false;
            if (this.sourceFlags.includeDolby.message)
                return false;
            if (this.sourceFlags.withVideotext.message)
                return false;
            if (this.sourceFlags.withSubtitles.message)
                return false;

            return true;
        }
    }

}