/// <reference path="../vcrnet.tsx" />

namespace VCRNETClient {
    interface IHomeStatic {
    }

    interface IHomeDynamic {
    }

    export class Home extends React.Component<IHomeStatic, IHomeDynamic>{
        render(): JSX.Element {
            return <div className="vcrnet-home">[HOME]</div>;
        }
    }
}
