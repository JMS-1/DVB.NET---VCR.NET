namespace VCRNETClient.App.Favorites {

    export class Favorite implements IFavorite {

        private static _loader: JMSLib.App.IHttpPromise<void>;

        static resetLoader(): void {
            Favorite._loader = new JMSLib.App.Promise<void, JMSLib.App.IHttpErrorInformation>(success => success(undefined));
        }

        constructor(public readonly model: VCRServer.SavedGuideQueryContract) {
        }

        site: JMSLib.App.ISite;

        readonly remove = new JMSLib.App.Command(() => this._remove(this), "Löschen");

        readonly show = new JMSLib.App.Command(() => this._show(this), "Anzeigen");

        private _remove: (favorite: Favorite) => JMSLib.App.IHttpPromise<void>;

        private _show: (favorite: Favorite) => void;

        registerRemove(show: (favorite: Favorite) => void, remove: (favorite: Favorite) => JMSLib.App.IHttpPromise<void>): void {
            this._remove = remove;
            this._show = show;

            this.remove.reset();
            this.show.reset();
        }

        get title(): string {
            var display = 'Alle ';

            if ((this.model.source || '') == '') {
                if (this.model.encryption == VCRServer.GuideEncryption.FREE)
                    display += 'unverschlüsselten ';
                else if (this.model.encryption == VCRServer.GuideEncryption.PAY)
                    display += 'verschlüsselten ';

                if (this.model.sourceType == VCRServer.GuideSource.TV)
                    display += 'Fernseh-';
                else if (this.model.sourceType == VCRServer.GuideSource.RADIO)
                    display += 'Radio-';
            }

            display += 'Sendungen, die über das Gerät ';
            display += this.model.device;

            if (this.model.source != null)
                if (this.model.source.length > 0) {
                    display += ' von der Quelle ';
                    display += this.model.source;
                }

            display += ' empfangen werden und deren Name ';
            if (!this.model.titleOnly)
                display += 'oder Beschreibung ';

            display += ' "';
            display += this.model.text.substr(1);
            display += '" ';

            if (this.model.text[0] == '*')
                display += 'enthält';
            else
                display += 'ist';

            return display;
        }

        private _count: number = null;

        get count(): number {
            if (this._count !== null)
                return this._count;

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

            Favorite._loader = Favorite._loader.then(() =>
                VCRServer.countProgramGuide(filter).then(count => {
                    this._count = count;

                    if (this.site)
                        this.site.refreshUi();
                }));

            return null;
        }
    }

}