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
        private static readonly _allowedCharacters = /^[^\\\/\:\*\?\"\<\>\|]*$/;

        constructor(public readonly page: IPage, protected model: TModelType, isJob: boolean, favoriteSources: string[], onChange: () => void) {
            var noSource = () => (this.source.value || "").trim().length < 1;

            // Pflegekomponenten erstellen
            this.name = new JMSLib.App.String(this.model, "name", "Name", onChange);
            this.source = new ChannelEditor(this.model, "sourceName", favoriteSources, onChange);
            this.sourceFlags = {
                includeDolby: new JMSLib.App.Flag(this.model, "includeDolby", "Dolby Digital (AC3)", onChange, noSource),
                withSubtitles: new JMSLib.App.Flag(this.model, "withSubtitles", "DVB Untertitel", onChange, noSource),
                allLanguages: new JMSLib.App.Flag(this.model, "allLanguages", "Alle Sprachen", onChange, noSource),
                withVideotext: new JMSLib.App.Flag(this.model, "withVideotext", "Videotext", onChange, noSource),
                text: "Besonderheiten"
            };

            // Zusätzliche Prüfungen einrichten.
            if (isJob)
                this.name.addRequiredValidator(`Ein Auftrag muss einen Namen haben.`);

            this.name.addPatternValidator(JobScheduleEditor._allowedCharacters, `Der Name enthält ungültige Zeichen`);

             // Initiale Prüfung.
            this.name.validate();
            this.source.validate();
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
            if (this.name.message.length > 0)
                return false;
            if (this.source.message.length > 0)
                return false;
            if (this.sourceFlags.allLanguages.message.length > 0)
                return false;
            if (this.sourceFlags.includeDolby.message.length > 0)
                return false;
            if (this.sourceFlags.withVideotext.message.length > 0)
                return false;
            if (this.sourceFlags.withSubtitles.message.length > 0)
                return false;

            return true;
        }
    }

}