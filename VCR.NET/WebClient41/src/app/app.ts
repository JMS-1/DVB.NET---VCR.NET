namespace VCRNETClient.App {

    // Schnittstelle der Anwendung.
    export interface IApplication {
        // Die Überschrift der Anwendung als Ganzes.
        readonly title: string;

        // Der aktuell verwendete Navigationsbereich.
        readonly page: IPage;

        // Getzt solange einer neuer Navigationsbereich initialisiert wird.
        readonly isBusy: boolean;

        // Gesetzt während der VCR.NET Recording Service neu startet.
        readonly isRestarting: boolean;

        // Das Präsentationsmodell der Einstiegsseite.
        readonly homePage: IHomePage;

        // Das Präsentationsmodell der Hilfeseiten.
        readonly helpPage: IHelpPage;

        // Das Präsentationsmodell des Aufzeichnungsplans.
        readonly planPage: IPlanPage;

        // Das Präsentationsmodell der Pflegeseite für eine Aufzeichnung.
        readonly editPage: IEditPage;

        // Das Präsentationsmodell der Programmzeitschrift.
        readonly guidePage: IGuidePage;

        // Das Präsentationsmodell der Aufzeichnungsübersicht.
        readonly jobPage: IJobPage;

        // Das Präsentationsmodell der Protokollansicht.
        readonly logPage: ILogPage;

        // Das Präsentationsmodell für die Konfiguration.
        readonly adminPage: IAdminPage;

        // Das Präsentationsmodell für die Einstellungen des Benutzers.
        readonly settingsPage: ISettingsPage;

        // Das Präsentationsmodell für die gespeicherten Suchen.
        readonly favoritesPage: IFavoritesPage;

        // Das Präsentationsmodell der Geräteübersicht.
        readonly devicesPage: IDevicesPage;

        // Meldet die Verwaltung der Hilfeseiten - dies erfolgt primär im Kontext der Oberfläche.
        getHelpComponentProvider<TComponentType extends IHelpComponent>(): IHelpComponentProvider<TComponentType>;

        // Wechselt den Navigationsbereich.
        switchPage(name: string, sections: string[]): void;
    }

    // Die von der Oberfläche bereitzustellende Arbeitsumgebung für die Anwendung.
    export interface IApplicationSite extends JMSLib.App.IView {
        // Wechselt zu einem anderen Navigationsbereich.
        goto(page: string);

        // Meldet die Verwaltung der Hilfeseiten.
        getHelpComponentProvider<TComponentType extends IHelpComponent>(): IHelpComponentProvider<TComponentType>;
    }

    // Das Präsentationsmodell der Anwendung.
    export class Application implements IApplication {

        // Das Präsentationsmodell der Einstiegsseite.
        readonly homePage: HomePage;

        // Das Präsentationsmodell der Hilfeseiten.
        readonly helpPage: HelpPage;

        // Das Präsentationsmodell des Aufzeichnungsplans.
        readonly planPage: PlanPage;

        // Das Präsentationsmodell der Pflegeseite für eine Aufzeichnung.
        readonly editPage: EditPage;

        // Das Präsentationsmodell der Programmzeitschrift.
        readonly guidePage: GuidePage;

        // Das Präsentationsmodell der Aufzeichnungsübersicht.
        readonly jobPage: JobPage;

        // Das Präsentationsmodell der Protokollansicht.
        readonly logPage: LogPage;

        // Das Präsentationsmodell für die Konfiguration.
        readonly adminPage: AdminPage;

        // Das Präsentationsmodell für die Einstellungen des Benutzers.
        readonly settingsPage: SettingsPage;

        // Das Präsentationsmodell für die gespeicherten Suchen.
        readonly favoritesPage: FavoritesPage;

        // Das Präsentationsmodell der Geräteübersicht.
        readonly devicesPage: DevicesPage;

        // Die in der Anwendung bereitgestellten Navigationsbereiche.
        private _pageMapper: { [name: string]: Page } = {};

        // Gesetzt wenn der Dienst gerade neu startet.
        isRestarting = false;

        // Version des VCR.NET Recording Service.
        version: VCRServer.InfoServiceContract;

        // Einstellungen des Benutzers.
        profile: VCRServer.UserProfileContract;

        // Der aktuelle Navigationsbereich.
        page: App.IPage;

        // Der interne Arbeitsstand der Anwendung.
        private _pendingPage: App.IPage;

        get isBusy(): boolean {
            return !!this._pendingPage;
        }

        set isBusy(isBusy: boolean) {
            // Das geht nur intern!
            if (isBusy)
                throw `isBusy darf nur intern gesetzt werden`;

            // Keine echte Änderung.
            if (isBusy === this.isBusy)
                return;

            // Zustand vermerken.
            this.page = this._pendingPage;

            this._pendingPage = undefined;

            // Oberfläche zur Aktualisierung auffordern.
            this.refreshUi();
        }

        // Erstellt ein neues Präsentationsmodell für die Anwendung.
        constructor(private _site: IApplicationSite) {
            // Navigationsbereiche einmalig anlegen - das ist hier am einfachsten in der Handhabe.
            this.favoritesPage = this.addPage(FavoritesPage);
            this.settingsPage = this.addPage(SettingsPage);
            this.devicesPage = this.addPage(DevicesPage);
            this.adminPage = this.addPage(AdminPage);
            this.guidePage = this.addPage(GuidePage);
            this.editPage = this.addPage(EditPage);
            this.helpPage = this.addPage(HelpPage);
            this.homePage = this.addPage(HomePage);
            this.planPage = this.addPage(PlanPage);
            this.jobPage = this.addPage(JobPage);
            this.logPage = this.addPage(LogPage);
        }

        // Erstellt einen Navigationsbereich und vermerkt ihn dann einmalig.
        private addPage<TPageType extends Page>(factory: { new (application: Application): TPageType }): TPageType {
            // Konkretes Präsentationmodell für den Bereich anlegen.
            var page = new factory(this);

            // Neue Instanz vermerken und melden.
            this._pageMapper[page.route] = page;

            return page;
        }

        // Den Navigationsbereich wechseln.
        gotoPage(name: string): void {
            // Tatsächlich macht das die Anwendung.
            this._site.goto(name);
        }

        switchPage(name: string, sections: string[]): void {
            // Melden, dass alle ausstehenden asynchronen Anfragen von nun an nicht mehr interessieren.
            JMSLib.App.switchView();

            // Den Singleton der gewünschten Seite ermitteln.
            var page = this._pageMapper[name] || this.homePage;

            // Aktivieren.
            this._pendingPage = page;

            // Anzeige aktualisieren lassen.
            this.refreshUi();

            // Benutzereinstellungen anfordern.
            VCRServer.getUserProfile().then(profile => {
                // Benutzereinstellungen übernehmen.
                this.profile = profile;

                // Versionsinformationen anfordern.
                return VCRServer.getServerVersion();
            }).then(info => {
                // Versionsinformationen aktualisieren.
                this.version = info;

                // Navigationsbereich starten.
                page.reset(sections || []);
            });
        }

        // Oberfläche zur Aktualisierung auffordern.
        private refreshUi(): void {
            if (this._site)
                this._site.refreshUi();
        }

        // Name der Anwendung für den Browser ermitteln.
        get title(): string {
            var title = "VCR.NET Recording Service";

            // Nach Möglichkeit die Versionsinformationen einmischen.
            var version = this.version;

            if (version)
                return `${title} ${version.version}`;
            else
                return title;
        }

        // Oberfläche nach der Verwaltung der Hilfeseiten fragen.
        getHelpComponentProvider<TComponentType extends IHelpComponent>(): IHelpComponentProvider<TComponentType> {
            return this._site && this._site.getHelpComponentProvider<TComponentType>();
        }

        // Der Dienst wird neu gestartet.
        restart(): void {
            // Zustand vermerken.
            this._pendingPage = undefined;
            this.isRestarting = true;
            this.page = null;

            // Ein wenig warten - das Intervall istrein willkürlich.
            setTimeout(() => {
                // Zustand zurücksetzen.
                this.isRestarting = false;

                // Einstiegsseite anfordern.
                this.gotoPage(null);

                // Oberfläche zur Aktualisierung auffordern.
                this.refreshUi();
            }, 10000);

            // Oberfläche zur Aktualisierung auffordern.
            this.refreshUi();
        }
    }
}