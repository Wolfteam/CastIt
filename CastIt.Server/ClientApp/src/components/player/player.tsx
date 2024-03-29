import { makeStyles, Grid, createStyles, IconButton } from '@material-ui/core';
import PlayerProgressIndicator from './player_progress_indicator';
import PlayerControls from './player_controls';
import PlayerVolume from './player_volume';
import PlayerSettings from './player_settings';
import PlayerDevices from './player_devices';
import PlayerCurrentFile from './player_current_file';
import PlayerFileOptions from './player_file_options';
import { ExpandLess, ExpandMore } from '@material-ui/icons';
import { useState } from 'react';

const useStyles = makeStyles((theme) =>
    createStyles({
        root: {
            bottom: 0,
            left: 0,
            right: 0,
            position: 'sticky',
            overflowX: 'clip',
            backgroundColor: theme.palette.primary.dark,
        },
    })
);

interface State {
    isExpanded: boolean;
}

const initialState: State = {
    isExpanded: true,
};

function Player() {
    const classes = useStyles();
    const [state, setState] = useState(initialState);

    const handleToggleExpand = () =>
        setState({
            isExpanded: !state.isExpanded,
        });

    const toggleExpandButton = (
        <IconButton onClick={handleToggleExpand}>
            {state.isExpanded ? <ExpandMore fontSize="large" /> : <ExpandLess fontSize="large" />}
        </IconButton>
    );

    if (!state.isExpanded) {
        return (
            <Grid container className={classes.root} justifyContent="center" alignItems="center">
                <Grid item xs={10} md={11}>
                    <PlayerProgressIndicator />
                </Grid>
                <Grid item xs={2} md={1} style={{ textAlign: 'center' }}>
                    {toggleExpandButton}
                </Grid>
            </Grid>
        );
    }

    return (
        <Grid container className={classes.root} justifyContent="center" alignItems="center">
            <Grid item xs={12} md={3}>
                <PlayerCurrentFile />
            </Grid>
            <Grid item xs={12} md={6}>
                <Grid container direction="column">
                    <Grid item>
                        <PlayerControls />
                    </Grid>
                    <Grid item>
                        <PlayerProgressIndicator />
                    </Grid>
                </Grid>
            </Grid>
            <Grid item xs={12} md={3}>
                <Grid container alignItems="center" justifyContent="center">
                    <PlayerVolume />
                    <PlayerSettings />
                    <PlayerDevices />
                    <PlayerFileOptions />
                    {toggleExpandButton}
                </Grid>
            </Grid>
        </Grid>
    );
}

export default Player;
