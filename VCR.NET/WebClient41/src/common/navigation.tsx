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
                {page.navigation.refresh ? <InternalLink view={() => page.reload()} pict="refresh">Aktualisieren</InternalLink> : null}
                <InternalLink view={application.homePage.route} pict="home">Startseite</InternalLink>
                {page.navigation.favorites ? <InternalLink view="favorites" pict="fav">Favoriten</InternalLink> : null}
                {page.navigation.guide ? <InternalLink view="guide" pict="guide">Programmzeitschrift</InternalLink> : null}
                {page.navigation.plan ? <InternalLink view={application.planPage.route} pict="plan">Aufzeichnungsplan</InternalLink> : null}
                {page.navigation.new ? <InternalLink view={application.editPage.route} pict="new">Neue Aufzeichnung</InternalLink> : null}
                {page.navigation.current ? <InternalLink view="current" pict="devices">Geräte</InternalLink> : null}
            </div>;
        }
    }
}