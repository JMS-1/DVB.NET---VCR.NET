/// <reference path="../lib/reactUi.tsx" />

namespace VCRNETClient.Ui {

    // React.Js Komponente zur Anzeige der Navigationsleiste.
    export class Navigation extends JMSLib.ReactUi.Component<App.IPage>
    {
        // Erstellt die Anzeige.
        render(): JSX.Element {
            var page = this.props.noui;

            if (!page || !page.navigation)
                return null;

            var application = page.application;

            return <div className="vcrnet-navigation">
                {page.navigation.refresh ? <JMSLib.ReactUi.InternalLink view={() => page.reload()} pict="refresh">Aktualisieren</JMSLib.ReactUi.InternalLink> : null}
                <JMSLib.ReactUi.InternalLink view={application.homePage.route} pict="home">Startseite</JMSLib.ReactUi.InternalLink>
                {page.navigation.favorites ? <JMSLib.ReactUi.InternalLink view="favorites" pict="fav">Favoriten</JMSLib.ReactUi.InternalLink> : null}
                {page.navigation.guide ? <JMSLib.ReactUi.InternalLink view={application.guidePage.route} pict="guide">Programmzeitschrift</JMSLib.ReactUi.InternalLink> : null}
                {page.navigation.plan ? <JMSLib.ReactUi.InternalLink view={application.planPage.route} pict="plan">Aufzeichnungsplan</JMSLib.ReactUi.InternalLink> : null}
                {page.navigation.new ? <JMSLib.ReactUi.InternalLink view={application.editPage.route} pict="new">Neue Aufzeichnung</JMSLib.ReactUi.InternalLink> : null}
                {page.navigation.current ? <JMSLib.ReactUi.InternalLink view="current" pict="devices">Geräte</JMSLib.ReactUi.InternalLink> : null}
            </div>;
        }
    }
}