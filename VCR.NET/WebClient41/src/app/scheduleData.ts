namespace VCRNETClient.App {

    // Beschreibt die Daten einer Aufzeichnung
    export class ScheduleData {
        constructor(existingData: VCRServer.JobScheduleInfoContract) {
            // Schauen wir mal, ob wir geladen wurden
            if (existingData != null) {
                // Aufzeichnungsdaten prüfen
                var rawData = existingData.schedule;
                if (rawData != null) {
                    var repeat = rawData.repeatPattern;
                    var start = new Date(rawData.firstStart);
                    var end = new Date(start.getTime() + 60000 * rawData.duration);

                    // Übernehmen
                    this.exceptionInfos = $.map(rawData.exceptions, (rawException: VCRServer.PlanExceptionContract) => new PlanException(rawException));
                    this.lastDay = (repeat == 0) ? ScheduleData.maximumDate : new Date(rawData.lastDay);
                    this.firstStart = new Date(start.getFullYear(), start.getMonth(), start.getDate());
                    this.repeatWednesday = (repeat & ScheduleData.flagWednesday) != 0;
                    this.repeatThursday = (repeat & ScheduleData.flagThursday) != 0;
                    this.repeatSaturday = (repeat & ScheduleData.flagSaturday) != 0;
                    this.repeatTuesday = (repeat & ScheduleData.flagTuesday) != 0;
                    this.repeatMonday = (repeat & ScheduleData.flagMonday) != 0;
                    this.repeatFriday = (repeat & ScheduleData.flagFriday) != 0;
                    this.repeatSunday = (repeat & ScheduleData.flagSunday) != 0;
                    this.startTime = DateFormatter.getEndTime(start);
                    this.endTime = DateFormatter.getEndTime(end);
                    this.withSubtitles = rawData.withSubtitles;
                    this.withVideotext = rawData.withVideotext;
                    this.allLanguages = rawData.allLanguages;
                    this.includeDolby = rawData.includeDolby;
                    this.sourceName = rawData.sourceName,
                        this.id = existingData.scheduleId;
                    this.name = rawData.name;

                    // Fertig
                    return;
                }
            }

            var now = Date.now();
            var nowDate = new Date(now);

            // Eine ganz neue Aufzeichnung.
            this.firstStart = new Date(nowDate.getFullYear(), nowDate.getMonth(), nowDate.getDate());
            this.withSubtitles = VCRServer.UserProfile.global.defaultDVBSubtitles;
            this.allLanguages = VCRServer.UserProfile.global.defaultAllLanguages;
            this.withVideotext = VCRServer.UserProfile.global.defaultVideotext;
            this.includeDolby = VCRServer.UserProfile.global.defaultDolby;
            this.endTime = DateFormatter.getEndTime(new Date(now + 7200000));
            this.startTime = DateFormatter.getEndTime(nowDate);
            this.lastDay = ScheduleData.maximumDate;
            this.repeatWednesday = false;
            this.repeatThursday = false;
            this.repeatSaturday = false;
            this.repeatTuesday = false;
            this.repeatMonday = false;
            this.repeatFriday = false;
            this.repeatSunday = false;
            this.sourceName = '';
            this.name = '';
        }

        // Der kleinste erlaubte Datumswert.
        static minimumDate: Date = new Date(1963, 8, 29);

        // Der höchste erlaubte Datumswert.
        static maximumDate: Date = new Date(2999, 11, 31);

        // Das Bit für Montag.
        static flagMonday: number = 0x01;

        // Das Bit für Dienstag.
        static flagTuesday: number = 0x02;

        // Das Bit für Mittwoch.
        static flagWednesday: number = 0x04;

        // Das Bit für Donnerstag.
        static flagThursday: number = 0x08;

        // Das Bit für Freitag.
        static flagFriday: number = 0x10;

        // Das Bit für Samstag.
        static flagSaturday: number = 0x20;

        // Das Bit für Sonntag.
        static flagSunday: number = 0x40;

        // Die eindeutige Kennung der Aufzeichnung.
        id: string;

        // Der Name der Aufzeichnung.
        name: string;

        // Gesetzt, wenn alle Sprachen aufgezeichnet werden sollen.
        allLanguages: boolean;

        // Gesetzt, wenn auch die AC3 Tonspur aufgezeichnet werden soll.
        includeDolby: boolean;

        // Gesetzt, wenn auch der Videotext aufgezeichnet werden soll.
        withVideotext: boolean;

        // Gesetzt, wenn auch DVB Untertitel aufgezeichnet werden sollen.
        withSubtitles: boolean;

        // Der Name der Quelle, die aufgezeichnet werden soll.
        sourceName: string;

        // Das Startdatum der Aufzeichnung.
        firstStart: Date;

        // Die Startzeit der Aufzeichnung.
        startTime: string;

        // Die Endzeit der Aufzeichnung.
        endTime: string;

        // Der Zeitpunkt der letzten Aufzeichnung.
        lastDay: Date;

        // Gesetzt, wenn jeden Montag eine Aufzeichnung gewünscht wird.
        repeatMonday: boolean;

        // Gesetzt, wenn jeden Montag eine Aufzeichnung gewünscht wird.
        repeatTuesday: boolean;

        // Gesetzt, wenn jeden Montag eine Aufzeichnung gewünscht wird.
        repeatWednesday: boolean;

        // Gesetzt, wenn jeden Montag eine Aufzeichnung gewünscht wird.
        repeatThursday: boolean;

        // Gesetzt, wenn jeden Montag eine Aufzeichnung gewünscht wird.
        repeatFriday: boolean;

        // Gesetzt, wenn jeden Montag eine Aufzeichnung gewünscht wird.
        repeatSaturday: boolean;

        // Gesetzt, wenn jeden Montag eine Aufzeichnung gewünscht wird.
        repeatSunday: boolean;

        // Die zugehörige Ausnahmeregel.
        exceptionInfos: PlanException[] = [];

        // Meldet das Wiederholungsmuster.
        repeatPattern(): number {
            var pattern =
                (this.repeatMonday ? ScheduleData.flagMonday : 0) |
                (this.repeatTuesday ? ScheduleData.flagTuesday : 0) |
                (this.repeatWednesday ? ScheduleData.flagWednesday : 0) |
                (this.repeatThursday ? ScheduleData.flagThursday : 0) |
                (this.repeatFriday ? ScheduleData.flagFriday : 0) |
                (this.repeatSaturday ? ScheduleData.flagSaturday : 0) |
                (this.repeatSunday ? ScheduleData.flagSunday : 0);

            return pattern;
        }

        // Erstellt eine für die Datenübertragung geeignete Variante.
        toWebService(): VCRServer.EditScheduleContract {
            // Ein bißchen herumrechnen, um die Zeiten zu bekommen
            var startTime = DateFormatter.parseTime(this.startTime);
            var endTime = DateFormatter.parseTime(this.endTime);

            // Wir müssen sicherstellen, dass uns die Umstellung zwischen Sommer- und Winterzeit keinen Streich spielt
            var firstYear = this.firstStart.getFullYear();
            var firstMonth = this.firstStart.getMonth();
            var firstDay = this.firstStart.getDate();
            var fullStart = new Date(firstYear, firstMonth, firstDay, Math.floor(startTime / 3600000), (startTime / 60000) % 60);
            var fullEnd = new Date(firstYear, firstMonth, firstDay, Math.floor(endTime / 3600000), (endTime / 60000) % 60);

            if (startTime >= endTime)
                fullEnd.setDate(firstDay + 1);

            var duration = fullEnd.getTime() - fullStart.getTime();

            var localEnd = this.lastDay;
            if (localEnd == null)
                localEnd = new Date(2999, 11, 31);
            var utcEnd = new Date(Date.UTC(localEnd.getFullYear(), localEnd.getMonth(), localEnd.getDate()));

            // Nun noch die verbleibenden Ausnahmen einrichten
            var exceptions = new Array();

            $.each(this.exceptionInfos, (index: number, info: PlanException) => {
                if (info.isActive)
                    exceptions.push(info.rawException);
            });

            // Fertig
            var contract: VCRServer.EditScheduleContract = {
                firstStart: fullStart.toISOString(),
                repeatPattern: this.repeatPattern(),
                withVideotext: this.withVideotext,
                withSubtitles: this.withSubtitles,
                allLanguages: this.allLanguages,
                includeDolby: this.includeDolby,
                lastDay: utcEnd.toISOString(),
                sourceName: this.sourceName,
                duration: duration / 60000,
                exceptions: exceptions,
                name: this.name,
            };

            // Report
            return contract;
        }
    }

}