namespace VCRNETClient.App {

    export interface ILogEntry {
        readonly start: string;

        readonly end: string;

        readonly source: string;

        readonly showDetail: boolean;

        toggleDetail(): void;
    }

    export class LogEntry implements ILogEntry {

        get start(): string {
            return JMSLib.App.DateFormatter.getStartTime(new Date(this._model.start));
        }

        get end(): string {
            return JMSLib.App.DateFormatter.getEndTime(new Date(this._model.end));
        }

        readonly source: string;

        readonly isGuide: boolean = false;

        readonly isScan: boolean = false;

        readonly isLive: boolean = false;

        showDetail = false;

        constructor(private readonly _model: VCRServer.ProtocolEntryContract, private readonly _toggleDetail: (entry: LogEntry) => void) {
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

        toggleDetail(): void {
            this._toggleDetail(this);
        }
    }
}