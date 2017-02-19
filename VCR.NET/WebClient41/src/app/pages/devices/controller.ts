namespace VCRNETClient.App.Devices {

    export class Controller implements IDeviceController {

        site: JMSLib.App.ISite;

        readonly remaining = new JMSLib.App.NumberWithSlider({}, "value", () => this.refreshUi(), 0, 480);

        readonly stopNow = new JMSLib.App.Command(() => { this.remaining.sync(0); }, "Vorzeitig beenden", () => this.remaining.value !== 0);

        readonly noHibernate = new JMSLib.App.Flag({}, "value", "Übergang in den Schlafzustand unterdrücken");

        readonly update = new JMSLib.App.Command(() => this.save(), "Übernehmen");

        readonly live: string;

        readonly timeshift: string;

        constructor(private readonly _model: VCRServer.PlanCurrentContract, suppressHibernate: boolean, private readonly _reload: () => void) {
            this.remaining.value = _model.remainingMinutes;
            this.noHibernate.value = suppressHibernate;

            if (_model.streamIndex < 0)
                return;

            var url = `${VCRServer.getDeviceRoot()}${encodeURIComponent(_model.device)}/${_model.streamIndex}/`;

            this.live = `${url}Live`;
            this.timeshift = `${url}TimeShift`;
        }

        private get start(): Date {
            return new Date(this._model.start);
        }

        private get currentEnd(): Date {
            return new Date(this.start.getTime() + 1000 * this._model.duration + 60000 * (this.remaining.value - this._model.remainingMinutes));
        }

        get end(): string {
            return JMSLib.App.DateFormatter.getEndTime(this.currentEnd);
        }

        private refreshUi(): void {
            if (this.site)
                this.site.refreshUi();
        }

        private save(): JMSLib.App.IHttpPromise<void> {
            var end = (this.remaining.value > 0) ? this.currentEnd : this.start;

            return VCRServer.updateEndTime(this._model.device, this.noHibernate.value, this._model.referenceId, end).then(() => this._reload());
        }
    }

}