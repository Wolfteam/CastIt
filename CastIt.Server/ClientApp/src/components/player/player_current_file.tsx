import { Grid, LinearProgress, makeStyles, Tooltip, Typography } from '@material-ui/core';
import { useEffect, useState } from 'react';
import { onPlayerStatusChanged, onFileEndReached, onFileLoading, onFileLoaded, onStoppedPlayback } from '../../services/castithub.service';

const useStyles = makeStyles({
    image: {
        height: 100,
        objectFit: 'contain',
    },
    text: {
        textOverflow: 'ellipsis',
        overflow: 'hidden',
        whiteSpace: 'nowrap',
    },
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

const defaultImg = `${process.env.PUBLIC_URL}/no_img.png`;

function PlayerCurrentFile() {
    const classes = useStyles();
    const [state, setState] = useState(initialState);

    useEffect(() => {
        const playerStatusChangedSubscription = onPlayerStatusChanged.subscribe((status) => {
            if (!status.playedFile) {
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
                <img className={classes.image} src={image} alt="Current file" />
            </Grid>
            <Grid item className={classes.text} style={{ paddingLeft: '10px' }}>
                <Tooltip title={state.title}>
                    <Typography variant="h5" component="h2" className={classes.text}>
                        {state.title}
                    </Typography>
                </Tooltip>
                <Tooltip title={state.subtitle}>
                    <Typography color="textSecondary" className={classes.text}>
                        {state.subtitle}
                    </Typography>
                </Tooltip>
                {state.loading ? <LinearProgress /> : null}
            </Grid>
        </Grid>
    );
}

export default PlayerCurrentFile;
