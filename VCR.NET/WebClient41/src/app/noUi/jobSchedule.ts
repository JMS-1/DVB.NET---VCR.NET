namespace VCRNETClient.App.NoUi {

    // Schnittstelle zur Pflege der gemeinsamen Daten eines Auftrags oder einer Aufzeichnung.
    export interface IJobScheduleEditor {
        // Der Name des Auftrags.
        readonly name: IStringEditor;

        // Der Name der Quelle, die aufgezeichnet werden soll.
        readonly source: IChannelSelector;

        // Gesetzt um alle Sprachen aufzuzeichnen
        readonly allLanguages: IBooleanEditor;

        // Gesetzt, um die Dolby Digital Tonspur aufzuzeichnen
        readonly includeDolby: IBooleanEditor;

        // Gesetzt, um den Videotext aufzuzeichnen
        readonly withVideotext: IBooleanEditor;

        // Gesetzt, um die Untertitel aufzuzeichnen
        readonly withSubtitles: IBooleanEditor;
    }

    // Bietet die gemeinsamen Daten eines Auftrags oder einer Aufzeichnung zur Pflege an.
    export abstract class JobScheduleEditor<TModelType extends VCRServer.EditJobScheduleCommonContract> implements IJobScheduleEditor {
        constructor(protected model: TModelType, mustHaveName: boolean, favoriteSources: string[], onChange: () => void) {
            // Pflegekomponenten erstellen
            this.name = new StringEditor(this.model, "name", onChange, mustHaveName, "Ein Auftrag muss einen Namen haben.");
            this.allLanguages = new BooleanEditor(this.model, "allLanguages", onChange);
            this.includeDolby = new BooleanEditor(this.model, "includeDolby", onChange);
            this.withVideotext = new BooleanEditor(this.model, "withVideotext", onChange);
            this.withSubtitles = new BooleanEditor(this.model, "withSubtitles", onChange);
            this.source = new ChannelEditor(this.model, "sourceName", favoriteSources, onChange);
        }

        // Der Name des Auftrags.
        readonly name: StringEditor;

        // Der Name der Quelle, die aufgezeichnet werden soll.
        readonly source: ChannelEditor;

        // Gesetzt um alle Sprachen aufzuzeichnen
        readonly allLanguages: BooleanEditor;

        // Gesetzt, um die Dolby Digital Tonspur aufzuzeichnen
        readonly includeDolby: BooleanEditor;

        // Gesetzt, um den Videotext aufzuzeichnen
        readonly withVideotext: BooleanEditor;

        // Gesetzt, um die Untertitel aufzuzeichnen
        readonly withSubtitles: BooleanEditor;

        // Prüft alle Daten.
        validate(sources: VCRServer.SourceEntry[], sourceIsRequired: boolean): void {
            // Aktualisieren.
            this.source.setSources(sources, sourceIsRequired);

            // Lokalisierte Prüfungen.
            this.name.validate();
            this.source.validate();
            this.allLanguages.validate();
            this.includeDolby.validate();
            this.withVideotext.validate();
            this.withSubtitles.validate();
        }
    }

}