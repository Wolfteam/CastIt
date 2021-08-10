import { Grid, Slider, Typography } from '@material-ui/core';
import { useEffect, useState } from 'react';
import { onPlayerStatusChanged, gotoPosition } from '../../services/castithub.service';

interface State {
    playedTime: string;
    duration: string;
    playedPercentage: number;
    isPlayingOrPaused: boolean;
    isValueChanging: boolean;
}

const initialState: State = {
    playedTime: '',
    duration: '',
    playedPercentage: 0,
    isPlayingOrPaused: false,
    isValueChanging: false,
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
                    playedPercentage: status.player.playedPercentage,
                    isPlayingOrPaused: status.player.isPlayingOrPaused,
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

    const handleValueChanged = async (val: number, committed: boolean = false): Promise<void> => {
        if (committed) {
            await gotoPosition(val);
        }

        setState((s) => ({
            ...s,
            playedPercentage: val,
            isValueChanging: !committed,
        }));
    };

    //TODO: FORMAT WHILE DRAGGING
    return (
        <Grid container spacing={2} style={{ paddingRight: 10, paddingLeft: 10 }}>
            <Grid item>
                <Typography color="textSecondary">{state.playedTime}</Typography>
            </Grid>
            <Grid item xs>
                <Slider
                    min={1}
                    max={100}
                    step={1}
                    valueLabelDisplay="auto"
                    disabled={!state.isPlayingOrPaused}
                    value={state.playedPercentage}
                    valueLabelFormat={(_) => state.playedTime}
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
