import { createStyles, makeStyles, Typography } from '@material-ui/core';
import formatDuration from 'format-duration';
import React, { useEffect, useState } from 'react';
import { AppFile } from '../../enums';
import { IFileThumbnailRangeResponseDto } from '../../models';
import { onPlayerStatusChanged } from '../../services/castithub.service';
import { thumbnailImgHeight, thumbnailImgWidth } from '../../utils/app_constants';

const useStyles = makeStyles(() =>
    createStyles({
        root: {
            display: 'inline-block',
            position: 'relative',
            overflow: 'hidden',
            width: thumbnailImgWidth,
            height: thumbnailImgHeight,
            borderRadius: 20,
        },
        image: {
            width: thumbnailImgWidth,
            height: thumbnailImgHeight,
            position: 'absolute',
            transformOrigin: 'left top',
        },
        text: {
            position: 'absolute',
            right: 16,
            bottom: 8,
            fontWeight: 'bold',
            color: 'white',
        },
    })
);

interface Props {
    second: number;
}

interface State {
    x: number;
    y: number;
    url: string;
    useTransform: boolean;
}

const initialState: State = {
    x: 0,
    y: 0,
    url: '',
    useTransform: true,
};

const getPositionToUse = (second: number, thumbnailRanges: IFileThumbnailRangeResponseDto[]): State | null => {
    const range = thumbnailRanges.flatMap((r) => r.thumbnailRange).find((x) => x.minimum <= second && x.maximum >= second);
    if (!range) {
        console.log(`No range for second = ${second}`);
        return null;
    }

    const thumbRange = thumbnailRanges.find((f) => f.thumbnailRange === range);
    const position =
        thumbRange?.thumbnailPositions.find((p) => p.second === second) ??
        thumbRange?.thumbnailPositions.find((p) => p.second === second - 1);

    if (!position) {
        console.log('No position for thumbrange', thumbRange);
        return null;
    }

    const x = -position.x * thumbnailImgWidth;
    const y = -position.y * thumbnailImgHeight;

    return {
        x: x,
        y: y,
        url: thumbRange!.previewThumbnailUrl,
        useTransform: true,
    };
};

function PlayerProgressIndicatorThumbnail(props: Props) {
    const classes = useStyles();
    const [state, setState] = useState(initialState);

    useEffect(() => {
        const onPlayerStatusChangedSubscription = onPlayerStatusChanged.subscribe((status) => {
            if (!status) {
                return;
            }

            if ((status.playedFile!.type & AppFile.localVideo) !== AppFile.localVideo) {
                setState({ ...initialState, url: status.thumbnailRanges[0].previewThumbnailUrl, useTransform: false });
            } else {
                const second = props.second;
                const newState = getPositionToUse(second, status.thumbnailRanges);
                if (!newState) {
                    return;
                }
                setState(newState);
            }
        });
        return () => {
            onPlayerStatusChangedSubscription.unsubscribe();
        };
    }, [props.second]);

    const transform = state.useTransform ? `matrix(1, 0, 0, 1, ${state.x}, ${state.y}) scale(5, 5) ` : '';
    const elapsed = formatDuration(props.second * 1000, { leading: true });
    return (
        <div className={classes.root}>
            <img className={classes.image} style={{ transform: transform }} src={state.url} alt="Preview thumbnail" />
            <Typography className={classes.text}>{elapsed}</Typography>
        </div>
    );
}

export default React.memo(PlayerProgressIndicatorThumbnail);
