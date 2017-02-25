﻿/// <reference path="../../lib/edit/edit.ts" />

namespace VCRNETClient.App {

    // Die Einschränkung auf die Verschlüsselung.
    export enum EncryptionFilter {
        // Keine Einschränkung.
        all,

        // Nur verschlüsselte Sender.
        payTv,

        // Nur unverschlüsselte Sender.
        freeTv,
    }

    // Die Arten von zu berücksichtigenden Quellen.
    export enum TypeFilter {
        // Alle Quellen.
        all,

        // Nur Radiosender.
        radio,

        // Nur Fernsehsender.
        tv
    }

    // Schnitstelle zur Pflege der Senderauswahl.
    export interface IChannelSelector extends JMSLib.App.IProperty<string>, JMSLib.App.IConnectable {
        // Die Vorauswahl der Quellen vor allem nach dem ersten Zeichen des Namens.
        readonly section: JMSLib.App.IValueFromList<number>;

        // Die Vorauswahl der Quellen über die Art (Fernsehen oder Radio).
        readonly type: JMSLib.App.IValueFromList<TypeFilter>;

        // Die Vorauswahl der Quellen über die Verschlüsselung.
        readonly encryption: JMSLib.App.IValueFromList<EncryptionFilter>;

        // Die komplette Liste aller verfügbaren Quellen.
        readonly sourceName: JMSLib.App.IValueFromList<string>;

        // Gesetzt, wenn die zusätzlichen Filter angezeigt werden sollen.
        readonly showFilter: boolean;
    }

    // Stellt die Logik zur Auswahl eines Senders zur Verfügung.
    export class ChannelEditor extends JMSLib.App.Property<string> implements IChannelSelector {

        // Die Auswahl der Verschlüsselung.
        private static readonly _encryptions = [
            JMSLib.App.uiValue(EncryptionFilter.all, "Alle Quellen"),
            JMSLib.App.uiValue(EncryptionFilter.payTv, "Nur verschlüsselte Quellen"),
            JMSLib.App.uiValue(EncryptionFilter.freeTv, "Nur unverschlüsselte Quellen"),
        ];

        // Prüft ob eine Quelle der aktuellen Einschränkung der Verschlüsselung entspricht.
        private applyEncryptionFilter(source: VCRServer.SourceEntry): boolean {
            // Wenn wir nur die bevorzugten Sender anzeigen gibt es keine Einschränkung.
            if (!this.showFilter)
                return true;

            switch (this.encryption.value) {
                case EncryptionFilter.all:
                    return true;
                case EncryptionFilter.payTv:
                    return source.isEncrypted;
                case EncryptionFilter.freeTv:
                    return !source.isEncrypted;
                default:
                    return false;
            }
        }

        // Alle Auswahlmöglichkeiten der Verschlüsselung.
        readonly encryption = new JMSLib.App.SelectSingleFromList({ value: EncryptionFilter.all }, `value`, null, () => this.refreshFilter(), ChannelEditor._encryptions);

        // Die Auswahlmöglichkeiten zur Art der Quelle.
        private static readonly _types = [
            JMSLib.App.uiValue(TypeFilter.all, "Alle Quellen"),
            JMSLib.App.uiValue(TypeFilter.radio, "Nur Radio"),
            JMSLib.App.uiValue(TypeFilter.tv, "Nur Fernsehen")
        ];

        // Prüft, ob eine Quelle der aktuell ausgewählten Art entspricht.
        private applyTypeFilter(source: VCRServer.SourceEntry): boolean {
            // Wenn wir nur die bevorzugten Sender anzeigen gibt es keine Einschränkung.
            if (!this.showFilter)
                return true;

            switch (this.type.value) {
                case TypeFilter.all:
                    return true;
                case TypeFilter.radio:
                    return !source.isTelevision;
                case TypeFilter.tv:
                    return source.isTelevision;
                default:
                    return false;
            }
        }

        // Alle Auswahlmöglichkeiten für die Art der Quelle.
        readonly type = new JMSLib.App.SelectSingleFromList({ value: TypeFilter.all }, `value`, null, () => this.refreshFilter(), ChannelEditor._types);

        // Alle möglichen Einschränkungen auf die Namen der Quellen.
        private static readonly _sections = [
            JMSLib.App.uiValue(0, "(Zuletzt verwendet)"),
            JMSLib.App.uiValue(1, "A B C"),
            JMSLib.App.uiValue(2, "D E F"),
            JMSLib.App.uiValue(3, "G H I"),
            JMSLib.App.uiValue(4, "J K L"),
            JMSLib.App.uiValue(5, "M N O"),
            JMSLib.App.uiValue(6, "P Q R"),
            JMSLib.App.uiValue(7, "S T U"),
            JMSLib.App.uiValue(8, "V W X"),
            JMSLib.App.uiValue(9, "Y Z"),
            JMSLib.App.uiValue(10, "0 1 2 3 4 5 6 7 8 9"),
            JMSLib.App.uiValue(11, "(Andere)"),
            JMSLib.App.uiValue(12, "(Alle Quellen)")
        ];

