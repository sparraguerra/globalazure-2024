import axios, { AxiosBasicCredentials, AxiosRequestConfig, CancelTokenSource } from "axios";

export interface ISignalRConnection {
    url: string;
    accessToken: string;
}

export class SignalRService {
    
    async getSignalRConnectionInfo(): Promise<ISignalRConnection> {
        const url = "https://tenkaichibudokai.azurewebsites.net/api/negotiate";
        return (await apiFetch("POST", url)).execute<ISignalRConnection>();
    }
}

export class Request {
    private config: AxiosRequestConfig;
    private cancelSource: CancelTokenSource | undefined;

    constructor(method: string, url: string) {
        this.config = {
            method: method,
            url: url,
        };
    }

    /**
     * Add a parameter to the request base URI
     *
     * @param {string} key name of the parameter
     * @param {string} value of the parameter
     * @returns {Request}
     */
    withQueryParameter(key: string, value: string): Request {
        this.config.params = { ...this.config.params, [key]: value };
        return this;
    }

    /**
     * Add a header key-value to the request
     *
     * @param {string} key name of the header
     * @param {string} value of the header
     * @returns
     */
    withHeader(key: string, value: string): Request {
        this.config.headers = { ...this.config.headers, [key]: value };
        return this;
    }

    withMultiPartHeaders(): Request {
        this.config.headers = { ...this.config.headers, Accept: '*/*' };
        this.config.headers = { ...this.config.headers, 'Content-Type': 'multipart/form-data' };
        return this;
    }

    /**
     * Configure basic authentication
     *
     * @param {AxiosBasicCredentials} basic crecdentials
     * @returns
     */
    withAuthentication(auth: AxiosBasicCredentials): Request {
        this.config.auth = auth;
        return this;
    }
    /**
     * Add a cancellation token to the request
     *
     * @returns {Request}
     */
    withCancellation(): Request {
        this.cancelSource = axios.CancelToken.source();
        this.config.cancelToken = this.cancelSource.token;
        return this;
    }

    /**
     * Cancel the request
     *
     * @param {string} message Optional cancellation message
     */
    cancel(message?: string): void {
        if (this.cancelSource) {
            this.cancelSource.cancel(message);
        }
    }

    withLanguage(): Request {
        this.config.headers = { ...this.config.headers, 'Accept-Language': localStorage.i18nextLng };
        return this;
    }

    /**
     * Add a body to the request
     *
     * @param {any} body data of the request
     * @returns
     */
    withBody(body: string): Request {
        this.config.data = body;
        return this;
    }

    /**
     * Add a timeout value in milliseconds
     *
     * @param {number} milliseconds for waiting
     * @returns
     */
    withTimeout(millis: number): Request {
        this.config.timeout = millis;
        return this;
    }

    /**
     * Execute the type request
     *
     * @returns {Promise<T>} return a Promise with the type of the method
     */
    async execute<T>(): Promise<T> {
        const response = await axios.request(this.config);
        return response.data as T;
    }
    async executeBlob(): Promise<Blob> {
        const response = await axios.request({ ...this.config, responseType: 'blob' });
        return response.data;
    }
}

export const apiFetch = (method: string, url: string): Request => {
    return new Request(method, url).withLanguage();
};

// export const apiFetchOAuth = async (method: string, url: string): Promise<Request> => {
//     return (await new Request(method, url).withOAuth()).withCancellation().withLanguage();
// };
