/// <reference path='typings/jquery/jquery.d.ts' />
/// <reference path='typings/jqueryui/jqueryui.d.ts' />

module JMSLib {

    // Alle CSS Klassen, die vom Code aus gesetzt werden
    export class CSSClass {
        // Hebt eine ungültige Eingabe hervor
        static invalid = 'invalid';

        // Macht Oberflächenelemente unsichtbar
        static invisible = 'invisible';

        // Gebt eine kritische Eingabe hervor
        static warning = 'warning';

        // Die Klasse für die eingebettete Hilfe
        static inlineHelp = 'inlineHelp';

        // Eine Schaltfläche, die eine einzelne Stunde repräsentiert
        static hourSetting = 'hourChecker';

        // Wird beim Aufklappen einer Detailansicht gesetzt
        static detailView = 'detailView';

        // Eine Aufzeichnung umfasst eine ganze Sendung.
        static fullRecord = 'guideInsidePlan';

        // Eine Sendung wird nur teilweise aufgezeichnet.
        static partialRecord = 'guideOutsidePlan';
    }

    // Bietet die Eckdaten eines Eintrags aus der Programmzeitschrift an
    export interface IGuideItem {
        // Der Zeitpunkt, an dem die Sendung beginnt
        start: Date;

        // Die Dauer der Sendung in Sekunden
        duration: number;

        // Eine CSS Klasse, die den Überlapp einer Aufzeichnung mit dem Eintrag ausdrückt.
        overlapClass: string;
    }

    // Wertet eine Fehlermeldung von einem Web Dienst aus
    export function dispatchErrorMessage(onError: (message: string) => void): (result: JQueryXHR) => void {
        return (result: JQueryXHR) => {
            var info: any = $.parseJSON(result.responseText);

            onError(info.ExceptionMessage);
        }
    }
    
    // Bereitet die Anzeige der Hilfe vor
    export function activateHelp(): void {
        $('.' + CSSClass.inlineHelp).accordion({
            heightStyle: 'content',
            collapsible: true,
            animate: false,
            active: false
        });
    }

    // Überlappungsanzeige vorbereiten
    export function prepareGuideDisplay(entry: IGuideItem, template: JQuery, start: Date, end: Date): JQuery {
        // Zeiten ermitteln
        var epgStart = entry.start.getTime();
        var epgEnd = epgStart + entry.duration;
        var recStart = start.getTime();
        var recEnd = end.getTime();
        var fullTime = recEnd - recStart;

        // Zeigen, ob die Sendung vollständig aufgezeichnet wird
        if (recStart <= epgStart)
            if (recEnd >= epgEnd)
                entry.overlapClass = CSSClass.fullRecord;

        // Daten binden
        var html = HTMLTemplate.cloneAndApplyTemplate(entry, template);

        // Anzeige vorbereiten
        if (fullTime > 0)
            if (epgEnd > epgStart) {
                // Anzeigelement ermitteln
                var container = html.find('.guideOverlapContainer');
                var ruler = container.find('.guideOverlap');
                var current = container.find('.guideCurrentTime');

                // Grenzen korrigieren
                if (epgStart < recStart)
                    epgStart = recStart;
                else if (epgStart > recEnd)
                    epgStart = recEnd;
                if (epgEnd < recStart)
                    epgEnd = recStart;
                else if (epgEnd > recEnd)
                    epgEnd = recEnd;

                // Breiten berechnen
                var left = 90.0 * (epgStart - recStart) / fullTime;
                var middle = 90.0 * (epgEnd - epgStart) / fullTime;
                var right = Math.max(0, 90.0 - middle - left);

                // Elemente suchen
                var all = ruler.find('div');
                var preTime = $(all[0]);
                var recTime = $(all[1]);
                var postTime = $(all[2]);

                // Breiten festlegen
                recTime.width(middle + '%');

                // Bei den Rändern kann es auch sein, dass wir die ganz loswerden wollen
                if (right >= 1)
                    postTime.width(right + '%');
                else
                    postTime.remove();
                if (left >= 1)
                    preTime.width(left + '%');
                else
                    preTime.remove();

                // Schauen wir mal nach, wie spät es ist
                var now = $.now();
                if (now >= recStart)
                    if (now < recEnd) {
                        // Die relative Position der aktuellen Uhrzeit
                        var shift = 90.0 * (now - recStart) / fullTime;

                        // Und ein bisschen durch die Gegend schieben
                        current.css({ left: shift + '%' });

                        current.removeClass(CSSClass.invisible);
                    }

                // Sichtbar schalten
                container.removeClass(CSSClass.invisible);
            }

        // Fertiges Präsentationselement melden
        return html;
    }

