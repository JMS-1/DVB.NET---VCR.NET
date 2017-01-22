﻿namespace VCRNETClient.App {

    export interface IApplication {
        readonly homePage: IPage;

        readonly helpPage: IPage;

        readonly planPage: IPage;

        readonly editPage: IPage;

        getHelpComponentProvider<TComponentType extends IHelpComponent>(): IHelpComponentProvider<TComponentType>;
    }

    export interface IApplicationSite extends JMSLib.App.ISite {
        onFirstStart(): void;

        goto(page: string);

        getHelpComponentProvider<TComponentType extends IHelpComponent>(): IHelpComponentProvider<TComponentType>;
    }

    export class Application implements IApplication {
        readonly homePage = new HomePage(this);

        readonly helpPage = new HelpPage(this);

        readonly planPage = new PlanPage(this);

        readonly editPage = new EditPage(this);

        private _pageMapper: { [name: string]: Page<any> } = {};

        // Nach aussen hin sichtbarer globaler Zustand.
        version: VCRServer.InfoServiceContract;

        profile: VCRServer.UserProfileContract;

        page: App.IPage;

        // Initial sind wir gesperrt.
        private _busy = true;

        getIsBusy(): boolean {
            return this._busy;
        }

        // Wieviele ausstehende Zugriffe bis zum Start gibt es noch.
        private _startPending = 2;

        constructor(private _site: IApplicationSite) {
            // Alle bekannten Seiten.
            var pages: Page<any>[] = [
                this.homePage,
                this.helpPage,
                this.planPage,
                this.editPage,
            ];

            // Abbildung erstellen.
            pages.forEach(p => this._pageMapper[p.route] = p);

            var testStart = this.testStart.bind(this);

            // Alle Startvorgänge einleiten.
            VCRServer.getServerVersion().then(info => this.version = info).then(testStart);
            VCRServer.getUserProfile().then(profile => this.profile = profile).then(testStart);
        }

        private testStart(): void {
            if (this._startPending-- > 1)
                return;

            // Alle Startvorgänge sind abgeschlossen
            this.setBusy(false);

            // Wir können nun die Standardseite aktivieren
            this._site.onFirstStart();
        }

        gotoPage(name: string): void {
            this._site.goto(name);
        }

        switchPage(name: string, section: string): boolean {
            // Wir sind noch in einem Übergang oder beim Starten
            if (this._busy)
                return false;

            this.setBusy(true);

            // Den Singleton der gewünschten Seite ermitteln.
            var page = this._pageMapper[name] || this.homePage;

            // Aktivieren.
            this.page = page;

            // Zustand wie beim Erstaufruf vorbereiten.
            page.reset(section);

            return true;
        }

        setBusy(isBusy: boolean): void {
            if (isBusy === this._busy)
                return;

            this._busy = isBusy

            if (this._site)
                this._site.refreshUi();
        }

        getTitle(): string {
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