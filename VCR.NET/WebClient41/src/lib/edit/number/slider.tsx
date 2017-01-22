/// <reference path="../../reactUi.tsx" />

namespace JMSLib.ReactUi {

    // React.Js Komponente zur Auswahl einer Zahl über einen Schieberegler.
    export class EditNumberWithSlider extends ComponentWithSite<App.IEditNumberWithSlider>  {
        // Erstellt die Oberflächenelement - hier gibt es eine ganze Menge Kleinigkeiten im CSS zu beachten, esp. die Positionierung.
        render(): JSX.Element {
            return <div className="jmslib-slider">
                <div></div>
                <div>
                    <div className={this.props.noui.isDragging ? "jmslib-slider-selected" : undefined}
                        style={{ left: `${100 * this.props.noui.position}%` }}>
                    </div>
                </div>
                <div 
                    onMouseDown={ev => this.props.noui.isDragging = (ev.buttons === 1)}
                    onMouseMove={ev => this.doMove(ev)}
                    onMouseUp={ev => this.props.noui.isDragging = false} onMouseLeave={ev => this.props.noui.isDragging = false}
                    onKeyUp={ev => this.doKey(ev)} tabIndex={0}
                    draggable={false} onDragStart={() => false}>
                </div>
            </div>;
        }

        // Überwacht Bewegungen mit der Maus und gibe diese an die Anwendungslogik weiter.
        private doMove(ev: React.MouseEvent): void {
            // Zurzeit sind Änderungen nicht gestattet.
            if (!this.props.noui.isDragging)
                return;

            // Der äußere Bereich des Reglers.
            var bounds = (ev.target as HTMLDivElement).getBoundingClientRect();

            // Die relative horizontale Mausposition.
            var absX = ev.clientX;
            var relX = absX - bounds.left;
            
            // Als relativen Wert zwischen 0 und 1 an die Anwendungslogik melden.
            this.props.noui.position = relX / bounds.width;
        }

        // Zur Feinsteuerung setzen wir auch die Pfeiltasten nach links und rechts um.
        private doKey(ev: React.KeyboardEvent): void {
            if (ev.keyCode === 37)
                this.props.noui.delta(-1);
            else if (ev.keyCode === 39)
                this.props.noui.delta(+1);
        }
    }
}
