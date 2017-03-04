/// <reference path="../lib.react/reactUi.tsx" />

namespace VCRNETClient.Ui {

    // React.Js Komponente zur Anzeige der Konfiguration.
    export class Admin extends JMSLib.ReactUi.ComponentWithSite<App.IAdminPage>{

        // Alle Administrationsbereiche.

        // Oberflächenelemente erstellen.
        render(): JSX.Element {
            const section = this.props.uvm.sections.value;

            return <div className="vcrnet-admin">
                <div>
                    {this.props.uvm.sections.allowedValues.map(si =>
                        <div
                            key={si.display}
                            className={`${(si.value === section) ? `jmslib-command-checked` : ``}`}>
                            <JMSLib.ReactUi.InternalLink view={`${this.props.uvm.route};${si.value.route}`}>{si.display}</JMSLib.ReactUi.InternalLink>
                        </div>)}
                </div>
                <div>{this.renderSection()}</div>
            </div>;
        }

        // React.Js Komponente zum aktuellen Konfigurationsbereich ermitteln.
        private renderSection(): JSX.Element {
            const page = this.props.uvm.sections.value.section;

            if (page instanceof App.Admin.DevicesSection)
                return <AdminDevices uvm={page} />;

            if (page instanceof App.Admin.SecuritySection)
                return <AdminSecurity uvm={page} />;

            if (page instanceof App.Admin.DirectoriesSection)
                return <AdminDirectories uvm={page} />;

            if (page instanceof App.Admin.GuideSection)
                return <AdminGuide uvm={page} />;

            if (page instanceof App.Admin.ScanSection)
                return <AdminSources uvm={page} />;

            if (page instanceof App.Admin.RulesSection)
                return <AdminRules uvm={page} />;

            if (page instanceof App.Admin.OtherSection)
                return <AdminOther uvm={page} />;

            return null;
        }
    }

}
