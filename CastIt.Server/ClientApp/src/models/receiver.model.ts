export interface IReceiver {
    id: string;
    friendlyName: string;
    type: string;
    host: string;
    port: number;
    isConnected: boolean;
}