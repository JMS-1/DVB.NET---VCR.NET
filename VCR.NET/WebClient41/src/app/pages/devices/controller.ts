namespace VCRNETClient.App.Devices {

    export class Controller implements IDeviceController {

        site: JMSLib.App.ISite;

        readonly remaining = new JMSLib.App.NumberWithSlider({}, "value", () => this.refreshUi(), 0, 480);

        constructor(private readonly _end: Date, private readonly _originalRemaining: number) {
            this.remaining.value = _originalRemaining;
        }

        get end(): string {
            return JMSLib.DateFormatter.getEndTime(new Date(this._end.getTime() + (this.remaining.value - this._originalRemaining) * 60000));
        }

        private refreshUi(): void {
            if (this.site)
                this.site.refreshUi();
        }
    }

}