    // Verwaltet Vorlagen
    export class TemplateLoader {
        // Die Ablage für alle Vorlagen
        private static templateRoot = 'ui/templates/';

        // Alle bereits geladenden Vorlagen
        private static loaded: any = {};

        // Lädt eine Vorlage asynchron
        static load(templateName: string): JQueryPromise<any> {
            return TemplateLoader.loadAbsolute(TemplateLoader.templateRoot + templateName + '.html');
        }

        // Lädt eine Vorlage asynchron
        static loadAbsolute(fullName: string): JQueryPromise<any> {
            var template: string = TemplateLoader.loaded[fullName];

            if (template == undefined)
                return $.get(fullName).done((html: string) => TemplateLoader.loaded[fullName] = html);

            return $.Deferred().resolve(template);
        }
    }

    // Diese Schnittstelle muss von jeder prüfbaren Klasse angeboten werden
    export interface IValidator {
        // Die eigentlichen Modelldaten, die an Oberflächenelemente gebunden werden sollen
        model: any;

        // Methode zur Durchführung einer Prüfung
        validate(): void;

        // Das Oberflächenelement zur Repräsentation des Modells als Ganzes
        view: JQuery;
    }

    // Hilfsklasse zur Bindung von Formulareigenschaften an Modelldaten
    export class Bindings {
        // Die XML Eigenschaft mit dem Namen der zugehörigen Modelleigenschaft
        static propertyAttribute = 'data-property';

        // Die XML Eigenschaft mit der Referenz auf die Anzeige der Prüfergebnisse
        static validationAttribute = 'data-validation-target';

        // Führt die Bindung einer prüfbaren Klasse an ein Oberflächenelement aus.
        static bind(validator: IValidator, form: JQuery): void {
            // Die prüfbare Klasse weiß zu jeder Zeit, mit welche Oberflächenelement das Modell als Ganzes verbunden ist
            validator.view = form;

            var model = validator.model;

            // Alle Oberflächenelement über data-property an Eigenschaften des Modells binden
            form.find('[' + Bindings.propertyAttribute + ']').each((index: any, element: Element) => {
                var targetProperty = element.getAttribute(Bindings.propertyAttribute);
                var target = $(element);

                // Mal sehen, ob wir das an ein anderes Element übergeben wollen
                var targetSelector = element.getAttribute(HTMLTemplate.targetAttribute);
                if (targetSelector != null)
                    target = target.find(targetSelector);

                // Auf Eingaben reagieren - die Übernahme in das Modell erfolgt abhängig von der Art des Oberflächenelementes
                function propertyChanged(): void {
                    if (target.is(':checkbox'))
                        model[targetProperty] = target.prop('checked');
                    else if (target.data('datepicker') != undefined)
                        model[targetProperty] = target.datepicker('getDate');
                    else
                        model[targetProperty] = target.val();

                    Bindings.validate(validator);
                }

                // Alle Änderungen am Oberflächenelement überwachen
                target.on('input', propertyChanged);
                target.on('change', propertyChanged);
            });

            // Daten einmalig aus dem Modell in die Oberfläche übertragen und dann alles prüfen
            Bindings.fromModelToForm(model, form);
            Bindings.validate(validator);
        }

        // Markiert ein Oberflächenelement als Fehleingabe und setzt die Fehlermeldung als Tooltip
        static setErrorIndicator(target: JQuery, message: string): void {
            var element = target[0];

            if (message == null) {
                element.removeAttribute('title');
                target.removeClass(CSSClass.invalid);
            }
            else {
                element.setAttribute('title', message);
                target.addClass(CSSClass.invalid);
            }
        }

        // Führt alle Prüfungen auf einer prüfbaren Klasse aus.
        static validate(validator: IValidator): void {
            validator.validate();

            Bindings.synchronizeErrors(validator);
        }

