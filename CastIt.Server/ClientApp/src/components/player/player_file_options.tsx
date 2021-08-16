import { makeStyles, createStyles, IconButton, Divider, ListItemText, Menu, MenuItem } from '@material-ui/core';
import { Audiotrack, HighQuality, Search, Subtitles, CheckTwoTone } from '@material-ui/icons';
import MenuIcon from '@material-ui/icons/Menu';
import { Fragment, useEffect, useState } from 'react';
import PopupState, { bindTrigger, bindMenu, InjectedProps } from 'material-ui-popup-state';
import { onPlayerStatusChanged, setFileOptions } from '../../services/castithub.service';
import { IFileItemOptionsResponseDto } from '../../models';
import translations from '../../services/translations';

const useStyles = makeStyles((theme) =>
    createStyles({
        menuItemText: {
            marginLeft: 10,
        },
        subMenuItem: {
            marginLeft: 20,
        },
    })
);

interface State {
    contentIsBeingPlayed: boolean;
    selectedAudioIndex: number;
    audios: IFileItemOptionsResponseDto[];
    selectedSubtitleIndex: number;
    subtitles: IFileItemOptionsResponseDto[];
    selectedQualityIndex: number;
    qualities: IFileItemOptionsResponseDto[];
}

const initialState: State = {
    contentIsBeingPlayed: false,
    selectedAudioIndex: 0,
    audios: [],
    selectedSubtitleIndex: 0,
    subtitles: [],
    selectedQualityIndex: 0,
    qualities: [],
};

function PlayerFileOptions() {
    const classes = useStyles();
    const [state, setState] = useState(initialState);

    useEffect(() => {
        const onPlayerStatusChangedSubscription = onPlayerStatusChanged.subscribe((status) => {
            if (!status.playedFile) {
                setState(initialState);
                return;
            }

            setState({
                contentIsBeingPlayed: true,
                selectedAudioIndex: status.playedFile.currentFileAudioStreamIndex,
                audios: status.playedFile.currentFileAudios,
                selectedSubtitleIndex: status.playedFile.currentFileSubTitleStreamIndex,
                subtitles: status.playedFile.currentFileSubTitles,
                selectedQualityIndex: status.playedFile.currentFileQuality,
                qualities: status.playedFile.currentFileQualities,
            });
        });
        return () => {
            onPlayerStatusChangedSubscription.unsubscribe();
        };
    }, []);

    if (!state.contentIsBeingPlayed) {
        return null;
    }

    const handleOptionChange = async (id: number, popupState: InjectedProps): Promise<void> => {
        popupState.close();
        const options = [...state.audios, ...state.subtitles, ...state.qualities];
        const selected = options.find((op) => op.id === id);
        if (!selected) {
            return;
        }
        await setFileOptions(id, selected.isAudio, selected.isSubTitle, selected.isQuality);
    };

    const buildSubMenuItem = (
        key: string,
        text: string,
        isSelected: boolean,
        isEnabled: boolean,
        popupState: InjectedProps,
        icon: JSX.Element | null = null
    ): JSX.Element => (
        <MenuItem
            key={key}
            className={classes.subMenuItem}
            disabled={!isEnabled}
            onClick={() => handleOptionChange(Number(key), popupState)}
        >
            {isSelected ? <CheckTwoTone /> : icon}
            <ListItemText className={classes.menuItemText} primary={text} />
        </MenuItem>
    );

    const buildSubMenuItemFromOption = (option: IFileItemOptionsResponseDto, popupState: InjectedProps): JSX.Element =>
        buildSubMenuItem(`${option.id}`, option.text, option.isSelected, option.isEnabled, popupState);

    const audios = (popupState: InjectedProps): JSX.Element[] => {
        const items = state.audios.map((audio) => buildSubMenuItemFromOption(audio, popupState));

        if (items.length === 0) {
            items.push(buildSubMenuItem('na-audio', 'N/A', true, false, popupState));
        }
        return items;
    };

    // TODO: LOADSUBS
    const subtitles = (popupState: InjectedProps): JSX.Element[] =>
        [buildSubMenuItem('LoadSubs', translations.load, false, true, popupState, <Search />)].concat(
            state.subtitles.map((sub) => buildSubMenuItemFromOption(sub, popupState))
        );

    const qualities = (popupState: InjectedProps): JSX.Element[] => {
        const items = state.qualities.map((quality) => buildSubMenuItemFromOption(quality, popupState));

        if (items.length === 0) {
            items.push(buildSubMenuItem('na-quality', 'N/A', true, false, popupState));
        }
        return items;
    };

    return (
        <PopupState variant="popover">
            {(popupState) => (
                <Fragment>
                    <IconButton color="inherit" {...bindTrigger(popupState)}>
                        <MenuIcon fontSize="large" />
                    </IconButton>
                    <Menu {...bindMenu(popupState)}>
                        <MenuItem disabled>
                            <Subtitles />
                            <ListItemText className={classes.menuItemText} primary={translations.subtitles} />
                        </MenuItem>
                        {subtitles(popupState)}
                        <Divider />
                        <MenuItem disabled>
                            <Audiotrack />
                            <ListItemText className={classes.menuItemText} primary={translations.audio} />
                        </MenuItem>
                        {audios(popupState)}
                        <Divider />
                        <MenuItem disabled>
                            <HighQuality />
                            <ListItemText className={classes.menuItemText} primary={translations.quality} />
                        </MenuItem>
                        {qualities(popupState)}
                    </Menu>
                </Fragment>
            )}
        </PopupState>
    );
}

export default PlayerFileOptions;
