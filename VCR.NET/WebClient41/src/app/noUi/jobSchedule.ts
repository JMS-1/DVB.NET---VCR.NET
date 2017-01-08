namespace VCRNETClient.App.NoUi {

    // Schnittstelle zur Pflege der gemeinsamen Daten eines Auftrags oder einer Aufzeichnung.
    export interface ISourceFlagsEditor extends IDisplayText {
        // Gesetzt um alle Sprachen aufzuzeichnen
        readonly allLanguages: IBooleanEditor;

        // Gesetzt, um die Dolby Digital Tonspur aufzuzeichnen
        readonly includeDolby: IBooleanEditor;

        // Gesetzt, um den Videotext aufzuzeichnen
        readonly withVideotext: IBooleanEditor;

        // Gesetzt, um die Untertitel aufzuzeichnen
        readonly withSubtitles: IBooleanEditor;
    }

    // Schnittstelle zur Pflege der gemeinsamen Daten eines Auftrags oder einer Aufzeichnung.
    export interface IJobScheduleEditor {
        // Die zugehörige Seite der Anwendung.
        readonly page: IPage;

        // Der Name des Auftrags.
        readonly name: IStringEditor;

        // Der Name der Quelle, die aufgezeichnet werden soll.
        readonly source: IChannelSelector;

        // Aufzeichnungsoptionen.
        readonly sourceFlags: ISourceFlagsEditor;
    }

    // Bietet die gemeinsamen Daten eines Auftrags oder einer Aufzeichnung zur Pflege an.
    export abstract class JobScheduleEditor<TModelType extends VCRServer.EditJobScheduleCommonContract> implements IJobScheduleEditor {
        constructor(public readonly page: IPage, protected model: TModelType, mustHaveName: boolean, favoriteSources: string[], onChange: () => void) {
            var noSource = () => (this.source.val() || "").trim().length < 1;

            // Pflegekomponenten erstellen
            this.name = new StringEditor(this.model, "name", onChange, "Name", mustHaveName, "Ein Auftrag muss einen Namen haben.");
            this.source = new ChannelEditor(this.model, "sourceName", favoriteSources, onChange);
            this.sourceFlags = {
                includeDolby: new BooleanEditor(this.model, "includeDolby", onChange, "Dolby Digital (AC3)", noSource),
                withSubtitles: new BooleanEditor(this.model, "withSubtitles", onChange, "DVB Untertitel", noSource),
                allLanguages: new BooleanEditor(this.model, "allLanguages", onChange, "Alle Sprachen", noSource),
                withVideotext: new BooleanEditor(this.model, "withVideotext", onChange, "Videotext", noSource),
                text: "Besonderheiten"
            };
        }

        // Der Name des Auftrags.
        readonly name: StringEditor;

        // Der Name der Quelle, die aufgezeichnet werden soll.
        readonly source: ChannelEditor;

        // Aufzeichnungsoptionen.
        readonly sourceFlags: {
            readonly text: string;
            readonly allLanguages: BooleanEditor;
            readonly includeDolby: BooleanEditor;
            readonly withVideotext: BooleanEditor;
            readonly withSubtitles: BooleanEditor;
        };

        // Prüft alle Daten.
        validate(sources: VCRServer.SourceEntry[], sourceIsRequired: boolean): void {
            // Aktualisieren.
            this.source.setSources(sources, sourceIsRequired);

            // Lokalisierte Prüfungen.
            this.name.validate();
            this.source.validate();
            this.sourceFlags.allLanguages.validate();
            this.sourceFlags.includeDolby.validate();
            this.sourceFlags.withVideotext.validate();
            this.sourceFlags.withSubtitles.validate();
        }

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