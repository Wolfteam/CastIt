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

const initialState: State = {
    name: '',
    isNameValid: true,
    isNameDirty: false,
};

function RenamePlayListDialog(props: Props) {
    const [state, setState] = useState(initialState);

    useEffect(() => {
        setState((s) => ({
            ...s,
            name: props.name,
        }));
    }, []);

    const handleNameChange = (event: React.ChangeEvent<HTMLInputElement>) => {
        const newVal = event.target.value;
        const newState = { ...state };
        newState.name = newVal;
        newState.isNameDirty = true;
        newState.isNameValid = newVal !== '' && newVal !== null && newVal.length <= 50 && newVal.length > 1;

        setState(newState);
    };

    const showError = !state.isNameValid && state.isNameDirty;

    return (
        <Dialog open={props.isOpen} onClose={() => props.onClose(null)} aria-labelledby="form-dialog-title">
            <DialogTitle id="form-dialog-title">{translations.rename}</DialogTitle>
            <DialogContent>
                {/* <DialogContentText>
                    {props.name}
                </DialogContentText> */}
                <TextField
                    required
                    autoFocus
                    margin="dense"
                    label="Name"
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
                <Button onClick={() => props.onClose(null)} color="primary">
                    {translations.cancel}
                </Button>
                <Button variant="contained" onClick={() => props.onClose(state.name)} color="primary">
                    {translations.ok}
                </Button>
            </DialogActions>
        </Dialog>
    );
}

export default RenamePlayListDialog;
