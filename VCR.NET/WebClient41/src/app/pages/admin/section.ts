namespace VCRNETClient.App.Admin {

    // Gemeinsame Schnittstelle für alle Konfigurationsbereiche der Administration.
    export interface ISection extends JMSLib.App.IConnectable {
        readonly page: IAdminPage;

        readonly update: JMSLib.App.ICommand;
    }

    export abstract class Section<TSettingsType> implements ISection {

        private _update: JMSLib.App.Command<void>;

        get update(): JMSLib.App.Command<void> {
            if (!this._update)
                this._update = new JMSLib.App.Command(() => this.save(), this.saveCaption, () => this.canSave)

            return this._update;
        }

        constructor(public readonly page: AdminPage) {
        }

        site: JMSLib.App.ISite;

        protected refreshUi(): void {
            if (this.site)
                this.site.refreshUi();
        }

        abstract reset(): void;

        protected readonly saveCaption: string = "Ändern";

        protected readonly canSave: boolean = true;

        protected abstract saveAsync(): JMSLib.App.IHttpPromise<boolean>;

        private save(): JMSLib.App.IHttpPromise<void> {
            return this.page.update(this.saveAsync(), this.update);
        }
    }

    export interface ISectionInfo<TPageType extends ISection> {
        readonly display: string;

        readonly page: TPageType;
    }

    export interface ISectionInfos<TInterface extends ISectionInfo<any>> {
        readonly[section: string]: TInterface;
    }

}