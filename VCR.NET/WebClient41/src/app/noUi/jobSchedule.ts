namespace VCRNETClient.App.NoUi {

    // Bietet die Daten eines Auftrags zur Pflege an.
    export abstract class JobScheduleEditor<TModelType extends VCRServer.EditJobScheduleCommonContract>  {
        constructor(protected model: TModelType, mustHaveName: boolean, favoriteSources: string[], onChange: () => void) {
            // Pflegekomponenten erstellen
            this.name = new StringEditor(this.model, "name", onChange, mustHaveName);
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
        validate(sources: VCRServer.SourceEntry[]): void {
            // Aktualisieren.
            this.source.setSources(sources);

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