import { createContext, useCallback, useContext, useEffect, useState } from 'react';
import { CastItHubService, onClientDisconnected, onPlayerStatusChanged } from '../services/castithub.service';
import Loading from '../components/loading';
import { useSnackbar } from 'notistack';
import translations from '../services/translations';
import { Button } from '@material-ui/core';
import NothingFound from '../components/nothing_found';
import usePageVisibility from '../hooks/use_page_visibility.hook';
import { isMobile, isTablet } from 'react-device-detect';

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

    const isPageVisible = usePageVisibility();

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
        const onClientDisconnectedSubscription = onClientDisconnected.subscribe(() => {
            onPlayerStatusChanged.next(null);
            setState((s) => ({ ...s, isConnected: false, isError: true }));
        });
        return () => {
            onClientDisconnectedSubscription.unsubscribe();
        };
    }, []);

    //This one is for desktop
    useEffect(() => {
        if (!isMobile && !isTablet) {
            handleConnect();
        }
    }, [handleConnect]);

    //And this one is for mobile / tablet
    useEffect(() => {
        if (!isMobile && !isTablet) {
            return;
        }

        if (isPageVisible) {
            handleConnect();
        } else {
            hub.connection.disconnect();
        }
    }, [isPageVisible, handleConnect, hub.connection]);

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
