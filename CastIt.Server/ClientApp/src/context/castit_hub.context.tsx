import { createContext, useCallback, useContext, useEffect, useState } from 'react';
import { CastItHubService, onClientDisconnected } from '../services/castithub.service';
import Loading from '../components/loading';
import { useSnackbar } from 'notistack';
import translations from '../services/translations';
import { Button } from '@material-ui/core';
import NothingFound from '../components/nothing_found';

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

export const CastItHubContext = createContext<ICastItHubContext>(initialValue);

export const CastItHubContextProvider = (children: any): JSX.Element => {
    const hub = useContext(CastItHubContext);
    const [state, setState] = useState(initialValue);
    const { enqueueSnackbar } = useSnackbar();

    const onConnected = useCallback(() => setState((s) => ({ ...s, isConnected: true, isError: false })), []);

    const onConnectionFailed = useCallback(
        (error: any) => {
            console.log(error);
            setState((s) => ({ ...s, isConnected: false, isError: true }));
            enqueueSnackbar(translations.connectionFailedMsg, { variant: 'error' });
        },
        [enqueueSnackbar]
    );

    const handleConnect = useCallback(() => {
        setState((s) => ({ ...s, isError: false }));
        hub.connection
            .connect()
            .then(() => onConnected())
            .catch((error) => onConnectionFailed(error));
    }, [hub.connection, onConnected, onConnectionFailed]);

    useEffect(() => {
        const onClientDisconnectedSubscription = onClientDisconnected.subscribe(() =>
            setState((s) => ({ ...s, isConnected: false, isError: true }))
        );
        return () => {
            onClientDisconnectedSubscription.unsubscribe();
        };
    }, []);

    useEffect(() => {
        handleConnect();
    }, [handleConnect]);

    if (state.isError) {
        return (
            <NothingFound text={translations.connectionFailedMsg}>
                <Button color="primary" variant="contained" onClick={handleConnect}>
                    {translations.retry}
                </Button>
            </NothingFound>
        );
    }

    if (!state.isConnected) {
        return <Loading message={translations.connecting + '...'} />;
    }

    return <CastItHubContext.Provider value={state}>{children.children}</CastItHubContext.Provider>;
};

export const useCastItHub = () => useContext(CastItHubContext);
