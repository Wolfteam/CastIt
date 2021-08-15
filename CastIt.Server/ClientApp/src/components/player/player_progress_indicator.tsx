import { Grid, Slider, Typography } from '@material-ui/core';
import formatDuration from 'format-duration';
import { useEffect, useState } from 'react';
import { onPlayerStatusChanged, gotoSeconds } from '../../services/castithub.service';

interface State {
    playedTime: string;
    duration: string;
    mediaDuration: number;
    elapsedSeconds: number;
    isPlayingOrPaused: boolean;
    isValueChanging: boolean;
}

const initialState: State = {
    playedTime: '',
    duration: '',
    isPlayingOrPaused: false,
    isValueChanging: false,
    elapsedSeconds: 0,
    mediaDuration: 1,
};

function PlayerProgressIndicator() {
    const [state, setState] = useState(initialState);

    useEffect(() => {
        const onPlayerStatusChangedSubscription = onPlayerStatusChanged.subscribe((status) => {
            if (state.isValueChanging) {
                return;
            }
            if (status.playedFile) {
                setState({
                    duration: status.playedFile.duration,
                    playedTime: status.playedFile.playedTime,
                    isPlayingOrPaused: status.player.isPlayingOrPaused,
                    mediaDuration: status.player.currentMediaDuration,
                    elapsedSeconds: status.player.elapsedSeconds,
                    isValueChanging: false,
                });
                return;
            }

            setState(initialState);
        });
        return () => {
            onPlayerStatusChangedSubscription.unsubscribe();
        };
    }, [state.isValueChanging]);

    const handleValueChanged = async (seconds: number, committed: boolean = false): Promise<void> => {
        if (committed) {
            await gotoSeconds(seconds);
        }

        setState((s) => ({
            ...s,
            elapsedSeconds: seconds,
            isValueChanging: !committed,
        }));
    };

    return (
        <Grid container spacing={2} style={{ paddingRight: 10, paddingLeft: 10 }}>
            <Grid item>
                <Typography color="textSecondary">{state.playedTime}</Typography>
            </Grid>
            <Grid item xs>
                <Slider
                    min={0}
                    max={state.mediaDuration}
                    step={1}
                    valueLabelDisplay="auto"
                    disabled={!state.isPlayingOrPaused}
                    value={state.elapsedSeconds}
                    valueLabelFormat={(val) => formatDuration(val * 1000, { leading: true })}
                    getAriaValueText={(val) => formatDuration(val * 1000, { leading: true })}
                    onChange={(e, val) => handleValueChanged(val as number)}
                    onChangeCommitted={(e, val) => handleValueChanged(val as number, true)}
                />
            </Grid>
            <Grid item>
                <Typography color="textSecondary">{state.duration}</Typography>
            </Grid>
        </Grid>
    );
}

export default PlayerProgressIndicator;