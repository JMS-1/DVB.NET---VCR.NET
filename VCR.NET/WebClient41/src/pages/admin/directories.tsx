/// <reference path="section.tsx" />

namespace VCRNETClient.Ui {

    // React.js Komponente zur Konfiguration der Aufzeichnungsverzeichnisse.
    export class AdminDirectories extends AdminSection<App.Admin.IAdminDirectoriesPage>{

        // Das zugehörige Ui View Model.
        static get uvm(): IAdminSectionFactory<App.Admin.IAdminDirectoriesPage> {
            return App.Admin.DirectoriesSection;
        }

        // Die Überschrift für diesen Bereich.
        protected readonly title = `Aufzeichnungsverzeichnisse und Dateinamen`;

        // Oberflächenelemente erzeugen.
        protected renderSection(): JSX.Element {
            return <div className="vcrnet-admin-directories">
                Da Aufzeichnungsdateien von erheblicher Größe sein können erlaubt es der VCR.NET Recording Service für
                diesen Zweck separate Verzeichnisse festzugelegen. Dadurch können dann zum Beispiel gesonderte Festplatten
                oder Partitionen für Aufzeichnungen genutzt werden. Zusätzlich kann hier definiert werden, wie sich die
                Namen von Aufzeichnungsdateien aus den Daten einer Aufzeichnung zusammensetzen sollen.
                {this.getFolderHelp()}
                <div>
                    <JMSLib.ReactUi.MultiSelect uvm={this.props.uvm.directories} items={10} />
                    <div><JMSLib.ReactUi.ButtonCommand uvm={this.props.uvm.remove} /></div>
                </div>
                {this.getAddHelp()}
                <form>
                    <fieldset>
                        <legend>Neues Verzeichnis</legend>
                        <Field page={this.props.uvm.page} label={`${this.props.uvm.share.text}:`}>
                            <JMSLib.ReactUi.EditText uvm={this.props.uvm.share} chars={80} />
                        </Field>
                        {this.props.uvm.showBrowse && <div>
                            <i>oder</i>
                            <Field page={this.props.uvm.page} label={`${this.props.uvm.browse.text}:`}>
                                <JMSLib.ReactUi.SingleSelect uvm={this.props.uvm.browse} />
                            </Field>
                        </div>}
                        <div>
                            {this.props.uvm.showBrowse && <JMSLib.ReactUi.ButtonCommand uvm={this.props.uvm.parent} />}
                            <JMSLib.ReactUi.ButtonCommand uvm={this.props.uvm.add} />
                        </div>
                    </fieldset>
                    <Field page={this.props.uvm.page} label={`${this.props.uvm.pattern.text}:`}>
                        <JMSLib.ReactUi.EditText uvm={this.props.uvm.pattern} chars={60} />
                    </Field>
                </form>
                {this.getPatternHelp()}
            </div>;
        }

        // Hilfe für die Auswahl der Verzeichnisse.
        private getFolderHelp(): JSX.Element {
            return <InlineHelp title="Erläuterungen zur Bedienung">
                Im oberen Teil wird die Liste der Verzeichnisse angezeigt, die zurzeit als Aufzeichnunsverzeichnisse
                beim Anlegen eines neuen Auftrags ausgewählt werden können. Durch Auswählen eines oder mehrere
                Verzeichnisse können diese aus der Liste entfernt werden und stehen nicht mehr als Ziel für
                Aufzeichnungen zur Verfügung. Man beachte, dass dies auch bereits früher programmierte Aufträge
                betrifft: sollten diese ein Aufzeichnungsverzeichnis verwenden, dass nicht mehr zugelassen ist,
                so kann die Aufzeichnung nicht durchgeführt werden.
            </InlineHelp>;
        }

