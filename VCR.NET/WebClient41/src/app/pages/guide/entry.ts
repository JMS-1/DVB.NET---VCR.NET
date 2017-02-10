namespace VCRNETClient.App.Guide {

    // Schnittstelle zur Anzeige eines Eintrags in der Programmzeitschrift.
    export interface IGuideEntry {
        // Die Kennung des Eintrags.
        readonly id: string;

        // Beginn der Sendung.
        readonly startDisplay: string;

        // Ende der Sendung.
        readonly endDisplay: string;

        // Quelle.
        readonly source: string;

        // Name der Sendung.
        readonly name: string;

        // Sprache.
        readonly language: string;

        // Dauer.
        readonly duration: string;

        // Freigabe.
        readonly rating: string;

        // Art der Sendung.
        readonly content: string;

        // Kurzbeschreibung.
        readonly shortDescription: string;

        // Vollständige Beschreibung.
        readonly longDescription: string;

        // Gesetzt, wenn die Detailansicht eingeblendet werden soll.
        readonly showDetails: boolean;

        // Schaltet die Anzeige der Detailansicht um.
        toggleDetail(): void;

        // Auswahlliste mit allen Aufträgen auf dem zugehörigen Gerät.
        readonly jobSelector: JMSLib.App.IValueFromList<string>;

        // Methode zum Anlegen einer neuen Aufzeichnung.
        readonly createNew: JMSLib.App.ICommand;
    }

    // Repräsentiert einen Eintrag in der Programmzeitschrift.
    export class GuideEntry implements IGuideEntry {

        // Erstellt eine neue Beschreibung.
        constructor(private readonly _model: VCRServer.GuideItemContract, private _toggleDetails: (entry: GuideEntry) => void, createNew: (entry: GuideEntry) => void, public jobSelector: JMSLib.App.IValueFromList<string>) {
            // Zeitraum der Sendung.
            var start = new Date(_model.start);
            var end = new Date(start.getTime() + 1000 * _model.duration);

            // Zeitraum zur direkten Anzeige aufbereiten.
            this.startDisplay = JMSLib.DateFormatter.getStartTime(start);
            this.endDisplay = JMSLib.DateFormatter.getEndTime(end);

            // Befehl zum Neuanlegen einer Aufzeichnung einrichten.
            this.createNew = new JMSLib.App.Command(() => createNew(this), "Aufzeichnung anlegen", () => end > new Date());
        }

        // Befehl zum Anlegen einer neuen Aufzeichnung.
        readonly createNew: JMSLib.App.ICommand;

        // Startzeit der Sendung.
        readonly startDisplay: string;

        // Endzeit der Sendung.
        readonly endDisplay: string;

        // Gesetzt, wenn die Detailansicht eingeblendet werden soll.
        showDetails = false;

        // Die eindeutige Kennung des Eintrags.
        get id(): string {
            return this._model.id;
        }

        // Meldet die Quelle.
        get source(): string {
            return this._model.station;
        }

        // Meldet den Namen der Sendung.
        get name(): string {
            return this._model.name;
        }

        // Schaltet die Anzeige der Detailansicht um.
        toggleDetail(): void {
            this._toggleDetails(this);
        }

        // Meldet die Sprache der Sendung.
        get language(): string {
            return this._model.language;
        }

        // Meldet die Kurzbeschreibung der Sendung.
        get shortDescription(): string {
            return this._model.shortDescription;
        }

        // Meldet die ausführliche Beschreibung der Sendung.
        get longDescription(): string {
            return this._model.description;
        }

        // Meldet die Dauer der Sendung.
        get duration(): string {
            return JMSLib.DateFormatter.getDuration(new Date(1000 * this._model.duration));
        }

        // Meldet die Freigabe der Sendung.
        get rating(): string {
            return (this._model.ratings || []).join(" ");
        }

        // Meldet die Art der Sendung.
        get content(): string {
            return (this._model.categories || []).join(" ");
        }

    }
}