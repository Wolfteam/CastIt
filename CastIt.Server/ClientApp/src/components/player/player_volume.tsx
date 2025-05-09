import { IconButton, Popover, Slider } from '@mui/material';
import Grid from '@mui/material/Grid2';
import { useEffect, useState } from 'react';
import { Fragment } from 'react';
import { VolumeOff, VolumeUp } from '@mui/icons-material';
import { usePopupState, bindTrigger, bindPopover } from 'material-ui-popup-state/hooks';
import { onPlayerStatusChanged } from '../../services/castithub.service';
import { useCastItHub } from '../../context/castit_hub.context';

interface State {
    volume: number;
    isMuted: boolean;
    isConnected: boolean;
    isVolumeChanging: boolean;
}

const initialState: State = {
    volume: 100,
    isMuted: false,
    isConnected: false,
    isVolumeChanging: false,
};

function PlayerVolume() {
    const [state, setState] = useState(initialState);
    const castItHub = useCastItHub();

    const popupState = usePopupState({
        variant: 'popover',
        popupId: 'volumePopup',
    });

    useEffect(() => {
        const onPlayerStatusChangedSubscription = onPlayerStatusChanged.subscribe((status) => {
            if (state.isVolumeChanging) {
                return;
            }

            if (!status) {
                return;
            }

            if (status.playedFile) {
                setState({
                    volume: status.player.volumeLevel,
                    isMuted: status.player.isMuted,
                    isConnected: status.player.isPlayingOrPaused,
                    isVolumeChanging: false,
                });
                return;
            }

            setState((s) => ({
                ...s,
                isConnected: status.player.isPlayingOrPaused,
            }));
        });

        return () => {
            onPlayerStatusChangedSubscription.unsubscribe();
        };
    }, [state.isVolumeChanging]);

    const handleVolumeChange = async (volume: number, isMuted: boolean, commited: boolean = false): Promise<void> => {
        setState((s) => ({ ...s, isVolumeChanging: !commited, volume: volume, isMuted: isMuted }));
        if (commited) {
            await castItHub.connection.setVolume(volume, isMuted);
        }
    };

    return (
        <Fragment>
            <IconButton disabled={!state.isConnected} {...bindTrigger(popupState)} size="large">
                <VolumeUp fontSize="large" />
            </IconButton>
            <Popover
                {...bindPopover(popupState)}
                slotProps={{
                    paper: {
                        style: {
                            width: 320,
                            overflowY: 'hidden',
                        },
                    },
                }}
                anchorOrigin={{
                    vertical: 'bottom',
                    horizontal: 'center',
                }}
                transformOrigin={{
                    vertical: 'top',
                    horizontal: 'center',
                }}
            >
                <Grid
                    container
                    alignItems="center"
                    justifyContent="center"
                    style={{
                        paddingLeft: 10,
                        paddingRight: 10,
                    }}
                >
                    <Grid size={2}>
                        <IconButton onClick={() => handleVolumeChange(state.volume, !state.isMuted, true)} size="large">
                            {state.isMuted ? <VolumeOff fontSize="medium" /> : <VolumeUp fontSize="medium" />}
                        </IconButton>
                    </Grid>
                    <Grid size={10} style={{ paddingRight: 10 }}>
                        <Slider
                            step={0.1}
                            min={0}
                            max={1}
                            marks
                            disabled={!state.isConnected}
                            value={state.volume}
                            onChange={(e, val) => handleVolumeChange(val as number, state.isMuted)}
                            onChangeCommitted={(e, val) => handleVolumeChange(val as number, state.isMuted, true)}
                            style={{
                                marginTop: 6,
                            }}
                        />
                    </Grid>
                </Grid>
            </Popover>
        </Fragment>
    );
}

export default PlayerVolume;
