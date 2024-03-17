import { Box, Button, Grid, IconButton } from '@mui/material';
import PlayerProgressIndicator from './player_progress_indicator';
import PlayerControls from './player_controls';
import PlayerVolume from './player_volume';
import PlayerSettings from './player_settings';
import PlayerDevices from './player_devices';
import PlayerCurrentFile from './player_current_file';
import PlayerFileOptions from './player_file_options';
import { ExpandLess, ExpandMore } from '@mui/icons-material';
import { useState } from 'react';
import { createStyles, makeStyles } from '@mui/styles';

const useStyles = makeStyles((theme) =>
    createStyles({
        root: {
            backgroundColor: theme.palette.primary.dark,
        },
        expandButton: {
            bottom: 0,
            position: 'absolute',
            overflowX: 'clip',
            marginBottom: 30,
            padding: 0,
            zIndex: theme.zIndex.fab * 2,
            backgroundColor: theme.palette.primary.dark,
            '&:hover': {
                backgroundColor: theme.palette.primary.dark,
            },
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

    if (!state.isExpanded) {
        return (
            <Box className={classes.root}>
                <PlayerProgressIndicator />
                <Grid container justifyContent="center">
                    <Button
                        disableRipple={true}
                        disableTouchRipple={true}
                        disableFocusRipple={true}
                        className={classes.expandButton}
                        onClick={handleToggleExpand}
                    >
                        <ExpandLess fontSize="large" htmlColor="white" />
                    </Button>
                </Grid>
            </Box>
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
                    <IconButton onClick={handleToggleExpand} size="large">
                        <ExpandMore fontSize="large" />
                    </IconButton>
                </Grid>
            </Grid>
        </Grid>
    );
}

export default Player;
