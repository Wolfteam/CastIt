import { makeStyles, Grid, createStyles } from '@material-ui/core';
import PlayerProgressIndicator from './player_progress_indicator';
import PlayerControls from './player_controls';
import PlayerVolume from './player_volume';
import PlayerSettings from './player_settings';
import PlayerDevices from './player_devices';
import PlayerCurrentFile from './player_current_file';

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

function Player() {
    const classes = useStyles();
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
                </Grid>
            </Grid>
        </Grid>
    );
}

export default Player;
