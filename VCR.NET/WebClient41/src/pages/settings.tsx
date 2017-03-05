/// <reference path="../lib.react/reactUi.tsx" />

namespace VCRNETClient.Ui {

    // React.Js Komponente zur Pflege der Benutzereinstellungen.
    export class Settings extends JMSLib.ReactUi.ComponentWithSite<App.ISettingsPage> {

        // Oberflächenelemente erzeugen.
        render(): JSX.Element {
            return <div className="vcrnet-settings">
                Hier werden im Wesentlichen Voreinstellungen für einzelne Teil der Web Anwendung des VCR.NET Recording Service festgelegt.
                <form>
                    {this.getPlanHelp()}
                    <Field page={this.props.uvm} label={`${this.props.uvm.planDays.text}:`} >
                        <JMSLib.ReactUi.EditNumber uvm={this.props.uvm.planDays} chars={5} />
                    </Field>
                    {this.getSourceHelp()}
                    <div className="vcrnet-settings-field">
                        Inhalte der Senderlisten bei Aufzeichnungen:
                        <JMSLib.ReactUi.SingleSelect uvm={this.props.uvm.sourceType} />
                        <JMSLib.ReactUi.SingleSelect uvm={this.props.uvm.encryption} />
                    </div>
                    <Field page={this.props.uvm} label={`${this.props.uvm.maxFavorites.text}:`} >
                        <JMSLib.ReactUi.EditNumber uvm={this.props.uvm.maxFavorites} chars={5} />
                    </Field>
                    <div className="vcrnet-settings-field">
                        Bevorzugte Zusatzoptionen für Aufzeichnungen:
                        <JMSLib.ReactUi.EditBoolean uvm={this.props.uvm.dolby} />
                        <JMSLib.ReactUi.EditBoolean uvm={this.props.uvm.allAudio} />
                        <JMSLib.ReactUi.EditBoolean uvm={this.props.uvm.ttx} />
                        <JMSLib.ReactUi.EditBoolean uvm={this.props.uvm.subs} />
                    </div>
                    {this.getSleepHelp()}
                    <div className="vcrnet-settings-field">
                        <JMSLib.ReactUi.EditBoolean uvm={this.props.uvm.noSleep} />
                    </div>
                    {this.getGuideHelp()}
                    <Field page={this.props.uvm} label={`${this.props.uvm.guideRows.text}:`} >
                        <JMSLib.ReactUi.EditNumber uvm={this.props.uvm.guideRows} chars={5} />
                    </Field>
                    <Field page={this.props.uvm} label={`${this.props.uvm.preGuide.text}:`} >
                        <JMSLib.ReactUi.EditNumber uvm={this.props.uvm.preGuide} chars={5} />
                    </Field>
                    <Field page={this.props.uvm} label={`${this.props.uvm.postGuide.text}:`} >
                        <JMSLib.ReactUi.EditNumber uvm={this.props.uvm.postGuide} chars={5} />
                    </Field>
                    <div className="vcrnet-settings-field">
                        <JMSLib.ReactUi.EditBoolean uvm={this.props.uvm.backToGuide} />
                    </div>
                    <div><JMSLib.ReactUi.ButtonCommand uvm={this.props.uvm.update} /></div>
                </form>
            </div >;
        }

        // Hilfe zum Aufzeichnungsplan.
        private getPlanHelp(): JSX.Element {
            return <InlineHelp title="Erläuterungen">
                Im Aufzeichnungsplan<JMSLib.ReactUi.InternalLink view={this.props.uvm.application.planPage.route} pict="plan" /> werden
                die Daten aller geplanten Aufzeichnungen in einer Liste angezeigt. Um eine gewisse Übersichtlichkeit
                zu erhalten wird allerdings nur eine begrenzte Anzahl von Aufzeichnungen auf einmal angezeigt. Die im
                Folgenden angezeigte Zahl legt fest, wie viele Tage pro Seite im Aufzeichnungsplan berücksichtigt
                werden sollen.
            </InlineHelp>;
        }

        // Hilfe zur Auswahl der Quellen.
        private getSourceHelp(): JSX.Element {
            return <InlineHelp title="Erläuterungen">
                Bei der Programmierung neuer Aufzeichnungen können eine ganze Reihe von Einstellungen verwendet werden,
                die bei der Auswahl der zu verwendenden Quelle
                helfen.<HelpLink topic="sourcechooser" page={this.props.uvm} /> Hier wird die gewünschte Vorbelegung
                dieser Einstellungen festgelegt.
            </InlineHelp>;
        }

        // Hilfe zum Schalfzustand.
        private getSleepHelp(): JSX.Element {
            return <InlineHelp title="Erläuterungen">
                Wird eine aktive
                Aufzeichnung<JMSLib.ReactUi.InternalLink view={this.props.uvm.application.devicesPage.route} pict="devices" /> vorzeitig
                beendet, so wird der VCR.NET Recording Service prüfen, ob der Rechner in den Schlafzustand versetzt werden
                soll.<HelpLink topic="hibernation" page={this.props.uvm} /> Diese Verhalten kann pro Abbruch gesondert
                deaktiviert werden und die folgende Einstellung legt das bevorzugte Verhalten fest.
            </InlineHelp>;
        }

        // Hilfe zur Programmzeitschrift.
        private getGuideHelp(): JSX.Element {
            return <InlineHelp title="Erläuterungen">
                Wird eine neue Aufzeichnung aus der
                Programmzeitschrift<HelpLink topic="epg" page={this.props.uvm} /> heraus angelegt, so können hier vor
                allem die Vor- und Nachlaufzeiten der Aufzeichnung relativ zu den exakten Daten aus der Programmzeitschrift
                festgelegt werden. Es handelt sich allerdings nur um Vorschlagswerte, die in den Daten der neuen Aufzeichnung
                jederzeit korrigiert werden können. Da Sendungen in den seltensten Fällen genau wie von den Sendeanstalten
                geplant beginnen, macht der Einsatz dieser Zeiten im Allgemeinen sehr viel Sinn - selbst wenn wie bei der
                Tagesschau zumindest der Startzeitpunkt sehr exakt festliegt kann es immer noch sein, dass der Rechner, auf
                dem der VCR.NET Recording Service ausgeführt wird, nicht mit dieser genauen Zeit synchronisiert ist.
                <br />
                <br />
                Hier wird auch festgelegt, wie viele Einträge die
                Programmzeitschrift<JMSLib.ReactUi.InternalLink view={this.props.uvm.application.guidePage.route} pict="guide" /> pro
                Seite anzeigen soll. Zu große Werte erhöhen nicht nur die Zeit zur Anzeige einer Seite sondern sorgen oft
                auch dafür, dass nicht alle Sendungen einer Seite auf einen Blick erfasst werden können.
                <br />
                <br />
                Wenn die Programmierung einer Aufzeichnung aus der Programmzeitschrift abgeschlossen ist wird normalerweise zum
                Aufzeichnungsplan<JMSLib.ReactUi.InternalLink view={this.props.uvm.application.planPage.route} pict="plan" /> gewechselt.
                Ist die unten als letztes angebotene Einstellung aktiviert wird in diesem Fall erneut die Programmzeitschrift aufgerufen.
            </InlineHelp>;
        }
    }

}
