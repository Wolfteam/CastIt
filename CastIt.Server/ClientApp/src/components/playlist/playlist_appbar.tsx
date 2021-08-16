import {
    alpha,
    AppBar,
    createStyles,
    Divider,
    IconButton,
    InputBase,
    ListItemText,
    makeStyles,
    Menu,
    MenuItem,
    Theme,
    Toolbar,
    Typography,
} from '@material-ui/core';
import SearchIcon from '@material-ui/icons/Search';
import { ArrowBack, ArrowUpward, Redo, Audiotrack, HighQuality, Subtitles, Search } from '@material-ui/icons';
import MenuIcon from '@material-ui/icons/Menu';
import { playListsPath } from '../../routes';
import { useHistory } from 'react-router-dom';
import { Fragment, useEffect, useState } from 'react';
import PopupState, { bindMenu, bindTrigger } from 'material-ui-popup-state';
import PlayListLoopShuffleButton from './playlist_loop_shuffle_button';
import translations from '../../services/translations';
import { onPlayerStatusChanged } from '../../services/castithub.service';
import PlayListLoadingIndicator from './playlist_loading_indicator';

const useStyles = makeStyles((theme: Theme) =>
    createStyles({
        root: {
            flexGrow: 1,
        },
        menuButton: {
            marginRight: theme.spacing(2),
        },
        title: {
            flexGrow: 1,
            display: 'none',
            [theme.breakpoints.up('sm')]: {
                display: 'block',
            },
        },
        search: {
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
        },
        searchIcon: {
            padding: theme.spacing(0, 2),
            height: '100%',
            position: 'absolute',
            pointerEvents: 'none',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
        },
        inputRoot: {
            color: 'inherit',
        },
        inputInput: {
            padding: theme.spacing(1, 1, 1, 0),
            // vertical padding + font size from searchIcon
            paddingLeft: `calc(1em + ${theme.spacing(4)}px)`,
            transition: theme.transitions.create('width'),
            width: '100%',
            [theme.breakpoints.up('sm')]: {
                width: '12ch',
                '&:focus': {
                    width: '20ch',
                },
            },
        },
    })
);

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
    const classes = useStyles();
    const history = useHistory();
    const [search, setSearch] = useState<string>('');
    const [state, setState] = useState<State>({
        canGoToPlayedFile: false,
    });

    useEffect(() => {
        const timeout = setTimeout(async () => {
            if (search !== props.searchText) {
                props.onSearch(search);
            }
        }, 500);

        return () => clearTimeout(timeout);
    }, [props.onSearch, props.searchText, search]);

    useEffect(() => {
        const onPlayerStatusChangedSubscription = onPlayerStatusChanged.subscribe((status) => {
            if (!status.playedFile || status.playedFile.playListId != props.id) {
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
        history.replace(playListsPath);
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

    return (
        <Fragment>
            <AppBar position="fixed" color="default">
                <Toolbar>
                    <IconButton edge="start" className={classes.menuButton} color="inherit" onClick={handleGoBackClick}>
                        <ArrowBack />
                    </IconButton>
                    <Typography className={classes.title} variant="h6" noWrap>
                        {props.name}
                    </Typography>
                    <div className={classes.search}>
                        <div className={classes.searchIcon}>
                            <SearchIcon />
                        </div>
                        <InputBase
                            placeholder={`${translations.search}...`}
                            classes={{
                                root: classes.inputRoot,
                                input: classes.inputInput,
                            }}
                            onChange={(e) => searchChanged(e.target.value)}
                            inputProps={{ 'aria-label': 'search' }}
                        />
                    </div>
                    <PlayListLoopShuffleButton id={props.id} loop={props.loop} shuffle={props.shuffle} renderLoop />
                    <PlayListLoopShuffleButton id={props.id} loop={props.loop} shuffle={props.shuffle} />
                    {state.canGoToPlayedFile ? (
                        <IconButton color="inherit" onClick={handleGoToPlayedFile}>
                            <Redo />
                        </IconButton>
                    ) : null}
                    <IconButton color="inherit" onClick={handleGoToTheTop}>
                        <ArrowUpward />
                    </IconButton>
                </Toolbar>
                <PlayListLoadingIndicator playListId={props.id} />
            </AppBar>
            <Toolbar />
        </Fragment>
    );
}

export default PlayListAppBar;
