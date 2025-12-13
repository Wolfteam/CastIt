import { Box, Button, IconButton } from '@mui/material';
import Grid from '@mui/material/Grid';
import PlayerProgressIndicator from './player_progress_indicator';
import PlayerControls from './player_controls';
import PlayerVolume from './player_volume';
import PlayerSettings from './player_settings';
import PlayerDevices from './player_devices';
import PlayerCurrentFile from './player_current_file';
import PlayerFileOptions from './player_file_options';
import { ExpandLess, ExpandMore } from '@mui/icons-material';
import { useState } from 'react';

interface State {
    isExpanded: boolean;
}

const initialState: State = {
    isExpanded: true,
};

function Player() {
    const [state, setState] = useState(initialState);

    const handleToggleExpand = () =>
        setState({
            isExpanded: !state.isExpanded,
        });

    if (!state.isExpanded) {
        return (
            <Box sx={(theme) => ({ backgroundColor: theme.palette.primary.dark })}>
                <PlayerProgressIndicator />
                <Grid container justifyContent="center">
                    <Button
                        disableRipple={true}
                        disableTouchRipple={true}
                        disableFocusRipple={true}
                        sx={(theme) => ({
                            bottom: 0,
                            position: 'absolute',
                            overflowX: 'clip',
                            marginBottom: 4,
                            padding: 0,
                            zIndex: theme.zIndex.fab * 2,
                            backgroundColor: theme.palette.primary.dark,
                            borderRadius: '15px 15px 0px 0px',
                            '&:hover': {
                                backgroundColor: theme.palette.primary.dark,
                            },
                        })}
                        onClick={handleToggleExpand}
                    >
                        <ExpandLess fontSize="large" htmlColor="white" />
                    </Button>
                </Grid>
            </Box>
        );
    }

    const imageHeight = '120px';

    return (
        <Grid container justifyContent="center" alignItems="center" sx={(theme) => ({ backgroundColor: theme.palette.primary.dark })}>
            <Grid size={{ xs: 12, md: 'grow' }} alignItems="center" justifyContent="center" style={{ height: imageHeight }}>
                <PlayerCurrentFile imageHeight={imageHeight} />
            </Grid>
            <Grid size={{ xs: 12, md: 6 }} alignItems="center" justifyContent="center" direction="column">
                <PlayerControls />
                <PlayerProgressIndicator />
            </Grid>
            <Grid size={{ xs: 12, md: 'grow' }} container alignItems="center" justifyContent="center">
                <PlayerVolume />
                <PlayerSettings />
                <PlayerDevices />
                <PlayerFileOptions />
                <IconButton onClick={handleToggleExpand} size="large">
                    <ExpandMore fontSize="large" />
                </IconButton>
            </Grid>
        </Grid>
    );
}

export default Player;
