/// <reference path="../lib.react/reactUi.tsx" />

namespace VCRNETClient.Ui {

    // React.Js Komponente zur Anzeige der Navigationsleiste.
    export class Navigation extends JMSLib.ReactUi.Component<App.IPage>
    {
        // Erstellt die Anzeige.
        render(): JSX.Element {
            var page = this.props.uvm;

            if (!page || !page.navigation)
                return null;

            var application = page.application;

            return <div className="vcrnet-navigation vcrnet-bar">
                {page.navigation.refresh && <JMSLib.ReactUi.InternalLink view={() => page.reload()} pict="refresh">Aktualisieren</JMSLib.ReactUi.InternalLink>}
                <JMSLib.ReactUi.InternalLink view={application.homePage.route} pict="home">Startseite</JMSLib.ReactUi.InternalLink>
                {page.navigation.favorites && <JMSLib.ReactUi.InternalLink view={application.favoritesPage.route} pict="fav">Favoriten</JMSLib.ReactUi.InternalLink>}
                {page.navigation.guide && <JMSLib.ReactUi.InternalLink view={application.guidePage.route} pict="guide">Programmzeitschrift</JMSLib.ReactUi.InternalLink>}
                {page.navigation.plan && <JMSLib.ReactUi.InternalLink view={application.planPage.route} pict="plan">Aufzeichnungsplan</JMSLib.ReactUi.InternalLink>}
                {page.navigation.new && <JMSLib.ReactUi.InternalLink view={application.editPage.route} pict="new">Neue Aufzeichnung</JMSLib.ReactUi.InternalLink>}
                {page.navigation.current && <JMSLib.ReactUi.InternalLink view={application.devicesPage.route} pict="devices">Geräte</JMSLib.ReactUi.InternalLink>}
            </div>;
        }
    }
}