        // Aktualisiert die Darstellung der Fehlermeldungen
        static synchronizeErrors(validator: IValidator): void {
            // Über die Namenskonvention die Fehlermeldungen aus der prüfbaren Klasse auslesen - nicht die tatsächlichen Werte aus den Modelleigenschaften!
            validator.view.find('[' + Bindings.propertyAttribute + ']').each((index: any, element: Element) => {
                var targetProperty = element.getAttribute(Bindings.propertyAttribute);
                var errorMessage = validator[targetProperty];

                // Hier müssen wir Refenzgleichheit verlangen
                if (errorMessage === undefined)
                    return;

                // In seltenen Fällen wollen die Fehlermeldung an einem anderen Oberflächenelement anzeigen als das, in dem der Anwender die Eingabe tätigt
                var target = $(element);
                var errorProperty = element.getAttribute(Bindings.validationAttribute);
                if (errorProperty != null)
                    target = validator.view.find(errorProperty);

                Bindings.setErrorIndicator(target, errorMessage);
            });
        }

        // Alle Daten aus dem Modell in die Oberflächenelemente übertragen.
        private static fromModelToForm(model: any, form: JQuery): void {
            form.find('[' + Bindings.propertyAttribute + ']').each((index: any, element: Element) => {
                var targetProperty = element.getAttribute(Bindings.propertyAttribute);
                var data = model[targetProperty];
                var target = $(element);

                // Mal sehen, ob wir das an ein anderes Element übergeben wollen
                var targetSelector = element.getAttribute(HTMLTemplate.targetAttribute);
                if (targetSelector != null)
                    target = target.find(targetSelector);

                // Die Übertragung erfolgt abhängig von der Art des Oberfächenelementes - in einigen Fällen wird der Wert zurück kopiert um so ungültige Werte zu kompensieren
                if (target.is(':checkbox')) {
                    target.prop('checked', data);
                } else if (target.data('datepicker') != undefined) {
                    target.datepicker('setDate', data);

                    model[targetProperty] = target.datepicker('getDate');
                }
                else {
                    target.val(data);

                    if (element.nodeName == 'SPAN')
                        target.text(data);
                    else
                        model[targetProperty] = target.val();
                }
            });
        }

        // Muster zur Erkennung gültiger Zahlen
        private static numberPattern: RegExp = /^\d+$/;

        // Prüft, ob eine Zahl in einem bestimmten Wertebereich liegt und meldet bei Bedarf einen Fehlertext.
        public static checkNumber(input: any, min: number, max: number): string {
            if (!Bindings.numberPattern.test(input))
                return 'Keine gültige Zahl';

            var num = parseInt(input);
            if (num < min)
                return 'Der minimal erlaubte Wert ist ' + min;
            if (num > max)
                return 'Der maximal erlaubte Wert ist ' + max;

            return null;
        }
    }

    // Verwaltet ein unsichtbares HTML Element als Vorlage für eine Zeile in einer Liste
    export class HTMLTemplate {
        // Die XML Eigenschaft mit der Markierung für Pflichteigenschaften
        static requiredAttribute = 'data-property-required';

        // Die XML Eigenschaft mit der Werteformatierung
        static formatAttribute = 'data-format';

        // Der Platzhalter für die Ersetzung des Wertes in einer Formatierung
        static valuePlaceholder = '##value##';

        // Die XML Eigenschaft mit dem Selector für das Zielelement der Modelleigenschaft
        static targetAttribute = 'data-target';

        // Die XML Eigenschaft mit dem Namen der XML Zieleigenschaft für die Modelleigenschaft
        static targetAttributeAttribute = 'data-attribute';

        // Die XML Eigenschaft mit der Markierung für Werte, die in das Modell zurückfliessen sollen
        static writebackAttribute = 'data-writeback';

        // Die XML Eigenschaft mit dem Namen der Modelleigenschaft, die ein Prüfergebnis enthält
        static validationResultAttribute = 'data-validation-result';

        // Die XML Eigenschaft mit dem Namen der Modellmethode, die bei Aktivierung des Oberflächenelementes aufgerufen werden soll
        static clickAttribute = 'data-clickevent';

