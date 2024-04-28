import { alpha, AppBar, IconButton, InputBase, Theme, Toolbar, Typography } from '@mui/material';
import { styled } from '@mui/material/styles';
import SearchIcon from '@mui/icons-material/Search';
import { Add, ArrowBack, ArrowUpward, Redo } from '@mui/icons-material';
import { playListsPath } from '../../routes';
import { useNavigate } from 'react-router-dom';
import React, { useEffect, useState } from 'react';
import PlayListLoopShuffleButton from './playlist_loop_shuffle_button';
import translations from '../../services/translations';
import { onPlayerStatusChanged } from '../../services/castithub.service';
import PlayListLoadingIndicator from './playlist_loading_indicator';
import AddFilesDialog from '../dialogs/add_files_dialog';
import { useCastItHub } from '../../context/castit_hub.context';

const StyledSearchContainer = styled(`div`)(({ theme }) => ({
    position: 'relative',
    borderRadius: theme.shape.borderRadius,
    backgroundColor: alpha(theme.palette.common.white, 0.15),
    '&:hover': {
        backgroundColor: alpha(theme.palette.common.white, 0.25),
    },
    marginLeft: 0,
    width: '100%',
    [theme.breakpoints.up('sm')]: {
        marginLeft: theme.spacing(1),
        width: 'auto',
    },
}));

const StyledSearchIconContainer = styled(`div`)(({ theme }) => ({
    padding: theme.spacing(0, 2),
    height: '100%',
    position: 'absolute',
    pointerEvents: 'none',
    display: 'flex',
    alignItems: 'center',
    justifyContent: 'center',
}));

const StyledInputBase = styled(InputBase)(({ theme }) => ({
    color: 'inherit',
    width: '100%',
    '& .MuiInputBase-input': {
        padding: theme.spacing(1, 1, 1, 0),
        // vertical padding + font size from searchIcon
        paddingLeft: `calc(1em + ${theme.spacing(4)})`,
        transition: theme.transitions.create('width'),
        [theme.breakpoints.up('sm')]: {
            width: '12ch',
            '&:focus': {
                width: '30ch',
            },
        },
    },
}));

interface Props {
    id: number;
    loop?: boolean;
    shuffle?: boolean;
    name?: string;
    searchText?: string;

    onSearch(value: string | null): void;
}

interface State {
    canGoToPlayedFile: boolean;
}

function PlayListAppBar(props: Props) {
    const navigate = useNavigate();
    const [search, setSearch] = useState<string>('');
    const [state, setState] = useState<State>({
        canGoToPlayedFile: false,
    });
    const [showAddFilesDialog, setShowAddFilesDialog] = useState(false);
    const castItHub = useCastItHub();

    useEffect(() => {
        const timeout = setTimeout(async () => {
            if (search !== props.searchText) {
                props.onSearch(search);
            }
        }, 500);

        return () => clearTimeout(timeout);
    }, [props, search]);

    useEffect(() => {
        const onPlayerStatusChangedSubscription = onPlayerStatusChanged.subscribe((status) => {
            if (!status) {
                return;
            }
            if (!status.playedFile || status.playedFile.playListId !== props.id) {
                if (state.canGoToPlayedFile) {
                    setState((s) => ({ ...s, canGoToPlayedFile: false }));
                }
                return;
            }

            setState((s) => ({ ...s, canGoToPlayedFile: true }));
        });

        return () => {
            onPlayerStatusChangedSubscription.unsubscribe();
        };
    }, [props.id, state.canGoToPlayedFile]);

    const handleGoToTheTop = () => {
        const el = document.getElementById('page-content');
        el?.scrollTo({
            top: 0,
            left: 0,
            behavior: 'smooth',
        });
    };

    const handleGoBackClick = () => {
        navigate(playListsPath);
    };

    const handleGoToPlayedFile = () => {
        const elements = document.querySelectorAll('[data-played-file="true"]');
        if (elements.length === 0) {
            return;
        }
        const el = elements[0];
        el.scrollIntoView({
            behavior: 'smooth',
            block: 'center',
            inline: 'center',
        });
    };

    const searchChanged = (newVal: string) => {
        setSearch(newVal);
    };

    const handleAddFiles = async (path: string | null, includeSubFolder: boolean, onlyVideo: boolean): Promise<void> => {
        setShowAddFilesDialog(false);
        if (path) {
            await castItHub.connection.addFolderOrFileOrUrl(props.id, path, includeSubFolder, onlyVideo);
        }
    };

    return (
        <>
            <AppBar position="fixed" color="default">
                <Toolbar>
                    <IconButton
                        edge="start"
                        color="inherit"
                        onClick={handleGoBackClick}
                        size="large"
                        sx={(theme) => ({
                            marginRight: theme.spacing(2),
                        })}
                    >
                        <ArrowBack />
                    </IconButton>
                    <Typography variant="h6" noWrap sx={{ flexGrow: 1, display: { xs: 'none', sm: 'block' } }}>
                        {props.name}
                    </Typography>
                    <StyledSearchContainer>
                        <StyledSearchIconContainer>
                            <SearchIcon />
                        </StyledSearchIconContainer>
                        <StyledInputBase
                            placeholder={`${translations.search}...`}
                            onChange={(e) => searchChanged(e.target.value)}
                            inputProps={{ 'aria-label': 'search' }}
                        />
                    </StyledSearchContainer>
                    <IconButton onClick={() => setShowAddFilesDialog(true)} size="large">
                        <Add />
                    </IconButton>
                    <PlayListLoopShuffleButton id={props.id} loop={props.loop} shuffle={props.shuffle} renderLoop />
                    <PlayListLoopShuffleButton id={props.id} loop={props.loop} shuffle={props.shuffle} />
                    {state.canGoToPlayedFile ? (
                        <IconButton color="inherit" onClick={handleGoToPlayedFile} size="large">
                            <Redo />
                        </IconButton>
                    ) : null}
                    <IconButton color="inherit" onClick={handleGoToTheTop} size="large">
                        <ArrowUpward />
                    </IconButton>
                </Toolbar>
                <PlayListLoadingIndicator playListId={props.id} />
                <AddFilesDialog isOpen={showAddFilesDialog} onClose={handleAddFiles} />
            </AppBar>
            <Toolbar />
        </>
    );
}

export default React.memo(PlayListAppBar);
