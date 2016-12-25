/// <reference path="generic.ts" />

namespace VCRNETClient.App.NoUi {

    export interface IChannelSelector {
        section(newSection?: string): string;

        readonly sections: string[];

        type(newType?: string): string;

        readonly types: string[];

        encryption(newEncryption?: string): string;

        readonly encryptions: string[];
    }

    export class ChannelEditor extends ValueHolder<string> implements IChannelSelector {
        private static readonly _encryptions = [
            "Alle Quellen",
            "Nur verschlüsselte Quellen",
            "Nur unverschlüsselte Quellen"
        ];

        private _encryption = ChannelEditor._encryptions[0];

        encryptions = ChannelEditor._encryptions;

        encryption(newEncryption?: string): string {
            var oldEncryption = this._encryption;

            if (newEncryption !== undefined)
                if (newEncryption !== oldEncryption)
                    if (this.encryptions.indexOf(newEncryption) > 0) {
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

        types = ChannelEditor._types;

        type(newType?: string): string {
            var oldType = this._type;

            if (newType !== undefined)
                if (newType !== oldType)
                    if (this.types.indexOf(newType) > 0) {
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

        sections = ChannelEditor._sections;

        section(newSection?: string): string {
            var oldSection = this._section;

            if (newSection !== undefined)
                if (newSection !== oldSection)
                    if (this.sections.indexOf(newSection) > 0) {
                        this._section = newSection;

                        this.refreshFilter();
                    }

            return oldSection;
        }
        constructor(data: any, prop: string, onChange: () => void) {
            super(data, prop, onChange);
        }

        private refreshFilter(): void {
        }
    }
}