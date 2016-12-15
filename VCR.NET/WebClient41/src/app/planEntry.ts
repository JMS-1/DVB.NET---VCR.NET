namespace VCRNETClient.App {

    // Beschreibt einen einzelne Ausnahmeregel
    export class PlanException {
        constructor(rawData: VCRServer.PlanExceptionContract) {
            // Daten aus den Rohdaten übernehmen
            this.referenceDayDisplay = parseInt(rawData.referenceDayDisplay, 10);
            this.originalStart = new Date(rawData.originalStart);
            this.originalDuration = rawData.originalDuration;
            this.referenceDay = rawData.referenceDay;
            this.durationDelta = rawData.timeDelta;
            this.startDelta = rawData.startShift;
            this.rawException = rawData;
        }

        // Die Ausnahme, so wie der Web Service sie uns gemeldet hat.
        rawException: any;

        // Das Referenzdatum für die Ausnahme.
        private referenceDay: string;

        // Das Referenzdatum für die Ausnahme.
        private referenceDayDisplay: number;

        // Der ursprüngliche Startzeitpunkt.
        private originalStart: Date;

        // Die geplante Dauer der Aufzeichnung.
        originalDuration: number;

        // Die Verschiebung des Startzeitpunktes in Minuten.
        startDelta: number;

        // Die Veränderung der Dauer in Minuten.
        durationDelta: number;
    }

    // Beschreibt einen einzelnen Eintrag in Aufzeichnungsplan
    export class PlanEntry {
        constructor(rawData: any, public key: number) {
            // Zeiten umrechnen
            var duration = rawData.duration * 1000;
            var start = new Date(rawData.start);
            var end = new Date(start.getTime() + duration);

            // Daten aus der Rohdarstellung in das Modell kopieren
            this.station = (rawData.station == null) ? '(unbekannt)' : rawData.station;
            this.profile = (rawData.device == null) ? '' : rawData.device;
            this.displayStart = DateFormatter.getStartTime(start);
            this.displayEnd = DateFormatter.getEndTime(end);
            this.epgProfile = rawData.epgDevice;
            this.fullName = rawData.name;
            this.source = rawData.source;
            this.legacyId = rawData.id;
            this.start = start;
            this.end = end;

            // Aufzeichnungsoptionen
            this.currentGuide = rawData.epgCurrent;
            this.allAudio = rawData.allAudio;
            this.hasGuideEntry = rawData.epg;
            this.subTitles = rawData.dvbsub;
            this.videoText = rawData.ttx;
            this.dolby = rawData.ac3;

            // Für Aufgaben konfigurieren wir keine Verweise
            if (this.station == 'PSI')
                return;
            if (this.station == 'EPG')
                return;

            // Aufzeichungsmodus ermitteln
            if (rawData.lost)
                this.mode = 'lost';
            else if (rawData.late)
                this.mode = 'late';
            else
                this.mode = 'intime';

            // Ausnahmen auswerten
            if (rawData.exception != null)
                this.exceptionInfo = new PlanException(rawData.exception);

            // Die Endzeit könnte nicht wie gewünscht sein
            this.endTimeSuspect = rawData.suspectEndTime;
        }

        // Gesetzt zur Kennzeichnung von Aufzeichnungen über die Zeitumstellung hinweg
        endTimeSuspect: boolean;

        // Die Kennung der zugehörigen Quelle.
        private source: string;

        // Kennung einer Aufzeichnung, so wie sie in der ursprünglichen ASP.NET Anwendung verwendet wird.
        private legacyId: string;

        // Der volle Name des Senders.
        station: string;

        // Das DVB.NET Gerät, das die Aufzeichnung ausführen wird.
        profile: string;

        // Das DVB.NET Gerät, über das Sendungsinformationen nachgeschlagen werden können.
        private epgProfile: string;

        // Der volle Name der Aufzeichnung.
        fullName: string;

        // Der Zeitpunkt, an dem die Aufzeichnung beginnen wird.
        start: Date;

        // Der Zeitpunkt, an dem die Aufzeichnung enden wird.
        end: Date;

        // Der Startzeitpunkt formatiert für die Darstellung.
        displayStart: string;

        // Der Endzeitpunkt, formatiert für die Darstellung - es werden nur Stunden und Minuten angezeigt.
        displayEnd: string;

        // Gesetzt wenn alle Tonspuren aufgezeichnet werden sollen.
        allAudio: boolean;

        // Gesetzt wenn Dolby Digital Tonspuren aufgezeichnet werden soll.
        dolby: boolean;

        // Gesetzt wenn der Videotext aufgezeichnet werden soll.
        videoText: boolean;

        // Gesetzt wenn DVB Untertitel aufgezeichnet werden sollen.
        subTitles: boolean;

        // Gesetzt wenn die Aufzeichnung Informationen zu den Sendungen enthält.
        currentGuide: boolean;

        // Ein Kürzel für die Qualität der Aufzeichnung, etwa ob dieser verspätet beginnt.
        mode: string;

        // Die zugehörige Ausnahmeregel.
        exceptionInfo: PlanException;

        // Ein Kürzel für die Existenz von Ausnahmen.
        exceptionMode: boolean;

        // Gesetzt wenn Informationen der Programmzeitschrift abgerufen werden können.
        hasGuideEntry: boolean;
    }

}