        // Hilfe zum Hinzufügen neuer Verzeichnisse.
        private getAddHelp(): JSX.Element {
            return <InlineHelp title="Erläuterungen zur Bedienung">
                Die Liste der Aufzeichnungsverzeichnisse kann durch Auswahl eines Verzeichnisses oder durch
                direkte Eingabe eines Netzwerkpfades erweitert werden. Die Navigation durch die Verzeichnisstruktur
                des Rechners, auf dem der VCR.NET Recording Service läuft, erfolgt unmittelbar durch Auswahl
                eines Unterverzeichnisses in der angebotenen Liste. Mit der Schaltfläche direkt darunter kann
                auf das jeweils übergeordnete Verzeichnis zurück gewechselt werden.
                <br />
                <br />
                Wird das Hinzufügen eines Verzeichnisses gewählt, so wird die Eingabe eines Netzwerkverzeichnisses
                bevorzugt verwendet. Dabei wird dann zusätzlich geprüft, ob das eingegebene Verzeichnis auch vom
                VCR.NET Dienst angesprochen werden kann. Ist dies der Fall, so wird es in die Liste der
                Aufzeichnungsverzeichnisse übernommen und ansonsten ein Fehler angezeigt. Ist die Eingabe leer,
                so wird das ausgewählte Verzeichnis des Servers übernommen.
                <br />
                <br />
                Die Übernahme der Änderungen erfolgt erst, wenn dies durch Auswahl der entsprechenden Schaltfläche
                explizit angefordert wird. Ist zu diesem Zeitpunkt die Liste der Aufzeichnungsverzeichnisse leer,
                so wird ein Verzeichnis im Installationsverzeichnis des VCR.NET Recordings Service verwendet - es
                wird aber dringend empfohlen, immer mindestens ein Aufzeichnungsverzeichnis festzulegen. Änderungen
                an den Aufzeichnungsverzeichnissen oder dem Namensmuster für Aufzeichnungsdateien erfordern keinen
                Neustart des VCR.NET Dienstes.
            </InlineHelp>;
        }

