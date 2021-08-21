import { createContext, Dispatch, SetStateAction, useContext, useEffect } from 'react';
import { CastItHubService } from '../services/castithub.service';

interface ICastItHubContext {
    connection: CastItHubService;
    isConnected: boolean;
    isError: boolean;
}

const initialValue: ICastItHubContext = {
    connection: new CastItHubService(),
    isConnected: false,
    isError: false,
};

export const CastItHubContext = createContext<[ICastItHubContext, Dispatch<SetStateAction<ICastItHubContext>>]>([initialValue, (s) => s]);

export const CastItHubContextProvider = (children: any): JSX.Element => {
    const [hub, setHub] = useContext(CastItHubContext);

    useEffect(() => {
        hub.connection
            .connect()
            .then(() => {
                setHub((s) => ({ ...s, isConnected: true, isError: false }));
            })
            .catch((error) => {
                console.log(error);
                setHub((s) => ({ ...s, isConnected: false, isError: true }));
            });
    }, [hub, setHub]);

    return <CastItHubContext.Provider value={[hub, setHub]}>{children.children}</CastItHubContext.Provider>;
};
