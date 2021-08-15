import React, { Fragment, useEffect, useState } from 'react';
import {
    Typography,
    Slider,
    Divider,
    ListItem,
    ListItemText,
    Avatar,
    ListItemAvatar,
    ListItemSecondaryAction,
    makeStyles,
    Tooltip,
    createStyles,
    Menu,
    MenuItem,
    Grid,
} from '@material-ui/core';
import { IFileItemResponseDto } from '../../models';
import {
    onFileChanged,
    onFileEndReached,
    onPlayerStatusChanged,
    play,
    loopFile,
    deleteFile,
    removeAllMissingFiles,
    addFolderOrFileOrUrl,
} from '../../services/castithub.service';
import { Add, ClearAll, Delete, Loop, PlayArrow, Refresh } from '@material-ui/icons';
import translations from '../../services/translations';
import AddFilesDialog from '../dialogs/add_files_dialog';
import { Draggable } from 'react-beautiful-dnd';

const useStyles = makeStyles((theme) =>
    createStyles({
        position: {
            color: theme.palette.getContrastText(theme.palette.primary.main),
            backgroundColor: theme.palette.primary.main,
        },
        text: {
            overflow: 'hidden',
            whiteSpace: 'nowrap',
            textOverflow: 'ellipsis',
        },
        duration: {
            top: '40%',
        },
        beingPlayed: {
            backgroundColor: theme.palette.primary.light,
        },
        menuItemText: {
            marginLeft: 10,
        },
        slider: {
            color: `${theme.palette.primary.main} !important`,
        },
    })
);

interface Props {
    index: number;
    file: IFileItemResponseDto;
}

interface State {
    playListId: number;
    id: number;
    filename: string;
    subTitle: string;
    path: string;
    playedPercentage: number;
    position: number;
    fullTotalDuration: string;
    isBeingPlayed: boolean;
    loop: boolean;
}

interface ContextMenuState {
    mouseX: number | null;
    mouseY: number | null;
}

const initialState: State = {
    playListId: 0,
    id: 0,
    filename: '',
    subTitle: '',
    path: '',
    playedPercentage: 0,
    position: 1,
    fullTotalDuration: '',
    isBeingPlayed: false,
    loop: false,
};

const initialContextMenuState: ContextMenuState = {
    mouseX: null,
    mouseY: null,
};

