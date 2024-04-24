import { useEffect, useState } from "react";
import { BattleGroundStyles } from "./battleground.jss";
import { FighterStyles } from "./fighter.jss";
import { HubConnectionBuilder } from "@microsoft/signalr";
import { BattleLogsStyles } from "./battlelogs.jss";
// import { SignalRService } from "../services/signalrService";

export interface ISignalRNegotiation {
    Url: string;
    AccessToken: string;
}

interface IFighterProps {
    name: string;
    endpoint: string;
    displayGenki: boolean;
}

// const signalRService = new SignalRService();

export const Fighter = (props: IFighterProps) => {
    const styles = FighterStyles();
    const [power, setPower] = useState(0);

    // const signalR = new 

    function handleAttack(endpoint: string, power: number): void {
        // const attack = document.querySelector('input')?.value;
        console.log(power);
        if (power) {
            fetch(endpoint, {
                method: 'POST',
                cache: 'no-cache',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({ energy: power }),
            })
            // .then((res) => res.json())
            // .then((data) => {
            //     console.log(data);
            // });
        }
    }

    function handleGenki(endpoint: string, power: number): void {
        fetch("https://ca-songoku--4ua9qgj.purpleocean-c5769fe7.westeurope.azurecontainerapps.io/launch-genki-dama", {
            method: 'POST',
            cache: 'no-cache',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({ energy: power }),
        })
    }

    return (
        <div className={styles.container}>
            <img className={styles.image} src={`/images/${props.name.toLowerCase()}.png`} alt={props.name} />
            <div>
                <h1 className={styles.title}>{props.name}</h1>
                <input className={styles.attackInput}
                    type="text"
                    placeholder="Enter your attack"
                    onChange={(e) => setPower(parseInt(e.target.value) || 0)}
                    value={power} />
                <button className={styles.attackButton}
                    onClick={() => handleAttack(props.endpoint, power)}>
                    Attack
                </button>

                {props.displayGenki && (
                    <button className={styles.attackButton}
                        onClick={() => handleGenki(props.endpoint, power)}>
                        Genkidama
                    </button>
                )}
            </div>
        </div>
    )
}

export const Battleground = () => {
    const styles = BattleGroundStyles();
    const logStyles = BattleLogsStyles();
    const [negotiation, setNegotiation] = useState<ISignalRNegotiation | null>(null);
    // const [connection, setConnection] = useState<HubConnection | null>(null);
    const [message, setMessage] = useState("");
    const [url, setUrl] = useState("");
    const [accessToken, setAccessToken] = useState("");
    // const signalRResponse = signalRService.getSignalRConnectionInfo();

    useEffect(() => {
        // const newConnection = new HubConnectionBuilder()
        //     // .withUrl("https://sigr-acadragonball.service.signalr.net/client/?hub=tenkaichibudokai")
        //     .withUrl("https://tenkaichibudokai.azurewebsites.net/api")
        //     .withAutomaticReconnect()
        //     .build();
        //     console.log(newConnection.baseUrl);
        // setConnection(newConnection);

        fetch("https://tenkaichibudokai.azurewebsites.net/api/negotiate", {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
        })
            .then(response => response.json())
            .then(data => {
                setNegotiation(data);
                setUrl(data.Url);
                setAccessToken(data.AccessToken);
                // console.log(data)
            })
            .catch((error) => console.error('Error:', error));

    }, []);

    useEffect(() => {
        console.log(1);
        console.log
        if (url) {
            console.log(2);
            const options = {
                accessTokenFactory: () => accessToken,
            };
            const connection = new HubConnectionBuilder()
                .withUrl(url, options)
                .withAutomaticReconnect()
                .build();

            connection.start()
                .then((result) => {
                    console.log('Connected!');
                    connection.on('tenkaichibudokai-freezer', message => {
                        console.log('Received message: ', message);
                        setMessage(message);
                    });
                    connection.on('tenkaichibudokai-goku', message => {
                        console.log('Received message: ', message);
                        setMessage(message);
                    });
                    console.log("RESULT " + result);
                })
                .catch(e => console.log('Connection failed: ', e));
        }
    }, [negotiation]);

    return (
        <>
            <div className={styles.container}>
                <div className={styles.fighters}>
                    <Fighter name="Goku" endpoint="https://ca-songoku--4ua9qgj.purpleocean-c5769fe7.westeurope.azurecontainerapps.io/attack-freezer" displayGenki={true} />
                    <Fighter name="Freezer" endpoint="https://ca-freezer.purpleocean-c5769fe7.westeurope.azurecontainerapps.io/attack-goku" displayGenki={false} />
                    <div className={logStyles.container}>{message.text}</div>
                </div>
            </div>
        </>
    )
}