        // Erstellt eine neue Vorlage
        static dynamicCreate(list: JQuery, templateName: string): HTMLTemplate {
            var newTemplate = new HTMLTemplate();

            newTemplate.list = list;

            // Laden anstossen
            TemplateLoader.load(templateName).done((template: string) => {
                newTemplate.template = $(template).find('#template');
                newTemplate.refresh();
            });

            return newTemplate;
        }

        // Erstellt eine neue Vorlage
        static staticCreate(list: JQuery, template: JQuery): HTMLTemplate {
            var newTemplate = new HTMLTemplate();

            newTemplate.list = list;
            newTemplate.template = template;

            return newTemplate;
        }

        // Der Filter ist für alle Elemente gesetzt, die angezeigt werden sollen
        filter: (item: any) => boolean = (item: any) => true;

        // Die aktuellen Daten
        private items: Array<any>;

        // Die zu verwendende Vorlage
        private template: JQuery = null;

        // Die zu befüllende Liste
        private list: JQuery;

        // Baut die Darstellung gemäß der aktuellen Filterbedingung neu auf
        refresh(): void {
            // Die Daten stehen leider noch nicht zur Verfügung
            if (this.template == null)
                return;
            if (this.items == undefined)
                return;

            // Aktuelle Liste vollständig löschen
            this.list.children().remove();

            // Muster für jedes einzelne Listenelement erzeugen
            this.items.forEach((item: any, index: number) => {
                if (this.filter(item))
                    HTMLTemplate.cloneAndApplyTemplate(item, this.template).appendTo(this.list);
            });
        }

        // Ermittelt eine Eigenschaft gemäß dem Wert von data-property.
        private static retrieveProperty(data: any, propertyPath: string): any {
            // Zerlegen um auf Subobjektreferenzen zu prüfen
            var parts = propertyPath.split('.');

            // Alle Fragmente durchgehen
            for (var i = 0; i < parts.length; i++) {
                // Da ist nichts mehr
                if (data == null)
                    return null;

                // Schauen wir mal, ob ein Methodenaufruf gemacht werden soll.
                var name = parts[i];
                var coreLength = name.length - 2;
                var isCall = (coreLength > 0) && (name.substr(coreLength) == '()');

                // Wert direkt oder per Methodenaufruf laden
                if (isCall)
                    data = data[name.substr(0, coreLength)]();
                else
                    data = data[name];
            }

            // Aktuelles Ergebnis melden
            return data;
        }

        // Ersetzt alle Platzhalter in einem Oberflächenmodell und schaltet dieses dann sichtbar.
        static applyTemplate(model: any, element: JQuery): void {
            // Platzhalter zum Ersetzen durch Werte
            element.find('[' + Bindings.propertyAttribute + ']').addBack().each((index: any, element: Element) => {
                // Rohdaten auslesen
                var dataProperty = element.getAttribute(Bindings.propertyAttribute);
                if (dataProperty == null)
                    return;

                // Eventuell ganz entfernen
                var dataValue = HTMLTemplate.retrieveProperty(model, dataProperty);
                if (dataValue == null)
                    if (element.getAttribute(HTMLTemplate.requiredAttribute)) {
                        element.parentNode.removeChild(element);
                        return;
                    }

                // Auf Wunsch Rohdaten formatieren
                var dataFormat = element.getAttribute(HTMLTemplate.formatAttribute);
                if (dataFormat != null)
                    if (dataValue != null)
                        dataValue = dataFormat.replace(HTMLTemplate.valuePlaceholder, dataValue);

                // Zielelement ermitteln
                var target = $(element);
                var targetLookup = element.getAttribute(HTMLTemplate.targetAttribute);
                if (targetLookup != null)
                    target = target.find(targetLookup);

                // Attribut des Zielelementes ermitteln und formatierten oder unformatierten Wert einsetzen
                var attributeTarget = element.getAttribute(HTMLTemplate.targetAttributeAttribute);
                if (attributeTarget == null)
                    if (target.is(':checkbox')) {
                        target.prop('checked', dataValue);

                        if (element.getAttribute(HTMLTemplate.writebackAttribute) != null)
                            target.change(() => model[dataProperty] = target.prop('checked'));
                    }
                    else
                        target.text(dataValue);
                else
                    $.each(attributeTarget.split(','), (i: number, oneAttributeTarget: string) => target.attr(oneAttributeTarget, dataValue));
            });

            // Platzhalter für Prüfergebnisse
            element.find('[' + HTMLTemplate.validationResultAttribute + ']').each((index: any, element: Element) => {
                var target = $(element);
                var validationPath = element.getAttribute(HTMLTemplate.validationResultAttribute);
                if (HTMLTemplate.retrieveProperty(model, validationPath))
                    target.removeClass(CSSClass.warning);
                else
                    target.addClass(CSSClass.warning);
            });

            // Platzhalte für Raktion auf das Betätigen von Verweisen
            element.find('[' + HTMLTemplate.clickAttribute + ']').each((index: any, elem: Element) => {
                var dataProperty = elem.getAttribute(HTMLTemplate.clickAttribute);
                var dataValue = model[dataProperty];

                $(elem).on('click', dataValue);
            });

            // Erst anzeigen, nachdem alles ersetzt wurde
            element.removeClass(CSSClass.invisible);
            element.removeAttr('id');
        }

