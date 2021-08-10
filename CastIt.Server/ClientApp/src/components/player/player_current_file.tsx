import { Grid, makeStyles, Tooltip, Typography } from '@material-ui/core';
import { useEffect, useState } from 'react';
import { onPlayerStatusChanged, onFileEndReached } from '../../services/castithub.service';

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
}

const initialState: State = {
    title: '',
    subtitle: '',
};

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
                title: status.playedFile.name,
                subtitle: status.playedFile.subTitle,
                imageUrl: status.playedFile.thumbnailUrl,
            });
        });


        const onFileEndReachedSubscription = onFileEndReached.subscribe(_ => setState(initialState));
        return () => {
            playerStatusChangedSubscription.unsubscribe();
            onFileEndReachedSubscription.unsubscribe();
        };
    }, []);

    return (
        <Grid container wrap="nowrap" alignItems="center">
            <Grid item style={{ display: 'flex' }}>
                <img className={classes.image} src={state.imageUrl} alt="Current file" />
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
            </Grid>
        </Grid>
    );
}

export default PlayerCurrentFile;
