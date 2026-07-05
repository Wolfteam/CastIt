import { Dialog, DialogTitle, DialogContent, TextField, DialogActions, Button } from '@mui/material';
import React, { useEffect, useState } from 'react';
import translations from '../../services/translations';
import { IReceiver } from '../../models';
import { useCastItHub } from '../../context/castit_hub.context';

interface Props {
    isOpen: boolean;
    endpoint: IReceiver | null;
    onClose(): void;
}

interface State {
    name: string;
    host: string;
    port: number;
    isDirty: boolean;
}

const defaultPort = 8009;

function AddEditEndpointDialog(props: Props) {
    const castItHub = useCastItHub();

    const [state, setState] = useState<State>({
        name: '',
        host: '',
        port: defaultPort,
        isDirty: false,
    });

    // Re-initialize whenever the dialog opens (it stays mounted, so state would
    // otherwise persist across opens).
    useEffect(() => {
        if (props.isOpen) {
            setState({
                name: props.endpoint?.friendlyName ?? '',
                host: props.endpoint?.host ?? '',
                port: props.endpoint?.port ?? defaultPort,
                isDirty: false,
            });
        }
    }, [props.isOpen, props.endpoint]);

    const handleNameChange = (event: React.ChangeEvent<HTMLInputElement>) => {
        setState((s) => ({ ...s, name: event.target.value, isDirty: true }));
    };

    const handleHostChange = (event: React.ChangeEvent<HTMLInputElement>) => {
        setState((s) => ({ ...s, host: event.target.value, isDirty: true }));
    };

    const handlePortChange = (event: React.ChangeEvent<HTMLInputElement>) => {
        setState((s) => ({ ...s, port: parseInt(event.target.value, 10), isDirty: true }));
    };

    const isNameValid = state.name.trim() !== '';
    const isHostValid = state.host.trim() !== '';
    const isPortValid = Number.isInteger(state.port) && state.port >= 1 && state.port <= 65535;
    const isValid = isNameValid && isHostValid && isPortValid;

    const showNameError = !isNameValid && state.isDirty;
    const showHostError = !isHostValid && state.isDirty;
    const showPortError = !isPortValid && state.isDirty;

    const handleSubmit = async (event: React.FormEvent): Promise<void> => {
        event.preventDefault();
        if (!isValid) {
            return;
        }

        const name = state.name.trim();
        const host = state.host.trim();

        if (props.endpoint) {
            await castItHub.connection.updateCustomEndpoint(Number(props.endpoint.id), name, host, state.port);
        } else {
            await castItHub.connection.addCustomEndpoint(name, host, state.port);
        }

        props.onClose();
    };

    if (!props.isOpen) {
        return null;
    }

    return (
        <Dialog open={props.isOpen} onClose={() => props.onClose()} maxWidth="sm" fullWidth>
            <form onSubmit={handleSubmit}>
                <DialogTitle>{props.endpoint ? translations.editDevice : translations.addDevice}</DialogTitle>
                <DialogContent>
                    <TextField
                        required
                        autoFocus
                        margin="dense"
                        label={translations.deviceName}
                        type="text"
                        fullWidth
                        onChange={handleNameChange}
                        value={state.name}
                        error={showNameError}
                        helperText={showNameError ? translations.fieldIsNotValid : ''}
                    />
                    <TextField
                        required
                        margin="dense"
                        label={translations.host}
                        type="text"
                        fullWidth
                        onChange={handleHostChange}
                        value={state.host}
                        error={showHostError}
                        helperText={showHostError ? translations.fieldIsNotValid : ''}
                    />
                    <TextField
                        required
                        margin="dense"
                        label={translations.port}
                        type="number"
                        fullWidth
                        onChange={handlePortChange}
                        value={Number.isNaN(state.port) ? '' : state.port}
                        error={showPortError}
                        helperText={showPortError ? translations.fieldIsNotValid : ''}
                    />
                </DialogContent>
                <DialogActions>
                    <Button onClick={() => props.onClose()} color="primary">
                        {translations.cancel}
                    </Button>
                    <Button type="submit" variant="contained" color="primary" disabled={!isValid}>
                        {translations.ok}
                    </Button>
                </DialogActions>
            </form>
        </Dialog>
    );
}

export default AddEditEndpointDialog;
