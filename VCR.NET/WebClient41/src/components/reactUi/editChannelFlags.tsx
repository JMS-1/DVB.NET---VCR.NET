/// <reference path="../../vcrnet.tsx" />

namespace VCRNETClient.Ui {

    interface IEditChannelFlagsStatic {
        noui: App.NoUi.JobEditor;
    }

    interface IEditChannelFlagsDynamic {
    }

    export class EditChannelFlags extends React.Component<IEditChannelFlagsStatic, IEditChannelFlagsDynamic>  {
        componentWillMount(): void {
        }

        componentWillUnmount(): void {
        }

        render(): JSX.Element {
            return <div className="vcrnet-editchannelflags">
            </div>;
        }
    }
}
