/// <reference path="../vcrnet.tsx" />

namespace VCRNETClient {
    export class Navigation extends NoUiView<App.NoUi.IPage>
    {
        render(): JSX.Element {
            var page = this.props.noui;

            if (!page || !page.navigation)
                return null;

            return <div className="vcrnet-navigation">
                {page.navigation.refresh ? <InternalLink view={() => page.reload()} pict="refresh">Aktualisieren</InternalLink> : null}
                <InternalLink view="home" pict="home">Startseite</InternalLink>
                {page.navigation.favorites ? <InternalLink view="favorites" pict="fav">Favoriten</InternalLink> : null}
                {page.navigation.guide ? <InternalLink view="guide" pict="guide">Programmzeitschrift</InternalLink> : null}
                {page.navigation.plan ? <InternalLink view="plan" pict="plan">Aufzeichnungsplan</InternalLink> : null}
                {page.navigation.new ? <InternalLink view="edit" pict="new">Neue Aufzeichnung</InternalLink> : null}
                {page.navigation.current ? <InternalLink view="current" pict="devices">Geräte</InternalLink> : null}
            </div>;
        }
    }
}