/// <reference path="helpComponent.ts" />

namespace VCRNETClient.Ui.HelpPages {
    export class Nexus extends HelpComponent {
        readonly title = "TechnoTrend 2300 Premium Line / Hauppauge Nexus";

        render(page: App.IPage): JSX.Element {
            return <div>
                Der VCR.NET Recording Service würde im Jahre 2003 ursprünglich für
                die <JMSLib.ReactUi.ExternalLink url="http://www.hauppauge.de/">Hauppauge</JMSLib.ReactUi.ExternalLink> Nexus-S
                entwickelt - diese Karte ist eigentlich
                eine <JMSLib.ReactUi.ExternalLink url="http://www.technotrend.eu/">TechnoTrend</JMSLib.ReactUi.ExternalLink> S-2300
                PremiumLine und wurde damals von Hauppauge unter einem eigenen Namen vertrieben.
                Die meisten DVB Geräte verwenden damals wie heute mehr oder weniger einheitliche
                Treiber, die dem Microsoft BDA (<em>Broadcast Device Architecture</em>) Standard
                genügen. Dabei ist es im Allgemeinen (Ausnahmen bestätigen die Regel) vorgesehen,
                dass die verarbeitende Software alle Daten so entgegen nehmen kann, wie sie digital
                empfangen werden. Insbesondere werden nach Ansteuern einer Quellgruppe (Transponder,
                Frequenz, ...) alle darauf angebotenen Quellen (Radio- und
                Fernsehsender) <JMSLib.ReactUi.InternalLink view={`${page.route};parallelrecording`}>gleichzeitig</JMSLib.ReactUi.InternalLink> verfügbar.
                So ist es mit einem über BDA angesteuerten Gerät
                normalerweise problemlos möglich, zusätzlich zum Empfang zum Beispiel von RTL auch
                noch RTL2, VOX und SuperRTL Sendungen zu verarbeiten.
                <br />
                <br />
                Es gibt mehrere Gründe, warum das bei der Nexus nicht so ist - auch gute Gründe,
                aber darauf kann hier nicht eingegangen werden. Tatsache ist, dass über die proprietäre
                Softwareschnittelle der Nexus maximal
                8 <JMSLib.ReactUi.InternalLink view={`${page.route};filecontents`}>Nutzdatenströme</JMSLib.ReactUi.InternalLink> gleichzeitig
                empfangen werden können. Ein Nutzdatenstrom bedeutet dabei den Empfang
                eines Bild- oder Tonsignals, von Videotext oder DVB Untertiteln. Beschränkt man
                sich also nur auf das Bild und eine Tonspur, so kann man maximal 4 Quellen gleichzeitig
                empfangen. Möchte man einen Sender wie ZDF vollständig mit Bild, drei Stereotonspuren,
                einem Dolby Digital Signal, dem Videotext und DVB Untertiteln aufzeichnen, so bleibt
                noch nicht einmal Kapazität für einen weiteren Fernsehsender.
                <br />
                <br />
                Dazu kommt, dass die Firmware auf der Nexus nicht mehr mit den aktuellen Herausforderungen
                des digitalen Empfangs mithalten kann. Die Erfahrung zeigt, dass zwar der gleichzeitig
                Empfang von RTL, RTL2, VOX und SuperRTL möglich wäre, nicht aber ZDF, 3Sat, KIKA
                und ZDF.info - immer beschränkt auf nur das Bild und eine Tonspur. Das ZDF überträgt
                seine Sender mit deutlich höheren Datenraten, was auch zu einem besseren Bild führt.
                Nach aktuellen Kenntnisstand kommt die Nexus mit einer Gesamtbildrate von etwa 14
                MBit/s zurecht, ab etwa 15 MBit/s kommt es aber zu dramatischen Störungen, die alle
                parallelen Aufzeichnungen gleichzeitig unbrauchbar macht.
                <br />
                <br />
                Tatsächlich ist es in der Praxis bisher nicht vorgekommen, dass die Zahl gleichzeitiger
                Aufzeichnungen grundsätzlich nicht möglich war - mehr als zwei gleichzeitige Sendungen
                ist schon recht selten. Es bleibt aber immer die Beschränkung auf die 8 Nutzdatenströme,
                die je nach konkreter Situation von Unannehmlichkeiten bis zu fehlgeschlagenen Aufzeichnungen
                führen kann.
                <br />
                <br />
                Der VCR.NET Recording Service war eines der ersten Programme, die parallele Aufzeichnungen
                bei Einsatz einer Nexus / 2300 überhaupt erlaubten. Die ersten Versionen haben sich
                dabei auch große Mühe gegeben, die Einschränkungen der DVB Hardware bei der Aufzeichnungsplanung
                zu berücksichtigen und entsprechend intelligente Korrekturen vorzunehmen, um die
                Wünsche des Anwenders so gut wie möglich zu erfüllen. Heute ist die Nexus allerdings
                eher ein Exot und die Sonderbehandlungen wurden weitgehend aus dem VCR.NET Recording
                Service entfernt. Es ist natürlich immer noch 
                möglich, <JMSLib.ReactUi.InternalLink view={`${page.route};parallelrecording`}>mehrere Sendungen gleichzeitig</JMSLib.ReactUi.InternalLink> aufzuzeichnen.
                Allerdings liegt es nun weitgehend
                in der Hand des Anwenders durch intelligente Programmierung dazu beizutragen, dass
                Aufzeichnungen durch die existierenden Beschränkungen nicht gänzlich verloren gehen
                oder durch die Unzulänglichkeiten der Firmware nicht unbrauchbar werden.
                <br />
                <br />
                Ein ärgerlicher Punkt in der aktuellen Implementierung darf aber nicht unerwähnt
                bleiben. Der VCR.NET Recording Service hat in der vorliegenden Version keine Möglichkeit
                zu protokollieren, wenn das Starten einer zusätzlichen Aufzeichnung aufgrund der
                Hardwarebeschränkung nicht möglich war. Läuft im Beispiel oben die volle ZDF Aufzeichnung
                mit 7 Datenströmen und muss eine weitere Aufzeichnung gestartet werden, so geschieht
                dies nicht. Der VCR.NET Recording Service verhält sich aber, als wäre die Aufzeichnung
                aktiviert worden.
            </div>;
        }
    }
}
