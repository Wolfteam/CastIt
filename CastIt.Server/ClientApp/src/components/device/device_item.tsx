import { Grid, IconButton, Typography } from '@material-ui/core';
import { Tv, CastConnected, Cast } from '@material-ui/icons';
import { useCastItHub } from '../../context/castit_hub.context';

interface Props {
    id: string;
    name: string;
    ipAddress: string;
    isConnected?: boolean;
}

function DeviceItem(props: Props) {
    const castItHub = useCastItHub();
    const connectedIcon = props.isConnected ? <CastConnected /> : <Cast />;

    const handleToggleConnect = async (): Promise<void> => {
        const id = props.isConnected ? null : props.id;
        await castItHub.connection.connectToCastDevice(id);
    };

    return (
        <Grid container justifyContent="space-evenly" alignItems="center">
            <Grid item xs={1}>
                <Tv />
            </Grid>
            <Grid item xs={10}>
                <Grid container direction="column">
                    <Typography>{props.name}</Typography>
                    <Typography variant="subtitle1">{props.ipAddress}</Typography>
                </Grid>
            </Grid>
            <Grid item xs={1}>
                <IconButton onClick={handleToggleConnect}>{connectedIcon}</IconButton>
            </Grid>
        </Grid>
    );
}

export default DeviceItem;
