namespace VCRNETClient.App.Devices {

    // Ansicht einer Aktivität.
    export interface IDeviceInfo extends JMSLib.App.IConnectable {
        // Name - im Allgemeinen der Aufzeichnung.
        readonly name: string;

        // Beginn als Datum mit Uhrzeit.
        readonly displayStart: string;

        // Ende als Uhrzeit.
        readonly displayEnd: string;

        // Zugehörige Quelle, sofern vorhanden - wie bei Aufzeichnungen: Sonderaufgaben haben im Allgemeinen keine dedizierte Quelle.
        readonly source: string;

        // Verwendetes Gerät.
        readonly device: string;

        // Informationen zum Umfang der Aktivität - die Dateigröße bei Aufzeichnung, die Anzahl der Quellen beim Sendersuchlauf usw.
        readonly size: string;

        // Status der Aktivität - unterscheidet etwa zwischen laufenden und geplanten Aufzeichnungen.
        readonly mode: string;

        // Optional die eindeutige Kennung der Aufzeichnung.
        readonly id?: string;

        // Gesetzt um den zugehörigen Eintrag der Programmzeitschrift zu sehen.
        readonly showGuide: JMSLib.App.IFlag;

        // Gesetzt um eine laufende Aufzeichnung anzusehen und zu manipulieren.
        readonly showControl: JMSLib.App.IFlag;

        // Zugehörige Information aus der Programmzeitschrift - sofern vorhanden.
        readonly guideItem: Guide.IGuideInfo;

        // Zugehörige Zeitinformationen zur Information aus der Programmzeitschrift.
        readonly guideTime: JMSLib.App.ITimeBar;

        // Optional die Steuereinheit für laufende Aufzeichnung.
        readonly controller: IDeviceController;
    }

    // Präsentationsmodell zur Anzeige einer Aktivität.
    export class Info implements IDeviceInfo {

        // Das aktuell angebundene Oberflächenelement.
        site: JMSLib.App.ISite;

        // Erstellt ein neues Präsentationsmodell.
        constructor(private readonly _model: VCRServer.PlanCurrentContract, suppressHibernate: boolean, toggleDetails: (info: Info, guide: boolean) => void, reload: () => void, private readonly _findInGuide: (model: VCRServer.GuideItemContract) => void) {
            // Für Geräte ohne laufende oder geplante Aufzeichnung können wir nicht viel tun.
            if (!_model.isIdle) {
                // Ansonsten müssen wir die Zeiten aus der ISO Notation umrechnen.
                this._start = new Date(_model.start);
                this._end = new Date(this._start.getTime() + _model.duration * 1000);

                // Und zur Anzeige aufbereiten.
                this.displayStart = JMSLib.App.DateTimeUtils.formatStartTime(this._start);
                this.displayEnd = JMSLib.App.DateTimeUtils.formatEndTime(this._end);

                // Das Präsentationsmodell für die Steuerung bei Bedarf erstellen.
                if (this.mode === `running`)
                    this.controller = new Controller(_model, suppressHibernate, reload);
            }

            // Präsentationsmodell für die Detailansicht erstellen.
            this.showGuide = new JMSLib.App.Flag({}, "value", null, () => toggleDetails(this, true), () => !this._model.epg || !this._model.device || !this._model.source || (this.mode === `null`));
            this.showControl = new JMSLib.App.Flag({}, "value", null, () => toggleDetails(this, false), () => !this.controller);
        }

        // Gesetzt um den zugehörigen Eintrag der Programmzeitschrift zu sehen.
        readonly showGuide: JMSLib.App.IFlag;

        // Gesetzt um eine laufende Aufzeichnung anzusehen und zu manipulieren.
        readonly showControl: JMSLib.App.IFlag;

        // Status der Aktivität - unterscheidet etwa zwischen laufenden und geplanten Aufzeichnungen.
        get mode(): string {
            // Gerät wird nicht verwendet.
            if (this._model.isIdle)
                return undefined;

            // Aktivität wurde bereits beendet.
            if (this._end <= new Date())
                return 'null';

            // Aufzeichnung läuft.
            if (this._model.referenceId)
                return 'running';

            // Sonderaufgaben sollten eigentlich nur als laufend in der Liste erscheinen.
            if (this._model.sourceName === 'PSI')
                return undefined;
            if (this._model.sourceName === 'EPG')
                return undefined;

            // Geplanten Zustand melden.
            return this._model.late ? 'late' : 'intime';
        }

        // Optional die Steuereinheit für laufende Aufzeichnung.
        readonly controller: Controller;

        // Name - im Allgemeinen der Aufzeichnung.
        get name(): string {
            return this._model.name;
        }

        // Beginn der Aktivität.
        private readonly _start: Date;

        // Voraussichtliches Ende der Aktivität.
        private readonly _end: Date;

        // Beginn als Datum mit Uhrzeit.
        readonly displayStart: string;

        // Ende als Uhrzeit.
        readonly displayEnd: string;

        // Zugehörige Quelle, sofern vorhanden - wie bei Aufzeichnungen: Sonderaufgaben haben im Allgemeinen keine dedizierte Quelle.
        get source(): string {
            return this._model.sourceName;
        }

        // Verwendetes Gerät.
        get device(): string {
            return this._model.device;
        }

        // Informationen zum Umfang der Aktivität - die Dateigröße bei Aufzeichnung, die Anzahl der Quellen beim Sendersuchlauf usw.
        get size(): string {
            return this._model.size;
        }

        // Optional die eindeutige Kennung der Aufzeichnung.
        get id(): string {
            return this._model.id;
        }

        // Zugehörige Zeitinformationen zur Information aus der Programmzeitschrift.
        private _guideTime: JMSLib.App.TimeBar;

        get guideTime(): JMSLib.App.ITimeBar {
            return this._guideTime;
        }

        // Zugehörige Information aus der Programmzeitschrift - sofern vorhanden.
        private _guideItem: Guide.GuideInfo;

        get guideItem(): Guide.IGuideInfo {
            // Wird nicht unterstützt.
            if (this.showGuide.isReadonly)
                return null;

            // Es wurde bereits ein Ladeversuch unternommen und kein Eintrag gefunden.
            if (this._guideItem !== undefined)
                return this._guideItem;

            // Programmzeitschrift nach einem passenden Eintrag absuchen.
            VCRServer.getGuideItem(this._model.device, this._model.source, this._start, this._end).then(item => {
                // Ergebnis übernehmen.
                this._guideItem = item ? new Guide.GuideInfo(item, this._findInGuide) : null;

                // Im Erfolgsfall auch die Zeitschiene aufsetzen.
                if (this._guideItem)
                    this._guideTime = new JMSLib.App.TimeBar(this._start, this._end, this._guideItem.start, this._guideItem.end);

                // Anzeige zur Aktualisierung auffordern.
                this.refreshUi();
            });

            // Erst einmal abwarten.
            return null;
        }

        // Die Anzeige zur Aktualisierung auffordern.
        private refreshUi(): void {
            if (this.site)
                this.site.refreshUi();
        }

    }

}