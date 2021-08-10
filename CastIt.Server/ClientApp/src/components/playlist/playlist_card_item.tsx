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
import { addNewPlayList, setPlayListOptions, deletePlayList, updatePlayList } from '../../services/castithub.service';
import translations from '../../services/translations';
import PlayListLoopShuffleButton from './playlist_loop_shuffle_button';

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

    const handleOptionChanged = async (loop: boolean, shuffle: boolean): Promise<void> => {
        await setPlayListOptions(props.id, loop, shuffle);
    };

    const handleDelete = async (): Promise<void> => {
        await deletePlayList(props.id);
    };

    const handleAddNew = async (): Promise<void> => {
        await addNewPlayList();
    };

    const handleRename = async (newName: string | null): Promise<void> => {
        setShowRenameDialog(false);
        if (newName) {
            await updatePlayList(props.id, newName);
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
        <Card elevation={elevation} raised={raised} className={classes.root} onMouseOver={toggleRaised} onMouseOut={toggleRaised}>
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
            <CardActions className={classes.actionButtons}>
                <IconButton>
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
                    <Button className={classes.moreButtons} size="small" startIcon={<Edit />} onClick={() => setShowRenameDialog(true)}>
                        {translations.rename}
                    </Button>
                    <Button className={classes.moreButtons} size="small" startIcon={<Delete />} onClick={handleDelete}>
                        {translations.delete}
                    </Button>
                </Popover>
                <RenamePlayListDialog isOpen={showRenameDialog} name={props.name} onClose={handleRename} />
            </CardActions>
        </Card>
    );
}

export default PlayListCardItem;
