import { Grid, Typography, Slider, Theme, useMediaQuery } from '@mui/material';
import { createStyles, makeStyles } from '@mui/styles';

const useStyles = makeStyles((theme) =>
    createStyles({
        slider: {
            color: `${theme.palette.primary.main} !important`,
            padding: 0,
        },
    })
);

interface Props {
    playedTime: string;
    playedPercentage: number;
    duration: string;
}

function FileItemSlider(props: Props) {
    const classes = useStyles();
    const showInlineDurations = useMediaQuery((theme: Theme) => theme.breakpoints.down('lg'));

    const slider = <Slider size="small" value={props.playedPercentage} disabled className={classes.slider} />;
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
