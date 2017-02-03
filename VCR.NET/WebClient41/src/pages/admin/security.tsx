﻿/// <reference path="../../lib/reactUi.tsx" />

namespace VCRNETClient.Ui {

    // ReacJs Komponente zur Pflege der Sicherheitskonfiguration.
    export class AdminSecurity extends JMSLib.ReactUi.Component<App.Admin.IAdminSecurityPage>{

        // Oberflächenelemente erstellen.
        render(): JSX.Element {
            return <div className="vcrnet-admin-security">
                <h2>Auswahl der Benutzergruppen</h2>
                Der VCR.NET Recording Service unterscheidet zwischen regulären Anwendern, die Aufzeichnungen anlegen, ändern und entfernen dürfen
                und Administratoren, die zusätzlich sämtliche Betriebsparameter verändern dürfen.
                {this.getHelp()}
                <Field label={`${this.props.noui.userGroups.text}:`} page={this.props.noui.page} >
                    <JMSLib.ReactUi.EditTextWithList noui={this.props.noui.userGroups} />
                </Field>
                <Field label={`${this.props.noui.adminGroups.text}:`} page={this.props.noui.page} >
                    <JMSLib.ReactUi.EditTextWithList noui={this.props.noui.adminGroups} />
                </Field>
                <JMSLib.ReactUi.ButtonCommand noui={this.props.noui.update} />
            </div>;
        }

        // Eingebettete Erläuterungen erstellen.
        private getHelp(): JSX.Element {
            return <InlineHelp title="Erläuterungen zur Bedienung">
                In der Voreinstellung sind alle Windows Benutzer sowohl reguläre Anwender als auch Administratoren.
                Die Auswahllisten zeigen jeweils alle Windows Benutzergruppen des lokalen Rechners und erlauben
                eine einfache Zuordnung passend zur jeweiligen Anforderung - der VCR.NET Recording Service unterstützt
                die Verwendung von Domänengruppen nicht. Der jeweils erste Eintrag der Listen erlaubt die Rückkehr
                zur Voreinstellung. Die Änderung wird erst nach dem Betätigen der Schaltfläche aktiv, ein Neustart des
                VCR.NET Dienstes ist nicht notwendig.
                <br />
                <br />
                Man beachte, dass ein VCR.NET Administrator auch Rechte besitzt, die über den eigentlichen Betrieb
                des VCR.NET Recording Service hinaus gehen. So kann er etwa bei der Auswahl der Aufzeichnungsverzeichnisse
                im Allgemeinen sämtliche Verzeichnisse sehen, die auf dem Rechner vorhanden sind - einschließlich der
                Verzeichnisse, die einem normalen Anwender üblicherweise verborgen bleiben.
          </InlineHelp>;
        }
    }

}
