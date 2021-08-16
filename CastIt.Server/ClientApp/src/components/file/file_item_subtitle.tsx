import { Fragment } from 'react';
import { Typography, Tooltip } from '@material-ui/core';
import FileItemSlider from './file_item_slider';

interface Props {
    subTitle: string;
    path: string;
    playedPercentage: number;
    playedTime: string;
    duration: string;
}

function FileItemSubtitle(props: Props) {
    return (
        <Fragment>
            <Tooltip title={props.subTitle}>
                <Typography variant="subtitle1" className={'text-overflow-elipsis'}>
                    {props.subTitle}
                </Typography>
            </Tooltip>
            <Tooltip title={props.path}>
                <Typography variant="subtitle2" className={'text-overflow-elipsis'}>
                    {props.path}
                </Typography>
            </Tooltip>
            <FileItemSlider {...props} />
        </Fragment>
    );
}

export default FileItemSubtitle;
