namespace VCRNETClient.App.Devices {

    export class Info implements IDeviceInfo {

        site: JMSLib.App.ISite;

        constructor(private readonly _model: VCRServer.PlanCurrentContract, suppressHibernate: boolean, _refresh: (info: Info, guide: boolean) => void, reload: () => void, private readonly _findInGuide: (model: VCRServer.GuideItemContract) => void) {
            if (!_model.isIdle) {
                this._start = new Date(_model.start);
                this._end = new Date(this._start.getTime() + _model.duration * 1000);

                this.displayStart = JMSLib.DateFormatter.getStartTime(this._start);
                this.displayEnd = JMSLib.DateFormatter.getEndTime(this._end);

                this.controller = new Controller(_model, suppressHibernate, reload);
            }

            this.showGuide = new JMSLib.App.Flag({}, "value", null, () => _refresh(this, true), () => !this._model.epg || !this._model.device || !this._model.source);
            this.showControl = new JMSLib.App.Flag({}, "value", null, () => _refresh(this, false), () => this.mode !== `running`);
        }

        readonly showGuide: JMSLib.App.IFlag;

        readonly showControl: JMSLib.App.IFlag;

        get mode(): string {
            if (this._model.isIdle)
                return undefined;

            if (this._model.duration <= 0)
                return 'null';
            if (this._model.referenceId)
                return 'running';

            if (this._model.sourceName === 'PSI')
                return undefined;
            if (this._model.sourceName === 'EPG')
                return undefined;

            if (this._model.late)
                return 'late';
            else
                return 'intime';
        }

        readonly controller: Controller;

        get name(): string {
            return this._model.name;
        }

        private readonly _start: Date;

        private readonly _end: Date;

        readonly displayStart: string;

        readonly displayEnd: string;

        get source(): string {
            return this._model.sourceName;
        }

        get device(): string {
            return this._model.device;
        }

        get size(): string {
            return this._model.size;
        }

        get id(): string {
            return this._model.id;
        }

        private _guideItem: Guide.GuideInfo;

        private _guideTime: JMSLib.App.TimeBar;

        get guideTime(): JMSLib.App.ITimeBar {
            return this._guideTime;
        }

        get guideItem(): Guide.IGuideInfo {
            if (this.showGuide.isReadonly)
                return null;

            if (this._guideItem !== undefined)
                return this._guideItem;

            VCRServer.getGuideItem(this._model.device, this._model.source, this._start, this._end).then(item => {
                this._guideItem = item ? new Guide.GuideInfo(item, this._findInGuide) : null;

                if (this._guideItem)
                    this._guideTime = new JMSLib.App.TimeBar(this._start, this._end, this._guideItem.start, this._guideItem.end);

                this.refreshUi();
            });

            return null;
        }

        private refreshUi(): void {
            if (this.site)
                this.site.refreshUi();
        }
    }

}