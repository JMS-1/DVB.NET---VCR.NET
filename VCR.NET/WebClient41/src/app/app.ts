namespace VCRNETClient.App {

    export interface IApplication {
        readonly homePage: IHomePage;

        readonly helpPage: IHelpPage;

        readonly planPage: IPlanPage;

        readonly editPage: IEditPage;

        readonly guidePage: IGuidePage;

        readonly jobPage: IJobPage;

        readonly logPage: ILogPage;

        readonly adminPage: IAdminPage;

        readonly settingsPage: ISettingsPage;

        getHelpComponentProvider<TComponentType extends IHelpComponent>(): IHelpComponentProvider<TComponentType>;
    }

    export interface IApplicationSite extends JMSLib.App.ISite {
        onFirstStart(): void;

        goto(page: string);

        getHelpComponentProvider<TComponentType extends IHelpComponent>(): IHelpComponentProvider<TComponentType>;
    }

    export class Application implements IApplication {
        readonly homePage: HomePage;

        readonly helpPage: HelpPage;

        readonly planPage: PlanPage;

        readonly editPage: EditPage;

        readonly guidePage: GuidePage;

        readonly jobPage: JobPage;

        readonly logPage: LogPage;

        readonly adminPage: AdminPage;

        readonly settingsPage: SettingsPage;

        private _pageMapper: { [name: string]: Page } = {};

        // Nach aussen hin sichtbarer globaler Zustand.
        version: VCRServer.InfoServiceContract;

        profile: VCRServer.UserProfileContract;

        page: App.IPage;

        // Initial sind wir gesperrt.
        private _busy = true;

        get isBusy(): boolean {
            return this._busy;
        }

        constructor(private _site: IApplicationSite) {
            // Alle bekannten Seiten.
            this.settingsPage = this.addPage(SettingsPage);
            this.adminPage = this.addPage(AdminPage);
            this.guidePage = this.addPage(GuidePage);
            this.editPage = this.addPage(EditPage);
            this.helpPage = this.addPage(HelpPage);
            this.homePage = this.addPage(HomePage);
            this.planPage = this.addPage(PlanPage);
            this.jobPage = this.addPage(JobPage);
            this.logPage = this.addPage(LogPage);

            VCRServer.getServerVersion().then(info => this.setVersion(info));
        }

        private addPage<TPageType extends Page>(factory: { new (application: Application): TPageType }): TPageType {
            var page = new factory(this);

            this._pageMapper[page.route] = page;

            return page;
        }

        private setVersion(info: VCRServer.InfoServiceContract): void {
            this.version = info;

            VCRServer.getUserProfile().then(profile => this.setUserProfile(profile));
        }

        private setUserProfile(profile: VCRServer.UserProfileContract): void {
            this.profile = profile;

            // Alle Startvorgänge sind abgeschlossen
            this.isBusy = false;

            // Wir können nun die Standardseite aktivieren
            this._site.onFirstStart();
        }

        gotoPage(name: string): void {
            this._site.goto(name);
        }

        switchPage(name: string, sections: string[]): boolean {
            // Melden, dass alle ausstehenden asynchronen Anfragen von nun an nicht mehr interessieren.
            JMSLib.App.switchView();

            this.isBusy = true;

            // Den Singleton der gewünschten Seite ermitteln.
            var page = this._pageMapper[name] || this.homePage;

            // Aktivieren.
            this.page = page;

            // Zustand wie beim Erstaufruf vorbereiten.
            page.reset(sections || []);

            return true;
        }

        set isBusy(isBusy: boolean) {
            if (isBusy === this._busy)
                return;

            this._busy = isBusy

            if (this._site)
                this._site.refreshUi();
        }

        get title(): string {
            var title = "VCR.NET Recording Service";
            var version = this.version;

            if (version)
                return `${title} ${version.version}`;
            else
                return title;
        }

        getHelpComponentProvider<TComponentType extends IHelpComponent>(): IHelpComponentProvider<TComponentType> {
            return this._site && this._site.getHelpComponentProvider<TComponentType>();
        }
    }
}