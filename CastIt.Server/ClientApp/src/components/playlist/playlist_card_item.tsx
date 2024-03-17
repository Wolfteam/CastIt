import {
    Button,
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
} from '@mui/material';
import { makeStyles } from '@mui/styles';
import React, { useEffect, useState } from 'react';
import { Add, Delete, MoreVert, Edit, Sort } from '@mui/icons-material';
import { useNavigate } from 'react-router-dom';
import { playListPath } from '../../routes';
import RenamePlayListDialog from '../dialogs/rename_playlist_dialog';
import { onPlayerStatusChanged } from '../../services/castithub.service';
import translations from '../../services/translations';
import PlayListLoopShuffleButton from './playlist_loop_shuffle_button';
import AddFilesDialog from '../dialogs/add_files_dialog';
import { IGetAllPlayListResponseDto } from '../../models';
import { useCastItHub } from '../../context/castit_hub.context';
import { defaultImg } from '../../utils/app_constants';

const useStyles = makeStyles({
    root: {
        minWidth: 175
    },
    title: {
        fontSize: 14,
    },
    image: {
        width: '100%',
        height: 250,
        objectFit: "fill"
    },
    moreButtons: {
        width: '100%',
    },
    actionButtons: {
        justifyContent: 'flex-end',
        '& button': {
            padding: 5,
        },
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
    toAddNewItem?: boolean;
    playList: IGetAllPlayListResponseDto;
    onReOrderClick?(): void;
}

interface State {
    id: number;
    name: string;
    totalDuration: string;
    shuffle?: boolean;
    loop?: boolean;
    numberOfFiles: number;
    position: number;
    imageUrl: string;
    loaded: boolean;
}

const initialState: State = {
    id: 0,
    name: '',
    totalDuration: '',
    numberOfFiles: 0,
    position: 0,
    imageUrl: '',
    loaded: false,
};

function PlayListCardItem(props: Props): JSX.Element {
    const classes = useStyles();
    const navigate = useNavigate();
    const [anchorEl, setAnchorEl] = useState<HTMLElement>();

    const [state, setState] = useState(initialState);
    const [raised, setRaised] = useState(false);
    const [showRenameDialog, setShowRenameDialog] = useState(false);
    const [showAddFilesDialog, setShowAddFilesDialog] = useState(false);

    const castItHub = useCastItHub();

    useEffect(() => {
        setState({ ...props.playList, loaded: true });
    }, [props.playList]);

    useEffect(() => {
        const onPlayerStatusChangedSubscription = onPlayerStatusChanged.subscribe((status) => {
            if (!status) {
                return;
            }

            if (!status.playList) {
                return;
            }

            if (props.playList.id !== status.playList.id) {
                return;
            }

            setState((s) => ({ ...s, ...status.playList! }));
        });
        return () => {
            onPlayerStatusChangedSubscription.unsubscribe();
        };
    }, [props.playList]);

    const handleShowMoreClick = (event: React.MouseEvent<HTMLElement>): void => {
        setAnchorEl(event.currentTarget);
    };

    const handleClose = (): void => {
        setAnchorEl(undefined);
    };

    const handleClick = (): void => {
        const route = playListPath.replace(':id', `${state.id}`);
        navigate(route);
    };

    const toggleRaised = (): void => {
        setRaised(!raised);
    };

    const handleDelete = async (): Promise<void> => {
        await castItHub.connection.deletePlayList(state.id);
        handleClose();
    };

    const handleAddNew = async (): Promise<void> => {
        await castItHub.connection.addNewPlayList();
    };

    const handleRename = async (newName: string | null): Promise<void> => {
        handleClose();
        setShowRenameDialog(false);
        if (newName) {
            await castItHub.connection.updatePlayList(state.id, newName);
        }
    };

    const handleAddFiles = async (path: string | null, includeSubFolder: boolean, onlyVideo: boolean): Promise<void> => {
        setShowAddFilesDialog(false);
        if (path) {
            await castItHub.connection.addFolderOrFileOrUrl(state.id, path, includeSubFolder, onlyVideo);
        }
    };

    const handleSort = () => {
        handleClose();
        props.onReOrderClick!();
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

    if (!state.loaded) {
        return <Card elevation={elevation} raised={raised} className={classes.root} />;
    }

    const image = state.imageUrl ?? defaultImg;

    return (
        <Card elevation={elevation} raised={raised} className={classes.root} onMouseOver={toggleRaised} onMouseOut={toggleRaised}>
            <CardActionArea onClick={handleClick}>
                <CardMedia className={classes.image} component="img" image={image} title={state.name} />
                <CardContent className={classes.cardContent}>
                    <Fab className={classes.fab} color="primary" component="div">
                        {state.numberOfFiles}
                    </Fab>
                    <Typography className={classes.title} color="textSecondary" gutterBottom>
                        {translations.playList}
                    </Typography>
                    <Tooltip title={state.name}>
                        <Typography variant="h5" component="h2" className={classes.name}>
                            {state.name}
                        </Typography>
                    </Tooltip>
                    <Tooltip title={state.totalDuration}>
                        <Typography color="textSecondary" className={classes.name}>
                            {state.totalDuration}
                        </Typography>
                    </Tooltip>
                </CardContent>
            </CardActionArea>
            <CardActions className={classes.actionButtons} disableSpacing={true}>
                <IconButton onClick={() => setShowAddFilesDialog(true)} size="large">
                    <Add />
                </IconButton>
                <PlayListLoopShuffleButton id={state.id} loop={state.loop} shuffle={state.shuffle} renderLoop />
                <PlayListLoopShuffleButton id={state.id} loop={state.loop} shuffle={state.shuffle} />
                <IconButton onClick={handleShowMoreClick} size="large">
                    <MoreVert />
                </IconButton>
                {!showMorePopup ? null : (
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
                        <Button className={classes.moreButtons} size="small" startIcon={<Edit />} onClick={() => setShowRenameDialog(true)}>
                            {translations.rename}
                        </Button>
                        <Button className={classes.moreButtons} size="small" startIcon={<Sort />} onClick={handleSort}>
                            {translations.sort}
                        </Button>
                        <Button className={classes.moreButtons} size="small" startIcon={<Delete />} onClick={handleDelete}>
                            {translations.delete}
                        </Button>
                    </Popover>
                )}
                <AddFilesDialog isOpen={showAddFilesDialog} onClose={handleAddFiles} />
                <RenamePlayListDialog isOpen={showRenameDialog} name={state.name} onClose={handleRename} />
            </CardActions>
        </Card>
    );
}

export default PlayListCardItem;