        // Hilfe zum Dateinamenmuster,
        private getPatternHelp(): JSX.Element {
            return <InlineHelp title="Mögliche Platzhalter in den Dateinamen">
                Für jede Aufzeichnung wird im Allgemeinen genau eine Aufzeichnungsdatei
                erstellt.<HelpLink topic="numberoffiles" page={this.props.uvm.page} /> Der VCR.NET Recording Service
                nutzt die Daten der Programmierung sowie weitere Umgebungsparameter wie die aktuelle Uhrzeit zur
                Erzeugung eines Dateinamens. Die Regeln zur Komposition dieses Namens können frei konfiguriert werden,
                es wird allerdings dringend empfohlen ein Schema zu wählen, das eine Eindeutigkeit der Dateinamen
                garantiert - ohne Einbeziehung des Zeitpunktes der Aufzeichnung kann es etwa leicht dazu kommen,
                dass eine Aufzeichnung die Aufzeichnungsdatei eine frühere Ausführung überschreibt.
                <br />
                <br />
                <table className="vcrnet-admin-pattern">
                    <thead>
                        <tr>
                            <td>Regel</td>
                            <td>&nbsp;</td>
                            <td>Bedeutung (in Klammern einige Beispiele)</td>
                        </tr>
                    </thead>
                    <tbody>
                        <tr>
                            <td>%Job%</td>
                            <td>&nbsp;</td>
                            <td>Name des Auftrags zur Aufzeichnung (James Bond)</td>
                        </tr>
                        <tr>
                            <td>%Schedule%</td>
                            <td>&nbsp;</td>
                            <td>Name der Aufzeichnung (Octupussy)</td>
                        </tr>
                        <tr>
                            <td>%Start%</td>
                            <td>&nbsp;</td>
                            <td>Beginn der Aufzeichnung (06-12-2006 13-43-22)</td>
                        </tr>
                        <tr>
                            <td>%Duration%</td>
                            <td>&nbsp;</td>
                            <td>Dauer der Aufzeichnung in Minuten</td>
                        </tr>
                        <tr>
                            <td>%AllAudio%</td>
                            <td>&nbsp;</td>
                            <td><em>MultiLang</em>, wenn alle MP2 Sprachen mit aufgezeichnet wurden, sonst leer</td>
                        </tr>
                        <tr>
                            <td>%AC3%</td>
                            <td>&nbsp;</td>
                            <td><em>AC3</em>, wenn der Dolby Digital Ton mit aufgezeichnet wurde, sonst leer</td>
                        </tr>
                        <tr>
                            <td>%TTX%</td>
                            <td>&nbsp;</td>
                            <td><em>TTX</em>, wenn der Videotext mit aufgezeichnet wurde, sonst leer</td>
                        </tr>
                        <tr>
                            <td>%DVBSub%</td>
                            <td>&nbsp;</td>
                            <td><i>DVBSUB</i>, wenn DVB Untertitel mit aufgezeichnet wurden, sonst leer</td>
                        </tr>
                        <tr>
                            <td>%StationFull%</td>
                            <td>&nbsp;</td>
                            <td>Vollständiger, normalerweise eindeutiger Name des Senders (RTL2 [RTL World])</td>
                        </tr>
                        <tr>
                            <td>%Transponder%</td>
                            <td>&nbsp;</td>
                            <td>Name des Anbieters (RTL World)</td>
                        </tr>
                        <tr>
                            <td>%Station%</td>
                            <td>&nbsp;</td>
                            <td>Name des Senders (RTL2)</td>
                        </tr>
                        <tr>
                            <td>%Profile%</td>
                            <td>&nbsp;</td>
                            <td>Name des verwendeten DVB.NET Geräteprofils (Nexus)</td>
                        </tr>
                        <tr>
                            <td>%Audio%</td>
                            <td>&nbsp;</td>
                            <td>Optionaler Name der primären Tonspur (Deutsch [402])</td>
                        </tr>
                        <tr>
                            <td>%SortableStart%</td>
                            <td>&nbsp;</td>
                            <td>Beginn der Aufzeichnung (2006-12-06 13-43-22)</td>
                        </tr>
                        <tr>
                            <td>%SortableDate%</td>
                            <td>&nbsp;</td>
                            <td>Datum, an dem die Aufzeichnung begonnen hat (2006-12-06)</td>
                        </tr>
                        <tr>
                            <td>%Date%</td>
                            <td>&nbsp;</td>
                            <td>Datum, an dem die Aufzeichnung begonnen hat (06-12-2006)</td>
                        </tr>
                        <tr>
                            <td>%Time%</td>
                            <td>&nbsp;</td>
                            <td>Uhrzeit zu der die Aufzeichnung begonnen hat (13-43-22)</td>
                        </tr>
                        <tr>
                            <td>%Year%</td>
                            <td>&nbsp;</td>
                            <td>Jahr (vierstellig), in dem die Aufzeichnung begonnen hat (2006)</td>
                        </tr>
                        <tr>
                            <td>%Minute%</td>
                            <td>&nbsp;</td>
                            <td>Minute (zweistellig), in der die Aufzeichnung begonnen hat (43)</td>
                        </tr>
                        <tr>
                            <td>%Second%</td>
                            <td>&nbsp;</td>
                            <td>Sekunde (zweistellig), zu der die Aufzeichnung begonnen hat (22)</td>
                        </tr>
                        <tr>
                            <td>%Month%</td>
                            <td>&nbsp;</td>
                            <td>Monat (zweistellig), in dem die Aufzeichnung begonnen hat (12)</td>
                        </tr>
                        <tr>
                            <td>%Hour%</td>
                            <td>&nbsp;</td>
                            <td>Stunde (zweistellig), in der die Aufzeichnung begonnen hat (13)</td>
                        </tr>
                        <tr>
                            <td>%Day%</td>
                            <td>&nbsp;</td>
                            <td>Tag im Monat (zweistellig), an dem die Aufzeichnung begonnen hat (06)</td>
                        </tr>
                    </tbody>
                </table>
            </InlineHelp>;
        }
    }

}
