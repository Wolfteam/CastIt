import { Tooltip, TooltipProps, styled, tooltipClasses } from '@mui/material';
import { thumbnailImgHeight, thumbnailImgWidth } from '../../utils/app_constants';
import PlayerProgressIndicatorThumbnail from './player_progress_indicator_thumbnail';

const StyledTooltip = styled(({ className, ...props }: TooltipProps) => <Tooltip {...props} classes={{ popper: className }} />)({
    [`& .${tooltipClasses.tooltip}`]: {
        backgroundColor: 'transparent',
        maxWidth: thumbnailImgWidth, 
        width: thumbnailImgWidth,
        height: thumbnailImgHeight,
        padding: 0,
        margin: 0,
    }
});

interface Props {
    children: React.ReactElement;
    open: boolean;
    value: number;
}

function PlayerProgressIndicatorValue(props: Props) {
    const { children, open, value } = props;
    const second = value < 0 ? 0 : Math.round(value);

    return (
        <StyledTooltip
            open={open}
            enterTouchDelay={0}
            placement="top"
            arrow
            title={<PlayerProgressIndicatorThumbnail second={second} />}
        >
            {children}
        </StyledTooltip>
    );
}

export default PlayerProgressIndicatorValue;
