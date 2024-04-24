import * as signalR from "@microsoft/signalr";

const URL = "https://tenkaichibudokai.azurewebsites.net/api/negotiate"; //or whatever your backend port is
// const URL = "https://localhost:5001/hub"; //or whatever your backend port is
class SignalRConnector {
    private connection: signalR.HubConnection;
    public events: (onMessageReceived: (username: string, message: string) => void) => void;
    static instance: SignalRConnector;
    constructor() {
        this.connection = new signalR.HubConnectionBuilder()
            .withUrl(URL)
            .withAutomaticReconnect()
            .build();
        this.connection.start().catch(err => document.write(err));
        this.events = (onMessageReceived) => {
            this.connection.on("messageReceived", (username, message) => {
                onMessageReceived(username, message);
            });
        };
    }
    public newMessage = (messages: string) => {
        this.connection.send("newMessage", "foo", messages).then(() => {console.log("sent")})
    }
    public static getInstance(): SignalRConnector {
        if (!SignalRConnector.instance)
            SignalRConnector.instance = new SignalRConnector();
        return SignalRConnector.instance;
    }
}
export default SignalRConnector.getInstance;