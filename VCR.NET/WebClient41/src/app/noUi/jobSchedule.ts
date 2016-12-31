namespace VCRNETClient.App.NoUi {

    // Schnittstelle zur Pflege der gemeinsamen Daten eines Auftrags oder einer Aufzeichnung.
    export interface ISourceFlagsEditor {
        readonly name: string;

        // Gesetzt um alle Sprachen aufzuzeichnen
        readonly allLanguages: IBooleanEditor;

        // Gesetzt, um die Dolby Digital Tonspur aufzuzeichnen
        readonly includeDolby: IBooleanEditor;

        // Gesetzt, um den Videotext aufzuzeichnen
        readonly withVideotext: IBooleanEditor;

        // Gesetzt, um die Untertitel aufzuzeichnen
        readonly withSubtitles: IBooleanEditor;

        // Meldet, ob eine Pflege möglich ist.
        isEnabled(): boolean;
    }

    // Schnittstelle zur Pflege der gemeinsamen Daten eines Auftrags oder einer Aufzeichnung.
    export interface IJobScheduleEditor {
        // Der Name des Auftrags.
        readonly name: IStringEditor;

        // Der Name der Quelle, die aufgezeichnet werden soll.
        readonly source: IChannelSelector;

        // Aufzeichnungsoptionen.
        readonly sourceFlags: ISourceFlagsEditor;
    }

    // Bietet die gemeinsamen Daten eines Auftrags oder einer Aufzeichnung zur Pflege an.
    export abstract class JobScheduleEditor<TModelType extends VCRServer.EditJobScheduleCommonContract> implements IJobScheduleEditor {
        constructor(protected model: TModelType, mustHaveName: boolean, favoriteSources: string[], onChange: () => void) {
            // Pflegekomponenten erstellen
            this.name = new StringEditor(this.model, "name", onChange, "Name", mustHaveName, "Ein Auftrag muss einen Namen haben.");
            this.source = new ChannelEditor(this.model, "sourceName", favoriteSources, onChange);
            this.sourceFlags = {
                includeDolby: new BooleanEditor(this.model, "includeDolby", onChange, "Dolby Digital (AC3)"),
                withSubtitles: new BooleanEditor(this.model, "withSubtitles", onChange, "DVB Untertitel"),
                allLanguages: new BooleanEditor(this.model, "allLanguages", onChange, "Alle Sprachen"),
                withVideotext: new BooleanEditor(this.model, "withVideotext", onChange, "Videotext"),
                isEnabled: () => (this.source.val() || "").trim().length > 0,
                name: "Besonderheiten"
            };
        }

        // Der Name des Auftrags.
        readonly name: StringEditor;

        // Der Name der Quelle, die aufgezeichnet werden soll.
        readonly source: ChannelEditor;

        // Aufzeichnungsoptionen.
        readonly sourceFlags: {
            readonly name: string;
            readonly allLanguages: BooleanEditor;
            readonly includeDolby: BooleanEditor;
            readonly withVideotext: BooleanEditor;
            readonly withSubtitles: BooleanEditor;
            isEnabled(): boolean;
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
    }

}