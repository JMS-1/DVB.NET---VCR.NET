/// <reference path="../lib.react/reactUi.tsx" />

namespace VCRNETClient.Ui {

    // Hilfsschnittstelle zur Signature des Konstruktors eines Ui View Models.
    export interface IAdminSectionFactory<TSectionType extends App.Admin.ISection> {
        // Der eigentliche Konstruktor.
        new (page: App.AdminPage): App.Admin.ISection;

        // Die statische Eigenschaft mit dem eindeutigen Namen.
        route: string;
    }

    // Schnittstelle zum anlegen der React.Js Komponente für einen einzelnen Konfigurationsbereich.
    interface IAdminUiSectionFactory<TSectionType extends App.Admin.ISection> {
        // Der eigentliche Konstruktor.
        new (props?: JMSLib.ReactUi.IComponent<TSectionType>, context?: JMSLib.ReactUi.IEmpty): AdminSection<TSectionType>;

        // Das zugehörige Ui View Model.
        readonly uvm: IAdminSectionFactory<TSectionType>;
    }

    // Nachschlageliste für die React.Js Komponenten der Konfigurationsbereiche.
    interface IAdminSectionFactoryMap {
        // Ermittelt einen Konfigurationsbereich.
        [route: string]: IAdminUiSectionFactory<any>;
    }

    // React.Js Komponente zur Anzeige der Konfiguration.
    export class Admin extends JMSLib.ReactUi.ComponentWithSite<App.IAdminPage>{

        // Alle bekannten Konfigurationsbereiche.
        static _sections: IAdminSectionFactoryMap;

        // Einen einzelnen Konfigurationsbereich ergänzen.
        static addSection<TSectionType extends App.Admin.ISection>(factory: IAdminUiSectionFactory<TSectionType>): void {
            Admin._sections[factory.uvm.route] = factory;
        }

        // Oberflächenelemente erstellen.
        render(): JSX.Element {
            var test: IAdminUiSectionFactory<any> = AdminDevices;

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
            // Einmalig erzeugen.
            if (!Admin._sections) {
                // Leer anlegen.
                Admin._sections = {};

                // Alle unterstützten Seiten anlegen.
                Admin.addSection(AdminDevices);
                Admin.addSection(AdminSecurity);
                Admin.addSection(AdminDirectories);
                Admin.addSection(AdminGuide);
                Admin.addSection(AdminSources);
                Admin.addSection(AdminRules);
                Admin.addSection(AdminOther);
            }

            // Oberlfächenkomponente ermitteln.
            const factory = Admin._sections[this.props.uvm.sections.value.route];

            // Ui View Model ermitteln undReact.Js Komponente erstellen.
            return factory && React.createElement(factory, { uvm: this.props.uvm.getOrCreateCurrentSection() }, {});
        }
    }

}
