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
    const isXsScreen = useMediaQuery((theme: Theme) => theme.breakpoints.down('sm'));

    const slider = <Slider size="small" value={props.playedPercentage} disabled className={classes.slider} />;
    if (!isXsScreen) {
        return slider;
    }

    return (
        <Grid container justifyContent="space-between" alignItems="center">
            <Grid item xs={2}>
                <Typography variant="subtitle1" className={'text-overflow-elipsis'} align="left">
                    {props.playedTime}
                </Typography>
            </Grid>
            <Grid item xs={8} style={{ marginBottom: 7 }}>
                {slider}
            </Grid>
            <Grid item xs={2}>
                <Typography variant="subtitle1" className={'text-overflow-elipsis'} align="right">
                    {props.duration}
                </Typography>
            </Grid>
        </Grid>
    );
}

export default FileItemSlider;
