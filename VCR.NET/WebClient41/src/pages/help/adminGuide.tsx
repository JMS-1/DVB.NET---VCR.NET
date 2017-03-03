/// <reference path="helpComponent.ts" />

namespace VCRNETClient.Ui.HelpPages {
    export class AdminProgramGuide extends HelpComponent {
        readonly title = "Konfiguration der Programmzeitschrift";

        render(page: App.IPage): JSX.Element {
            return <div>
                Damit der VCR.NET Recording Service eine <JMSLib.ReactUi.InternalLink view={`${page.route};epg`}>Programmzeitschrift</JMSLib.ReactUi.InternalLink> zur
                Programmierung neuer Aufzeichnungen zur Verfügung stellen kann muss erst einmal
                festgelegt werden, mit welchem Inhalt diese zu füllen ist. Im Administrationsbereich
                der Web Oberfläche gibt es dazu eine gesonderten
                Seite<JMSLib.ReactUi.InternalLink view={`${page.application.adminPage.route};guide`} pict="admin" />.
                Damit die Programmzeitschrift überhaupt erstellt wird, muss erst einmal die Sammlung 
                der Informationen an sich aktiviert werden.
                <ScreenShot description="Sammlung aktivieren" name="FAQ/epgonoff" />
                Die wesentliche Information ist dann, für welche Quellen der VCR.NET Recording Service
                versuchen soll Sendungsdaten zu ermitteln. Bedenkt man, dass alleine auf Astra 1 mehr 
                als 1,000 Radio- und Fernsehsender angeboten werden, so macht eine Einschränkungen
                auf die eigenen Lieblingssender durchaus Sinn. Nach Vorauswahl eines der vom VCR.NET
                Recording Service verwendeten DVB Geräte können mit
                den <JMSLib.ReactUi.InternalLink view={`${page.route};sourcechooser`}>üblichen
                Methoden</JMSLib.ReactUi.InternalLink> Quellen ausgewählt und zur Liste hinzugefügt 
                werden - natürlich können Quellen auch wieder aus der Liste entfernt werden.
                <ScreenShot description="Quellen konfigurieren" name="FAQ/epgsources" />
                Sind unter den Quellen auch englische Sender von Astra 2, so muss die <em>FreeSat
                UK</em> Option aktiviert werden. Die Programmzeitschrift der BBC, ITV et al Sender 
                wird auf einem anderen Wege übertragen als bei uns üblich. Ohne die Aktivierung der 
                Option würden immer nur die Sendungsbeschreibungen in der Programmzeitschrift des 
                VCR.NET Recording Service landen, die zum Zeitpunkt der Sammlung entweder gerade 
                liefen oder als nächstes gesendet würden. Diese Information ist für eine Programmierung
                fast immer unbrauchbar.
                <br />
                <br />
                Nun muss dem VCR.NET Recording Service nur noch mitgeteilt werden, wann die Programmzeitschrift
                zu aktualisieren ist. Dazu können bis zu die Stunden pro Tag angegeben werden, zu dem 
                eine entsprechende Aktivität gestartet werden soll - wie bei normalen Aufzeichnung
                wird der Rechner dazu wenn notwendig aus
                dem <JMSLib.ReactUi.InternalLink view={`${page.route};hibernation`}>Schlafzustand</JMSLib.ReactUi.InternalLink> aufgeweckt.
                Damit diese Sammlung auch in der Planung der Aufzeichnungen berücksichtigt werden kann, 
                muss auch immer eine maximale Laufzeit für die Aktualisierung angegeben werden. Ist 
                diese zu kurz eingestellt, so wird der VCR.NET Recording Service die Aktualisierung 
                vorzeitig beenden und die Programmzeitschrift ist unvollständig. Der genaue Wert ist 
                abhängig von den ausgewählten Quellen und kann nur individuell selbst bestimmt werden. 
                Ist die Aktualisierung doch vorzeitig abgeschlossen, so wird auch die
                Aktivität <JMSLib.ReactUi.InternalLink view={page.application.devicesPage.route} pict="devices" /> auf
                dem DVB Gerät beendet. Mit Hilfe
                der <JMSLib.ReactUi.InternalLink view={`${page.route};log`}>Protokollliste</JMSLib.ReactUi.InternalLink> lässt
                sich dann leicht feststellen, was ein typischer Wert für die Laufzeit ist.
                <ScreenShot description="Aktualisierungszeitpunkte" name="FAQ/epgtimes" />
                In manchen Fällen ist die Einstellung von vier festen Uhrzeiten unzureichend, daher
                bietet der VCR.NET Recording Service zwei weitere Konfigurationswerte an. Die <em>Wartezeit
                zwischen zwei Aktualisierungen</em> stellt sicher, dass zwischen zwei Aktualisierungen
                mindestens die angegebene Anzahl von Stunden verstreicht. Umgekehrt erlaubt es
                die <em>Latenzzeit</em> das Vorziehen einer Aktualisierung, wenn der Rechner sowieso
                gerade für eine Aufzeichnung oder Sonderaufgabe aktiviert wurde und ansonsten in
                den Schlafzustand versetzt würde.
                <br />
                <br />
                Ergänzend ist es jederzeit möglich, eine Aktualisierung
                manuell <JMSLib.ReactUi.InternalLink view={page.application.homePage.route} pict="home" /> anzufordern.
                Der VCR.NET Recording Service wird dann zum nächstmöglichen Zeitpunkt die gewünschte Aktion 
                ausführen. Hier gilt wie grundsätzlich für alle Sonderaufgaben: ist eine programmiert 
                Aufzeichnung aktiv oder steht kurz bevor, so wird die Aktualisierung verschoben, bis das 
                verwendete DVB Gerät sicher für die konfigurierte maximale Laufzeit unbenutzt ist.
            </div>;
        }
    }
}
