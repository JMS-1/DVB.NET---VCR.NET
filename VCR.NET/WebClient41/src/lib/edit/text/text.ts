/// <reference path="../edit.ts" />

namespace JMSLib.App {

    // Beschreibt eine Eigenschaft der Art Zeichenkette mit Prüfergebnissen.
    export interface IString extends IProperty<string> {
    }

    // Verwaltet eine Eigenschaft der Art Zeichenkette.
    export class String extends Property<string> implements IString {

        // Legt eine neue Verwaltung an.
        constructor(data: any, prop: string, name: string, onChange: () => void) {
            super(data, prop, name, onChange);
        }

        // Ergänzt eine Prüfung auf eine leere Zeichenkette.
        addRequiredValidator(message: string = `Es muss ein Wert angegeben werden.`): this {
            return this.addValidator(p => {
                // Der Wert darf nicht die leere Zeichenkette sein - und auch nicht nur aus Leerzeichen et al bestehen.
                var value = (p.value || "").trim();

                if (value.length < 1)
                    return message;
            });
        }

        // Ergänzt eine Prüfung auf ein festes Muster.
        addPatternValidator(matcher: RegExp, message: string): this {
            return this.addValidator(p => {
                if (!matcher.test(p.value))
                    return message;
            });
        }
    }
}