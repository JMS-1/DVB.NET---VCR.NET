/// <reference path="../vcrnet.tsx" />

namespace VCRNETClient {
    interface IEditStatic {
        page: App.EditPage;
    }

    interface IEditDynamic {
    }

    export class Edit extends React.Component<IEditStatic, IEditDynamic> {

        render(): JSX.Element {
            return <div className="vcrnet-edit">[TBD]</div>;
        }
    }
}
