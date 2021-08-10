import { Grid, IconButton, Popover, Slider } from "@material-ui/core";
import { useEffect, useState } from "react";
import { Fragment } from "react";
import { VolumeDown, VolumeUp } from "@material-ui/icons";
import { usePopupState, bindTrigger, bindPopover } from "material-ui-popup-state/hooks";
import { onPlayerStatusChanged, setVolume } from "../../services/castithub.service";

interface State {
    volume: number;
    isConnected: boolean;
    isVolumeChanging: boolean;
}

const initialState: State = {
    volume: 100,
    isConnected: false,
    isVolumeChanging: false,
};

function PlayerVolume() {
    const [state, setState] = useState(initialState);

    const popupState = usePopupState({
        variant: "popover",
        popupId: "volumePopup",
    });

    useEffect(() => {
        const onPlayerStatusChangedSubscription = onPlayerStatusChanged.subscribe((status) => {
            if (state.isVolumeChanging) {
                return;
            }

            if (status.playedFile) {
                setState({
                    volume: status.player.volumeLevel,
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

    const handleVolumeChange = async (val: number, commited: boolean = false): Promise<void> => {
        //TODO: MUTED
        if (commited) {
            await setVolume(val, false);
        }
        setState((s) => ({ ...s, isVolumeChanging: !commited, volume: val }));
    };

    return (
        <Fragment>
            <IconButton {...bindTrigger(popupState)}>
                <VolumeUp fontSize="large" />
            </IconButton>
            <Popover
                {...bindPopover(popupState)}
                PaperProps={{
                    style: {
                        width: 300,
                        overflowY: "hidden",
                    },
                }}
                anchorOrigin={{
                    vertical: "bottom",
                    horizontal: "center",
                }}
                transformOrigin={{
                    vertical: "top",
                    horizontal: "center",
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
                    <Grid item xs={1}>
                        <VolumeDown />
                    </Grid>
                    <Grid item xs={10} style={{ paddingRight: 10 }}>
                        <Slider
                            step={0.1}
                            min={0}
                            max={1}
                            marks
                            disabled={!state.isConnected}
                            value={state.volume}
                            onChange={(e, val) => handleVolumeChange(val as number)}
                            onChangeCommitted={(e, val) => handleVolumeChange(val as number, true)}
                        />
                    </Grid>
                    <Grid item xs={1}>
                        <VolumeUp />
                    </Grid>
                </Grid>
            </Popover>
        </Fragment>
    );
}

export default PlayerVolume;
