namespace VCRNETClient.App.Favorites {

    // Schnittstelle zur Anzeige und Pflege einer gespeicherten Suche.
    export interface IFavorite extends JMSLib.App.IConnectable {
        // Die Beschreibung der gespeicherten Suche.
        readonly title: string;

        // Die Anzahl der Sendungen, die zur Suche passen.
        readonly count: number;

        // Entfernt die gespeicherte Suche.
        readonly remove: JMSLib.App.ICommand;

        // Zeit die Sendungen der gespeicherten Suche in der Programmzeitschrift an.
        readonly show: JMSLib.App.ICommand;
    }

    // Präsentationsmodell  zurn Anzeige und Pflege einer gespeicherten Suche.
    export class Favorite implements IFavorite {

        // Synchronisert das Ermitteln der Anzeige der Sendungen - es wird zu jeder Zeit immer nur eine Anfrage an den VCR.NET Recording Service gestellt.
        private static _loader: JMSLib.App.IHttpPromise<void>;

        // Beginnt die Synchronisation neu.
        static resetLoader(): void {
            Favorite._loader = new JMSLib.App.Promise<void, JMSLib.App.IHttpErrorInformation>(success => success(void (0)));
        }

        // Legt ein Präsentationsmodell an.
        constructor(public readonly model: VCRServer.SavedGuideQueryContract, show: (favorite: Favorite) => void, remove: (favorite: Favorite) => JMSLib.App.IHttpPromise<void>, private _refresh: () => void) {
            this.remove = new JMSLib.App.Command<void>(() => remove(this), "Löschen");
            this.show = new JMSLib.App.Command<void>(() => show(this), "Anzeigen");
        }

        // Das aktuell angemeldete Oberflächenelement.
        view: JMSLib.App.IView;

        // Entfernt die gespeicherte Suche.
        readonly remove: JMSLib.App.Command<void>;

        // Zeit die Sendungen der gespeicherten Suche in der Programmzeitschrift an.
        readonly show: JMSLib.App.Command<void>;

        // Die Beschreibung der gespeicherten Suche.
        get title(): string {
            var display = 'Alle ';

            // Einige Einschränkungen machen nur Sinn, wenn keine Quelle ausgewählt ist.
            if ((this.model.source || '') == '') {
                // Verschlüsselung.
                if (this.model.encryption == VCRServer.GuideEncryption.FREE)
                    display += 'unverschlüsselten ';
                else if (this.model.encryption == VCRServer.GuideEncryption.PAY)
                    display += 'verschlüsselten ';

                // Art der Quelle.
                if (this.model.sourceType == VCRServer.GuideSource.TV)
                    display += 'Fernseh-';
                else if (this.model.sourceType == VCRServer.GuideSource.RADIO)
                    display += 'Radio-';
            }

            // Gerät.
            display += 'Sendungen, die über das Gerät ';
            display += this.model.device;

            // Quelle.
            if (this.model.source != null)
                if (this.model.source.length > 0) {
                    display += ' von der Quelle ';
                    display += this.model.source;
                }

            // Suchtext.
            display += ' empfangen werden und deren Name ';
            if (!this.model.titleOnly)
                display += 'oder Beschreibung ';

            display += ' "';
            display += this.model.text.substr(1);
            display += '" ';

            // Art der Suche.
            if (this.model.text[0] == '*')
                display += 'enthält';
            else
                display += 'ist';

            return display;
        }

        // Die Anzahl der Sendungen, die zur Suche passen.
        private _count: number;

        get count(): number {
            // Das haben wir schon einmal probiert.
            if (this._count !== undefined)
                return this._count;

            // Sicherstellen, dass nur einmal geladen wird.
            this._count = null;

            // Suchbedingung in die Protokollnotation wandeln - naja, das ist nicht wirklich schwer.
            var filter: VCRServer.GuideFilterContract = {
                content: this.model.titleOnly ? undefined : this.model.text,
                cryptFilter: this.model.encryption,
                typeFilter: this.model.sourceType,
                station: this.model.source,
                device: this.model.device,
                title: this.model.text,
                start: null,
                index: 0,
                size: 0,
            };

            // Laden anstossen.
            Favorite._loader = Favorite._loader.then(() =>
                VCRServer.countProgramGuide(filter).then(count => {
                    // Wert vermerken.
                    this._count = count;

                    // Oberfläche zur Aktualisierung auffordern.
                    if (this.view)
                        this.view.refreshUi();

                    // Eventuell auch die Liste aktualisieren.
                    if (count !== 0)
                        if (this._refresh)
                            this._refresh();
                }));

            return this._count;
        }
    }

}