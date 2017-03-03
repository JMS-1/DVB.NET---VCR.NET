namespace VCRNETClient.App.Log {

    // Schnittstelle zur Anzeige eines einzelnen Protokolleintrags.
    export interface ILogEntry {
        // Start der Aufzeichnung.
        readonly start: string;

        // Ende der Aufzeichnung.
        readonly end: string;

        // Ende der Aufzeichnung (nur Uhrzeit).
        readonly endTime: string;

        // Verwendete Quelle.
        readonly source: string;

        // Umfang der Aufzeichnung (etwa Größe der Aufzeichnungsdateien oder Anzahl der Einträge der Programmzeitschrift).
        readonly size: string;

        // Name der (eventuell transienten) primäre Aufzeichnungsdatei.
        readonly primary: string;

        // Gesetzt wenn Aufzeichnungsdateien existieren.
        readonly hasFiles: boolean;

        // Verweise zur Anzeige der Aufzeichnungsdateien.
        readonly files: string[];

        // Umschaltung für die Detailansicht.
        readonly showDetail: JMSLib.App.IFlag;
    }

    export class LogEntry implements ILogEntry {

        // Start der Aufzeichnung.
        get start(): string {
            return JMSLib.App.DateTimeUtils.formatStartTime(new Date(this._model.start));
        }

        // Ende der Aufzeichnung.
        get end(): string {
            return JMSLib.App.DateTimeUtils.formatStartTime(new Date(this._model.end));
        }

        // Ende der Aufzeichnung (nur Uhrzeit).
        get endTime(): string {
            return JMSLib.App.DateTimeUtils.formatEndTime(new Date(this._model.end));
        }

        // Umfang der Aufzeichnung (etwa Größe der Aufzeichnungsdateien oder Anzahl der Einträge der Programmzeitschrift).
        get size(): string {
            return this._model.size;
        }

        // Name der (eventuell transienten) primäre Aufzeichnungsdatei.
        get primary(): string {
            return this._model.primaryFile;
        }

        // Gesetzt wenn Aufzeichnungsdateien existieren.
        get hasFiles(): boolean {
            return this._model.files && (this._model.files.length > 0);
        }

        // Verweise zur Anzeige der Aufzeichnungsdateien.
        get files(): string[] {
            return (this._model.files || []).map(VCRServer.getFilePlayUrl);
        }

        // Verwendete Quelle.
        readonly source: string;

        // Gesetzt, wenn es sich um die Aktualisierung der Programmzeitschrift handelt.
        readonly isGuide: boolean = false;

        // Gesetzt, wenn es sich um einen Sendersuchlauf handelt.
        readonly isScan: boolean = false;

        // Gesetzt, wenn es sich um die LIVE Verwendung handelt.
        readonly isLive: boolean = false;

        // Umschaltung für die Detailansicht.
        readonly showDetail: JMSLib.App.Flag;

        // Erstellt ein neues Präsentationsmodell.
        constructor(private readonly _model: VCRServer.ProtocolEntryContract, toggleDetail: (entry: LogEntry) => void) {
            this.showDetail = new JMSLib.App.Flag({ value: false }, `value`, null, () => toggleDetail(this));

            // Art der Aufzeichnung zum Filtern umsetzen.
            switch (_model.firstSourceName) {
                case "EPG":
                    this.source = "Programmzeitschrift";
                    this.isGuide = true;
                    break;
                case "PSI":
                    this.source = "Sendersuchlauf";
                    this.isScan = true;
                    break;
                case "LIVE":
                    this.source = "Zapping";
                    this.isLive = true;
                    break;
                default:
                    this.source = _model.firstSourceName;
                    break;
            }
        }

    }
}