import React, { useState } from 'react';
import { Dialog, DialogContent, DialogTitle, DialogActions, Button, TextField, FormGroup } from '@mui/material';
import Grid from '@mui/material/Grid2';
import translations from '../../services/translations';

interface Props {
    isOpen: boolean;
    onClose(path: string | null): void;
}

interface State {
    path: string;
    isPathValid: boolean;
    isPathDirty: boolean;
}

const initialState: State = {
    path: '',
    isPathValid: false,
    isPathDirty: false,
};

function AddSubtitlesDialog(props: Props) {
    const [state, setState] = useState(initialState);

    const handlePathChange = (event: React.ChangeEvent<HTMLInputElement>): void => {
        const newVal = event.target.value;
        const newState = { ...state };
        newState.path = newVal;
        newState.isPathDirty = true;
        newState.isPathValid = newVal !== '' && newVal !== null && newVal.length > 1;

        setState(newState);
    };

    const handleClose = (saveChanges: boolean): void => {
        const path = saveChanges ? state.path : null;
        props.onClose(path);
        setState(initialState);
    };

    const showError = !state.isPathValid && state.isPathDirty;
    if (!props.isOpen) {
        return null;
    }
    return (
        <Dialog open={props.isOpen} onClose={() => handleClose(false)} maxWidth="sm" fullWidth>
            <form onSubmit={() => handleClose(true)}>
                <DialogTitle>{translations.subtitles}</DialogTitle>
                <DialogContent>
                    <Grid container alignItems="stretch" justifyContent="center">
                        <FormGroup row style={{ width: '100%' }}>
                            <TextField
                                required
                                autoFocus
                                margin="dense"
                                label={translations.path}
                                type="text"
                                fullWidth
                                onChange={handlePathChange}
                                value={state.path}
                                error={showError}
                                helperText={showError ? translations.fieldIsNotValid : ''}
                            />
                        </FormGroup>
                    </Grid>
                </DialogContent>
                <DialogActions>
                    <Button onClick={() => handleClose(false)} color="primary">
                        {translations.cancel}
                    </Button>
                    <Button type="submit" variant="contained" color="primary" disabled={!state.isPathValid}>
                        {translations.ok}
                    </Button>
                </DialogActions>
            </form>
        </Dialog>
    );
}

export default AddSubtitlesDialog;
