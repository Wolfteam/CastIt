import { Grid, Slider, Typography } from '@mui/material';
import { useEffect, useState } from 'react';
import { useCastItHub } from '../../context/castit_hub.context';
import { onPlayerStatusChanged } from '../../services/castithub.service';
import PlayerProgressIndicatorValue from './player_progress_indicator_value';

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
    const castItHub = useCastItHub();

    useEffect(() => {
        const onPlayerStatusChangedSubscription = onPlayerStatusChanged.subscribe((status) => {
            if (state.isValueChanging) {
                return;
            }

            if (!status) {
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
        //this can happen on live streams
        if (seconds < 0) {
            return;
        }

        if (committed) {
            await castItHub.connection.gotoSeconds(seconds);
        }

        setState((s) => ({
            ...s,
            elapsedSeconds: seconds,
            isValueChanging: !committed,
        }));
    };

    const isPlaying = state.duration && state.duration != '';

    return (
        <Grid container spacing={1} style={{ paddingRight: 10, paddingLeft: 10 }} alignItems="center" justifyContent="center">
            {isPlaying && (
                <Grid size="grow">
                    <Typography color="textSecondary" textAlign={'center'} textOverflow={'ellipsis'}>
                        {state.playedTime}
                    </Typography>
                </Grid>
            )}
            <Grid size={isPlaying ? 9 : 12}>
                <Slider
                    min={0}
                    max={state.mediaDuration}
                    step={1}
                    valueLabelDisplay="auto"
                    style={{ marginTop: '5px' }}
                    size="small"
                    disabled={!state.isPlayingOrPaused}
                    value={state.elapsedSeconds}
                    onChange={(e, val) => handleValueChanged(val as number)}
                    onChangeCommitted={(e, val) => handleValueChanged(val as number, true)}
                    slots={{ valueLabel: PlayerProgressIndicatorValue }}
                />
            </Grid>
            {isPlaying && (
                <Grid size="grow">
                    <Typography color="textSecondary" textAlign={'center'} textOverflow={'ellipsis'}>
                        {state.duration}
                    </Typography>
                </Grid>
            )}
        </Grid>
    );
}

export default PlayerProgressIndicator;
