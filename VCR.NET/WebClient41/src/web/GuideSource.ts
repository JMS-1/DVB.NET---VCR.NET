module VCRServer {

    // Die Art der zu suchenden Quelle
    export enum GuideSource {
        // Nur Fernsehsender
        TV = 1,

        // Nur Radiosender
        RADIO = 2,

        // Einfach alles
        ALL = TV + RADIO,
    }

}

