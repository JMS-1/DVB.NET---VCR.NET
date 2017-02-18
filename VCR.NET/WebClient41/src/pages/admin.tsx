/// <reference path="../lib.react/reactUi.tsx" />

namespace VCRNETClient.Ui {

    export class Admin extends JMSLib.ReactUi.ComponentWithSite<App.IAdminPage>{
        render(): JSX.Element {
            const section = this.props.noui.sections.value;

            return <div className="vcrnet-admin">
                <div className="vcrnet-admin-tabs">
                    <div>
                        {this.props.noui.sections.allowedValues.map(si => <div key={si.display} data-jmslib-checked={(si.value === section) ? "yes" : null} >
                            <JMSLib.ReactUi.InternalLink view={`${this.props.noui.route};${si.value.route}`}>{si.display}</JMSLib.ReactUi.InternalLink>
                        </div>)}
                    </div>
                    <div>
                        {this.renderSection()}
                    </div>
                </div>
            </div>;
        }

        private renderSection(): JSX.Element {
            const page = this.props.noui.sections.value.page;

            if (page instanceof App.Admin.DevicesSection)
                return <AdminDevices noui={page} />;

            if (page instanceof App.Admin.SecuritySection)
                return <AdminSecurity noui={page} />;

            if (page instanceof App.Admin.DirectoriesSection)
                return <AdminDirectories noui={page} />;

            if (page instanceof App.Admin.GuideSection)
                return <AdminGuide noui={page} />;

            if (page instanceof App.Admin.ScanSection)
                return <AdminSources noui={page} />;

            if (page instanceof App.Admin.RulesSection)
                return <AdminRules noui={page} />;

            if (page instanceof App.Admin.OtherSection)
                return <AdminOther noui={page} />;

            return null;
        }
    }

}
