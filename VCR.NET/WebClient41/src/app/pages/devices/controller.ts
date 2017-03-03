namespace VCRNETClient.App.Devices {

    // Schnittstelle zur Anzeige und Manipulation einer Aktivität.
    export interface IDeviceController extends JMSLib.App.IConnectable {
        // Aktueller Endzeitpunkt.
        readonly end: string;

        // Einstellung für die verbleibende Restzeit.
        readonly remaining: JMSLib.App.INumberWithSlider;

        // Verweis zur LIVE Betrachtung.
        readonly live: string;

        // Verweis zur Zeitversetzen Betrachtung.
        readonly timeshift: string;

        // Aktueller Ziel für den Netzwerkversand.
        readonly target: string;

        // Befehl um das sofortige Beenden vorzubereiten.
        readonly stopNow: JMSLib.App.ICommand;

        // Einstellung zum Umgang mit dem Schlafzustand beim Vorzeitigen beenden.
        readonly noHibernate: JMSLib.App.IFlag;

        // Befehl zur Aktualisierung der Endzeit.
        readonly update: JMSLib.App.ICommand;
    }

    // Präsentationsmodell zur Ansicht und Pflege einer laufenden Aufzeichnung.
    export class Controller implements IDeviceController {

        // Aktuell verbundenes Oberflächenelement.
        view: JMSLib.App.IView;

        // Einstellung für die verbleibende Restzeit.
        readonly remaining = new JMSLib.App.NumberWithSlider({}, "value", () => this.refreshUi(), 0, 480);

        // Befehl um das sofortige Beenden vorzubereiten.
        readonly stopNow = new JMSLib.App.Command(() => this.remaining.sync(0), "Vorzeitig beenden", () => this.remaining.value !== 0);

        // Einstellung zum Umgang mit dem Schlafzustand beim Vorzeitigen beenden.
        readonly noHibernate = new JMSLib.App.Flag({}, "value", "Übergang in den Schlafzustand unterdrücken");

        // Befehl zur Aktualisierung der Endzeit.
        readonly update = new JMSLib.App.Command(() => this.save(), "Übernehmen");

        // Verweis zur LIVE Betrachtung.
        readonly live: string;

        // Verweis zur Zeitversetzen Betrachtung.
        readonly timeshift: string;

        constructor(private readonly _model: VCRServer.PlanCurrentContract, suppressHibernate: boolean, private readonly _reload: () => void) {
            // Präsentationsmodelle aufsetzen.
            this.remaining.value = _model.remainingMinutes;
            this.noHibernate.value = suppressHibernate;

            // Nur wenn es sich um eine Aufzeichnung handelt müssen wir mehr tun - Sonderaufgaben kann man nicht ansehen!
            if (_model.streamIndex < 0)
                return;

            // Verweise zur Ansicht mit dem DVB.NET / VCR.NET Viewer aufsetzen.
            var url = `${VCRServer.getDeviceRoot()}${encodeURIComponent(_model.device)}/${_model.streamIndex}/`;

            this.live = `${url}Live`;
            this.timeshift = `${url}TimeShift`;
        }

        // Start der Aufzeichnung.
        private get start(): Date {
            return new Date(this._model.start);
        }

        // Das aktuelle Ende der Aufzeichnung.
        private get currentEnd(): Date {
            return new Date(this.start.getTime() + 1000 * this._model.duration + 60000 * (this.remaining.value - this._model.remainingMinutes));
        }

        // Aktueller Endzeitpunkt formatiert als Uhrzeit.
        get end(): string {
            return JMSLib.App.DateTimeUtils.formatEndTime(this.currentEnd);
        }

        // Aktueller Ziel für den Netzwerkversand.
        get target(): string {
            return this._model.streamTarget;
        }

        // Fordert die Oberfläche zur Aktualisierung auf.
        private refreshUi(): void {
            if (this.view)
                this.view.refreshUi();
        }

        // Aktualisiert den Endzeitpunkt.
        private save(): JMSLib.App.IHttpPromise<void> {
            // Beim vorzeitigen Beenden sind wir etwas übervorsichtig.
            var end = (this.remaining.value > 0) ? this.currentEnd : this.start;

            // Asynchronen Aufruf absetzen und im Erfolgsfall die Aktivitäten neu laden.
            return VCRServer.updateEndTime(this._model.device, this.noHibernate.value, this._model.referenceId, end).then(() => this._reload());
        }

    }

}