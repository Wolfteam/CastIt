import { Fragment } from 'react';
import { Typography, Tooltip } from '@mui/material';
import FileItemSlider from './file_item_slider';

interface Props {
    subTitle: string;
    path: string;
    playedPercentage: number;
    playedTime: string;
    duration: string;
    lastPlayedDate: string;
}

function FileItemSubtitle(props: Props) {
    return (
        <Fragment>
            <Tooltip title={props.subTitle}>
                <Typography variant="caption" sx={{ display: 'block' }} className={'text-overflow-elipsis'}>
                    {props.subTitle}
                </Typography>
            </Tooltip>
            <Tooltip title={props.path}>
                <Typography variant="caption" sx={{ display: 'block' }} className={'text-overflow-elipsis'}>
                    {props.path}
                </Typography>
            </Tooltip>
            <Tooltip title={props.lastPlayedDate}>
                <Typography variant="caption" sx={{ display: 'block' }} className={'text-overflow-elipsis'}>
                    {props.lastPlayedDate}
                </Typography>
            </Tooltip>
            <FileItemSlider {...props} />
        </Fragment>
    );
}

export default FileItemSubtitle;
