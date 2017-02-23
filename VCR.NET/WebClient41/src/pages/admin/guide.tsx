/// <reference path="../../lib.react/reactUi.tsx" />

namespace VCRNETClient.Ui {

    export class AdminGuide extends JMSLib.ReactUi.ComponentWithSite<App.Admin.IAdminGuidePage>{
        render(): JSX.Element {
            return <div className="vcrnet-admin-guide">
                <h2>Einstellungen zum Aufbau der Programmzeitschrift</h2>
                <div>
                    Auf Wunsch kann der VCR.NET Recording Service die Elektronische Programmzeitschrift
                    (EPG)<HelpLink topic="epg" page={this.props.uvm.page} /> periodisch
                    aktualisieren<HelpLink topic="epgconfig" page={this.props.uvm.page} /> und
                    dann zur Programmierung von neuen Aufzeichnungen
                    anbieten.<JMSLib.ReactUi.InternalLink view={this.props.uvm.page.application.editPage.route} pict="new" /> Hier
                    werden die Eckdaten für die Aktualisierung festgelegt.
                </div>
                <JMSLib.ReactUi.EditBoolean uvm={this.props.uvm.isActive} />
                {this.props.uvm.isActive.value ? <form>
                    {this.getSourceHelp()}
                    <div>
                        <JMSLib.ReactUi.SelectMultipleFromList uvm={this.props.uvm.sources} items={10} />
                        <JMSLib.ReactUi.ButtonCommand uvm={this.props.uvm.remove} />
                    </div>
                    {this.getUkHelp()}
                    <JMSLib.ReactUi.EditBoolean uvm={this.props.uvm.ukTv} />
                    <Field page={this.props.uvm.page} label={`${this.props.uvm.device.text}:`}>
                        <JMSLib.ReactUi.SelectSingleFromList uvm={this.props.uvm.device} />
                        <JMSLib.ReactUi.ButtonCommand uvm={this.props.uvm.add} />
                    </Field>
                    <EditChannel uvm={this.props.uvm.source} />
                    {this.getDurationHelp()}
                    <Field page={this.props.uvm.page} label={`${this.props.uvm.duration.text}:`} >
                        <JMSLib.ReactUi.EditNumber uvm={this.props.uvm.duration} chars={5} />
                    </Field>
                    <Field page={this.props.uvm.page} label={`${this.props.uvm.hours.text}:`} >
                        <JMSLib.ReactUi.MultiButtonsFromList uvm={this.props.uvm.hours} />
                    </Field>
                    <Field page={this.props.uvm.page} label={`${this.props.uvm.delay.text}:`} >
                        <JMSLib.ReactUi.EditNumber uvm={this.props.uvm.delay} chars={5} />
                    </Field>
                    <Field page={this.props.uvm.page} label={`${this.props.uvm.latency.text}:`} >
                        <JMSLib.ReactUi.EditNumber uvm={this.props.uvm.latency} chars={5} />
                    </Field>
                </form> : null}
                {this.getUpdateHelp()}
                <JMSLib.ReactUi.ButtonCommand uvm={this.props.uvm.update} />
            </div>;
        }

        private getUpdateHelp(): JSX.Element {
            return <InlineHelp title="Erläuterungen zur Bedienung">
                Alle Änderungen müssen durch das explizite Betätigen der entsprechenden Schaltfläche
                bestätigt werden und werden auch damit erst übernommen. Änderungen an der Konfiguration
                der Aktualisierung der Programmzeitschrift erfordern keinen Neustart des VCR.NET Dienstes.
            </InlineHelp>;
        }

        private getSourceHelp(): JSX.Element {
            return <InlineHelp title="Erläuterungen zur Bedienung">
                Der VCR.NET Recording Service verwaltet eine geräteübergreifende Liste von Quellen, die in der
                Programmzeitschrift zu berücksichtigen sind. Nach Auswahl von Quellen aus der Liste können diese
                einfach daraus entfernt werden.
            </InlineHelp>;
        }

        private getUkHelp(): JSX.Element {
            return <InlineHelp title="Erläuterungen zur Bedienung">
                Wenn auch Quellen zu britischen Sendern in der Liste enthalten sind, so muss auch die Option
                aktiviert werden um die zugehörigen Vorschaudaten mit einzusammeln. Dies ist notwendig, da
                hierfür andere technische Voraussetzungen beim Emfpang der Elektronischen Programmzeitschrift
                gelten.
                <br />
                <br />
                Sollen Quellen zur Liste hinzugeführt werden, so ist zuerst einmal das Gerät auszuwählen,
                über das die gewünschten Quellen empfangen werden können. Danach können die von der Programmierung
                neuer Aufzeichnungen her bekannten Mechanismen zur schnellen Auswahl der Quelle verwendet
                werden.<HelpLink topic="sourcechooser" page={this.props.uvm.page} />
            </InlineHelp>;
        }

        private getDurationHelp(): JSX.Element {
            return <InlineHelp title="Erläuterungen zur Bedienung">
                Zur Planung der Aktualisierungen zwischen regulären Aufzeichnungen benötigt der VCR.NET
                Recording Service eine zeitliche Begrenzung für die Ausführung der Aktualisierung. Wird
                diese erreicht, so wird die Aktualisierung vorzeitig beendet und die Programmzeitschrift
                ist eventuell unvollständig. Zu große Werte können dazu führen, dass Aktualisierungen
                durch zeitliche Kollisionen mit regulären Aufzeichnungen später als geplant ausgeführt
                werden, da reguläre Aufzeichnung immer vorrangig ausgeführt werden. Ein realistischer
                Wert für die Laufzeit macht hier immer Sinn - kann die Aktualisierung in kürzerer Zeit
                abgeschlossen werden, so wird der VCR.NET Recording Service diese auch zeitnah beenden -
                das nutzt aber für die Aufzeichnungsplanung nichts.
                <br />
                <br />
                Für die Aktualisierungen werden oft eine Reihe fester Uhrzeiten vorgegeben. Ergänzend oder
                alternativ kann auch ein Zeitintervall vorgegeben werden, in dem Aktualisierungen durchgeführt
                werden sollen - etwa alle 4 Stunden. Eine Besonderheit ist die zusätzliche Latenzzeit: hat
                der VCR.NET Recording Service gerade eine Aufzeichnung ausgeführt und würde nun keines
                der Geräte mehr nutzen, so kann eine anstehende Aktualisierung vorzogen werden, wenn die
                vorherige Aktualisierung bereits mehr als das konfigurierte Intervall in der Vergangenheit
                liegt.
            </InlineHelp>;
        }
    }

}
