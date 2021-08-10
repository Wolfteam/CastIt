import { useSnackbar } from 'notistack';
import { useEffect } from 'react';
import { onServerMessage } from '../services/castithub.service';
import { getErrorCodeTranslation } from '../services/translations';

interface Props {
    children: JSX.Element;
}

function ServerMessage(props: Props) {
    const { enqueueSnackbar } = useSnackbar();

    useEffect(() => {
        const onServerMessageSubscription = onServerMessage.subscribe((serverMessage) => {
            const message = getErrorCodeTranslation(serverMessage);
            enqueueSnackbar(message);
        });
        return () => {
            onServerMessageSubscription.unsubscribe();
        };
    }, [enqueueSnackbar]);

    return props.children;
}

export default ServerMessage;
