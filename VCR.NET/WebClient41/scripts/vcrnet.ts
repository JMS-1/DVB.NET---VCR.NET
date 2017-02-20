toWebService(): VCRServer.EditScheduleContract {
    // Ein bißchen herumrechnen, um die Zeiten zu bekommen
    var startTime = JMSLib.DateFormatter.parseTime(this.startTime);
    var endTime = JMSLib.DateFormatter.parseTime(this.endTime);

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

function validate(): void {
    // Name
    schedule.name = schedule.name.trim();
    if (schedule.name.length > 0)
        if (!JobDataValidator.isNameValid(schedule.name))
            this.name = 'Der Name der Aufzeichnung enthält ungültige Zeichen';


    // Tage
    var firstDay = schedule.firstStart;
    if (firstDay == null)
        this.firstStart = 'Es muss ein Datum für die erste Aufzeichnung angegeben werden';
    else if (firstDay < ScheduleData.minimumDate)
        this.firstStart = 'Das Datum für die erste Aufzeichnung ist ungültig';

    // Sichtbarkeit prüfen
    var endDate = this.view.find('#endDateContainer');
    if (schedule.repeatPattern() == 0) {
        endDate.addClass(JMSLib.CSSClass.invisible);
    }
    else {
        endDate.removeClass(JMSLib.CSSClass.invisible);

        var lastDay = schedule.lastDay;
        if (lastDay == null)
            this.lastDay = 'Es muss ein Datum für die letzte Aufzeichnung angegeben werden';
        else if (firstDay != null)
            if (lastDay < firstDay)
                this.lastDay = 'Das Datum der letzten Aufzeichnung darf nicht vor der ersten Aufzeichnung liegen';
    }
}

var forbidenCharacters: RegExp = /[\\\/\:\*\?\"\<\>\|]/;

static function isNameValid(name: string): boolean {
    return name.search(JobDataValidator.forbidenCharacters) < 0;
}

