namespace JMSLib.App {
    export interface ITimeBar {
        readonly prePercent: number;

        readonly recPercent: number;

        readonly recClass: string;

        readonly postPercent: number;

        readonly nowPercent: number;
    }

    export class TimeBar implements ITimeBar {
        private static readonly  _goodClass = `jmslib-timebar-good`;

        private static readonly  _badClass = `jmslib-timebar-bad`;

        constructor(startRecording: Date, endRecording: Date, startProgram: Date, endProgram: Date) {
            if (startRecording > startProgram)
                this.recClass = TimeBar._badClass;
            else if (endRecording < endProgram)
                this.recClass = TimeBar._badClass;
            else
                this.recClass = TimeBar._goodClass;

            var total = endRecording.getTime() - startRecording.getTime();

            if (startRecording >= startProgram)
                this.prePercent = 0;
            else
                this.prePercent = Math.floor((startProgram.getTime() - startRecording.getTime()) * 100 / total);

            if (endRecording <= endProgram)
                this.postPercent = 0;
            else
                this.postPercent = Math.floor((endRecording.getTime() - endProgram.getTime()) * 100 / total);

            this.recPercent = 100 - (this.prePercent + this.postPercent);

            var now = new Date();

            if (now >= startRecording)
                if (now <= endRecording)
                    this.nowPercent = Math.floor((now.getTime() - startRecording.getTime()) * 100 / total);
        }

        readonly prePercent: number;

        readonly recPercent: number;

        readonly recClass: string;

        readonly postPercent: number;

        readonly nowPercent: number;
    }
}