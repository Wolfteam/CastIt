import { Grid, Typography, Slider, Theme, useMediaQuery } from '@mui/material';

interface Props {
    playedTime: string;
    playedPercentage: number;
    duration: string;
}

function FileItemSlider(props: Props) {
    const showInlineDurations = useMediaQuery((theme: Theme) => theme.breakpoints.down('lg'));
    const slider = (
        <Slider
            size="small"
            value={props.playedPercentage}
            disabled
            sx={(theme) => ({
                color: `${theme.palette.primary.main} !important`,
                padding: '0 !important',
            })}
        />
    );
    if (!showInlineDurations) {
        return slider;
    }

    return (
        <>
            <Grid container justifyContent="space-between" alignItems="center">
                <Grid item xs={6}>
                    <Typography variant="subtitle1" className={'text-overflow-elipsis'} align="left">
                        {props.playedTime}
                    </Typography>
                </Grid>

                <Grid item xs={6}>
                    <Typography variant="subtitle1" className={'text-overflow-elipsis'} align="right">
                        {props.duration}
                    </Typography>
                </Grid>
            </Grid>
            <Grid container>{slider}</Grid>
        </>
    );
}

export default FileItemSlider;
