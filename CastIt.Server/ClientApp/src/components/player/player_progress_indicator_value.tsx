import {Tooltip} from '@mui/material';
import {thumbnailImgHeight, thumbnailImgWidth} from '../../utils/app_constants';
import PlayerProgressIndicatorThumbnail from './player_progress_indicator_thumbnail';
import {withStyles} from "@mui/styles";

interface Props {
    children: React.ReactElement;
    open: boolean;
    value: number;
}

const CustomTooltip = withStyles({
    tooltip: {
        backgroundColor: 'transparent',
        width: thumbnailImgWidth,
        height: thumbnailImgHeight,
        padding: 0,
        margin: 0,
    }
})(Tooltip);

function PlayerProgressIndicatorValue(props: Props) {
    const {children, open, value} = props;
    const second = value < 0 ? 0 : Math.round(value);

    return (
        <CustomTooltip
            open={open}
            enterTouchDelay={0}
            placement="top"
            arrow
            title={<PlayerProgressIndicatorThumbnail second={second} />}
        >
            {children}
        </CustomTooltip>
    );
}

export default PlayerProgressIndicatorValue;
