/// <reference path="../vcrnet.tsx" />

namespace VCRNETClient {
    export class Plan extends NoUiView<App.PlanPage> implements App.IPlanSite {
        // Beim Einbinden der Anzeige in die Oberfläche wird eine Verbindung zur Logik hergestellt.
        componentWillMount(): void {
            this.props.noui.setSite(this);
        }

        // Beim Ausklinken der Anzeige wird die Verbindung zur Logik aufgehoben.
        componentWillUnmount(): void {
            this.props.noui.setSite(undefined);
        }

        render(): JSX.Element {
            var jobs = this.props.noui.getJobs();

            return <div className="vcrnet-plan">
                Hier sieht man einen Ausschnitt der geplanten Aufzeichnungen für die nächsten Wochen.<HelpLink page="faq;parallelrecording" />
                {this.getHelp()}
                <div className="vcrnet-plan-filter">
                    <RadioGroup>
                        {this.props.noui.getStartFilter().map((f, index) =>
                            <Radio key={index} groupName="filterStart" isChecked={f.active} onClick={() => f.activate()}>
                                {(index === 0) ? "Jetzt" : DateFormatter.getShortDate(f.date)}
                            </Radio>)}
                    </RadioGroup>
                    <CheckBox onToggle={() => this.props.noui.toggleTaskFilter()} isChecked={this.props.noui.showTasks()}>
                        Aufgaben einblenden
                    </CheckBox>
                </div>
                {
                    jobs ? <table>
                        <thead>
                            <tr>
                                <td>&nbsp;</td>
                                <td>Beginn</td>
                                <td>Ende</td>
                                <td>Quelle</td>
                                <td>Name</td>
                                <td>Gerät</td>
                            </tr>
                        </thead>
                        <tbody>
                            {jobs.map(job => [
                                <Ui.PlanRow noui={job} key={job.key} />,
                                job.showEpg ?
                                    <DetailRow prefixColumns={1} dataColumns={5} key={`${job.key}Details`}>
                                        [EPGINFO]
                                    </DetailRow> : null,
                                job.showException ?
                                    <DetailRow prefixColumns={1} dataColumns={5} key={`${job.key}Exceptions`}>
                                        [EXTENSIONEDIT]
                                    </DetailRow> : null
                            ])}
                        </tbody>
                    </table> : null
                }
            </div >;
        }

        onRefresh(): void {
            this.forceUpdate();
        }

        private getHelp(): JSX.Element {
            return <InlineHelp title="Erläuterungen zur Bedienung">
                <p>
                    Über die Datumsauswahl im linken Bereich kann
                        der zeitliche Anfang des angezeigten Ausschnitts festgelegt werden. Das Ende des Ausschnitts ergibt sich daraus
                        und aus der gewünschten Anzahl von zu berücksichtigenden Tagen.<InternalLink view="settings" pict="settings" />
                </p>
                <p>
                    Die beiden Schaltflächen direkt rechts neben der Datumsauswahl erlauben es zusätzlich zu
                        den regulären Aufzeichnungen auch die vorgesehenen Zeiten für die Aktualisierung<HelpLink page="faq;tasks" />
                    der Programmzeitschrift oder der
                        Senderliste in der Liste darzustellen.
                    </p>
                <p>
                    Links vor jeder Aufzeichnung in der Liste befindet sich ein kleines Symbol, dass darüber informiert, ob die
                        Aufzeichnung wie gewünscht ausgeführt werden kann oder nicht.
                    </p>
                <table>
                    <tbody>
                        <tr>
                            <td>
                                <Pictogram name="intime" type="gif" />
                            </td>
                            <td>Die Aufzeichnung wird wie programmiert ausgeführt.</td>
                        </tr>
                        <tr>
                            <td>
                                <Pictogram name="late" type="gif" />
                            </td>
                            <td>Die Aufzeichnung beginnt verspätet, eventuell fehlt der Anfang.</td>
                        </tr>
                        <tr>
                            <td>
                                <Pictogram name="lost" type="gif" />
                            </td>
                            <td>Die Aufzeichnung kann nicht ausgeführt werden.</td>
                        </tr>
                    </tbody>
                </table>
                <p>
                    Direkt rechts neben dem Symbol wird der Beginn der
                        Aufzeichnung als Verweis angezeigt. Wird dieser Verweis angeklickt, so werden weitere Details zur Aufzeichnung
                        sichtbar. Je nach vorhandenen Daten wird auch der zugehörige Eintrag der Programmzeitschrift<HelpLink page="faq;epg" />
                    abgerufen und angezeigt.
                    </p>
                <p>
                    Durch Anwählen des Verweises auf den Namen der Aufzeichnung kann diese bearbeitet werden.
                    </p>
                <table>
                    <tbody>
                        <tr>
                            <td>
                                <Pictogram name="exceptOff" />
                            </td>
                            <td>
                                Handelt es sich um eine sich wiederholende Aufzeichnung<HelpLink page="faq;repeatingschedules" />,
                                so kann auch das Symbol direkt rechts neben dem Namen angeklickt werden,
                                um die Ausnahmeregelung für die jeweilige Aufzeichnung anzuzeigen und zu ändern.
                            </td>
                        </tr>
                    </tbody>
                </table>
            </InlineHelp>;
        }
    }
}
