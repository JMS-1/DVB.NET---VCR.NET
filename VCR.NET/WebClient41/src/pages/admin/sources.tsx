/// <reference path="section.tsx" />

namespace VCRNETClient.Ui {

    // React.Js Komponente zur Konfiguration des Sendersuchlaufs.
    export class AdminSources extends AdminSection<App.Admin.IAdminScanPage>{

        // Das zugehörige Ui View Model.
        static get uvm(): IAdminSectionFactory<App.Admin.IAdminScanPage> {
            return App.Admin.ScanSection;
        }

        // Die Überschrift für diesen Bereich.
        protected readonly title = `Aktualisierung der Quellen konfigurieren`;

        // Oberflächenelemente erstellen.
        protected renderSection(): JSX.Element {
            return <div className="vcrnet-admin-sources">
                <div>
                    Der VCR.NET Recording Service ist in der Lage, die Liste der Quellen der verwendeten DVB.NET
                    Geräte zu aktualisieren.<HelpLink topic="psiconfig" page={this.props.uvm.page} /> Dies kann
                    automatisch oder gemäß eines Zeitplans erfolgen.
                    </div>
                <div><JMSLib.ReactUi.SingleSelect uvm={this.props.uvm.mode} /></div>
                {this.props.uvm.showConfiguration && <form>
                    {this.getDurationHelp()}
                    <Field page={this.props.uvm.page} label={`${this.props.uvm.duration.text}:`} >
                        <JMSLib.ReactUi.EditNumber uvm={this.props.uvm.duration} chars={5} />
                    </Field>
                    <JMSLib.ReactUi.EditBoolean uvm={this.props.uvm.merge} />
                    {this.props.uvm.configureAutomatic && <div>
                        {this.getHourHelp()}
                        <Field page={this.props.uvm.page} label={`${this.props.uvm.hours.text}:`} >
                            <JMSLib.ReactUi.MultiSelectButton uvm={this.props.uvm.hours} merge={true} />
                        </Field>
                        <Field page={this.props.uvm.page} label={`${this.props.uvm.gapDays.text}:`} >
                            <JMSLib.ReactUi.EditNumber uvm={this.props.uvm.gapDays} chars={5} />
                        </Field>
                        <Field page={this.props.uvm.page} label={`${this.props.uvm.latency.text}:`} >
                            <JMSLib.ReactUi.EditNumber uvm={this.props.uvm.latency} chars={5} />
                        </Field>
                    </div>}
                </form>}
                {this.getSaveHelp()}
            </div>;
        }

        // Erläuterungen zu den Zeitparametern.
        private getDurationHelp(): JSX.Element {
            return <InlineHelp title="Erläuterungen zur Bedienung">
                Wenn die Aktualisierung der Quellen (auch als Sendersuchlauf bekannt) nicht gänzlich deaktiviert ist,
                so muss zumindest eine Laufzeitbegrenzung für die Aktualisierung angegeben werden. Wird dieser
                während einer Aktualisierung überschritten, so wird die Aktualisierung vorzeitig beenden und es
                wurden möglicherweise nicht alle Quellen korrekt berücksichtigt. Da diese Zeit allerdings auch
                bei der Aufzeichnungsplanung berücksichtigt wird, sollte der Wert nicht unnötig groß gewählt
                werden - ist eine Aktualisierung vor Ablauf des Zeitraums abgeschlossen, so wird sie zwar
                vorzeitig beendet, die Planung erfolgt allerdings immer mit dem gesamten Zeitraum. Mit Abschluss
                der Aktualisierung werden die neuen Informationen zu den Quellen in die aktuelle Liste der
                Quellen eingemischt. Auf Wunsch kann die Liste auch ersetzt werden, was allerdings in einigen
                Fällen zum unerwünschten Verlust von Quellen führen kann.
            </InlineHelp>;
        }

        // Erläuterung zur Stundenauswahl.
        private getHourHelp(): JSX.Element {
            return <InlineHelp title="Erläuterungen zur Bedienung">
                Für eine automatische Aktualisierung muss immer angegeben werden, wieviel Zeit zwischen zwei
                Läufen mindestens verstreichen muss. Zusätzlich kann die Aktualisierung auf feste Stunden
                festgelegt werden. Als Besonderheit erlaubt die Latenzzeit eine vorgezogene Aktualisierung
                für den Fall, dass der VCR.NET Recording Service gerade alle ausstehenden Aufzeichnungen
                abgeschlössen hat und die vorherige Aktualisierung mehr als das konfigurierte Intervall
                in der Vergangenheit liegt.
            </InlineHelp>;
        }

        // Erläuterungen zum Speichern der Konfiguration.
        private getSaveHelp(): JSX.Element {
            return <InlineHelp title="Erläuterungen zur Bedienung">
                Für eine Übernahme der hier vorgenommenen Änderungen muss die entsprechende Schaltfläche
                explizit betätigt werden. Die Konfiguration der Aktualisierung der Liste der Quellen
                erfordert keinen Neustart des VCR.NET Dienstes.
            </InlineHelp>;
        }
    }

}
