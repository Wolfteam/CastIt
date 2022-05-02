import { Grid, Typography, Slider, createStyles, makeStyles, Theme, useMediaQuery } from '@material-ui/core';

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
    const isXsScreen = useMediaQuery((theme: Theme) => theme.breakpoints.down('xs'));

    const slider = <Slider value={props.playedPercentage} disabled className={classes.slider} />;
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
            <Grid item xs={8} style={{ marginBottom: 7, paddingRight: '5px' }}>
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
