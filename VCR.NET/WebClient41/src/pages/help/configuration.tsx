/// <reference path="helpComponent.ts" />

namespace VCRNETClient.Ui.HelpPages {
    export class Configuration extends HelpComponent {
        readonly title = "Sonstige Konfiguration";

        render(page: App.IPage): JSX.Element {
            return <div>
                Der VCR.NET Recording Service bietet auch zusätzliche Konfigurationsmöglichkeiten
                an<JMSLib.ReactUi.InternalLink view={page.application.adminPage.route} pict="admin" />,
                die hier kurz vorgestellt werden sollen.
                <br />
                <br />
                Nach der Installation darf jeder Benutzer nicht nur Aufzeichnungen programmieren
                sondern auch sämtliche Konfigurationsoptionen nutzen. Mit Hilfe von Windows Benutzergruppen
                ist es aber möglich
                festzulegen<JMSLib.ReactUi.InternalLink view={`${page.application.adminPage.route};security`} pict="admin" />,
                welche Anwender Aufzeichnungen programmieren dürfen und welchen Anwendern zusätzlich
                die Konfiguration des VCR.NET Recording Service gestattet wird.
                <br />
                <br />
                Es empfiehlt sich unbedingt mindestens ein Verzeichnis zu
                bestimmen<JMSLib.ReactUi.InternalLink view={`${page.application.adminPage.route};directories`} pict="admin" />,
                in dem die Aufzeichnungsdateien abgelegt werden
                - nach der Installation wird ein Unterverzeichnis des Installationsverzeichnisses
                verwendet, was fast immer unerwünscht ist. Es können mehrere Verzeichnisse angegeben
                werden und mit der Auswahl eines Verzeichnisses werden auch alle Unterverzeichnisse
                zur Ablage freigeschaltet. Wenn keine gesonderte Auswahl bei
                der <JMSLib.ReactUi.InternalLink view={`${page.route};jobsandschedules`}>Programmierung einer Aufzeichnung</JMSLib.ReactUi.InternalLink> erfolgt,
                so verwendet der VCR.NET Recording Service immer das erste angegebene Verzeichnis.
                <br />
                <br />
                Sind auf dem Rechner mehrere DVB Geräte vorhanden, so kann es dem VCR.NET Recording Service gestattet
                werden<JMSLib.ReactUi.InternalLink view={`${page.application.adminPage.route};devices`} pict="admin" />, eine
                beliebige Anzahl davon zu verwenden. Dazu werden die entsprechenden DVB.NET Geräteprofile
                in einer Auswahlliste markiert und eines davon als Voreinstellung ausgezeichnet.<br />
                <br />
                Die Web Oberfläche des VCR.NET Recording Service ist üblicherweise über den Standard Web Server
                Port 80 zu erreichen. Dieser kann an die eigenen Bedürfnissen angepasst
                werden<JMSLib.ReactUi.InternalLink view={`${page.application.adminPage.route};other`} pict="admin" /> -
                man beachte allerdings, dass eine Umschaltung einen erneuten Aufruf des Browsers mit dem veränderten Port erforderlich macht.<br />
                <br />
                Zusätzlich ist es möglich, den Umfang der Protokollierung
                einzustellen<JMSLib.ReactUi.InternalLink view={`${page.application.adminPage.route};other`} pict="admin" />,
                die der VCR.NET Recording Service im Ereignisprokoll von Windows (Bereich Anwendung) vornimmt.
            </div>;
        }
    }
}
