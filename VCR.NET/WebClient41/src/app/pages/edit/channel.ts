/// <reference path="../../../lib/edit/edit.ts" />

namespace VCRNETClient.App {

    // Schnitstelle zur Pflege der Senderauswahl.
    export interface IChannelSelector extends JMSLib.App.IProperty<string>, JMSLib.App.IConnectable {
        // Die Vorauswahl der Quellen vor allem nach dem ersten Zeichen des Namens.
        section: string;

        readonly sections: string[];

        // Die Vorauswahl der Quellen über die Art (Fernsehen oder Radio).
        type: string;

        readonly types: string[];

        // Die Vorauswahl der Quellen über die Verschlüsselung.
        encryption: string;

        readonly encryptions: string[];

        // Die komplette Liste aller verfügbaren Quellen.
        sourceNames: JMSLib.App.IUiValue<string>[];

        // Gesetzt, wenn die zusätzlichen Filter angezeigt werden sollen.
        readonly showFilter: boolean;
    }

    // Stellt die Logik zur Auswahl eines Senders zur Verfügung.
    export class ChannelEditor extends JMSLib.App.Property<string> implements IChannelSelector {

        // Die Auswahl der Verschlüsselung.
        private static readonly _encryptions = [
            "Alle Quellen",
            "Nur verschlüsselte Quellen",
            "Nur unverschlüsselte Quellen"
        ];

        // Die aktuelle Einschränkung bezüglich der Verschlüsselung.
        private _encryption = ChannelEditor._encryptions[0];

        // Prüft ob eine Quelle der aktuellen Einschränkung der Verschlüsselung entspricht.
        private applyEncryptionFilter(source: VCRServer.SourceEntry): boolean {
            // Wenn wir nur die bevorzugten Sender anzeigen gibt es keine Einschränkung.
            if (!this.showFilter)
                return true;

            switch (this.encryptions.indexOf(this._encryption)) {
                case 0:
                    return true;
                case 1:
                    return source.isEncrypted;
                case 2:
                    return !source.isEncrypted;
                default:
                    return false;
            }
        }

        // Alle Auswahlmöglichkeiten der Verschlüsselung.
        encryptions = ChannelEditor._encryptions;

        // Meldet oder ändert die aktuelle Auswahl der Verschlüsselung.
        get encryption(): string {
            return this._encryption;
        }

        set encryption(newEncryption: string) {
            if (newEncryption !== this._encryption)
                if (this.encryptions.indexOf(newEncryption) >= 0) {
                    this._encryption = newEncryption;

                    this.refreshFilter();
                }
        }

        // Die Auswahlmöglichkeiten zur Art der Quelle.
        private static readonly _types = [
            "Alle Quellen",
            "Nur Radio",
            "Nur Fernsehen"
        ];

        // Die aktuelle Einschränung auf die Art der Quelle.
        private _type = ChannelEditor._types[0];

        // Prüft, ob eine Quelle der aktuell ausgewählten Art entspricht.
        private applyTypeFilter(source: VCRServer.SourceEntry): boolean {
            // Wenn wir nur die bevorzugten Sender anzeigen gibt es keine Einschränkung.
            if (!this.showFilter)
                return true;

            switch (this.types.indexOf(this._type)) {
                case 0:
                    return true;
                case 1:
                    return !source.isTelevision;
                case 2:
                    return source.isTelevision;
                default:
                    return false;
            }
        }

        // Alle Auswahlmöglichkeiten für die Art der Quelle.
        types = ChannelEditor._types;

        // Meldet oder ändert die Auswahl der Art der Quellen.
        get type(): string {
            return this._type;
        }

        set type(newType: string) {
            if (newType !== this._type)
                if (this.types.indexOf(newType) >= 0) {
                    this._type = newType;

                    this.refreshFilter();
                }
        }

        // Alle möglichen Einschränkungen auf die Namen der Quellen.
        private static readonly _sections = [
            "(Zuletzt verwendet)",
            "A B C",
            "D E F",
            "G H I",
            "J K L",
            "M N O",
            "P Q R",
            "S T U",
            "V W X",
            "Y Z",
            "0 1 2 3 4 5 6 7 8 9",
            "(Andere)",
            "(Alle Quellen)"
        ];