        // Erzeugt eine Kopie einer Vorlage und erstzt dann in dieser Kopie alle Platzhalter.
        static cloneAndApplyTemplate(model: any, element: JQuery): JQuery {
            // Kopie erstellen
            element = element.clone();

            // Platzhalter anwenden
            HTMLTemplate.applyTemplate(model, element);

            // Kopie melden
            return element;
        }

        // Ersetzt die darzustellenden Daten.
        loadList(items: any): void {
            this.items = items;
            this.refresh();
        }
    }

    // Verwaltet in einer Tabellendarstellung die Möglichkeit, Details aufzuklappen.
    export class DetailManager {
        constructor(nodesUp: number, ...args: string[]) {
            var templates: JQuery[] = new Array();

            $.each(args, (index: number, template: string) =>
                TemplateLoader.load(template).done((template: string) =>
                    templates[index] = $(template).find('#template')));

            this.nodesToMoveUp = nodesUp;
            this.templates = templates;
        }

        // Alle Vorlagen
        private templates: JQuery[];

        // Der relative Bezugspunkt für das Aufklappen der Details
        private nodesToMoveUp: number;

        // Das aktuell angezeigte Oberflächenelement
        private activeNode: Node = null;

        // Die laufende Nummer der gerade angezeigten Vorlage
        private activeTemplate: number = -1;

        // Setzt die Verwaltung auf den Grundzustand zurück
        reset(): void {
            this.activeNode = null;
        }

        // Blendet eine neue Detailansicht ein oder eine existierende aus
        toggle(item: any, origin: any, templateIndex: number, factory: (item: any, template: JQuery) => JQuery = null): JQuery {
            // Noch nicht geladen - schade
            var template = this.templates[templateIndex];
            if (template == undefined)
                return null;

            // Auslösepunkt ermitteln und daraus den Bezugspunkt bestimmen
            var row: Node = origin;
            for (var i = this.nodesToMoveUp; i-- > 0;)
                row = row.parentNode;

            // Schauen wir mal, ob wir gerade ein Detail anzeigen
            var active = this.activeNode;
            if (active != null) {
                active.parentNode.removeChild(active.nextSibling);

                // Wir sollen einfach nur zuklappen
                if (this.activeTemplate == templateIndex)
                    if (active === row) {
                        this.activeNode = null;

                        // Fertig
                        return null;
                    }
            }

            // Aktualisieren
            this.activeTemplate = templateIndex;
            this.activeNode = row;

            // Neues Oberflächenelement aus der Vorlage erzeugen, befüllen und anzeigen
            var newElement;
            if (factory == null)
                newElement = HTMLTemplate.cloneAndApplyTemplate(item, template);
            else
                newElement = factory(item, template);
            newElement.addClass(CSSClass.detailView);
            newElement.removeAttr('id');
            newElement.insertAfter(row);

            return newElement;
        }
    }

    // Formatiert Datum und Uhrzeit
    export class DateFormatter {
        // Die Kürzel für die Wochentage
        static germanDays: string[] = ['So', 'Mo', 'Di', 'Mi', 'Do', 'Fr', 'Sa'];

        // Stellt sicher, dass eine Zahl immer zweistellig ist
        static formatNumber(num: number): string {
            var asString = num.toString();
            if (asString.length > 1)
                return asString;
            else
                return '0' + asString;
        }

