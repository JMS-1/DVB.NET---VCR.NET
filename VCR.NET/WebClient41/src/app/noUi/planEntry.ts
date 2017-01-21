namespace VCRNETClient.App.NoUi {

    // Erweiterte Schnittstelle (View Model) zur Anzeige eines Eintrags des Aufzeichnunsplans.
    export interface IPlanEntry {
        // Ein Kürzel für die Qualität der Aufzeichnung, etwa ob dieser verspätet beginnt.
        readonly mode: string;

        // Anwendungsverweis zum Ändern dieses Eintrags.
        readonly editLink: string;

        // Die zugehörige Ausnahmeregel.
        readonly exception: IPlanException;

        // Das verwendete Gerät.
        readonly device: string;

        // Der zugehörige Sender.
        readonly station: string;

        // Der Startzeitpunkt formatiert für die Darstellung.
        readonly displayStart: string;

        // Der Endzeitpunkt, formatiert für die Darstellung - es werden nur Stunden und Minuten angezeigt.
        readonly displayEnd: string;

        // Die Dauer der Aufzeichnung.
        readonly duration: number;

        // Der Name der Aufzeichnung.
        readonly name: string;

        // Zeigt die Programmzeitschrift an.
        readonly showEpg: boolean;

        // Zeigt die Pflege der Ausnahmeregel an.
        readonly showException: boolean;

        // Schaltet die Detailanzeige um.
        toggleDetail(epg: boolean): void;
    }

    export class PlanEntry implements IPlanEntry {
        constructor(private model: VCRServer.PlanActivityContract, private _toggleDetail: (entry: PlanEntry, epg: boolean) => void, application: App.Application, reload: () => void) {
            // Zeiten umrechnen
            this.duration = parseInt(model.duration);
            this.start = new Date(model.start);
            this.end = new Date(this.start.getTime() + 1000 * this.duration);

            // Ausnahmen auswerten
            if (model.exception)
                this.exception = new PlanException(model.exception, model.id, reload);
        }

        // Zeigt die Programmzeitschrift an.
        private _showEpg = false;

        get showEpg(): boolean {
            return this._showEpg;
        }

        set showEpg(newValue: boolean) {
            this._showEpg = newValue;
        }

        // Zeigt die Pflege der Ausnahmeregel an.
        private _showException = false;

        get showException(): boolean {
            return this._showException;
        }

        set showException(newValue: boolean) {
            if (this.exception)
                this.exception.reset();

            this._showException = newValue;
        }

        // Die Dauer der Aufzeichnung.
        readonly duration: number;

        // Der Zeitpunkt, an dem die Aufzeichnung beginnen wird.
        readonly start: Date;

        // Der Zeitpunkt, an dem die Aufzeichnung enden wird.
        readonly end: Date;

        // Der Name der Aufzeichnung.
        get name(): string {
            return this.model.name;
        }

        // Der Startzeitpunkt formatiert für die Darstellung.
        get displayStart(): string {
            return DateFormatter.getStartTime(this.start);
        }

        // Der Endzeitpunkt, formatiert für die Darstellung - es werden nur Stunden und Minuten angezeigt.
        get displayEnd(): string {
            return DateFormatter.getEndTime(this.end);
        }

        // Die zugehörige Ausnahmeregel.
        readonly exception: PlanException;

        // Das verwendete Gerät.
        get device(): string {
            return this.model.device || '';
        }

        // Der zugehörige Sender.
        get station(): string {
            return this.model.station || '(unbekannt)';
        }

        // Ein Kürzel für die Qualität der Aufzeichnung, etwa ob dieser verspätet beginnt.
        get mode(): string {
            if (this.model.station === 'PSI')
                return undefined;
            if (this.model.station === 'EPG')
                return undefined;

            if (this.model.lost)
                return 'lost';
            else if (this.model.late)
                return 'late';
            else
                return 'intime';
        }

        // Anwendungsverweis zum Ändern dieses Eintrags.
        get editLink(): string {
            return this.mode && `edit;id=${this.model.id}`;
        }

        // Schaltet die Detailanzeige um.
        toggleDetail(epg: boolean): void {
            return this._toggleDetail(this, epg);
        }
    }
}