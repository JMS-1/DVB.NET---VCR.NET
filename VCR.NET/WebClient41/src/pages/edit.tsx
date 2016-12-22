﻿/// <reference path="../vcrnet.tsx" />

namespace VCRNETClient {
    interface IEditStatic {
        page: App.EditPage;
    }

    interface IEditDynamic {
    }

    export class Edit extends React.Component<IEditStatic, IEditDynamic> {

        render(): JSX.Element {
            return <div className="vcrnet-edit">
                <div>
                    Mit diesem Formular werden alle Daten erfasst, die für die Ausführung einer Aufzeichnung benötigt werden. Im
                    oberen Bereich finden sich die Einstellungen des Auftrags<HelpLink page="faq;jobsandschedules" />,
                    die allen Aufzeichnungen des Auftrags gemeinsam sind.
                    In der Mitte werden die eigentlichen Aufzeichnungsdaten festgelegt. Der untere Bereich ist für sich 
                    wiederholende Aufzeichnungen aktiv, wenn für einzelne Tage Ausnahmeregeln definiert wurden.
                </div>
                <InlineHelp title="Erläuterungen zu den Daten eines Auftrags">
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
                </InlineHelp>
            </div>;
        }
    }
}
