// Das muss noch geklärt werden: import geht leider nicht mit AMD und sonst können wir das nicht alles in eine Quelldatei bekommen.
import React = __React;
import ReactDOM = __React.__DOM;

namespace JMSLib.ReactUi {

    // Beschreibt eine React.Js Komponente für ein Präsentationsmodell.
    export interface IComponent<TViewModelType> {
        // Das Präsentationsmodell (Ui View Model).
        uvm: TViewModelType;
    }

    // Beschreibt einen nicht vorhandenen Zustand einer React.Js Komponente.
    export interface IEmpty {
    }

    // Implementierung einer React.Js Komponente für ein Präsentationsmodell.
    export abstract class ComponentEx<TViewModelType, TConfigType extends IComponent<TViewModelType>> extends React.Component<TConfigType, IEmpty>
    {
    }

    export abstract class Component<TViewModelType> extends ComponentEx<TViewModelType, IComponent<TViewModelType>>
    {
    }

    // Implementierung einer React.Js Komponente für ein Präsentationsmodell mit Benachrichtigungen.
    export abstract class ComponentExWithSite<TViewModelType extends App.IConnectable, TConfigType extends IComponent<TViewModelType>> extends ComponentEx<TViewModelType, TConfigType> implements App.ISite {
        // Führt die Anmeldung auf Benachrichtigungen aus.
        componentWillMount(): void {
            this.props.uvm.site = this;
        }

        // Meldet sich von Benachrichtigungen ab.
        componentWillUnmount(): void {
            this.props.uvm.site = undefined;
        }

        // Fordert eine Aktualisierung der Anzeige an.
        refreshUi(): void {
            this.forceUpdate();
        }
    }

    export abstract class ComponentWithSite<TViewModelType extends App.IConnectable> extends ComponentExWithSite<TViewModelType, IComponent<TViewModelType>> {
    }
}
