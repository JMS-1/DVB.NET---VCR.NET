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
                {page.showRefresh() ? <InternalLink text="Aktualisieren" view={page.refresh} pict="refresh" /> : null}
                <InternalLink text="Startseite" view="home" pict="home" />
                {page.showFavorites() ? <InternalLink text="Favoriten" view="favorites" pict="fav" /> : null}
                {page.showGuide() ? <InternalLink text="Programmzeitschrift" view="guide" pict="guide" /> : null}
                {page.showPlan() ? <InternalLink text="Aufzeichnungsplan" view="plan" pict="plan" /> : null}
                {page.showNew() ? <InternalLink text="Neue Aufzeichnung" view="edit" pict="new" /> : null}
                {page.showCurrent() ? <InternalLink text="Geräte" view="current" pict="devices" /> : null}
            </div>;
        }
    }
}