        // Prüft, ob der Name einer Quelle der aktuellen Auswahl entspricht.
        private applySectionFilter(source: VCRServer.SourceEntry): boolean {
            var first = source.firstNameCharacter;

            switch (this.section.value) {
                case 0:
                    return this._favorites[source.name];
                case 1:
                    return (first >= "A") && (first <= "C");
                case 2:
                    return (first >= "D") && (first <= "F");
                case 3:
                    return (first >= "G") && (first <= "I");
                case 4:
                    return (first >= "J") && (first <= "L");
                case 5:
                    return (first >= "M") && (first <= "O");
                case 6:
                    return (first >= "P") && (first <= "R");
                case 7:
                    return (first >= "S") && (first <= "U");
                case 8:
                    return (first >= "V") && (first <= "X");
                case 9:
                    return (first >= "Y") && (first <= "Z");
                case 10:
                    return (first >= "0") && (first <= "9");
                case 11:
                    return !(((first >= "A") && (first <= "Z")) || ((first >= "0") && (first <= "9")));
                case 12:
                    return true;
                default:
                    return false;
            }
        }

        // Alle Auswahlmöglichkeiten zum Namen der Quellen.
        readonly section = new JMSLib.App.SelectSingleFromList({ value: 0 }, `value`, null, () => this.refreshFilter(), ChannelEditor._sections);

        // Alle aktuell bezüglich aller Einschränkungen relevanten Quellen.
        readonly sourceName = new JMSLib.App.SelectSingleFromList<string>(this, `value`);

        // Die bevorzugten Quellen des Anwenders - hier in einem Dictionary zur Prüfung optimiert.
        private _favorites: { [source: string]: boolean } = {};

        // Gesetzt, wenn die zusätzlichen Filter angezeigt werden sollen.
        get showFilter(): boolean {
            return this.section.value !== 0;
        }

        // Gesetzt, wenn der Sender bekannt ist.
        private _hasChannel = false;

        // Erstellt eine neue Logik zur Senderauswahl.
        constructor(data: any, prop: string, favoriteSources: string[], onChange: () => void) {
            super(data, prop, "Quelle", onChange);

            // Prüfungen einrichten
            this.addValidator(c => !this._hasChannel && `Die Quelle wird von dem ausgewählten Gerät nicht empfangen.`);

            // Übernimmt die lineare Liste aller bevorzugten Sender zur schnelleren Auswahl in ein Dictionary.
            if (favoriteSources.length < 1)
                this.section.value = this.section.allowedValues.length - 1;
            else
                favoriteSources.forEach(s => this._favorites[s] = true);
        }

        // Ermittelt die Liste der relevanten Quellen neu.
        private refreshFilter(): void {
            // Alle Quellen bezüglich der aktiven Filter untersuchen.
            var sourceNames = this.allSources.filter(s => {
                if (!this.applyEncryptionFilter(s))
                    return false;
                if (!this.applyTypeFilter(s))
                    return false;
                if (!this.applySectionFilter(s))
                    return false;

                return true;
            }).map(s => s.name);

            // Aktuelle Quelle zusätzliche in die Liste einmischen, so dass immer eine korrekte Auswahl existiert.
            var source = this.value;
            var hasSource = ((source || "").trim().length > 0);

            if (hasSource && (sourceNames.indexOf(source) < 0)) {
                var cmp = source.toLocaleUpperCase();
                var insertAt = -1;

                for (var i = 0; i < sourceNames.length; i++)
                    if (cmp.localeCompare(sourceNames[i].toLocaleUpperCase()) < 0) {
                        insertAt = i;

                        break;
                    }

                // Bereits gewählte Quelle an der korrekten Position in der Liste eintragen.
                if (insertAt < 0)
                    sourceNames.push(source);
                else
                    sourceNames.splice(insertAt, 0, source);
            }

            // Der erste Eintrag erlaubt es immer auch einfach mal keinen Sender auszuwählen.
            this.sourceName.allowedValues = [JMSLib.App.uiValue(``, "(Keine Quelle)")].concat(sourceNames.map(s => JMSLib.App.uiValue(s)));

            // Schauen wir mal, ob wir die Quelle überhaupt kennen.
            this._hasChannel = (!hasSource || this.allSources.some(s => s.name === source));

            // Anzeige aktualisieren.
            this.refresh();
        }

        // Sämtliche bekannten Quellen.
        private _allSources: VCRServer.SourceEntry[] = [];

        get allSources(): VCRServer.SourceEntry[] {
            return this._allSources;
        }

        set allSources(sources: VCRServer.SourceEntry[]) {
            // Falls wir auf der gleichen Liste arbeiten müssen wir gar nichts machen.
            if (this._allSources === sources)
                return;

            // Die Liste der Quellen wurde verändert.
            this._allSources = sources;

            this.refreshFilter();
        }
    }
}