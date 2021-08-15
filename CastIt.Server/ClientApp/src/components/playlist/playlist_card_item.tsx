import {
    Button,
    makeStyles,
    CardActions,
    CardActionArea,
    CardContent,
    CardMedia,
    Fab,
    IconButton,
    Popover,
    Tooltip,
    Typography,
    Card,
    Grid,
} from '@material-ui/core';
import React, { useState } from 'react';
import { Add, Delete, MoreVert, Edit } from '@material-ui/icons';
import { useHistory } from 'react-router-dom';
import { playListPath } from '../../routes';
import RenamePlayListDialog from './rename_playlist_dialog';
import { addFolderOrFileOrUrl, addNewPlayList, deletePlayList, updatePlayList } from '../../services/castithub.service';
import translations from '../../services/translations';
import PlayListLoopShuffleButton from './playlist_loop_shuffle_button';
import AddFilesDialog from '../dialogs/add_files_dialog';
import { Draggable } from 'react-beautiful-dnd';

const useStyles = makeStyles({
    root: {
        minWidth: 175,
        maxWidth: 375,
        '&:hover': { transform: 'scale3d(1.05, 1.05, 1)' },
    },
    title: {
        fontSize: 14,
    },
    image: {
        width: '100%',
        height: 250,
    },
    moreButtons: {
        width: '100%',
    },
    actionButtons: {
        justifyContent: 'flex-end',
    },
    cardContent: {
        paddingBottom: 0,
    },
    fab: {
        float: 'right',
        marginTop: -50,
    },
    name: {
        textOverflow: 'ellipsis',
        overflow: 'hidden',
        whiteSpace: 'nowrap',
    },
});

interface Props {
    index: number;
    id: number;
    name: string;
    totalDuration: string;
    shuffle?: boolean;
    loop?: boolean;
    numberOfFiles: number;
    position: number;
    imageUrl: string;
    toAddNewItem?: boolean;
}

function PlayListCardItem(props: Props): JSX.Element {
    const classes = useStyles();
    const history = useHistory();
    const [anchorEl, setAnchorEl] = useState<HTMLElement>();
    const [raised, setRaised] = useState(false);
    const [showRenameDialog, setShowRenameDialog] = useState(false);
    const [showAddFilesDialog, setShowAddFilesDialog] = useState(false);

    const handleShowMoreClick = (event: React.MouseEvent<HTMLElement>): void => {
        setAnchorEl(event.currentTarget);
    };

    const handleClose = (): void => {
        setAnchorEl(undefined);
    };

    const handleClick = (): void => {
        const route = playListPath.replace(':id', `${props.id}`);
        history.push(route);
    };

    const toggleRaised = (): void => {
        setRaised(!raised);
    };

    const handleDelete = async (): Promise<void> => {
        await deletePlayList(props.id);
        handleClose();
    };

    const handleAddNew = async (): Promise<void> => {
        await addNewPlayList();
    };

    const handleRename = async (newName: string | null): Promise<void> => {
        handleClose();
        setShowRenameDialog(false);
        if (newName) {
            await updatePlayList(props.id, newName);
        }
    };

    const handleAddFiles = async (path: string | null, includeSubFolder: boolean, onlyVideo: boolean): Promise<void> => {
        setShowAddFilesDialog(false);
        if (path) {
            await addFolderOrFileOrUrl(props.id, path, includeSubFolder, onlyVideo);
        }
    };

    const showMorePopup = Boolean(anchorEl);
    const moreId = anchorEl ? 'open-more-popover' : undefined;

    const elevation = raised ? 24 : 2;
    if (props.toAddNewItem) {
        return (
            <Card
                className={classes.root}
                elevation={elevation}
                raised={raised}
                onClick={handleAddNew}
                onMouseOver={toggleRaised}
                onMouseOut={toggleRaised}
            >
                <CardActionArea style={{ height: '100%', textAlign: 'center' }}>
                    <CardContent>
                        <Grid container justifyContent="center" alignItems="center">
                            <Add fontSize="large" color="primary" />
                            <Typography color="textSecondary">{translations.addNewPlayList}</Typography>
                        </Grid>
                    </CardContent>
                </CardActionArea>
            </Card>
        );
    }

    return (
        <Draggable index={props.index} draggableId={`${props.id}_${props.name}`}>
            {(provided) => (
                <Card
                    elevation={elevation}
                    raised={raised}
                    className={classes.root}
                    onMouseOver={toggleRaised}
                    onMouseOut={toggleRaised}
                    ref={provided.innerRef}
                    {...provided.dragHandleProps}
                    {...provided.draggableProps}
                >
                    <CardActionArea onClick={handleClick}>
                        <CardMedia className={classes.image} image={props.imageUrl} title={props.name} />
                        <CardContent className={classes.cardContent}>
                            <Fab className={classes.fab} color="primary">
                                {props.numberOfFiles}
                            </Fab>
                            <Typography className={classes.title} color="textSecondary" gutterBottom>
                                {translations.playList}
                            </Typography>
                            <Tooltip title={props.name}>
                                <Typography variant="h5" component="h2" className={classes.name}>
                                    {props.name}
                                </Typography>
                            </Tooltip>
                            <Tooltip title={props.totalDuration}>
                                <Typography color="textSecondary" className={classes.name}>
                                    {props.totalDuration}
                                </Typography>
                            </Tooltip>
                        </CardContent>
                    </CardActionArea>
                    <CardActions className={classes.actionButtons} disableSpacing={true}>
                        <IconButton onClick={() => setShowAddFilesDialog(true)}>
                            <Add />
                        </IconButton>
                        <PlayListLoopShuffleButton id={props.id} loop={props.loop} shuffle={props.shuffle} renderLoop />
                        <PlayListLoopShuffleButton id={props.id} loop={props.loop} shuffle={props.shuffle} />
                        <IconButton onClick={handleShowMoreClick}>
                            <MoreVert />
                        </IconButton>
                        <Popover
                            id={moreId}
                            open={showMorePopup}
                            anchorEl={anchorEl}
                            onClose={handleClose}
                            anchorOrigin={{
                                vertical: 'bottom',
                                horizontal: 'center',
                            }}
                            transformOrigin={{
                                vertical: 'top',
                                horizontal: 'center',
                            }}
                        >
                            <Button
                                className={classes.moreButtons}
                                size="small"
                                startIcon={<Edit />}
                                onClick={() => setShowRenameDialog(true)}
                            >
                                {translations.rename}
                            </Button>
                            <Button className={classes.moreButtons} size="small" startIcon={<Delete />} onClick={handleDelete}>
                                {translations.delete}
                            </Button>
                        </Popover>
                        <AddFilesDialog isOpen={showAddFilesDialog} onClose={handleAddFiles} />
                        <RenamePlayListDialog isOpen={showRenameDialog} name={props.name} onClose={handleRename} />
                    </CardActions>
                </Card>
            )}
        </Draggable>
    );
}

export default PlayListCardItem;
