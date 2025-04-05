import { Typography } from '@mui/material';
import formatDuration from 'format-duration';
import React, { useEffect, useState } from 'react';
import { AppFile } from '../../enums';
import { IFileThumbnailRangeResponseDto } from '../../models';
import { onPlayerStatusChanged } from '../../services/castithub.service';
import { thumbnailImgHeight, thumbnailImgWidth, thumbnailsPerImageRow } from '../../utils/app_constants';

interface Props {
    second: number;
}

interface State {
    x: number;
    y: number;
    url?: string;
    useTransform: boolean;
}

const initialState: State = {
    x: 0,
    y: 0,
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
    const [state, setState] = useState(initialState);

    useEffect(() => {
        const onPlayerStatusChangedSubscription = onPlayerStatusChanged.subscribe((status) => {
            if (!status) {
                return;
            }
            if ((status.playedFile!.type & AppFile.localVideo) !== AppFile.localVideo) {
                const url = status.thumbnailRanges[0].previewThumbnailUrl ?? status.playedFile?.thumbnailUrl;
                setState({ ...initialState, url: url, useTransform: false });
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

    const transform = state.useTransform ? `matrix(${thumbnailsPerImageRow}, 0, 0, ${thumbnailsPerImageRow}, ${state.x}, ${state.y})` : '';
    const elapsed = formatDuration(props.second * 1000, { leading: true });
    return (
        <div
            style={{
                overflow: 'hidden',
                borderRadius: 20,
                width: thumbnailImgWidth,
                height: thumbnailImgHeight,
            }}
        >
            <img
                style={{
                    width: thumbnailImgWidth,
                    height: thumbnailImgHeight,
                    transformOrigin: 'left top',
                    transform: transform,
                }}
                src={state.url}
                alt="Preview thumbnail"
            />
            <Typography
                sx={{
                    position: 'absolute',
                    width: '100%',
                    right: 16,
                    bottom: 8,
                    textAlign: 'end',
                    fontWeight: 'bold',
                    color: 'white',
                    textShadow: '1px 1px 2px black',
                }}
            >
                {elapsed}
            </Typography>
        </div>
    );
}

export default React.memo(PlayerProgressIndicatorThumbnail);
