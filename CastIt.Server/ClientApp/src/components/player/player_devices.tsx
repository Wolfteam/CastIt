import { Fragment, useEffect, useState } from 'react';
import {
    Button,
    createStyles,
    Dialog,
    DialogActions,
    DialogContent,
    DialogTitle,
    IconButton,
    LinearProgress,
    makeStyles,
} from '@material-ui/core';
import { Tv } from '@material-ui/icons';
import DeviceItem from '../device/device_item';
import { onCastDevicesChanged, onCastDeviceSet, refreshCastDevices } from '../../services/castithub.service';
import translations from '../../services/translations';
import { IReceiver } from '../../models';
import AppDialogTitle from '../dialogs/app_dialog_title';

const useStyles = makeStyles((theme) =>
    createStyles({
        dialogTitle: {
            backgroundColor: theme.palette.primary.main,
        },
        refreshButton: {
            width: '100%',
            marginLeft: 20,
            marginRight: 20,
        },
    })
);

function PlayerDevices() {
    const classes = useStyles();
    const [devices, setDevices] = useState<IReceiver[]>([]);
    const [open, setOpen] = useState(false);
    const [isRefreshing, setIsRefreshing] = useState(false);

    useEffect(() => {
        const onCastDeviceSetSubscription = onCastDeviceSet.subscribe((device) => {
            const existing = devices.find((d) => d.id === device.id);
            if (devices.filter((d) => d.id === device.id).length > 0) {
                const index = devices.indexOf(existing!);
                const updatedDevices = [...devices];
                updatedDevices.splice(index, 1);
                updatedDevices.splice(index, 0, device);
                setDevices(updatedDevices);
                return;
            }
            setDevices([...devices, device]);
        });

        const onCastDevicesChangedSubscription = onCastDevicesChanged.subscribe((devices) => {
            setIsRefreshing(false);
            setDevices(devices);
        });

        return () => {
            onCastDeviceSetSubscription.unsubscribe();
            onCastDevicesChangedSubscription.unsubscribe();
        };
    }, [devices]);

    const handleClickOpen = () => {
        setOpen(true);
    };

    const handleClose = () => {
        setOpen(false);
    };

    const handleRefreshDevices = async (): Promise<void> => {
        setIsRefreshing(true);
        await refreshCastDevices();
    };

    const deviceItems = devices.map((d) => (
        <DeviceItem key={d.id} id={d.id} name={d.friendlyName} ipAddress={`${d.host}:${d.port}`} isConnected={d.isConnected} />
    ));

    return (
        <Fragment>
            <IconButton onClick={handleClickOpen}>
                <Tv fontSize="large" />
            </IconButton>
            <Dialog fullWidth={true} open={open} maxWidth="xs" onClose={handleClose}>
                <AppDialogTitle title={translations.devices} icon={<Tv />} close={handleClose} />
                <DialogContent>{isRefreshing ? <LinearProgress /> : deviceItems}</DialogContent>
                <DialogActions>
                    <Button
                        color="primary"
                        variant="outlined"
                        disabled={isRefreshing}
                        className={classes.refreshButton}
                        onClick={handleRefreshDevices}
                    >
                        {isRefreshing ? translations.refreshing : translations.refresh}
                    </Button>
                </DialogActions>
            </Dialog>
        </Fragment>
    );
}

export default PlayerDevices;