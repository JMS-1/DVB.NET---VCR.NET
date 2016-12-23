/// <reference path="../vcrnet.tsx" />
/// <reference path="../pages/faq/parallelRecording.tsx" />

namespace VCRNETClient {
    export interface IHelpComponentProvider {
        getHelpComponent(section: string): HelpComponent;
    }

    interface IMainStatic {
    }

    interface IMainDynamic {
        active?: boolean;
    }

    export class Main extends React.Component<IMainStatic, IMainDynamic> implements App.IApplicationSite, App.IHelpSite {
        private static _faq: { [section: string]: HelpComponent } = {
            parallelrecording: new HelpPages.ParallelRecording(),
            epgconfig: new HelpPages.AdminProgramGuide(),
            epg: new HelpPages.ProgramGuide(),
            archive: new HelpPages.Archive(),
            log: new HelpPages.Log()
        };

        private _application = new App.Application(this);

        private _onhashchange: () => void;

        constructor() {
            super();

            this._onhashchange = this.onhashchange.bind(this);
        }

        componentDidMount(): void {
            this._application.helpPage.setSite(this);

            window.addEventListener("hashchange", this._onhashchange);
        }

        componentWillUnmount(): void {
            window.removeEventListener("hashchange", this._onhashchange);

            this._application.helpPage.setSite(undefined);
        }

        onBusyChanged(isBusy: boolean): void {
            this.setState({ active: !isBusy });
        }

        onFirstStart(): void {
            this.onhashchange();
        }

        render(): JSX.Element {
            var title = this._application.getTitle();
            var page = this._application.page;

            if (document.title !== title)
                document.title = title;

            if (this.state && this.state.active)
                return <div className="vcrnet-main">
                    <h1>{page ? page.getTitle() : title}</h1>
                    <Navigation page={page} />
                    <View page={page} faqs={this} />
                </div>;
            else
                return <div className="vcrnet-main">
                    <h1>(Bitte etwas Geduld)</h1>
                </div>;
        }

        private onhashchange(): void {
            // Auslesen der Kennung - für FireFox ist es nicht möglich, .hash direkt zu verwenden, da hierbei eine Decodierung durchgeführt wird
            var query = window.location.href.split("#");
            var hash = (query.length > 1) ? query[1] : "";

            if (hash.length < 1)
                this.setPage();
            else {
                var sep = hash.indexOf(";");
                if (sep < 0)
                    this.setPage(hash);
                else
                    this.setPage(hash.substr(0, sep), hash.substr(sep + 1));
            }
        }

        private setPage(name: string = "", section: string = "") {
            this._application.switchPage(name, section);
        }

        getHelpComponent(section: string): HelpComponent {
            return Main._faq[section];
        }

        getCurrentHelpTitle(section: string): string {
            var faq = this.getHelpComponent(section);

            return faq && faq.getTitle();
        }
    }
}
