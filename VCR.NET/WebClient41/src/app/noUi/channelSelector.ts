/// <reference path="generic.ts" />

namespace VCRNETClient.App.NoUi {

    // Wird von der Anzeige der Senderauswahl bereitgestellt.
    export interface IChannelSelectorSite {
        // Die Anzeige muss erneuert werden, weil sich vermutlich die Liste der Quellen verändert hat.
        refresh(): void;
    }

    // Schnitstelle zur Pflege der Senderauswahl.
    export interface IChannelSelector extends IValidatableValue<string> {
        // Legt die Arbeitsumgebung fest.
        setSite(site: IChannelSelectorSite): void;

        // Die Vorauswahl der Quellen vor allem nach dem ersten Zeichen des Namens.
        section(newSection?: string): string;

        readonly sections: string[];

        // Die Vorauswahl der Quellen über die Art (Fernsehen oder Radio).
        type(newType?: string): string;

        readonly types: string[];

        // Die Vorauswahl der Quellen über die Verschlüsselung.
        encryption(newEncryption?: string): string;

        readonly encryptions: string[];

        // Die komplette Liste aller verfügbaren Quellen.
        sourceNames: ISelectableValue<string>[];

        // Gesetzt, wenn die zusätzlichen Filter angezeigt werden sollen.
        readonly showFilter: boolean;
    }

    // Stellt die Logik zur Auswahl eines Senders zur Verfügung.
    export class ChannelEditor extends ValueHolder<string> implements IChannelSelector {

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
        encryption(newEncryption?: string): string {
            var oldEncryption = this._encryption;

            if (newEncryption !== undefined)
                if (newEncryption !== oldEncryption)
                    if (this.encryptions.indexOf(newEncryption) >= 0) {
                        this._encryption = newEncryption;

                        this.refreshFilter();
                    }

            return oldEncryption;
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
        type(newType?: string): string {
            var oldType = this._type;

            if (newType !== undefined)
                if (newType !== oldType)
                    if (this.types.indexOf(newType) >= 0) {
                        this._type = newType;

                        this.refreshFilter();
                    }

            return oldType;
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
        section(newSection?: string): string {
            var oldSection = this._section;

            if (newSection !== undefined)
                if (newSection !== oldSection) {
                    var sectionIndex = this.sections.indexOf(newSection);

                    if (sectionIndex >= 0) {
                        this.showFilter = (sectionIndex > 0);
                        this._section = newSection;

                        this.refreshFilter();
                    }
                }

            return oldSection;
        }

        // Alle aktuell bezüglich aller Einschränkungen relevanten Quellen.
        sourceNames: ISelectableValue<string>[];

        // Sämtliche bekannten Quellen.
        private _sources: VCRServer.SourceEntry[] = [];

        // Die bevorzugten Quellen des Anwenders - hier in einem Dictionary zur Prüfung optimiert.
        private _favorites: { [source: string]: boolean } = {};

        // Die aktuelle Arbeitsumgebung der Senderauswahl.
        private _site: IChannelSelectorSite;

        setSite(site: IChannelSelectorSite): void {
            this._site = site;
        }

        // Gesetzt, wenn die zusätzlichen Filter angezeigt werden sollen.
        showFilter = false;

        // Gesetzt, wenn eine Quelle angegeben werden muss.
        private _isRequired = false;

        // Gesetzt, wenn der Sender bekannt ist.
        private _hasChannel = false;

        // Erstellt eine neue Logik zur Senderauswahl.
        constructor(data: any, prop: string, favoriteSources: string[], onChange: () => void) {
            super(data, prop, onChange, "Quelle");

            // Übernimmt die lineare Liste aller bevorzugten Sender zur schnelleren Auswahl in ein Dictionary.
            if (this.showFilter = (favoriteSources.length < 1))
                this._section = this.sections[this.sections.length - 1];
            else
                favoriteSources.forEach(s => this._favorites[s] = true);
        }

        // Ermittelt die Liste der relevanten Quellen neu.
        private refreshFilter(): void {
            // Alle Quellen bezürglich der aktiven Filter untersuchen.
            this.sourceNames = this._sources.filter(s => {
                if (!this.applyEncryptionFilter(s))
                    return false;
                if (!this.applyTypeFilter(s))
                    return false;
                if (!this.applySectionFilter(s))
                    return false;

                return true;
            }).map(s => <ISelectableValue<string>>{ value: s.name, display: s.name });

            // Aktuelle Quelle zusätzliche in die Liste einmischen, so dass immer eine korrekte Auswahl existiert.
            var source = this.val();
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
                    this.sourceNames.push({ value: source, display: source });
                else
                    this.sourceNames.splice(insertAt, 0, { value: source, display: source });
            }

            // Der erste Eintrag erlaubt es immer auch einfach mal keinen Sender auszuwählen.
            this.sourceNames.unshift({ value: "", display: "(Keine Quelle)" });

            // Schauen wir mal, ob wir die Quelle überhaupt kennen.
            this._hasChannel = (!hasSource || this._sources.some(s => s.name === source));

            // Anzeige aktualisieren.
            if (this._site)
                this._site.refresh();
        }

        // Aktuelle Liste der Quellen festlegen, etwa nach der Änderung des zu verwendenden Geräteprofils.
        setSources(sources: VCRServer.SourceEntry[], sourceIsRequired: boolean): void {
            this._isRequired = sourceIsRequired;

            // Falls wir auf der gleichen Liste arbeiten müssen wir gar nichts machen.
            if (this._sources === sources)
                return;

            // Die Liste der Quellen wurde verändert.
            this._sources = sources;

            this.refreshFilter();
        }

        // Prüft den aktuellen Wert.
        validate(): void {
            // Sollte die Basisklasse bereits einen Fehler melden so ist dieser so elementar, dass er unbedingt verwendet werden soll.
            super.validate();

            if (this.message.length > 0)
                return;

            // Unbekannter Sender.
            if (!this._hasChannel) {
                this.message = "Die Quelle wird von dem ausgewählten Gerät nicht empfangen.";

                return;
            }

            // Die Quelle darf eventuell auch leer sein.
            if (!this._isRequired)
                return;

            // Quelle prüfen.
            var value = (this.val() || "").trim();

            if (value.length < 1)
                this.message = "Entweder für die Aufzeichnung oder für den Auftrag muss eine Quelle angegeben werden.";
        }
    }
}