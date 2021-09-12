import { Dialog, DialogTitle, DialogContent, TextField, DialogActions, Button } from '@material-ui/core';
import React, { useEffect, useState } from 'react';
import translations from '../../services/translations';

interface Props {
    name: string;
    isOpen: boolean;
    onClose(newValue: string | null): void;
}

interface State {
    name: string;
    isNameValid: boolean;
    isNameDirty: boolean;
}

function RenamePlayListDialog(props: Props) {
    const initialState: State = {
        name: props.name,
        isNameValid: true,
        isNameDirty: false,
    };

    const [state, setState] = useState(initialState);

    useEffect(() => {
        setState((s) => ({
            ...s,
            name: props.name,
        }));
    }, [props.name]);

    const handleNameChange = (event: React.ChangeEvent<HTMLInputElement>) => {
        const newVal = event.target.value;
        const newState = { ...state };
        newState.name = newVal;
        newState.isNameDirty = true;
        newState.isNameValid = newVal !== '' && newVal !== null && newVal.length <= 50 && newVal.length > 1;

        setState(newState);
    };

    const handleClose = (saveChanges: boolean): void => {
        const name = saveChanges ? state.name : null;
        props.onClose(name);
        setState(initialState);
    };

    const showError = !state.isNameValid && state.isNameDirty;

    if (!props.isOpen) {
        return null;
    }

    return (
        <Dialog open={props.isOpen} onClose={() => handleClose(false)} maxWidth="sm" fullWidth>
            <form onSubmit={() => handleClose(true)} >
                <DialogTitle>{translations.rename}</DialogTitle>
                <DialogContent>
                    <TextField
                        required
                        autoFocus
                        margin="dense"
                        label={translations.playList}
                        type="text"
                        fullWidth
                        onChange={handleNameChange}
                        value={state.name}
                        error={showError}
                        helperText={showError ? translations.fieldIsNotValid : ''}
                        InputProps={{
                            inputProps: {
                                maxLength: 50,
                            },
                        }}
                    />
                </DialogContent>
                <DialogActions>
                    <Button onClick={() => handleClose(false)} color="primary">
                        {translations.cancel}
                    </Button>
                    <Button type="submit" variant="contained" color="primary" disabled={!state.isNameValid}>
                        {translations.ok}
                    </Button>
                </DialogActions>
            </form>
        </Dialog>
    );
}

export default RenamePlayListDialog;
