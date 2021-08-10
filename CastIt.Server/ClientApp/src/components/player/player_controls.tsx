import { Grid, IconButton } from "@material-ui/core";
import { useEffect, useState } from "react";
import { SkipPrevious, SkipNext, FastForward, FastRewind, PlayArrow, Stop, Pause } from "@material-ui/icons";
import {
    onPlayerStatusChanged,
    togglePlayBack,
    skipSeconds,
    goTo,
    stopPlayBack,
} from "../../services/castithub.service";

interface State {
    isPlayingOrPaused: boolean;
    isPlaying: boolean;
    isPaused: boolean;
}

const initialState: State = {
    isPlayingOrPaused: false,
    isPlaying: false,
    isPaused: false,
};

function PlayerControls() {
    const [state, setState] = useState(initialState);

    useEffect(() => {
        const onPlayerStatusChangedSubscription = onPlayerStatusChanged.subscribe((status) => {
            setState({
                isPaused: status.player.isPaused,
                isPlaying: status.player.isPlaying,
                isPlayingOrPaused: status.player.isPlayingOrPaused,
            });
        });
        return () => {
            onPlayerStatusChangedSubscription.unsubscribe();
        };
    }, []);

    const handleSkipOrPrevious = async (next: boolean, previous: boolean): Promise<void> => {
        await goTo(next, previous);
    };

    const handleGoToSeconds = async (negative: boolean): Promise<void> => {
        await skipSeconds(negative ? -30 : 30);
    };

    const handleStopPlayback = async (): Promise<void> => {
        await stopPlayBack();
    };

    const handleTogglePlayBack = async (): Promise<void> => {
        await togglePlayBack();
    };

    return (
        <Grid container alignItems="stretch" justifyContent="center">
            <IconButton disabled={!state.isPlayingOrPaused} onClick={() => handleSkipOrPrevious(false, true)}>
                <SkipPrevious fontSize="large" />
            </IconButton>
            <IconButton disabled={!state.isPlayingOrPaused} onClick={() => handleGoToSeconds(true)}>
                <FastRewind fontSize="large" />
            </IconButton>
            <IconButton disabled={!state.isPlayingOrPaused} onClick={handleTogglePlayBack}>
                {state.isPaused ? <PlayArrow fontSize="large" /> : <Pause fontSize="large" />}
            </IconButton>
            <IconButton disabled={!state.isPlayingOrPaused} onClick={handleStopPlayback}>
                <Stop fontSize="large" />
            </IconButton>
            <IconButton disabled={!state.isPlayingOrPaused} onClick={() => handleGoToSeconds(false)}>
                <FastForward fontSize="large" />
            </IconButton>
            <IconButton disabled={!state.isPlayingOrPaused} onClick={() => handleSkipOrPrevious(true, false)}>
                <SkipNext fontSize="large" />
            </IconButton>
        </Grid>
    );
}

export default PlayerControls;
