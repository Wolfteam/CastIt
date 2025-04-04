import { Typography, ListItemSecondaryAction, useMediaQuery, Theme } from '@mui/material';
import Grid from '@mui/material/Grid2';
import { Loop } from '@mui/icons-material';

interface Props {
    loop: boolean;
    fullTotalDuration: string;
}

function FileItemDuration(props: Props) {
    const hideDuration = useMediaQuery((theme: Theme) => theme.breakpoints.down('lg'));

    return (
        <ListItemSecondaryAction sx={{ top: '44%' }}>
            <Grid container justifyContent="center" alignItems="center" className={'text-overflow-elipsis'}>
                {props.loop ? <Loop fontSize="small" /> : null}
                {!hideDuration ? <Typography>{props.fullTotalDuration}</Typography> : null}
            </Grid>
        </ListItemSecondaryAction>
    );
}

export default FileItemDuration;
