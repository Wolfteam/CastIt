import { Tooltip } from '@mui/material';
import { thumbnailImgHeight, thumbnailImgWidth } from '../../utils/app_constants';
import PlayerProgressIndicatorThumbnail from './player_progress_indicator_thumbnail';

interface Props {
    children: React.ReactElement;
    open: boolean;
    value: number;
}

function PlayerProgressIndicatorValue(props: Props) {
    const { children, open, value } = props;
    const second = value < 0 ? 0 : Math.round(value);

    return (
        <Tooltip
            open={open}
            enterTouchDelay={0}
            placement="top"
            arrow
            sx={{
                backgroundColor: 'transparent',
                width: thumbnailImgWidth,
                height: thumbnailImgHeight,
                padding: 0,
                margin: 0,
            }}
            title={<PlayerProgressIndicatorThumbnail second={second} />}
        >
            {children}
        </Tooltip>
    );
}

export default PlayerProgressIndicatorValue;
