import { Typography, ListItemSecondaryAction, Grid, useMediaQuery, Theme } from '@mui/material';
import { createStyles, makeStyles } from '@mui/styles';
import { Loop } from '@mui/icons-material';

const useStyles = makeStyles((theme) =>
    createStyles({
        duration: {
            top: '44%',
        },
    })
);

interface Props {
    loop: boolean;
    fullTotalDuration: string;
}

function FileItemDuration(props: Props) {
    const classes = useStyles();
    const hideDuration = useMediaQuery((theme: Theme) => theme.breakpoints.down('lg'));

    return (
        <ListItemSecondaryAction className={classes.duration}>
            <Grid container justifyContent="center" alignItems="center" className={'text-overflow-elipsis'}>
                {props.loop ? <Loop fontSize="small" /> : null}
                {!hideDuration ? <Typography>{props.fullTotalDuration}</Typography> : null}
            </Grid>
        </ListItemSecondaryAction>
    );
}

export default FileItemDuration;
