/// <reference path="../lib.react/reactUi.tsx" />

namespace VCRNETClient.Ui {

    // React.Js Komponente zur Pflege einer Aufzeichnung.
    export class Edit extends JMSLib.ReactUi.ComponentWithSite<App.IEditPage> {

        // Oberflächenelemente erstellen.
        render(): JSX.Element {
            const schedule = this.props.uvm.schedule;

            return <div className="vcrnet-edit">
                <div>
                    Mit diesem Formular werden alle Daten erfasst, die für die
                    Ausführung einer Aufzeichnung benötigt werden. Im oberen
                    Bereich finden sich die Einstellungen des
                    Auftrags<HelpLink page={this.props.uvm} topic="jobsandschedules" />,
                    die allen Aufzeichnungen des Auftrags gemeinsam sind. In der
                    Mitte werden die eigentlichen Aufzeichnungsdaten festgelegt.
                    Der untere Bereich ist für sich wiederholende Aufzeichnungen
                    aktiv, wenn für einzelne Tage Ausnahmeregeln definiert wurden.
                </div>
                {this.renderJobHelp()}
                <form><JobData uvm={this.props.uvm.job} /></form>
                {this.renderScheduleHelp()}
                <form><ScheduleData uvm={schedule} /></form>
                {schedule.hasExceptions && this.renderExceptionHelp()}
                {schedule.hasExceptions && <form>
                    <fieldset className="vcrnet-edit-exception">
                        <legend>Aktive Ausnahmeregeln</legend>
                        <table className="vcrnet-table">
                            <thead>
                                <tr>
                                    <td>Aktiv</td>
                                    <td>Datum</td>
                                    <td>Startverschiebung</td>
                                    <td>Laufzeitanpassung</td>
                                </tr>
                            </thead>
                            <tbody>{schedule.exceptions.map((e, index) => <EditException key={index} uvm={e} />)}</tbody>
                        </table>
                    </fieldset>
                </form>}
                {this.renderButtonHelp()}
                <div>
                    <JMSLib.ReactUi.ButtonCommand uvm={this.props.uvm.save} />
                    <JMSLib.ReactUi.ButtonCommand uvm={this.props.uvm.del} />
                </div>
            </div>;
        }

        // Hilfe zu Aufzeichnungsdaten.
        private renderScheduleHelp(): JSX.Element {
            return <InlineHelp title="Erläuterungen zu den Daten einer Aufzeichnung">
                <div>
                    Einem Auftrag können beliebig viele Aufzeichnungen zugeordnet sein, die sich alle die
                    gemeinsamen Einstellungen des Auftrags teilen. Darüber hinaus können pro Aufzeichnung
                    folgende Angaben gemacht werden:
                    <table>
                        <tbody>
                            <tr>
                                <td>Name</td>
                            </tr>
                            <tr>
                                <td>die Angabe des Namens einer Aufzeichnung ist optional und wird wenn vorhanden im Aufzeichnungsplan hinter dem Namen des Auftrags angezeigt</td>
                            </tr>
                            <tr>
                                <td>Quelle</td>
                            </tr>
                            <tr>
                                <td>ist keine Quelle zur Aufzeichnung ausgewählt, so wird die für den Auftrag konfigurierte Quelle verwendet - ist für einen Auftrag keine Quelle
                                angegeben, so muss jede Aufzeichnung eine solche festlegen</td>
                            </tr>
                            <tr>
                                <td>Aufzeichnungsoptionen</td>
                            </tr>
                            <tr>
                                <td>ist eine Quelle festgelegt, so können diese Optionen genau wie bei dem Auftrag eingestellt werden</td>
                            </tr>
                            <tr>
                                <td>Datum der ersten Aufzeichnung</td>
                            </tr>
                            <tr>
                                <td>das Datum, ab dem die Aufzeichnung erstmalig ausgeführt werden soll</td>
                            </tr>
                            <tr>
                                <td>Aufzeichnungszeitraum</td>
                            </tr>
                            <tr>
                                <td>die Uhrzeiten, zwischen denen die Aufzeichnung nach Möglichkeit erfolgen soll - liegt der Endzeitpunkt vor dem Beginn so wird
                                davon ausgegangen, dass das Ende auf den Folgetag fällt</td>
                            </tr>
                            <tr>
                                <td>Wiederholung</td>
                            </tr>
                            <tr>
                                <td>optional ist es möglich, die Wochentage anzugeben, an denen die Aufzeichnung ausgeführt werden soll - ist dies geschehen, so kann
                                zusätzlich das Datum der letzten Ausführung eingegeben werden</td>
                            </tr>
                        </tbody>
                    </table>
                </div>
            </InlineHelp>;
        }

