/// <reference path="helpComponent.ts" />

namespace VCRNETClient.Ui.HelpPages {
    export class SourceChooser extends HelpComponent {
        readonly title = "Quellen (Sender)";

        render(page: App.IPage): JSX.Element {
            return <div>
                Jedes DVB Gerät kann bestimmte Quellen (Radio- und Fernsehsender) empfangen. Die
                Anzahl dieser Quellen kann erheblich sei: für Astra 1 auf 19.2° Ost sind es mehr als
                1.000 verschiedene Quellen, aus denen ausgewählt werden kann. Der VCR.NET Recording
                Service kennt zwar kein Favoritensystem wie es bei Fernsehern üblich ist, bietet
                aber einige andere Möglichkeiten an, die bei der Auswahl der benötigten Quelle helfen
                sollten.
                <ScreenShot description="Quelle auswählen" name="FAQ/sources" />
                Abhängig von der zweiten Auswahlliste wird die Liste der Quellen links daneben wie
                folgt beschränkt - in der Reihenfolge der Einträge der im Bild geöffnet gezeigten
                Liste:
                <br />
                <ul>
                    <li>Interessant ist die Voreinstellung der ersten Auswahlliste direkt neben dem Namen der Quelle. Bei
                        dieser Auswahl werden die zuletzt verwendeten Quellen in alphabetischer Anordnung
                        angezeigt - jeder Anwender kann individuell
                        festlegen<JMSLib.ReactUi.InternalLink view={page.application.settingsPage.route} pict="settings" />,
                        wie viele Quellen die Liste maximal enthalten darf. Nach einiger gewissen Nutzungszeit
                        des VCR.NET Recording Service zeigt sich im Allgemeinen, dass diese Liste fast immer
                        die gewünschte Quelle enthält. Ist die Liste leer, so werden automatisch alle möglichen
                        Quellen zur Auswahl angeboten.</li>
                    <li>Da meistens der Name der gewünschten Quelle bekannt ist kann alternativ der Einstieg
                        auch über den ersten Buchstaben des Namens der Quelle erfolgen. Der Übersichtlichkeit
                        halber erfolgt die Unterteilung nicht auf Basis einzelner Buchstaben sondern auf
                        einer Gruppierung von im Allgemeinen drei aufeinanderfolgenden Buchstaben.</li>
                    <li>Es werden im Prinzip alle verfügbaren Quellen angezeigt. Mit den beiden Auswahllisten
                        rechts ist es dann noch möglich, sich auf Fernseh- oder Radiosender sowie
                        verschlüsselte respektive unverschlüsselte Quellen zu beschränken.</li>
                </ul>
                Die Liste der zuletzt verwendeten Quellen hat bei der Auswahl eine weitere Besonderheit:
                in diesem Auswahlmodus werden keine weiteren Auswahllisten zur Einschränkung der
                angezeigten Quellen angeboten. Das macht im Allgemeinen auch Sinn, da die Anzahl der
                Quellen in der Liste beschränkt ist.
                <br />
                <br />
                Interessant wird es aber, wenn eine andere Auswahlmöglichkeit verwendet wird. Hier
                erscheinen nun zwei weitere Auswahllisten, wie oben im Bild auch gezeigt:
                <ul>
                    <li>Es ist möglich, nur Radio- oder nur Fernsehsender zur Auswahl anzubieten - oder
                        einfach alle Quellen, aber im Allgemeinen weiß man ja, was man programmieren möchte.
                        Auch diese Voreinstellung ist individuell konfigurierbar.</li>
                    <li>Die Quellen können auf verschlüsselte und unverschlüsselte Quellen beschränkt werden
                        - die Beschränkung kann auch jederzeit wiederrufen werden. Die Voreinstellung kann
                        jeder Anwender für sich frei wählen.</li>
                </ul>
                Wird in einer Aufzeichnung keine Quelle angegeben, so wird die Quelle
                des <JMSLib.ReactUi.InternalLink view={`${page.route};jobsandschedules`}>Auftrags</JMSLib.ReactUi.InternalLink> verwendet. 
                Ist auch diese nicht definiert, so kann die Programmierung
                nicht erfolgreich abgeschlossen werden.
            </div>;
        }
    }
}
