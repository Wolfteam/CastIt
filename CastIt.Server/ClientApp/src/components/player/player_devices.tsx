import { useEffect, useState } from 'react';
import { Button, Dialog, DialogActions, DialogContent, IconButton, LinearProgress, List } from '@mui/material';
import { Tv } from '@mui/icons-material';
import DeviceItem from '../device/device_item';
import { onCastDevicesChanged, onCastDeviceSet } from '../../services/castithub.service';
import translations from '../../services/translations';
import { IReceiver } from '../../models';
import AppDialogTitle from '../dialogs/app_dialog_title';
import { useCastItHub } from '../../context/castit_hub.context';

function PlayerDevices() {
    const [devices, setDevices] = useState<IReceiver[]>([]);
    const [open, setOpen] = useState(false);
    const [isRefreshing, setIsRefreshing] = useState(false);
    const castItHub = useCastItHub();

    useEffect(() => {
        const onCastDeviceSetSubscription = onCastDeviceSet.subscribe((device) => {
            const existing = devices.find((d) => d.id === device.id);
            if (!existing) {
                return;
            }
            const index = devices.indexOf(existing!);
            const updatedDevices = [...devices];
            updatedDevices.splice(index, 1);
            updatedDevices.splice(index, 0, device);
            setDevices(updatedDevices);
        });

        const onCastDevicesChangedSubscription = onCastDevicesChanged.subscribe((devices) => {
            setIsRefreshing(false);
            setDevices(devices);
        });

        return () => {
            onCastDeviceSetSubscription.unsubscribe();
            onCastDevicesChangedSubscription.unsubscribe();
        };
    }, []);

    const handleClickOpen = () => {
        setOpen(true);
    };

    const handleClose = () => {
        setOpen(false);
    };

    const handleRefreshDevices = async (): Promise<void> => {
        setIsRefreshing(true);
        await castItHub.connection.refreshCastDevices();
    };

    const deviceItems = devices.map((d) => (
        <DeviceItem key={d.id} id={d.id} name={d.friendlyName} ipAddress={`${d.host}:${d.port}`} isConnected={d.isConnected} />
    ));

    return (
        <>
            <IconButton onClick={handleClickOpen} size="large">
                <Tv fontSize="large" />
            </IconButton>
            <Dialog fullWidth={true} open={open} maxWidth="xs" onClose={handleClose}>
                <AppDialogTitle title={translations.devices} icon={<Tv />} close={handleClose} />
                {isRefreshing && <LinearProgress />}
                <DialogContent style={{ paddingBottom: 0, paddingTop: 0 }}>
                    <List>{deviceItems}</List>
                </DialogContent>
                <DialogActions>
                    <Button
                        color="primary"
                        variant="outlined"
                        disabled={isRefreshing}
                        sx={{ width: '100%', marginLeft: 20, marginRight: 20 }}
                        onClick={handleRefreshDevices}
                    >
                        {isRefreshing ? translations.refreshing : translations.refresh}
                    </Button>
                </DialogActions>
            </Dialog>
        </>
    );
}

export default PlayerDevices;
