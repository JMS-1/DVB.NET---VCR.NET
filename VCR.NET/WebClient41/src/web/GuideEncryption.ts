module VCRServer {

    // Die Verschlüsselung der Quelle
    export enum GuideEncryption {
        // Nur kostenlose Quellen
        FREE = 1,

        // Nur Bezahlsender
        PAY = 2,

        // Alle Sender
        ALL = FREE + PAY,
    }

}