        // Hilfe zu den Auftragsdaten.
        private renderJobHelp(): JSX.Element {
            return <InlineHelp title="Erläuterungen zu den Daten eines Auftrags">
                <div>
                    Für einen Auftrag gibt es erst einmal die Informationen, die allen Aufzeichnungen gemeinsam sind,
                        die dem Auftrag zugeordnet sind.
                        <table>
                        <tbody>
                            <tr>
                                <td>Geräteprofil</td>
                            </tr>
                            <tr>
                                <td>die zur Auswahl angebotenen Quellen beschränken sich auf die Quellen, die über
                                    dieses Gerät aufgezeichnet werden können</td>
                            </tr>
                            <tr>
                                <td>Verwendung des Geräteprofils</td>
                            </tr>
                            <tr>
                                <td>wird diese Option aktiviert, so erfolgt die Aufzeichnung auf dem ausgewählten Gerät -
                                    ansonsten ist es möglich, dass zum Beispiel bei zeitlichen Kollisionen zur Aufzeichnung
                                    auf ein anderes Gerät ausgewichen wird</td>
                            </tr>
                            <tr>
                                <td>Name</td>
                            </tr>
                            <tr>
                                <td>jeder Auftrag muss einen Namen haben, unter dem seine Aufzeichnungen im
                                    Aufzeichnungsplan erscheinen</td>
                            </tr>
                            <tr>
                                <td>Aufzeichnungsverzeichnis</td>
                            </tr>
                            <tr>
                                <td>optional kann hier ein alteratives Aufzeichnungsverzeichnis ausgewählt werden,
                                    wobei allerdings nur die konfigurierten Verzeichnisse erlaubt sind</td>
                            </tr>
                        </tbody>
                    </table>
                    Ergänzend können eine Quelle und Aufzeichnungsoptionen festlegt werden. Diese Einstellungen kommen
                        allerdings nur dann zum Einsatz, wenn für eine Aufzeichnung keine Quelle explizit festgelegt wurde.
                        Als Aufzeichnungsoptionen stehen folgende Einstellungen zur Verfügung:
                        <table>
                        <tbody>
                            <tr>
                                <td>Dolby Digital</td>
                            </tr>
                            <tr>
                                <td>sollte die Quelle Dolby Digital Spuren anbieten, so wird zusätzlich zum üblichen
                                    Stereoton die primäre Dolby Spur mit aufgezeichnet</td>
                            </tr>
                            <tr>
                                <td>Alle Sprachen</td>
                            </tr>
                            <tr>
                                <td>es werden alle Tonspuren aufgezeichnet - und nicht nur die jeweils primäre Sprache</td>
                            </tr>
                            <tr>
                                <td>Videotext</td>
                            </tr>
                            <tr>
                                <td>der Videotext wird mit in die Aufzeichnungsdatei übernommen, sofern die Quelle einen
                                    solchen ausstrahlt - daraus können später dann je nach Quelle Untertitel extrahiert werden</td>
                            </tr>
                            <tr>
                                <td>Untertitel</td>
                            </tr>
                            <tr>
                                <td>wenn die Quelle DVB Untertitel anbietet, so werden diese vollständig mit aufgezeichnet</td>
                            </tr>
                        </tbody>
                    </table>
                </div>
            </InlineHelp>;
        }

        // Hilfe zu Ausnahmeregeln.
        private renderExceptionHelp(): JSX.Element {
            return <InlineHelp title="Erläuterungen zu den Ausnahmeregelungen">
                <div>
                    Es werden alle noch aktiven Ausnahmeregeln angezeigt. Änderungen an den Regeln selbst können an
                    dieser Stelle allerdings nicht vorgenommen werden, dazu dient alleine der Aufzeichnungsplan. Es
                    ist allerdings möglich, über die erste Spalte eine aktive Ausnahmeregel gänzlich zu entfernen. Die
                    Änderung wird erst beim Speichern des Formulars gültig.
                </div>
            </InlineHelp>;
        }

        // Hilfe zur Übernahme von Änderungen.
        private renderButtonHelp(): JSX.Element {
            return <InlineHelp title="Erläuterungen zur Bedienung">
                <div>
                    Beim Löschen wird nur die angezeigte Aufzeichnung unwiederbringlich aus dem Auftrag entfernt. Handelt es sich um
                    die letzte Aufzeichnung des Auftrags, so wird auch dieser endgültig aus der Verwaltung des VCR.NET Recording
                    Service entfernt. Da das Löschen in beiden Fällen eine kritische Änderung auslöst, muss die Schaltfläche
                    zweimal betätigt werden - nach dem ersten Aktivieren ändert sich lediglich die Farbe und signalisiert
                    damit, dass auf eine Bestätigung gewartet wird. Während des Neuanlegens einer Aufzeichnung wird die
                    Schaltfläche zum Löschen nicht angeboten.
                </div>
            </InlineHelp>;
        }
    }
}
