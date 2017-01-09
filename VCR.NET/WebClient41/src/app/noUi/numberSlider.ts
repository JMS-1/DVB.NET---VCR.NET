/// <reference path="generic.ts" />

namespace VCRNETClient.App.NoUi {

    // Steuert die Pflege einer Zahl über einen Schieberegler.
    export interface INumberSlider extends INoUiWithSite {
        // Meldet oder ändert die relative (0..1) Position des Reglers.
        position(newPosition?: number): number;

        // Aktiviert die Positionsveränderung.
        isDragging(newSelected?: boolean): boolean;

        // Erlaubt eine Feineinstellung des zugrundeliegenden Wertes.
        delta(delta: number): void;
    }

    // Steuerung für einen Schieberegeler für die Auswahl eines Wertes.
    export class NumberSlider extends ValueHolderWithSite<number> implements INumberSlider {
        // Die aktuelle relative (0..1) Position des Reglers.
        private _position = 0;

        // Gesetzt, wenn Bewegungsbefehle umgesetzt werden sollen.
        private _moving = false;

        // Der Anfangswert.
        private readonly _initial: number;

        // Erstellt eine neue Steuerung.
        constructor(data: any, prop: string, onChange: () => void, private _min: number, private _max: number) {
            super(data, prop, onChange, null);

            // Anfangswert merken.
            this._initial = this.val();

            // Synchronisiert den Regler mit dem aktuellen Wert.
            this.sync();
        }

        // Bei der Anzeige werden immer alle Einstellungen auf den Grundwert zurückgesetzt.
        setSite(newSite: INoUiSite): void {
            super.setSite(newSite);

            // Zurück auf den Anfang.
            this.sync(this._initial);

            this._moving = false;
        }

        // Synchronisiert den Regeler mit dem aktuellen Wert.
        sync(newValue?: number): void {
            // Auf Wunsch kann direkt ein neuer Wert eingestellt werden.
            if (newValue !== undefined)
                this.val(newValue);

            // In eine relative Position umsetzen.
            this.position((this.val() - this._min) / (this._max - this._min));
        }

        // Nimmt eine Feineinstellung vor.
        delta(delta: number): void {
            // Relative Position setzen, der Wert gleicht sich dann automatisch mit an.
            this.position((this.val() + delta - this._min) / (this._max - this._min));
        }

        // Meldet oder ändert die aktuelle Position des Schiebereglers.
        position(newPosition?: number): number {
            // Die vorherige Position.
            var oldPosition = this._position;

            // Auf einen neuen Wert prüfen.
            if (newPosition !== undefined) {
                // Relative Grenzen beachten.
                if (newPosition < 0)
                    newPosition = 0;
                else if (newPosition > 1)
                    newPosition = 1;

                if (newPosition !== oldPosition) {
                    // Schiebregler anpassen.
                    this._position = newPosition;

                    // Anzeige des Schiebereglers verändern.
                    this.refresh();

                    // Tatsächlichen Wert direkt mit ändern.
                    this.val(Math.round(this._min + newPosition * (this._max - this._min)));
                }
            }

            // Ursprüngliche Position melden.
            return oldPosition;
        }

        // Meldet oder legt fest, ob sich die Position aktuell verändert.
        isDragging(newSelected?: boolean): boolean {
            // Aktuellen Änderungsstand ermitteln.
            var oldSelected = this._moving;

            // Auf Wunsch Änderungsstand verändern.
            if (newSelected !== undefined) {
                if (newSelected !== oldSelected) {
                    this._moving = newSelected;

                    // Eventuell die Oberfläche anpassen (Feedback).
                    this.refresh();
                }
            }

            // Vorherige Einstellung melden.
            return oldSelected;
        }
    }
}