        // Die aktuelle Auswahl auf die Namen der Quellen.
        private _section = ChannelEditor._sections[0];

        // Prüft, ob der Name einer Quelle der aktuellen Auswahl entspricht.
        private applySectionFilter(source: VCRServer.SourceEntry): boolean {
            var first = source.firstNameCharacter;

            switch (this.sections.indexOf(this._section)) {
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
        sections = ChannelEditor._sections;

        // Meldet oder ändert die Auswahl für die Einschränkung auf den Namen der Quellen.
        get section(): string {
            return this._section;
        }

        set section(newSection: string) {
            if (newSection !== this._section) {
                var sectionIndex = this.sections.indexOf(newSection);

                if (sectionIndex >= 0) {
                    this.showFilter = (sectionIndex > 0);
                    this._section = newSection;

                    this.refreshFilter();
                }
            }
        }

        // Alle aktuell bezüglich aller Einschränkungen relevanten Quellen.
        sourceNames: JMSLib.App.IUiValue<string>[];

        // Die bevorzugten Quellen des Anwenders - hier in einem Dictionary zur Prüfung optimiert.
        private _favorites: { [source: string]: boolean } = {};

        // Gesetzt, wenn die zusätzlichen Filter angezeigt werden sollen.
        showFilter = false;

        // Gesetzt, wenn der Sender bekannt ist.
        private _hasChannel = false;

        // Erstellt eine neue Logik zur Senderauswahl.
        constructor(data: any, prop: string, favoriteSources: string[], onChange: () => void) {
            super(data, prop, "Quelle", onChange);

            // Prüfungen einrichten
            this.addValidator(c => !this._hasChannel && `Die Quelle wird von dem ausgewählten Gerät nicht empfangen.`);

            // Übernimmt die lineare Liste aller bevorzugten Sender zur schnelleren Auswahl in ein Dictionary.
            if (this.showFilter = (favoriteSources.length < 1))
                this._section = this.sections[this.sections.length - 1];
            else
                favoriteSources.forEach(s => this._favorites[s] = true);
        }

        // Ermittelt die Liste der relevanten Quellen neu.
        private refreshFilter(): void {
            // Alle Quellen bezüglich der aktiven Filter untersuchen.
            this.sourceNames = this.sources.filter(s => {
                if (!this.applyEncryptionFilter(s))
                    return false;
                if (!this.applyTypeFilter(s))
                    return false;
                if (!this.applySectionFilter(s))
                    return false;

                return true;
            }).map(s => JMSLib.App.uiValue(s.name));

            // Aktuelle Quelle zusätzliche in die Liste einmischen, so dass immer eine korrekte Auswahl existiert.
            var source = this.value;
            var hasSource = ((source || "").trim().length > 0);

            if (hasSource && !this.sourceNames.some(s => s.value === source)) {
                var cmp = source.toLocaleUpperCase();
                var insertAt = -1;

                for (var i = 0; i < this.sourceNames.length; i++)
                    if (cmp.localeCompare(this.sourceNames[i].value.toLocaleUpperCase()) < 0) {
                        insertAt = i;

                        break;
                    }

                // Bereits gewählte Quelle an der korrekten Position in der Liste eintragen.
                if (insertAt < 0)
                    this.sourceNames.push(JMSLib.App.uiValue(source));
                else
                    this.sourceNames.splice(insertAt, 0, JMSLib.App.uiValue(source));
            }

            // Der erste Eintrag erlaubt es immer auch einfach mal keinen Sender auszuwählen.
            this.sourceNames.unshift(JMSLib.App.uiValue("", "(Keine Quelle)"));

            // Schauen wir mal, ob wir die Quelle überhaupt kennen.
            this._hasChannel = (!hasSource || this.sources.some(s => s.name === source));

            // Anzeige aktualisieren.
            this.refresh();
        }

        // Sämtliche bekannten Quellen.
        private _sources: VCRServer.SourceEntry[] = [];

        get sources(): VCRServer.SourceEntry[] {
            return this._sources;
        }

        set sources(sources: VCRServer.SourceEntry[]) {
            // Falls wir auf der gleichen Liste arbeiten müssen wir gar nichts machen.
            if (this._sources === sources)
                return;

            // Die Liste der Quellen wurde verändert.
            this._sources = sources;

            this.refreshFilter();
        }
    }
}