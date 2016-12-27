/// <reference path="../../vcrnet.tsx" />

namespace VCRNETClient.Ui {
    interface IScheduleDataStatic {
    }

    interface IScheduleDataDynamic {
    }

    export class ScheduleData extends React.Component<IScheduleDataStatic, IScheduleDataDynamic>{
        render(): JSX.Element {
            return <fieldset className="vcrnet-scheduledata">
                <legend>Daten zur Aufzeichnung</legend>
            </fieldset>;
        }
    }
}
