import React, {useEffect, useState} from 'react';
import {
    onFileEndReached,
    onFileLoaded,
    onFileLoading,
    onPlayListBusy,
    onStoppedPlayback
} from '../../services/castithub.service';
import {IFileItemResponseDto} from '../../models';
import {LinearProgress} from '@mui/material';
import {createStyles, makeStyles} from '@mui/styles';

const useStyles = makeStyles((theme) =>
    createStyles({
        loading: {
            position: 'sticky',
            top: 0,
        },
    })
);

interface Props {
    playListId: number;
}

function PlayListLoadingIndicator(props: Props) {
    const [isBusy, setIsBusy] = useState(false);
    const classes = useStyles();

    useEffect(() => {
        const handleFileChanged = (file: IFileItemResponseDto, isBusy: boolean): void => {
            if (file.playListId !== props.playListId) {
                return;
            }

            setIsBusy(isBusy);
        };

        const onPlayListBusySubscription = onPlayListBusy.subscribe((busy) => setIsBusy(busy.isBusy));

        const onFileLoadingSubscription = onFileLoading.subscribe((file) => handleFileChanged(file, true));

        const onFileLoadedSubscription = onFileLoaded.subscribe((file) => handleFileChanged(file, false));

        const onFileEndReachedSubscription = onFileEndReached.subscribe((file) => handleFileChanged(file, false));

        const onStoppedPlaybackSubscription = onStoppedPlayback.subscribe(() => setIsBusy(false));

        return () => {
            onPlayListBusySubscription.unsubscribe();
            onFileLoadingSubscription.unsubscribe();
            onFileLoadedSubscription.unsubscribe();
            onFileEndReachedSubscription.unsubscribe();
            onStoppedPlaybackSubscription.unsubscribe();
        };
    }, [props.playListId]);
    const loading = !isBusy ? null : <LinearProgress variant="indeterminate" className={classes.loading}/>;
    return loading;
}

export default React.memo(PlayListLoadingIndicator);
