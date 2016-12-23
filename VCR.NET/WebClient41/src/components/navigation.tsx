/// <reference path="../vcrnet.tsx" />

namespace VCRNETClient {
    interface INavigationStatic {
        page: App.Page;
    }

    interface INavigationDynamic {
        active?: boolean;
    }

    export class Navigation extends React.Component<INavigationStatic, INavigationDynamic>  {
        render(): JSX.Element {
            var page = this.props.page;

            if (!page || !page.showNavigation())
                return null;

            return <div className="vcrnet-navigation">
                {page.showRefresh() ? <InternalLink view={page.refresh} pict="refresh">Aktualisieren</InternalLink> : null}
                <InternalLink view="home" pict="home">Startseite</InternalLink>
                {page.showFavorites() ? <InternalLink view="favorites" pict="fav">Favoriten</InternalLink> : null}
                {page.showGuide() ? <InternalLink view="guide" pict="guide">Programmzeitschrift</InternalLink> : null}
                {page.showPlan() ? <InternalLink view="plan" pict="plan">Aufzeichnungsplan</InternalLink> : null}
                {page.showNew() ? <InternalLink view="edit" pict="new">Neue Aufzeichnung</InternalLink> : null}
                {page.showCurrent() ? <InternalLink view="current" pict="devices">Geräte</InternalLink> : null}
            </div>;
        }
    }
}