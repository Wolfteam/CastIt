import { Close } from '@material-ui/icons';
import { createStyles, DialogTitle, Grid, IconButton, makeStyles, Typography } from '@material-ui/core';

const useStyles = makeStyles((theme) =>
    createStyles({
        dialogTitle: {
            display: 'flex',
            justifyContent: 'space-between',
            alignItems: 'center',
            backgroundColor: theme.palette.primary.main,
        },
        title: {
            paddingLeft: 10,
        },
    })
);

interface Props {
    icon: JSX.Element;
    title: string;
    close(): void;
}

function AppDialogTitle(props: Props) {
    const classes = useStyles();
    return (
        <DialogTitle disableTypography className={classes.dialogTitle}>
            <Grid container alignItems="center">
                {props.icon}
                <Typography className={classes.title} variant="h6">{props.title}</Typography>
            </Grid>
            <IconButton onClick={props.close}>
                <Close />
            </IconButton>
        </DialogTitle>
    );
}

export default AppDialogTitle;