        // Ermittelt die Uhrzeit
        static getEndTime(end: Date): string {
            return DateFormatter.formatNumber(end.getHours()) + ':' + DateFormatter.formatNumber(end.getMinutes());
        }

        // Ermittelt eine Dauer in Minuten und stellt diese als Uhrzeit dar
        static getDuration(duration: Date): string {
            return DateFormatter.formatNumber(duration.getUTCHours()) + ':' + DateFormatter.formatNumber(duration.getUTCMinutes());
        }

        // Ermittelt ein Datum
        static getStartDate(start: Date): string {
            return DateFormatter.getShortDate(start) + '.' + start.getFullYear().toString();
        }

        // Ermittelt ein Datum ohne Jahresangabe
        static getShortDate(start: Date): string {
            return DateFormatter.germanDays[start.getDay()] + ' ' + DateFormatter.formatNumber(start.getDate()) + '.' + DateFormatter.formatNumber(1 + start.getMonth());
        }

        // Ermittelt einen Startzeitpunkt
        static getStartTime(start: Date): string {
            var time = DateFormatter.formatNumber(start.getHours()) + ':' + DateFormatter.formatNumber(start.getMinutes());

            return DateFormatter.getStartDate(start) + ' ' + time;
        }

        // Prüft eine Eingabe auf eine gültige Uhrzeit (H:M, jeweils ein oder zweistellig)
        static parseTime(time: string): number {
            var parts = time.split(':');
            if (parts.length != 2)
                return null;

            var hour = DateFormatter.parseHourMinute(parts[0]);
            if (hour == null)
                return null;
            if (hour > 23)
                return null;
            var minute = DateFormatter.parseHourMinute(parts[1]);
            if (minute == null)
                return null;
            if (minute > 59)
                return null;

            return (60 * hour + minute) * 60000;
        }

        // Analyisiert eine Eingabe auf eine gültige, maximal zweistellige nicht negative Zahl
        private static parseHourMinute(hourMinute: string): number {
            if (hourMinute.length == 1)
                hourMinute = '0' + hourMinute;
            if (hourMinute.length != 2)
                return null;

            var upper = DateFormatter.parseDigit(hourMinute.charCodeAt(0));
            if (upper == null)
                return null;
            var lower = DateFormatter.parseDigit(hourMinute.charCodeAt(1));
            if (lower == null)
                return null;

            return upper * 10 + lower;
        }

        // Anlysiert die Eingabe einer Ziffer
        private static parseDigit(digit: number): number {
            if (digit < 0x30)
                return null;
            if (digit > 0x39)
                return null;

            return digit - 0x30;
        }
    }

    // Verwaltung einer Auswahl von N aus 24 Stunden für die Aktualisierungen
    export class HourListSettings {
        // Zerlegt eine Liste von Zahlen in einzelne Markierungen
        static decompress(settings: any, hours: number[]): void {
            for (var i = 0; i < 24; i++)
                settings['hour' + DateFormatter.formatNumber(i)] = false;

            $.each(hours, (index: number, hour: number) => {
                if (hour >= 0)
                    if (hour <= 23)
                        settings['hour' + DateFormatter.formatNumber(hour)] = true;
            });
        }

        // Kombiniert Markierungen zu einer Liste
        static compress(settings: any): number[] {
            var hours: number[] = new Array();
            for (var i = 0; i < 24; i++)
                if (settings['hour' + DateFormatter.formatNumber(i)])
                    hours.push(i);

            return hours;
        }

        // Prüft, ob ein bestimmter Name eine unserer Markierungseigenschaften ist
        static isHourFlag(name: string): boolean {
            return (name.length == 6) && (name.substr(0, 4) == 'hour');
        }

        // Erstellt die Schaltflächen für die Auswahl der Uhrzeiten.
        static createHourButtons(hours: JQuery, namePrefix: string): void {
            for (var i = 0; i < 24; i++) {
                var fullHour = DateFormatter.formatNumber(i);
                var name = namePrefix + fullHour;
                var prop = 'hour' + fullHour;

                hours.append('<input id="' + name + '" type="checkbox" ' + Bindings.propertyAttribute + '="' + prop + '" class="' + CSSClass.hourSetting + '"/>');
                hours.append('<label for="' + name + '">' + fullHour + '</label>');
            }
        }
    }

}

