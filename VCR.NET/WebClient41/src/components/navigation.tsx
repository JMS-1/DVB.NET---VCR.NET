/// <reference path="../vcrnet.tsx" />

namespace VCRNETClient {
    interface INavigationStatic {
    }

    interface INavigationDynamic {
        active?: boolean;
    }

    export class Navigation extends React.Component<INavigationStatic, INavigationDynamic>  {
        render(): JSX.Element {
            return <div className="vcrnet-navigation">
                <InternalLink text="Aktualisieren" view={this.onRefresh.bind(this)} pict="refresh" />
                <InternalLink text="Startseite" view="home" pict="home" />
                <InternalLink text="Favoriten" view="favorites" pict="fav" />
                <InternalLink text="Programmzeitschrift" view="guide" pict="guide" />
                <InternalLink text="Aufzeichnungsplan" view="plan" pict="plan" />
                <InternalLink text="Neue Aufzeichnung" view="edit" pict="new" />
                <InternalLink text="Geräte" view="current" pict="devices" />
            </div>;
        }

        private onRefresh(): void {
        }
    }
}