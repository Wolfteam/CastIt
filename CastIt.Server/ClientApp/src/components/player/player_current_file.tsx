import { LinearProgress, Tooltip, Typography } from '@mui/material';
import Grid from '@mui/material/Grid';
import { useEffect, useState } from 'react';
import { onPlayerStatusChanged, onFileEndReached, onFileLoading, onFileLoaded, onStoppedPlayback } from '../../services/castithub.service';
import { defaultImg } from '../../utils/app_constants';

interface Props {
    imageHeight: string;
}

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

function PlayerCurrentFile(props: Props) {
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
        <Grid container alignItems="center">
            <Grid>
                <img style={{ height: props.imageHeight, width: '100%', objectFit: 'fill' }} src={image} alt="Current file" />
            </Grid>
            <Grid direction="column" size="grow" className="text-overflow-elipsis" paddingX={1}>
                <Tooltip title={state.title}>
                    <Typography variant="h6" className="text-overflow-elipsis">
                        {state.title}
                    </Typography>
                </Tooltip>
                <Tooltip title={state.subtitle}>
                    <Typography className="text-overflow-elipsis" color="textSecondary">
                        {state.subtitle}
                    </Typography>
                </Tooltip>
                {state.loading ? <LinearProgress /> : null}
            </Grid>
        </Grid>
    );
}

export default PlayerCurrentFile;