function FileItem(props: Props) {
    const classes = useStyles();
    const [state, setState] = useState<State>(initialState);
    const [contextMenu, setContextMenu] = useState(initialContextMenuState);
    const [showAddFilesDialog, setShowAddFilesDialog] = useState(false);

    useEffect(() => {
        setState({
            playListId: props.file.playListId,
            id: props.file.id,
            filename: props.file.filename,
            subTitle: props.file.subTitle,
            path: props.file.path,
            playedPercentage: props.file.playedPercentage,
            position: props.file.position,
            fullTotalDuration: props.file.fullTotalDuration,
            isBeingPlayed: props.file.isBeingPlayed,
            loop: props.file.loop,
        });
    }, [props.file]);

    useEffect(() => {
        const onFileChangedSubscription = onFileChanged.subscribe((file) => {
            if (file.id !== props.file.id) {
                if (state.isBeingPlayed) {
                    setState((s) => ({
                        ...s,
                        isBeingPlayed: false,
                    }));
                }
                return;
            }
            setState((s) => ({
                ...s,
                name: file.name,
                subTitle: file.subTitle,
                path: file.path,
                playedPercentage: file.playedPercentage,
                position: file.position,
                fullTotalDuration: file.fullTotalDuration,
                isBeingPlayed: file.isBeingPlayed,
                loop: file.loop,
            }));
        });

        const onPlayerStatusChangedSubscription = onPlayerStatusChanged.subscribe((status) => {
            if (status.playedFile?.id !== props.file.id) {
                if (state.isBeingPlayed) {
                    setState((s) => ({
                        ...s,
                        isBeingPlayed: false,
                    }));
                }
                return;
            }

            setState((s) => ({
                ...s,
                name: status.playedFile!.name,
                subTitle: status.playedFile!.subTitle,
                path: status.playedFile!.path,
                playedPercentage: status.playedFile!.playedPercentage,
                position: status.playedFile!.position,
                fullTotalDuration: status.playedFile!.fullTotalDuration,
                isBeingPlayed: status.playedFile!.isBeingPlayed,
                loop: status.playedFile!.loop,
            }));
        });

        const onFileEndReachedSubscription = onFileEndReached.subscribe((file) => {
            if (file.id !== props.file.id) {
                return;
            }

            setState((s) => ({ ...s, playedPercentage: 100 }));
        });

        return () => {
            onFileChangedSubscription.unsubscribe();
            onPlayerStatusChangedSubscription.unsubscribe();
            onFileEndReachedSubscription.unsubscribe();
        };
    }, [props.file.id, state.isBeingPlayed]);

    const handleOpenContextMenu = (event: React.MouseEvent<HTMLDivElement>) => {
        event.preventDefault();
        setContextMenu({
            mouseX: event.clientX - 2,
            mouseY: event.clientY - 4,
        });
    };

    const handlePlay = async (force: boolean = false): Promise<void> => {
        handleCloseContextMenu();
        await play(props.file.playListId, state.id, force, false);
    };

    const handleCloseContextMenu = (): void => {
        setContextMenu(initialContextMenuState);
    };

    const handleToggleLoop = async (): Promise<void> => {
        handleCloseContextMenu();
        await loopFile(state.playListId, state.id, !state.loop);
    };

    const handleAddFiles = async (path: string | null, includeSubFolder: boolean, onlyVideo: boolean): Promise<void> => {
        handleCloseContextMenu();
        setShowAddFilesDialog(false);
        if (path) {
            await addFolderOrFileOrUrl(state.playListId, path, includeSubFolder, onlyVideo);
        }
    };

    const handleDelete = async (): Promise<void> => {
        handleCloseContextMenu();
        await deleteFile(props.file.playListId, props.file.id);
    };

    const handleRemoveAllMissing = async (): Promise<void> => {
        handleCloseContextMenu();
        await removeAllMissingFiles(props.file.playListId);
    };

    const title = (
        <Tooltip title={state.filename}>
            <Typography className={classes.text}>{state.filename}</Typography>
        </Tooltip>
    );

    const subtitle = (
        <Fragment>
            <Tooltip title={state.subTitle}>
                <Typography variant="subtitle1" className={classes.text}>
                    {state.subTitle}
                </Typography>
            </Tooltip>
            <Tooltip title={state.path}>
                <Typography variant="subtitle2" className={classes.text}>
                    {state.path}
                </Typography>
            </Tooltip>

            <Slider value={state.playedPercentage} disabled className={classes.slider} />
        </Fragment>
    );

    const toggleLoopMenuItem = state.isBeingPlayed ? (
        <MenuItem onClick={handleToggleLoop}>
            <Loop fontSize="small" />
            <ListItemText className={classes.menuItemText} primary={!state.loop ? translations.loop : translations.disableLoop} />
        </MenuItem>
    ) : null;

    return (
        <Draggable draggableId={`${props.file.id}_${props.file.filename}`} index={props.index}>
            {(provided) => (
                <div
                    data-played-file={state.isBeingPlayed}
                    onContextMenu={handleOpenContextMenu}
                    style={{ cursor: 'context-menu' }}
                    {...provided.dragHandleProps}
                    {...provided.draggableProps}
                    ref={provided.innerRef}
                >
                    <ListItem button onDoubleClick={() => handlePlay()} className={state.isBeingPlayed ? classes.beingPlayed : ''}>
                        <ListItemAvatar>
                            <Avatar className={classes.position}>{state.position}</Avatar>
                        </ListItemAvatar>
                        <ListItemText primary={title} secondary={subtitle} />
                        <ListItemSecondaryAction className={classes.duration}>
                            <Grid container justifyContent="center" alignItems="center">
                                {state.loop ? <Loop fontSize="small" /> : null}
                                <Typography>{state.fullTotalDuration}</Typography>
                            </Grid>
                        </ListItemSecondaryAction>
                    </ListItem>
                    <Menu
                        keepMounted
                        open={contextMenu.mouseY !== null}
                        onClose={handleCloseContextMenu}
                        anchorReference="anchorPosition"
                        anchorPosition={
                            contextMenu.mouseY !== null && contextMenu.mouseX !== null
                                ? { top: contextMenu.mouseY, left: contextMenu.mouseX }
                                : undefined
                        }
                    >
                        <MenuItem onClick={() => handlePlay()}>
                            <PlayArrow fontSize="small" />
                            <ListItemText className={classes.menuItemText} primary={translations.play} />
                        </MenuItem>
                        <MenuItem onClick={() => handlePlay(true)}>
                            <Refresh fontSize="small" />
                            <ListItemText className={classes.menuItemText} primary={translations.playFromTheStart} />
                        </MenuItem>
                        {toggleLoopMenuItem}
                        <MenuItem onClick={() => setShowAddFilesDialog(true)}>
                            <Add fontSize="small" />
                            <ListItemText className={classes.menuItemText} primary={translations.addFiles} />
                        </MenuItem>
                        {/* TODO: REMOVE ALL SELECTED AND SELECT ALL */}
                        <MenuItem onClick={handleDelete}>
                            <Delete fontSize="small" />
                            <ListItemText className={classes.menuItemText} primary={translations.remove} />
                        </MenuItem>
                        <MenuItem onClick={handleRemoveAllMissing}>
                            <ClearAll fontSize="small" />
                            <ListItemText className={classes.menuItemText} primary={translations.removeAllMissing} />
                        </MenuItem>
                    </Menu>
                    <Divider variant="middle" />
                    <AddFilesDialog isOpen={showAddFilesDialog} onClose={handleAddFiles} />
                </div>
            )}
        </Draggable>
    );
}

export default FileItem;
