import { Grid, LinearProgress, Tooltip, Typography } from '@mui/material';
import { styled } from '@mui/material/styles';
import { useEffect, useState } from 'react';
import { onPlayerStatusChanged, onFileEndReached, onFileLoading, onFileLoaded, onStoppedPlayback } from '../../services/castithub.service';
import { defaultImg } from '../../utils/app_constants';

const StyledTypography = styled(Typography)({
    textOverflow: 'ellipsis',
    overflow: 'hidden',
    whiteSpace: 'nowrap',
});

interface State {
    title: string;
    subtitle: string;
    imageUrl?: string;
    loading?: boolean;
}

const initialState: State = {
    title: '',
    subtitle: '',
};

function PlayerCurrentFile() {
    const [state, setState] = useState(initialState);

    useEffect(() => {
        const playerStatusChangedSubscription = onPlayerStatusChanged.subscribe((status) => {
            if (!status || !status.playedFile) {
                setState(initialState);
                return;
            }

            setState({
                title: status.playedFile.filename,
                subtitle: status.playedFile.subTitle,
                imageUrl: status.playedFile.thumbnailUrl,
            });
        });

        const onFileLoadingSubscription = onFileLoading.subscribe((_) => {
            setState((s) => ({ ...s, loading: true }));
        });

        const onFileLoadedSubscription = onFileLoaded.subscribe((_) => {
            setState((s) => ({ ...s, loading: false }));
        });

        const onFileEndReachedSubscription = onFileEndReached.subscribe((_) => setState(initialState));

        const onStoppedPlaybackSubscription = onStoppedPlayback.subscribe(() => setState(initialState));
        return () => {
            playerStatusChangedSubscription.unsubscribe();
            onFileLoadingSubscription.unsubscribe();
            onFileLoadedSubscription.unsubscribe();
            onFileEndReachedSubscription.unsubscribe();
            onStoppedPlaybackSubscription.unsubscribe();
        };
    }, []);

    const image = state.imageUrl ?? defaultImg;

    return (
        <Grid container wrap="nowrap" alignItems="center">
            <Grid item style={{ display: 'flex' }}>
                <img style={{ height: 100, objectFit: 'contain' }} src={image} alt="Current file" />
            </Grid>
            <Grid item style={{ paddingLeft: '10px' }}>
                <Tooltip title={state.title}>
                    <StyledTypography variant="h5">
                        {state.title}
                    </StyledTypography>
                </Tooltip>
                <Tooltip title={state.subtitle}>
                    <StyledTypography color="textSecondary">{state.subtitle}</StyledTypography>
                </Tooltip>
                {state.loading ? <LinearProgress /> : null}
            </Grid>
        </Grid>
    );
}

export default PlayerCurrentFile;
