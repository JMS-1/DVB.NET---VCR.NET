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
        constructor(private _entry: VCRServer.PlanActivityContract, private _toggleDetail: (entry: PlanEntry, epg: boolean) => void, application: App.Application, reload: () => void) {
            // Zeiten umrechnen
            this.duration = parseInt(_entry.duration);
            this.start = new Date(_entry.start);
            this.end = new Date(this.start.getTime() + 1000 * this.duration);

            // Ausnahmen auswerten
            if (_entry.exception)
                this.exception = new PlanException(_entry.exception, _entry.id, reload);
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
            return this._entry.name;
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
            return this._entry.device || '';
        }

        // Der zugehörige Sender.
        get station(): string {
            return this._entry.station || '(unbekannt)';
        }

        // Ein Kürzel für die Qualität der Aufzeichnung, etwa ob dieser verspätet beginnt.
        get mode(): string {
            if (this._entry.station === 'PSI')
                return undefined;
            if (this._entry.station === 'EPG')
                return undefined;

            if (this._entry.lost)
                return 'lost';
            else if (this._entry.late)
                return 'late';
            else
                return 'intime';
        }

        // Anwendungsverweis zum Ändern dieses Eintrags.
        get editLink(): string {
            return this.mode && `edit;id=${this._entry.id}`;
        }

        // Schaltet die Detailanzeige um.
        toggleDetail(epg: boolean): void {
            return this._toggleDetail(this, epg);
        }
    }
}