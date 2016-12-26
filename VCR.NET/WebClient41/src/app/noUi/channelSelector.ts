/// <reference path="generic.ts" />

namespace VCRNETClient.App.NoUi {

    export interface IChannelSelectorSite {
        refresh(): void;
    }

    export interface IChannelSelector extends IValidatableValue<string> {
        setSite(site: IChannelSelectorSite): void;

        section(newSection?: string): string;

        readonly sections: string[];

        type(newType?: string): string;

        readonly types: string[];

        encryption(newEncryption?: string): string;

        readonly encryptions: string[];

        sourceNames: ISelectableValue<string>[];
    }

    export class ChannelEditor extends ValueHolder<string> implements IChannelSelector {
        private static readonly _encryptions = [
            "Alle Quellen",
            "Nur verschlüsselte Quellen",
            "Nur unverschlüsselte Quellen"
        ];

        private _encryption = ChannelEditor._encryptions[0];

        private applyEncryptionFilter(source: VCRServer.SourceEntry): boolean {
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

        encryptions = ChannelEditor._encryptions;

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

        private static readonly _types = [
            "Alle Quellen",
            "Nur Radio",
            "Nur Fernsehen"
        ];

        private _type = ChannelEditor._types[0];

        private applyTypeFilter(source: VCRServer.SourceEntry): boolean {
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

        types = ChannelEditor._types;

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

        private _section = ChannelEditor._sections[0];

        private applySectionFilter(source: VCRServer.SourceEntry): boolean {
            var first = source.firstNameCharacter;

            switch (this.sections.indexOf(this._section)) {
                case 0:
                    return false;
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

        sections = ChannelEditor._sections;

        section(newSection?: string): string {
            var oldSection = this._section;

            if (newSection !== undefined)
                if (newSection !== oldSection)
                    if (this.sections.indexOf(newSection) >= 0) {
                        this._section = newSection;

                        this.refreshFilter();
                    }

            return oldSection;
        }

        sourceNames: ISelectableValue<string>[];

        private _site: IChannelSelectorSite;

        private _sources: VCRServer.SourceEntry[] = [];

        setSite(site: IChannelSelectorSite): void {
            this._site = site;
        }

        constructor(data: any, prop: string, onChange: () => void) {
            super(data, prop, onChange);
        }

        private refreshFilter(): void {
            this.sourceNames = this._sources.filter(s => {
                if (!this.applyEncryptionFilter(s))
                    return false;
                if (!this.applyTypeFilter(s))
                    return false;
                if (!this.applySectionFilter(s))
                    return false;

                return true;
            }).map(s => <ISelectableValue<string>>{ value: s.name, display: s.name });

            var source = this.val();

            if ((source || "").trim().length > 0)
                if (!this.sourceNames.some(s => s.value === source)) {
                    var cmp = source.toLocaleUpperCase();
                    var insertAt = -1;

                    for (var i = 0; i < this.sourceNames.length; i++)
                        if (cmp.localeCompare(this.sourceNames[i].value.toLocaleUpperCase()) < 0) {
                            insertAt = i;

                            break;
                        }

                    if (insertAt < 0)
                        this.sourceNames.push({ value: source, display: source });
                    else
                        this.sourceNames.splice(insertAt, 0, { value: source, display: source });
                }

            this.sourceNames.unshift({ value: "", display: "(Keine Quelle)" });

            if (this._site)
                this._site.refresh();
        }

        setSources(sources: VCRServer.SourceEntry[]): void {
            this._sources = sources;

            this.refreshFilter();
        }
    }
}