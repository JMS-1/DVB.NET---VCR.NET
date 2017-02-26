namespace VCRNETClient.App.Guide {

    // Schnittstelle zur Anzeige eines Eintrags in der Programmzeitschrift.
    export interface IGuideInfo {
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

        // Suche nach ähnlichen Einträgen in der Programmzeitschrift.
        readonly findSimiliar: JMSLib.App.ICommand;
    }

    // Schnittstelle zur Anzeige eines Eintrags in der Programmzeitschrift.
    export interface IGuideEntry extends IGuideInfo {
        // Die Kennung des Eintrags.
        readonly id: string;

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
    export class GuideInfo implements IGuideInfo {

        // Erstellt eine neue Beschreibung.
        constructor(protected readonly model: VCRServer.GuideItemContract, private readonly _findInGuide: (model: VCRServer.GuideItemContract) => void) {
            // Zeitraum der Sendung.
            this.start = new Date(model.start);
            this.end = new Date(this.start.getTime() + 1000 * model.duration);
        }

        // Suche nach ähnlichen Einträgen in der Programmzeitschrift.
        readonly findSimiliar = new JMSLib.App.Command(() => this._findInGuide(this.model), "Mögliche Wiederholungen");

        // Startzeit der Sendung.
        readonly start: Date;

        get startDisplay(): string {
            return JMSLib.App.DateTimeUtils.formatStartTime(this.start);
        }

        // Endzeit der Sendung.
        readonly end: Date;

        get endDisplay(): string {
            return JMSLib.App.DateTimeUtils.formatEndTime(this.end);
        }

        // Meldet die Quelle.
        get source(): string {
            return this.model.station;
        }

        // Meldet den Namen der Sendung.
        get name(): string {
            return this.model.name;
        }

        // Meldet die Sprache der Sendung.
        get language(): string {
            return this.model.language;
        }

        // Meldet die Kurzbeschreibung der Sendung.
        get shortDescription(): string {
            return this.model.shortDescription;
        }

        // Meldet die ausführliche Beschreibung der Sendung.
        get longDescription(): string {
            return this.model.description;
        }

        // Meldet die Dauer der Sendung.
        get duration(): string {
            return JMSLib.App.DateTimeUtils.formatDuration(new Date(1000 * this.model.duration));
        }

        // Meldet die Freigabe der Sendung.
        get rating(): string {
            return (this.model.ratings || []).join(" ");
        }

        // Meldet die Art der Sendung.
        get content(): string {
            return (this.model.categories || []).join(" ");
        }
    }

    // Repräsentiert einen Eintrag in der Programmzeitschrift.
    export class GuideEntry extends GuideInfo implements IGuideEntry {

        // Erstellt eine neue Beschreibung.
        constructor(model: VCRServer.GuideItemContract, findInGuide: (model: VCRServer.GuideItemContract) => void, private _toggleDetails: (entry: GuideEntry) => void, createNew: (entry: GuideEntry) => void, public jobSelector: JMSLib.App.IValueFromList<string>) {
            super(model, findInGuide);

            // Befehl zum Neuanlegen einer Aufzeichnung einrichten.
            this.createNew = new JMSLib.App.Command(() => createNew(this), "Aufzeichnung anlegen", () => this.end > new Date());
        }

        // Befehl zum Anlegen einer neuen Aufzeichnung.
        readonly createNew: JMSLib.App.ICommand;

        // Gesetzt, wenn die Detailansicht eingeblendet werden soll.
        showDetails = false;

        // Die eindeutige Kennung des Eintrags.
        get id(): string {
            return this.model.id;
        }

        // Schaltet die Anzeige der Detailansicht um.
        toggleDetail(): void {
            this._toggleDetails(this);
        }
    }
}