/// <reference path="../../../lib/edit/edit.ts" />

namespace VCRNETClient.App.Edit {

    export interface IDurationEditor extends JMSLib.App.IDisplay {
        readonly startTime: JMSLib.App.IEditTime;

        readonly endTime: JMSLib.App.IEditTime;
    }

    export class DurationEditor extends JMSLib.App.Property<number> implements IDurationEditor {
        readonly startTime: JMSLib.App.EditTime;

        readonly endTime: JMSLib.App.EditTime;

        constructor(data: any, propTime: string, propDuration: string, text: string, onChange: () => void) {
            super(data, propDuration, text, onChange);

            this.startTime = new JMSLib.App.EditTime(data, propTime, null, () => this.onChanged());

            var end = new Date(new Date(this.startTime.value).getTime() + 60000 * this.value);

            this.endTime = new JMSLib.App.EditTime({ time: end.toISOString() }, "time", null, () => this.onChanged(), this.checkLimit.bind(this));
        }

        private onChanged(): void {
            var start = JMSLib.App.DateFormatter.parseTime(this.startTime.time);

            if (start === null)
                return;

            var end = JMSLib.App.DateFormatter.parseTime(this.endTime.time);

            if (end === null)
                return;

            var duration = (end - start) / 60000;

            if (duration <= 0)
                duration += 24 * 60;

            this.value = duration;
        }

        private checkLimit(): string {
            return (this.value >= 24 * 60) ? "Die Aufzeichnungsdauer muss kleiner als ein Tag sein." : undefined;
        }

        validate(): void {
            super.validate();

            this.startTime.validate();
            this.endTime.validate();

            if (this.message.length < 1)
                if (this.startTime.message.length > 0)
                    this.message = this.startTime.message;
                else
                    this.message = this.endTime.message;
        }
    }
}