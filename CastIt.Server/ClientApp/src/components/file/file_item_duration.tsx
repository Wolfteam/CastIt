import { Typography, ListItemSecondaryAction, makeStyles, createStyles, Grid, useMediaQuery, Theme } from '@material-ui/core';
import { Loop } from '@material-ui/icons';

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
    const isXsScreen = useMediaQuery((theme: Theme) => theme.breakpoints.down('xs'));

    return (
        <ListItemSecondaryAction className={classes.duration}>
            <Grid container justifyContent="center" alignItems="center" className={'text-overflow-elipsis'}>
                {props.loop ? <Loop fontSize="small" /> : null}
                {!isXsScreen ? <Typography>{props.fullTotalDuration}</Typography> : null}
            </Grid>
        </ListItemSecondaryAction>
    );
}

export default FileItemDuration;
