import {
    Dialog,
    DialogContent,
    DialogTitle,
    Grid,
    DialogActions,
    Button,
    TextField,
    FormGroup,
    Switch,
    FormControlLabel,
} from '@material-ui/core';
import React, { useState } from 'react';
import translations from '../../services/translations';

interface Props {
    isOpen: boolean;
    onClose(path: string | null, includeSubFolder: boolean, onlyVideo: boolean): void;
}

interface State {
    path: string;
    isPathValid: boolean;
    isPathDirty: boolean;
    includeSubFolder: boolean;
    onlyVideo: boolean;
}

const initialState: State = {
    path: '',
    isPathValid: false,
    isPathDirty: false,
    includeSubFolder: true,
    onlyVideo: true,
};

function AddFilesDialog(props: Props) {
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
        props.onClose(path, state.includeSubFolder, state.onlyVideo);
        setState(initialState);
    };

    const showError = !state.isPathValid && state.isPathDirty;

    return (
        <Dialog open={props.isOpen} onClose={() => handleClose(false)}>
            <DialogTitle id="form-dialog-title">{translations.addFiles}</DialogTitle>
            <DialogContent>
                <Grid container alignItems="flex-start" justifyContent="space-between">
                    <FormGroup row>
                        <TextField
                            required
                            autoFocus
                            margin="dense"
                            label={'Path'}
                            type="text"
                            fullWidth
                            onChange={handlePathChange}
                            value={state.path}
                            error={showError}
                            helperText={showError ? translations.fieldIsNotValid : ''}
                        />
                        <FormControlLabel
                            control={
                                <Switch
                                    checked={state.includeSubFolder}
                                    onChange={(_, checked) => setState((s) => ({ ...s, includeSubFolder: checked }))}
                                    color="primary"
                                />
                            }
                            label={translations.includeSubFolders}
                        />
                        <FormControlLabel
                            control={
                                <Switch
                                    checked={state.onlyVideo}
                                    onChange={(_, checked) => setState((s) => ({ ...s, onlyVideo: checked }))}
                                    color="primary"
                                />
                            }
                            label={translations.onlyVideo}
                        />
                    </FormGroup>
                </Grid>
            </DialogContent>
            <DialogActions>
                <Button onClick={() => handleClose(false)} color="primary">
                    {translations.cancel}
                </Button>
                <Button variant="contained" onClick={() => handleClose(true)} color="primary" disabled={!state.isPathValid}>
                    {translations.ok}
                </Button>
            </DialogActions>
        </Dialog>
    );
}

export default AddFilesDialog;
