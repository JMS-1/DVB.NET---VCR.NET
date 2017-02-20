/// <reference path="../vcrnet.tsx" />

namespace VCRNETClient.Ui {
    export class Main extends React.Component<JMSLib.ReactUi.IEmpty, JMSLib.ReactUi.IEmpty> implements App.IApplicationSite {
        private static _topics: { [section: string]: App.IHelpComponent; };

        private static initStatic(): void {
            Main._topics = {
                repeatingschedules: new HelpPages.RepeatingSchedules(),
                parallelrecording: new HelpPages.ParallelRecording(),
                jobsandschedules: new HelpPages.JobsAndSchedules(),
                customschedule: new HelpPages.CustomSchedule(),
                configuration: new HelpPages.Configuration(),
                controlcenter: new HelpPages.ControlCenter(),
                currentstream: new HelpPages.CurrentStream(),
                epgconfig: new HelpPages.AdminProgramGuide(),
                numberoffiles: new HelpPages.NumberOfFiles(),
                sourcechooser: new HelpPages.SourceChooser(),
                filecontents: new HelpPages.FileContents(),
                psiconfig: new HelpPages.AdminSourceScan(),
                editcurrent: new HelpPages.EditCurrent(),
                hibernation: new HelpPages.Hibernation(),
                sourcelimit: new HelpPages.SourceLimit(),
                websettings: new HelpPages.WebSettings(),
                decryption: new HelpPages.Decryption(),
                streaming: new HelpPages.Streaming(),
                overview: new HelpPages.Overview(),
                tsplayer: new HelpPages.TsPlayer(),
                epg: new HelpPages.ProgramGuide(),
                archive: new HelpPages.Archive(),
                dvbnet: new HelpPages.DvbNet(),
                nexus: new HelpPages.Nexus(),
                tasks: new HelpPages.Tasks(),
                log: new HelpPages.Log(),
            };
        }

        private _application = new App.Application(this);

        private _onhashchange: () => void;

        constructor() {
            super();

            if (!Main._topics)
                Main.initStatic();

            this._onhashchange = this.onhashchange.bind(this);

            this.onhashchange();
        }

        componentDidMount(): void {
            window.addEventListener("hashchange", this._onhashchange);
        }

        componentWillUnmount(): void {
            window.removeEventListener("hashchange", this._onhashchange);
        }

        refreshUi(): void {
            this.forceUpdate();
        }

        render(): JSX.Element {
            var title = this._application.title;
            var page = this._application.page;

            if (document.title !== title)
                document.title = title;

            return <div className="vcrnet-main">
                {this._application.isRestarting ?
                    <div>Der VCR.NET Recording Service startet nun neu und steht in Kürze wieder zur Verfügung.</div> :
                    (this._application.isBusy ?
                        <div><h1>(Bitte etwas Geduld)</h1></div> :
                        <div><h1>{page ? page.title : title}</h1><Navigation noui={page} /><View noui={page} /></div>)}
            </div>;
        }

        private onhashchange(): void {
            // Auslesen der Kennung - für FireFox ist es nicht möglich, .hash direkt zu verwenden, da hierbei eine Decodierung durchgeführt wird
            var query = window.location.href.split("#");
            var hash = (query.length > 1) ? query[1] : "";

            if (hash.length < 1)
                this.setPage();
            else {
                var sections = hash.split(";");

                this.setPage(sections[0], sections.slice(1));
            }
        }

        private setPage(name: string = "", sections?: string[]) {
            this._application.switchPage(name, sections);
        }

        goto(name: string): void {
            window.location.href = name ? `#${name}` : `#`;
        }

        getHelpComponentProvider<TComponentType extends App.IHelpComponent>(): App.IHelpComponentProvider<TComponentType> {
            return Main._topics as App.IHelpComponentProvider<TComponentType>;
        }

    }
}
